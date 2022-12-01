using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DataMigration.Data.Interfaces;
using DataMigration.Trace;
using DataMigration.Utils;

namespace DataMigration.Data
{
    public class TargetObjectsSaver<TTarget>
    {
        private readonly IMigrationTracer _tracer;
        public  IDataTarget<TTarget> TargetSource { get; set; }

        /// <summary>
        /// Call SaveObjects when transitioned objects count reached this value
        /// </summary>
        public int SaveCount { get; set; } = 10;

        private readonly List<TTarget> _transittedObjects = new List<TTarget>();

        public TargetObjectsSaver(IMigrationTracer tracer)
        {
            _tracer = tracer;
        }

        internal void Push(TTarget objectToSave)
        {
            // if (objectToSave.IsEmpty() || objectToSave.Key.IsEmpty())
            //     return;

            if (_transittedObjects.Contains(objectToSave))
                return;

            _transittedObjects.Add(objectToSave);

            if (_transittedObjects.Count >= SaveCount)
                SaveTargetObjects(_transittedObjects);
        }

        protected virtual void SaveTargetObjects(List<TTarget> targetObjects)
        {
            try
            {
                TraceLine($"\nSaving {targetObjects.Count} objects...");
                //TraceLine($"New objects: {targetObjects.Count(i => i.IsNew)}\n");

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                TargetSource.SaveObjects(targetObjects);
                
                stopWatch.Stop();
                TraceLine($"Saved {targetObjects.Count} objects, time: {stopWatch.Elapsed.TotalMinutes} min\n");
            }
            catch (Exception ex)
            {
                TraceLine("Error while saving transitted objects: \n" + ex, color: ConsoleColor.Red);
                //var objectsInfo = targetObjects.Select(i => i.GetInfo()).Join("\n===========================\n");
                //TraceLine("\nObjects to save: \n\n" + objectsInfo, ConsoleColor.Gray);
                throw;
            }

            targetObjects.Clear();
        }

        void TraceLine(string message, ConsoleColor color = ConsoleColor.Blue)
        {
            _tracer.TraceLine(message, color: color);
        }

        internal void TrySave()
        {
            if (_transittedObjects.Any())
                SaveTargetObjects(_transittedObjects);
        }
    }
}