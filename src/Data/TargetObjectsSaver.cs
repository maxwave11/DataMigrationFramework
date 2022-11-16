using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DataMigration.Data.DataSources;
using DataMigration.Pipeline.Commands;
using DataMigration.Utils;

namespace DataMigration.Data
{
    public class TargetObjectsSaver 
    {
        public  IDataTarget TargetSource { get; set; }

        /// <summary>
        /// Call SaveObjects when transitioned objects count reached this value
        /// </summary>
        public int SaveCount { get; set; } = 10;

        public CommandBase OnSave { get; set; }

        private readonly List<IDataObject> _transittedObjects = new List<IDataObject>();

        internal void Push(IDataObject objectToSave)
        {
            if (objectToSave.IsEmpty() || objectToSave.Key.IsEmpty())
                return;

            if (_transittedObjects.Contains(objectToSave))
                return;

            _transittedObjects.Add(objectToSave);

            if (_transittedObjects.Count >= SaveCount)
                SaveTargetObjects(_transittedObjects);
        }

        private void TraceLine(string message) 
        {
            Migrator.Current.Tracer.TraceLine(message, color:ConsoleColor.DarkCyan);
        }

        protected virtual void SaveTargetObjects(List<IDataObject> targetObjects)
        {
            try
            {
                TraceLine($"Saving {targetObjects.Count} objects...");
                var newObjectsCount = targetObjects.Count(i => i.IsNew);

                if (newObjectsCount > 0)
                    TraceLine($"New objects: {newObjectsCount}");

                var stopWath = new Stopwatch();
                stopWath.Start();

                TargetSource.SaveObjects(targetObjects);
                stopWath.Stop();

                TraceLine($"Saved {targetObjects.Count} objects, time: {stopWath.Elapsed.TotalMinutes} min");
            }
            catch (Exception ex)
            {
                var objectsInfo = targetObjects.Select(i => i.GetInfo()).Join("\n===========================\n");
                TraceLine("=====Error while saving transitted objects: " + ex + objectsInfo);
                throw;
            }

            targetObjects.Clear();
        }

        internal void TrySave()
        {
            if (_transittedObjects.Any())
                SaveTargetObjects(_transittedObjects);
        }
    }
}