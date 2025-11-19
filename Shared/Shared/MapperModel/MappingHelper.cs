using Shared.Extensions;
using System.Linq.Expressions;
using System.Reflection;

namespace Shared.MapperModel
{
    public class MappingHelper
    {
        public static void MapEntity(object entity, object reference)
        {
            MapEntity(entity, reference, new string[] { });
        }

        public static void MapEntity<T>(object entity, object reference, Expression<Func<T, object>> columns)
        {
            MapEntity(entity, reference, columns.GetExpressionFields());
        }

        public static void MapEntity(object entity, object reference, string[] columns)
        {
            if (entity == null || reference == null)
                throw new ArgumentNullException("Entity or reference cannot be null.");

            foreach (var pd in reference.GetType().GetRuntimeProperties()
                .Where(x => columns.Length == 0 || columns.Contains(x.Name)))
            {
                var ps = entity.GetType().GetRuntimeProperty(pd.Name);
                if (ps != null)
                {
                    var value = ps.GetValue(entity);
                    if (value is IMapperModel model)
                    {
                        var newReference = Activator.CreateInstance(model.GetEntityType());
                        MapEntity(model, newReference);
                        pd.SetValue(reference, newReference);
                    }
                    else
                    {
                        try
                        {
                            pd.SetValue(reference, ps.GetValue(entity));
                        }
                        catch (Exception ex)
                        {
                            // Ignored
                        }
                    }
                }
            }
        }

        public static E MapModelToEntity<E>(object model)
        {
            var entity = (E)Activator.CreateInstance(typeof(E));
            MapEntity(model, entity);
            return entity;
        }

        public static IEnumerable<E2> MapMapperListToList<E1, E2>(IEnumerable<E1> list)
        {
            var ret = new List<E2>();

            if (list == null)
                list = new List<E1>();

            foreach (var model in list)
                ret.Add(MapModelToEntity<E2>(model));

            return ret;
        }

        public static IEnumerable<E> MapMapperModelListToEntityList<M, E>(IEnumerable<M> list) where M : IMapperModel
        {
            var ret = new List<E>();

            if (list == null)
                list = new List<M>();

            foreach (var model in list)
            {
                var toEntity = model.GetType()
                    .GetRuntimeMethods()
                    .SingleOrDefault(x => x.Name == "ToEntity");
                ret.Add((E)toEntity.Invoke(model, null));
            }

            return ret;
        }

        public static List<M> MapEntityListToMapperModelList<E, M>(IEnumerable<E> list) where M : IMapperModel
        {
            var ret = new List<M>();

            if (list == null)
                list = new List<E>();

            foreach (var entity in list)
            {
                var model = MapModelToEntity<M>(entity);
                var extraMap = model.GetType()
                    .GetRuntimeMethods()
                    .SingleOrDefault(x => x.Name == "ExtraMapFromEntity");
                extraMap?.Invoke(model, new object[] { entity });

                ret.Add(model);
            }

            return ret;
        }
    }
}