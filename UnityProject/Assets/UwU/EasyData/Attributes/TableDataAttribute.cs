using System;

namespace UwU.EasyData.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableDataAttribute : Attribute
    {
        public string TableName { get; }

        public TableDataAttribute(string tableName)
        {
            this.TableName = tableName;
        }
    }
}