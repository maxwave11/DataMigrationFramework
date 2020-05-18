using System;
using System.Collections.Generic;
using System.Linq;

namespace XQ.DataMigration.Data.DataSources
{
    public class CompositeDataSource : List<IDataSource>, IDataSource
    {
        public IEnumerable<IValuesObject> GetData()
        {
            return this.SelectMany(i => i.GetData());
        }
    }
}