using Application.Dto.Campus;
using Application.Dto.Common;
using Application.Dto.Mcc;
using Application.Dto.Security;
using Application.Services.Common;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Dto.Filters;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Shared.Configuration;
using Shared.Extensions;
using Shared.Helper;
using Shared.MapperModel;
using Shared.Pagination;
using Shared.Response;
using System.Linq.Expressions;
using static QRCoder.PayloadGenerator;
using static Shared.Constants;
using NotificationType = Domain.Enums.NotificationType;
namespace Infrastructure.Services
{
    public interface ITeacherService
    {
        Task<ServiceResponse<LookupUserBundleDto>> GetAllUserLookupsAsync(Guid teacherId);
        Task<ServiceResponse<List<StudentEnrollmentDto>>> GetAllStudentAsync(Guid campusGradeId);
        Task<ServiceResponse<PaginatedList<TeacherDto>>> GetAllAsync(int pageIndex, int pageSize, UserInfoDto filter);
        Task<ServiceResponse<TeacherDto>> GetByIdAsync(Guid id);
        Task<ServiceResponse> SaveAsync(Guid id, TeacherDto data);
        Task<ServiceResponse<ProcessFormDto>> GenereteFormAsync(CreateProcessFormDto data);
        Task<ServiceResponse<ProcessFormDataDto>> GetFormAsync(string token);
        Task<ServiceResponse> UpdateFormAsync(string token, string status, ProcessFormDataDto req);
        Task<ServiceResponse> AddProcessFormReviewAsync(string token, string type, CreateProcessFormReviewDto req);
        Task<ServiceResponse> DeleteAsync(Guid id);
        Task<ServiceResponse<ProcessFormInboxDto>> GetInboxAsync(Guid teacherId, string status, int pageIndex, int pageSize);

    }

