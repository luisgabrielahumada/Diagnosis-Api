using System;

namespace Shared.MapperModel
{
    public abstract class MapperModel<M, E, K> : IMapperModel
    {
        public abstract void InitializateData();
        protected abstract void ExtraMapFromEntity(E entity);
        protected abstract void ExtraMapToEntity(E entity);
        protected bool IsInitialized { get; set; }

        private void EntityToModel(E entity)
        {
            this.Map(entity);
            ExtraMapFromEntity(entity);
        }

        protected MapperModel()
        {
        }

        protected MapperModel(K key, Func<K, E> function) : this()
        {
            InitializateData();
            IsInitialized = true;

            EntityToModel(function(key));
        }

        protected MapperModel(E entity, bool initializeData = false)
        {
            if (initializeData)
                InitializateData();

            EntityToModel(entity);
        }


        protected MapperModel(E entity)
        {
            EntityToModel(entity);
        }

        public E ToEntity()
        {
            E entity = this.MapToEntity<E>();
            ExtraMapToEntity(entity);
            return entity;
        }

        public Type GetModelType()
        {
            return typeof(M);
        }

        public Type GetEntityType()
        {
            return typeof(E);
        }

        public Type GetKeyType()
        {
            return typeof(K);
        }
    }
}