namespace XQ.DataMigration.Data.DataSources
{
    public interface IDataSourceSettings
    {
        int DataStartRowNumber { get; }
        int HeaderRowNumber { get; }
    }
}