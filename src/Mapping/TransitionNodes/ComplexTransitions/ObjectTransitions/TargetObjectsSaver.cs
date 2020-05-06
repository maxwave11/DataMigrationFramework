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

        private readonly List<IValuesObject> _transittedObjects = new List<IValuesObject>();

        public TargetObjectsSaver(ITargetProvider targetProvider)
        {
            TargetProvider = targetProvider;
        }

        internal void Push(IEnumerable<IValuesObject> objectsToSave)
        {
            _transittedObjects.AddRange(objectsToSave.Where(obj => !obj.IsEmpty() && !obj.Key.IsEmpty()));

            //need to save after each child transition to avoid referencing to unsaved data
            if (_transittedObjects.Count >= SaveCount)
                SaveTargetObjects(_transittedObjects);
        }


        private void SaveTargetObjects(List<IValuesObject> targetObjects)
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