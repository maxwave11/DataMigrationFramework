using System.Collections.Generic;

namespace XQ.DataMigration.Data
{
    public interface IValuesObject
    {
        object this[string name] { get; set; }
        string[] FieldNames { get; }
        string Key { get; set; }
        object Native { get; }
        bool IsNew { get; }
        uint RowNumber { get; set; }

        object GetValue(string name);
        void SetValue(string name, object value);
        bool IsEmpty();
        string GetInfo();
    }
}