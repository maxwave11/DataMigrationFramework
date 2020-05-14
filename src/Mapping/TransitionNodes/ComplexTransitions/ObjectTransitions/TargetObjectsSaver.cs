﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XQ.DataMigration.Data;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions
{
    public class TargetObjectsSaver 
    {
        public  ITargetProvider TargetProvider { get; set; }

        /// <summary>
        /// Call SaveObjects when transitioned objects count reached this value
        /// </summary>
        public int SaveCount { get; set; } = 10;

        public TransitionNode OnSave { get; set; }

        private readonly List<IValuesObject> _transittedObjects = new List<IValuesObject>();

        internal void Push(IValuesObject objectToSave)
        {
            if (objectToSave.IsEmpty() || objectToSave.Key.IsEmpty())
                return;

            if (_transittedObjects.Contains(objectToSave))
                return;

            _transittedObjects.Add(objectToSave);

            if (_transittedObjects.Count >= SaveCount)
                SaveTargetObjects(_transittedObjects);
        }

        protected virtual void SaveTargetObjects(List<IValuesObject> targetObjects)
        {
            try
            {
                Migrator.Current.Tracer.TraceLine($"Saving {targetObjects.Count} objects...");
                var newObjectsCount = targetObjects.Count(i => i.IsNew);

                if (newObjectsCount > 0)
                    Migrator.Current.Tracer.TraceLine($"New objects: {newObjectsCount}");

                var stopWath = new Stopwatch();
                stopWath.Start();

                TargetProvider.SaveObjects(targetObjects);
                stopWath.Stop();

                Migrator.Current.Tracer.TraceLine($"Saved {targetObjects.Count} objects, time: {stopWath.Elapsed.TotalMinutes} min");
            }
            catch (Exception ex)
            {
                var objectsInfo = targetObjects.Select(i => i.GetInfo()).Join("\n===========================\n");
                Migrator.Current.Tracer.TraceLine("=====Error while saving transitted objects: " + ex + objectsInfo);
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