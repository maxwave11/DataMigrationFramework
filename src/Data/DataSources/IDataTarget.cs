﻿using System.Collections.Generic;

namespace DataMigration.Data.DataSources
{
    public interface IDataTarget: IDataSource
    {
        IDataObject GetObjectByKeyOrCreate(string key);
        void SaveObjects(IEnumerable<IDataObject> objects);
        void InvalidateObject(IDataObject dataObject);
    }
}