namespace XQ.DataMigration.Data
{
    public interface IValuesObject
    {
        object this[string name] { get; }
        string[] FieldNames { get; }
        string Key { get; set; }
        object Native { get; }
        bool IsNew { get; }

        object GetValue(string name);
        void SetValue(string name, object value);
        bool IsEmpty();
        string GetInfo();
    }
}