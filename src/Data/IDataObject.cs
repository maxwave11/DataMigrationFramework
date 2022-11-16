using System.Collections.Generic;

namespace DataMigration.Data
{
    public interface IDataObject
    {
        object this[string name] { get; set; }
        string[] FieldNames { get; }
        string Key { get; set; }
        object Native { get; }
        bool IsNew { get; }
        uint RowNumber { get; set; }
        string Query{ get; set; }

        object GetValue(string name);
        void SetValue(string name, object value);
        bool IsEmpty();
        string GetInfo();
    }
}