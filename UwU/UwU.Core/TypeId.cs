using Collections.Pooled;
using System;

namespace UwU.Core
{
    public class TypeId
    {
        private readonly PooledDictionary<Type, long> indexCache;
        private readonly PooledList<Type> typeCache;

        private long currentId;

        public TypeId()
        {
            this.currentId = 0;
            this.indexCache = new PooledDictionary<Type, long>(16);
            this.typeCache = new PooledList<Type>(16);
        }

        public long GetId<Type>()
        {
            long index;
            var type = typeof(Type);

            if (this.indexCache.TryGetValue(type, out index) == false)
            {
                this.typeCache.Add(type);
                this.indexCache.Add(type, this.currentId);
                index = this.currentId;
                this.currentId++;
            }

            return index;
        }

        public long GetId(Type type)
        {
            long index;

            if (this.indexCache.TryGetValue(type, out index) == false)
            {
                this.typeCache.Add(type);
                this.indexCache.Add(type, this.currentId);
                index = this.currentId;
                this.currentId++;
            }

            return index;
        }

        public Type GetTypeFromID(int id)
        {
            return this.typeCache[id];
        }
    }
}