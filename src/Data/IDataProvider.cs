using System;

namespace XQ.DataMigration.Data
{
    /// <summary>
    /// Common interface for any data provider which provides access to 
    /// particular DataSet of this provider
    /// </summary>
    public interface IDataProvider : IDisposable
    {
        IDataSet GetDataSet(string dataSetId);
    }
}