    public class TeacherService : ITeacherService
    {
        private readonly IReadRepository<StudentEnrollment> _readStudentEnrollment;
        private readonly IReadRepository<Teacher> _read;
        private readonly IReadRepository<Parent> _readCustomer;
        private readonly IWriteRepository<Teacher> _write;
        private readonly IMemoryCache _cache;
        private readonly IIdentityUserService _identityUser;
        private readonly ILoggingService _log;
        private readonly IUserService _userService;
        private readonly IWriteRepository<ProcessForm> _writeProcessForm;
        private readonly IWriteRepository<ProcessFormData> _writeProcessFormData;
        private readonly IReadRepository<ProcessForm> _readProcessForm;
        private readonly IWriteRepository<ProcessFormReview> _writeProcessFormReview;
        private readonly IReadRepository<ProcessFormReview> _readProcessFormReview;
        private readonly INotificationQueueService _svc;
        public TeacherService(IReadRepository<Teacher> read,
                              IWriteRepository<Teacher> write,
                              IMemoryCache cache,
                              IIdentityUserService identityUser,
                              ILoggingService log,
                              IReadRepository<StudentEnrollment> readStudentEnrollment,
                              IUserService userService,
                              IWriteRepository<ProcessForm> writeProcessForm,
                              IReadRepository<ProcessForm> readProcessForm,
                              IWriteRepository<ProcessFormData> writeProcessFormData,
                              IReadRepository<ProcessFormReview> readProcessFormReview,
                              IWriteRepository<ProcessFormReview> writeProcessFormReview,
                              INotificationQueueService svc,
                              IReadRepository<Parent> readCustomer
                               )
        {
            _read = read;
            _cache = cache;
            _identityUser = identityUser;
            _log = log;
            _write = write;
            _readStudentEnrollment = readStudentEnrollment;
            _userService = userService;
            _writeProcessForm = writeProcessForm;
            _readProcessForm = readProcessForm;
            _writeProcessFormData = writeProcessFormData;
            _readProcessFormReview = readProcessFormReview;
            _writeProcessFormReview = writeProcessFormReview;
            _svc = svc;
            _readCustomer = readCustomer;
        }
        public async Task<ServiceResponse<PaginatedList<TeacherDto>>> GetAllAsync(int pageIndex, int pageSize, UserInfoDto filter)
        {
            var sr = new ServiceResponse<PaginatedList<TeacherDto>>();
            try
            {
                var parameters = new PagerParameters(pageIndex, pageSize);
                parameters.SortDirection = "ASC";
                parameters.SortField = "NAME";
                var includes = new[]
                {
                    nameof(Role)
                };


                Expression<Func<Teacher, bool>> predicate = x =>
                (!filter.IsDeleted.HasValue || x.IsDeleted == filter.IsDeleted.Value)
                && (string.IsNullOrEmpty(filter.Text) || x.User.Name.Contains(filter.Text))
                && (string.IsNullOrEmpty(filter.Text) || x.User.Email.Contains(filter.Text))
                && (string.IsNullOrEmpty(filter.Text) || x.User.Login.Contains(filter.Text));


                var items = await _read.GetPaginationAsync<object>(parameters, includes, predicate, orderBy: q => q.CreatedAt);

                if (!items.Status)
                {
                    return new ServiceResponse<PaginatedList<TeacherDto>>
                    {
                        Errors = items.Errors
                    };
                }

                // 4) Mapear a DTO y devolver
                var data = new PaginatedList<TeacherDto>
                {
                    Count = items.Data.Count,
                    List = MappingHelper
                                .MapEntityListToMapperModelList<Teacher, TeacherDto>(items.Data.List)
                };
                sr.Data = data;
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }
        public async Task<ServiceResponse<TeacherDto>> GetByIdAsync(Guid id)
        {
            var sr = new ServiceResponse<TeacherDto>();
            try
            {

                var item = await _read
                                    .GetByIdAsync(id, new[] { nameof(User) });
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }

                sr.Data = new TeacherDto(item.Data);

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse> SaveAsync(Guid id, TeacherDto data)
        {
            var sr = new ServiceResponse();
            try
            {
                if (id == Guid.Empty)
                {
                    var entity = data.ToEntity();
                    await _write.AddAsync(_identityUser.UserEmail, entity);
                }
                else
                {
                    var getResp = await _read.GetByIdAsync(id);
                    if (!getResp.Status)
                    {
                        sr.AddErrors(getResp.Errors);
                        return sr;
                    }

                    var entity = getResp.Data;

                    entity.User.Name = data.User.Name;
                    entity.User.Email = data.User.Email;
                    entity.User.PasswordHash = data.User.PasswordHash ?? entity.User.PasswordHash;
                    entity.User.Phone = data.User.Phone;
                    entity.User.IsActive = data.User.IsActive;
                    entity.UpdatedAt = DateTime.UtcNow;
                    await _write.UpdateAsync(_identityUser.UserEmail, entity);
                }
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse> DeleteAsync(Guid id)
        {
            var sr = new ServiceResponse();
            try
            {

                var item = await _read.GetByIdAsync(id);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }
                item.Data.IsDeleted = !item.Data.IsDeleted;
                await _write.UpdateAsync(_identityUser.UserEmail, item.Data);
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse<LookupUserBundleDto>> GetAllUserLookupsAsync(Guid teacherId)
        {
            var sr = new ServiceResponse<LookupUserBundleDto>();
            try
            {

                //var currentUser = await _userService.GetByIdAsync(_identityUser.UserEmail);
                //if (!currentUser.Status)
                //{
                //    sr.AddErrors(currentUser.Errors);
                //    return sr;
                //}
                var teacherConfiguration = await _cache.GetOrCreateAsync($"{KeyCache.TeacherConfiguration}", async entry =>
                {
                    var includes = new[]
                    {
                        // Root: Teacher
                        nameof(Teacher.User),
                        $"{nameof(Teacher.User)}.{nameof(User.Role)}",
                        nameof(Teacher.TeacherCampuses),
                        $"{nameof(Teacher.TeacherCampuses)}.{nameof(TeacherCampus.Campus)}",
                        nameof(Teacher.TeachingAssignments),
                        $"{nameof(Teacher.TeachingAssignments)}.{nameof(TeachingAssignment.AcademicPeriod)}",
                        $"{nameof(Teacher.TeachingAssignments)}.{nameof(TeachingAssignment.CampusGrade)}",
                        $"{nameof(Teacher.TeachingAssignments)}.{nameof(TeachingAssignment.CampusGrade)}.{nameof(CampusGrade.Course)}",
                        $"{nameof(Teacher.TeachingAssignments)}.{nameof(TeachingAssignment.CampusGrade)}.{nameof(CampusGrade.Grade)}",

                        // Si los necesitas más adelante:
                        // $"{nameof(Teacher.TeachingAssignments)}.{nameof(TeachingAssignment.CampusGrade)}.{nameof(CampusGrade.Enrollments)}",
                        // $"{nameof(Teacher.TeachingAssignments)}.{nameof(TeachingAssignment.CampusGrade)}.{nameof(CampusGrade.Enrollments)}.{nameof(StudentEnrollment.Student)}",
                    };

                    var resp = await _read.GetByIdAsync(teacherId, includes);
                    //if (!resp.Status)
                    //{
                    //    sr.AddErrors(resp.Errors);
                    //    return sr;
                    //}
                    return resp?.Data ?? new Teacher();
                });
                var teacherDto =
                    teacherConfiguration is { } teacher
                        ? new TeacherDto()
                        {
                            Id = teacherId,
                            User = new UserDto(teacherConfiguration?.User)
                            {
                                Role = new RoleDto(teacherConfiguration.User.Role)
                            }
                        }
                        : null;

                var teacherCampusesDto =
                    teacherConfiguration?.TeacherCampuses?
                        .Where(tc => tc != null)
                        .Select(tc => new TeacherCampusDto()
                        {
                            Campus = new CampusDto(tc.Campus),
                            EndDate = DateTime.Now,
                            Id = tc.Id,
                            StartDate = DateTime.Now,
                            Teacher = new TeacherDto(tc.Teacher)
                        })
                        .ToList()
                    ?? new List<TeacherCampusDto>();

                var teachingAssignmentsDto =
                    teacherConfiguration?.TeachingAssignments?
                        .Where(ta => ta != null)
                        .Select(ta => new TeachingAssignmentDto()
                        {
                            AcademicPeriod = new AcademicPeriodDto(ta.AcademicPeriod),
                            Course = new CourseDto(ta.CampusGrade.Course),
                            Grade = new GradeDto(ta.CampusGrade.Grade),
                            Id = ta.Id,
                            CampusGradeId = ta.CampusGradeId,
                            EndDate = DateTime.Now,
                            StartDate = DateTime.Now,
                            Teacher = new TeacherDto(ta.Teacher),
                            TeacherType = ta.TeacherType
                        })
                        .ToList()
                    ?? new List<TeachingAssignmentDto>();

                sr.Data = new LookupUserBundleDto
                {
                    Teacher = teacherDto,
                    TeacherCampus = teacherCampusesDto,
                    TeachingAssignment = teachingAssignmentsDto
                };

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse<List<StudentEnrollmentDto>>> GetAllStudentAsync(Guid campusGradeId)
        {
            var sr = new ServiceResponse<List<StudentEnrollmentDto>>();
            try
            {

                //var currentUser = await _userService.GetByIdAsync(_identityUser.UserEmail);
                //if (!currentUser.Status)
                //{
                //    sr.AddErrors(currentUser.Errors);
                //    return sr;
                //}

                var includes = new[]
                {
                    $"{nameof(StudentEnrollment.Student)}",
                    $"{nameof(StudentEnrollment.Student)}.{nameof(StudentEnrollment.Student.ParentStudents)}",
                    $"{nameof(StudentEnrollment.Student)}.{nameof(StudentEnrollment.Student.ParentStudents)}.{nameof(ParentStudent.Parent)}",
                    $"{nameof(StudentEnrollment.Student)}.{nameof(StudentEnrollment.Student.ParentStudents)}.{nameof(ParentStudent.Parent)}.{nameof(ParentStudent.Parent.User)}",
                };

                var resp = await _readStudentEnrollment.GetAllAsync(includes, predicate: x => x.CampusGradeId == campusGradeId);
                if (!resp.Status)
                {
                    sr.AddErrors(resp.Errors);
                    return sr;
                }

                // Asumo que resp.Data es IEnumerable<StudentEnrollment>
                // y que sr.Data es List<StudentEnrollmentDto>
                sr.Data = (resp?.Data ?? Enumerable.Empty<StudentEnrollment>())
                    .Where(en => en != null)
                    .Select(en => new StudentEnrollmentDto(en)
                    {
                        Student = en.Student != null
                            ? new StudentDto(en.Student)
                            {
                                ParentStudents = (en.Student.ParentStudents ?? Enumerable.Empty<ParentStudent>())
                                    .Where(ps => ps != null)
                                    .Select(ps => new ParentStudentDto(ps)
                                    {
                                        Parent = ps.Parent != null
                                            ? new ParentDto(ps.Parent)
                                            {
                                                User = ps.Parent.User != null
                                                    ? new UserDto(ps.Parent.User)
                                                    : null
                                            }
                                            : null
                                    })
                                    .ToList()
                            }
                            : null
                    })
                    .ToList();


            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse<ProcessFormDto>> GenereteFormAsync(CreateProcessFormDto data)
        {
            var sr = new ServiceResponse<ProcessFormDto>();
            try
            {
                var includes = new[]
                {
                    $"{nameof(Parent.User)}",
                };
                var customerDB = await _readCustomer.GetByIdAsync(data.CustomerId, includes);
                if (!customerDB.Status)
                {
                    sr.AddErrors(customerDB.Errors);
                    return sr;
                }

                var accessToken = CodeGen.GenerateAccessCode(15, false);
                var entity = data.ToEntity();
                var highlight = string.Empty;
                var qr = string.Empty;
                var _type = "link";
                var url= $"{GeneralSettings.WebSite}/working/form-diap/{accessToken}";
                if (entity.SendMethod.Equals("qr", StringComparison.OrdinalIgnoreCase))
                {
                    qr = CodeGen.GenerateQrPngBase64(url);
                    highlight = string.IsNullOrWhiteSpace(qr) ? "" : "Escanee el código QR o use el enlace:";
                    _type = "qr";

                }
                entity.Version++;
                entity.AccessToken = accessToken;
                entity.Status = "pending";
                entity.Type = $"{NotificationType.FormDiap.ToString().ToLower()}";
                entity.Url = url;
                var resp = await _writeProcessForm.AddAsync(_identityUser.UserEmail, entity);
                if (resp is not ProcessForm)
                {
                    sr.AddError("Error", "Error en la generación del formulario");
                    return sr;
                }
                var item = new ProcessFormDto(resp);
                item.Qr = qr;

                sr.Data = item;



                await _svc.EnqueueEmailAsync(new CreateNotificationQueueDto
                {
                    Title = "Formulario DIAP generado",
                    Recipients = new List<string> { data.EmailSent },
                    Content = new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["reference"] = accessToken,
                        ["url"] = url,
                        ["message"] = "Formulario Diap para padres",
                        ["highlight"] = highlight,
                        ["qrImage"] = qr,
                        ["name"] = customerDB.Data.User.Name,
                        ["targetUser"] = _identityUser.UserEmail,
                        ["date"] = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                        ["subject"] = "Formulario DIAP",
                    },
                    NotificationType = $"{NotificationType.FormDiap.ToString().ToLower()}-{_type}"

                }, _identityUser.UserEmail);
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse<ProcessFormDataDto>> GetFormAsync(string token)
        {
            var sr = new ServiceResponse<ProcessFormDataDto>();
            try
            {
                static string Path(params string[] parts) => string.Join(".", parts);

                // using YourNamespaceWithEntities;
                var includes = new[]
                {
                    $"{nameof(ProcessForm.ProcessFormData)}",
                    $"{nameof(ProcessForm.ProcessFormReviews)}",
                    $"{nameof(ProcessForm.Student)}",
                    $"{nameof(ProcessForm.Student)}.{nameof(ProcessForm.Student.User)}",
                    $"{nameof(ProcessForm.Customer)}",
                    $"{nameof(ProcessForm.Customer)}.{nameof(ProcessForm.Customer.User)}",
                    $"{nameof(ProcessForm.Teacher)}",
                    $"{nameof(ProcessForm.Teacher)}.{nameof(ProcessForm.Teacher.User)}",
                    $"{nameof(ProcessForm.TeachingAssignment)}",
                    $"{nameof(ProcessForm.TeachingAssignment)}.{nameof(ProcessForm.TeachingAssignment.CampusGrade)}",
                    $"{nameof(ProcessForm.TeachingAssignment)}.{nameof(ProcessForm.TeachingAssignment.CampusGrade)}.{nameof(ProcessForm.TeachingAssignment.CampusGrade.Grade)}",
                    $"{nameof(ProcessForm.TeachingAssignment)}.{nameof(ProcessForm.TeachingAssignment.CampusGrade)}.{nameof(ProcessForm.TeachingAssignment.CampusGrade.Course)}",
                };
                var items = await _readProcessForm.GetAllAsync(includes, x => x.AccessToken == token);
                if (!items.Status)
                {
                    sr.AddErrors(items.Errors);
                    return sr;
                }


                var itemForm = items?.Data?.FirstOrDefault() ?? new ProcessForm();
                var dataEntity = itemForm?.ProcessFormData ?? new ProcessFormData();

                var resp = new ProcessFormDataDto(dataEntity)
                {
                    ProessFormId = itemForm.Id,
                    ProessForm = new ProcessFormDto(itemForm)
                    {
                        Student = new StudentDto(itemForm.Student)
                        {
                            User = new UserDto(itemForm.Student?.User)
                        },
                        Customer = new ParentDto(itemForm.Customer)
                        {
                            User = new UserDto(itemForm.Customer?.User)
                        },
                        Teacher = new TeacherDto(itemForm.Teacher)
                        {
                            User = new UserDto(itemForm.Teacher?.User)
                        },
                        TeachingAssignment = new TeachingAssignmentDto(itemForm.TeachingAssignment)
                        {
                            Course = new CourseDto(itemForm.TeachingAssignment?.CampusGrade?.Course),
                            Grade = new GradeDto(itemForm.TeachingAssignment?.CampusGrade?.Grade)
                        },
                        Reviews = itemForm.ProcessFormReviews.Select(t => new ProcessFormReviewDto(t)).ToList(),
                    },


                    FamilyInfo = string.IsNullOrWhiteSpace(itemForm?.ProcessFormData?.FamilyInfo) ? new FamilyInfoDto() : JsonConvert.DeserializeObject<FamilyInfoDto>(itemForm?.ProcessFormData?.FamilyInfo),
                    Plan = string.IsNullOrWhiteSpace(itemForm?.ProcessFormData?.Plan) ? new List<PlanDto>() : JsonConvert.DeserializeObject<List<PlanDto>>(itemForm?.ProcessFormData?.Plan),
                    Priorities = string.IsNullOrWhiteSpace(itemForm?.ProcessFormData?.Priorities) ? new List<PrioritiesDto>() : JsonConvert.DeserializeObject<List<PrioritiesDto>>(itemForm?.ProcessFormData?.Priorities),
                    Learning = string.IsNullOrWhiteSpace(itemForm?.ProcessFormData?.Learning) ? new List<LearningUnitDto>() : JsonConvert.DeserializeObject<List<LearningUnitDto>>(itemForm?.ProcessFormData?.Learning),
                    Signature = string.IsNullOrWhiteSpace(itemForm?.ProcessFormData?.Signature) ? new SignatureParentDto() : JsonConvert.DeserializeObject<SignatureParentDto>(itemForm?.ProcessFormData?.Signature),
                    SignatureTeacher = string.IsNullOrWhiteSpace(itemForm?.ProcessFormData?.Signature) ? new SignatureTeacherDto() : JsonConvert.DeserializeObject<SignatureTeacherDto>(itemForm?.ProcessFormData?.SignatureTeacher),
                    SignatureCoordinator = string.IsNullOrWhiteSpace(itemForm?.ProcessFormData?.Signature) ? new SignatureCoordinatorDto() : JsonConvert.DeserializeObject<SignatureCoordinatorDto>(itemForm?.ProcessFormData?.SignatureCoordinator),

                    ActionPlan = dataEntity.ActionPlan ?? string.Empty,
                    ActionPlanHouse = dataEntity.ActionPlanHouse ?? string.Empty,
                    ActionPlanSchool = dataEntity.ActionPlanSchool ?? string.Empty,
                    Observation = dataEntity.Observation ?? string.Empty,

                };

                sr.Data = resp;

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse> UpdateFormAsync(string token, string status, ProcessFormDataDto req)
        {
            var sr = new ServiceResponse();
            try
            {
                var includes = new[]
                {
                    nameof(ProcessFormData)
                };
                var items = await _readProcessForm.GetAllAsync(includes, x => x.AccessToken == token);
                if (!items.Status)
                {
                    sr.AddErrors(items.Errors);
                    return sr;
                }

                var item = items.Data.FirstOrDefault();
                var entity = items.Data.FirstOrDefault().ProcessFormData;
                if (entity == null)
                {
                    entity = new ProcessFormData
                    {
                        CreatedAt = DateTime.UtcNow,
                        ProessFormId = item.Id,
                        FamilyInfo = JsonConvert.SerializeObject(req.FamilyInfo),
                        Plan = JsonConvert.SerializeObject(req.Plan),
                        Priorities = JsonConvert.SerializeObject(req.Priorities),
                        Learning = JsonConvert.SerializeObject(req.Learning),
                        Signature = JsonConvert.SerializeObject(req.Signature),
                        SignatureTeacher = JsonConvert.SerializeObject(req.SignatureTeacher),
                        SignatureCoordinator = JsonConvert.SerializeObject(req.SignatureCoordinator),
                        ActionPlanHouse = req.ActionPlanHouse,
                        ActionPlanSchool = req.ActionPlanSchool,
                        IsActive = true,
                        Observation = req.Observation,
                    };
                    await _writeProcessFormData.AddAsync(_identityUser.UserEmail, entity);
                }
                else
                {
                    entity.UpdatedAt = DateTime.UtcNow;
                    entity.ProessFormId = item.Id;
                    entity.FamilyInfo = JsonConvert.SerializeObject(req.FamilyInfo);
                    entity.Plan = JsonConvert.SerializeObject(req.Plan);
                    entity.Priorities = JsonConvert.SerializeObject(req.Priorities);
                    entity.Learning = JsonConvert.SerializeObject(req.Learning);
                    entity.Signature = JsonConvert.SerializeObject(req.Signature);
                    await _writeProcessFormData.UpdateAsync(_identityUser.UserEmail, entity);
                }
                item.Status = status;
                item.UpdatedAt = DateTime.UtcNow;
                await _writeProcessForm.UpdateAsync(_identityUser.UserEmail, item);
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse<ProcessFormInboxDto>> GetInboxAsync(Guid teacherId, string status, int pageIndex, int pageSize)
        {
            var sr = new ServiceResponse<ProcessFormInboxDto>();
            try
            {
                var data = new PaginatedList<ProcessFormDto>();
                var parameters = new PagerParameters(pageIndex, pageSize);
                parameters.SortDirection = "DESC";
                parameters.SortField = "CreatedAt";

                var includes = new[]
                {
                    $"{nameof(ProcessForm.ProcessFormData)}",
                    $"{nameof(ProcessForm.Student)}",
                    $"{nameof(ProcessForm.Student)}.{nameof(ProcessForm.Student.User)}",
                    $"{nameof(ProcessForm.Customer)}",
                    $"{nameof(ProcessForm.Customer)}.{nameof(ProcessForm.Customer.User)}",
                    $"{nameof(ProcessForm.Teacher)}",
                    $"{nameof(ProcessForm.Teacher)}.{nameof(ProcessForm.Teacher.User)}",
                    $"{nameof(ProcessForm.TeachingAssignment)}",
                    $"{nameof(ProcessForm.TeachingAssignment)}.{nameof(ProcessForm.TeachingAssignment.CampusGrade)}",
                    $"{nameof(ProcessForm.TeachingAssignment)}.{nameof(ProcessForm.TeachingAssignment.CampusGrade)}.{nameof(ProcessForm.TeachingAssignment.CampusGrade.Grade)}",
                    $"{nameof(ProcessForm.TeachingAssignment)}.{nameof(ProcessForm.TeachingAssignment.CampusGrade)}.{nameof(ProcessForm.TeachingAssignment.CampusGrade.Course)}",
                    $"{nameof(ProcessForm.TeachingAssignment)}.{nameof(ProcessForm.TeachingAssignment.AcademicPeriod)}",
                };

                Expression<Func<ProcessForm, bool>> predicate = x => x.Status == status && x.TeacherId == teacherId;
                Expression<Func<ProcessForm, object>> orderBy = x => x.CreatedAt;
                var itemsDB = await _readProcessForm.GetPaginationAsync(parameters, includes, predicate, orderBy);
                if (!itemsDB.Status)
                {
                    sr.AddErrors(itemsDB.Errors);
                    return sr;
                }

                // Mapear la lista de resultados a DTO
                data = new PaginatedList<ProcessFormDto>
                {
                    Count = itemsDB.Data.Count,
                    List = itemsDB.Data.List.Select(t => new ProcessFormDto(t)
                    {
                        Student = new StudentDto(t.Student)
                        {
                            User = new UserDto(t.Student?.User)
                        },
                        Customer = new ParentDto(t.Customer)
                        {
                            User = new UserDto(t.Customer?.User)
                        },
                        Teacher = new TeacherDto(t.Teacher)
                        {
                            User = new UserDto(t.Teacher?.User)
                        },
                        TeachingAssignment = new TeachingAssignmentDto(t.TeachingAssignment)
                        {
                            AcademicPeriod = new AcademicPeriodDto(t.TeachingAssignment?.AcademicPeriod),
                            Course = new CourseDto(t.TeachingAssignment?.CampusGrade?.Course),
                            Grade = new GradeDto(t.TeachingAssignment?.CampusGrade?.Grade),
                        }
                    }).ToList()
                };
                Expression<Func<ProcessForm, bool>> predicateCount = x => x.TeacherId == teacherId;
                var items = await _readProcessForm.GetAllAsync(
                    includes: includes,
                    predicate: predicateCount
                );

                if (!items.Status)
                {
                    sr.AddErrors(items.Errors);
                    return sr;
                }

                var allStatus = Enum.GetNames(typeof(FormStatus));
                var statusWithQuantity = items
                    .Data
                    .GroupBy(p => p.Status.ToLower())
                    .ToDictionary(g => g.Key, g => g.Count());
                var respStatus = allStatus.ToDictionary(
                    s => s.ToLower(),
                    s => statusWithQuantity.ContainsKey(s.ToLower()) ? statusWithQuantity[s.ToLower()] : 0
                );


                // Retornar los datos empaquetados
                sr.Data = new ProcessFormInboxDto
                {
                    List = new PaginatedList<ProcessFormDto>
                    {
                        Count = data.Count,
                        List = data.List
                    },
                    Status = respStatus
                };
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }
        public async Task<ServiceResponse> AddProcessFormReviewAsync(string token, string type, CreateProcessFormReviewDto req)
        {
            var sr = new ServiceResponse();
            try
            {
                var items = await _readProcessForm.GetAllAsync(null, x => x.AccessToken == token);
                if (!items.Status)
                {
                    sr.AddErrors(items.Errors);
                    return sr;
                }
                var itemForm = items?.Data?.FirstOrDefault() ?? new ProcessForm();
                var resp = await _writeProcessFormReview.AddAsync(_identityUser.UserEmail, new ProcessFormReview
                {
                    CreatedAt = DateTime.UtcNow,
                    Observations = req.Observations,
                    ProessFormId = itemForm.Id,
                    Type = type
                });
                if (itemForm.Status.Equals(FormStatus.Sent.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    itemForm.Status = FormStatus.InProcess.ToString().ToLower();
                    itemForm.UpdatedAt = DateTime.UtcNow;
                    await _writeProcessForm.UpdateAsync(_identityUser.UserEmail, itemForm);
                }
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }
    }
}

