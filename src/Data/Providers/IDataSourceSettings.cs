namespace XQ.DataMigration.Data
{
    public interface IDataSourceSettings
    {
        int DataStartRowNumber { get; }
        int HeaderRowNumber { get; }
    }
}