using System;
using System.Collections.Generic;


namespace DataMigration.Data.DataSources
{
    /// <summary>
    /// Complex data source which consiss of many others. Jus return union of results from all
    /// nested data source. Represents a generic list of nested data sources.
    /// </summary>
    
    // [Yaml("composite-source")]
    // public class CompositeDataSource : List<IDataSource>, IDataSource
    // {
    //     public IEnumerable<IDataObject> GetData()
    //     {
    //         return this.SelectMany(i => i.GetData());
    //     }
    // }
}