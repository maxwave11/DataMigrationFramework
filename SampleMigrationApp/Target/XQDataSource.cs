using System;
using System.Collections.Generic;
using System.Linq;
using DataMigration.Data;
using DataMigration.Data.DataSources;
using XQTargetProvider;

namespace XQ.EqDataMigrator.TargetProvider
{
    public class XqDataSource: DataTargetBase
    {

        private static Dictionary<string, List<IDataObject>> _dummyStorage = new Dictionary<string, List<IDataObject>>();
        protected override IEnumerable<IDataObject> GetDataInternal()
        {
            var query = ActualQuery;

            if (!_dummyStorage.ContainsKey(query))
                _dummyStorage.Add(query, new List<IDataObject>());

            return _dummyStorage[query];
        }
     
        
       
        #region ITargetSource

        protected override IDataObject CreateObject(string key)
        {
            var newObject = new DataObject();
            newObject.Key = key;
            var query = ActualQuery;

            if (!_dummyStorage.ContainsKey(query))
                _dummyStorage.Add(query, new List<IDataObject>());

            _dummyStorage[query].Add(newObject);

            return newObject;
        }

        public override void SaveObjects(IEnumerable<IDataObject> objects)
        {
            foreach (DataObject targetObject in objects)
            {
                if (targetObject.IsNew)
                {
                    //put objects to some db context for saving
                }

                targetObject.IsNew = false;
            }

            //some saving logic
        }
        #endregion

    }
}
