using System;

namespace UwU.EasyData.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class TableFieldAttribute : Attribute
    {
        public string ColumnName { get; }

        public TableFieldAttribute(string columnName)
        {
            this.ColumnName = columnName;
        }
    }
}