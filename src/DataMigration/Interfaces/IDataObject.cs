using System.Collections.Generic;

namespace DataMigration.Data
{
    public interface IDataObject
    {
        object this[string name] { get; set; }
        string Key { get; set; }
        bool IsNew { get; set; }
        uint RowNumber { get; set; }
        string[] FieldNames { get; }
        
        object GetValue(string name);
        void SetValue(string name, object value);
        bool IsEmpty();
        string GetInfo();
    }
}