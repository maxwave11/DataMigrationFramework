using System;
using System.Collections.Generic;
using System.Linq;

namespace XQ.DataMigration.Data.DataSources
{
    /// <summary>
    /// Complex data source which consiss of many others. Jus return union of results from all
    /// nested data source. Represents a generic list of nested data sources.
    /// </summary>
    public class CompositeDataSource : List<IDataSource>, IDataSource
    {
        public IEnumerable<IDataObject> GetData()
        {
            return this.SelectMany(i => i.GetData());
        }
    }
}