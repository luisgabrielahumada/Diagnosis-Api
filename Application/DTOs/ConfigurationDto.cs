using Domain.Entities;
using Newtonsoft.Json;
using Shared.Extensions;
using Shared.MapperModel;

namespace Infrastructure.Dto
{
    public class ParameterDto : MapperModel<ParameterDto, Setting, int>
    {
        public int Id { get; set; }
        public string? Category { get; set; }
        public string Code { get; set; }
        public string? ControlTitle { get; set; }
        public string? Description { get; set; }
        public object? Value { get; set; }
        public string? ControlType { get; set; }
        public List<ParameterOption>? Options { get; set; }
        public string? InputType { get; set; }

        public ParameterDto() { }

        public ParameterDto(Setting entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(Setting entity)
        {
            if (entity.Options.IsNotNullOrWhiteSpace())
                Options = JsonConvert.DeserializeObject<List<ParameterOption>>(entity.Options);
        }

        protected override void ExtraMapToEntity(Setting entity) { }
    }


    public class ParameterOption
    {
        public string Label { get; set; }
        public string Value { get; set; }
    }

    public class Category
    {
        public string Name { get; set; }
        public List<ParameterDto> Parameters { get; set; }
    }

    public class ConfigurationDto
    {
        public List<Category> Categories { get; set; }
    }
}
