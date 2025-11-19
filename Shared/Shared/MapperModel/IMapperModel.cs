using System;

namespace Shared.MapperModel
{
    public interface IMapperModel
    {
        Type GetModelType();
        Type GetEntityType();
        Type GetKeyType();
    }
}