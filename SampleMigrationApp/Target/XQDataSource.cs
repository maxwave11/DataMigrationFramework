using System;
using System.Collections.Generic;
using System.Linq;
using XQ.DataMigration.Data;
using XQ.DataMigration.Data.DataSources;
using XQTargetProvider;

namespace XQ.EqDataMigrator.TargetProvider
{
    public class XqDataSource: DataTargetBase
    {
        protected override IEnumerable<IDataObject> GetDataInternal()
        {
            //InitializeDbContext();
            //var type = GetEntityType(ActualQuery);

            //_dbContext.Set(type).Load();
            //return _dbContext.Set(type).Local.Cast<object>().Select(i => new XqTargetObject(i));
            return new List<XqTargetObject>();
        }
     
        
        private Type GetEntityType(string entityType)
        {
            return typeof(string);
        }

       
        #region ITargetSource

        protected override IDataObject CreateObject(string key)
        {
            var type = GetEntityType(ActualQuery);
            var newObject = new XqTargetObject("some dummy object", true);
            newObject.Key = key;
            return newObject;
        }

        public override void SaveObjects(IEnumerable<IDataObject> objects)
        {
            foreach (XqTargetObject targetObject in objects)
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
