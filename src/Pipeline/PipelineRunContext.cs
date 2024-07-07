using System;
using System.Collections.Generic;
using DataMigrationCore.Data;
using DataMigrationCore.Enums;

namespace DataMigrationCore.Pipeline
{
    public class PipelineRunContext
    {
        public object Value { get; set; }
        public object Context { get; private set; }
        public PipelineFlowControl FlowControl { get; set; }
        private readonly HashSet<object> _objectsToAdd = new();
        private readonly HashSet<object> _objectsToUpdate = new();

        private readonly object _originalContext;

        internal PipelineRunContext(object context)
        {
            Context = context;
            _originalContext = Context;
        }

        internal void SetCustomContext(object context)
        {
            Context = context;
        }

        internal void RestoreOriginalContext()
        {
            Context = _originalContext;
        }

        internal void PushNewObjectToResults(object objectToAdd)
        {
            if (objectToAdd == null)
                throw new ArgumentNullException(nameof(objectToAdd));

            if (_objectsToUpdate.Contains(objectToAdd))
                throw new InvalidOperationException();

            _objectsToAdd.Add(objectToAdd);
        }

        internal void PushUpdatedObjectToResults(object objectToUpdate)
        {
            if (objectToUpdate == null)
                throw new ArgumentNullException(nameof(objectToUpdate));

            if (_objectsToAdd.Contains(objectToUpdate))
                return;

            _objectsToUpdate.Add(objectToUpdate);
        }

        internal PipelineResults GetResults()
        {
            return new PipelineResults
            {
                ObjectsToAdd = _objectsToAdd,
                ObjectsToUpdate = _objectsToUpdate,
            };
        }
    }
}
