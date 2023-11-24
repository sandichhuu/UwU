using System;

namespace UwU.IFS
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class GetComponentInChildren : Attribute
    {
        public bool includeInactive { get; set; }

        public GetComponentInChildren()
        {
            includeInactive = false;
        }

        public GetComponentInChildren(bool includeInactive)
        {
            this.includeInactive = includeInactive;
        }
    }
}