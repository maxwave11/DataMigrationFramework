using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using XQ.DataMigration.Data;
using XQ.DataMigration.Enums;
using XQ.DataMigration.MapConfiguration;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.Trace;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ValueTransitions;

namespace XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions
{
    /// <summary>
    /// Transition which transit objects data from DataSet of source system to DataSet of target system
    /// </summary>
    public class TransitDataCommand
    {
        public bool Enabled { get; set; } = true;
        public string Name{ get; set; }

        public IDataSource Source { get; set; }
        public IDataSource Target { get; set; }

        public int SaveCount { get; set; } = 50;

        public TargetObjectsSaver Saver { get; set; }
        
        public ObjectTransitMode TransitMode { get; set; }
        
        public List<ComplexTransition<TransitionNode>> Transitions { get; set; }

        private MigrationTracer Tracer => Migrator.Current.Tracer;

        public void Initialize()
        {
            if (Target == null)
                throw new ArgumentNullException();
            
            if (Source == null)
                throw new ArgumentNullException();

            Saver = new TargetObjectsSaver((ITargetProvider)Target)//NEED REFACTOR TO AVOID EXPLICIT CONVERTION
            {
                SaveCount = SaveCount
            };
            
            
            Transitions.ForEach(i=>i.Initialize(null));
        }

        public void Run()
        {
            Migrator.Current.Tracer.TraceLine($"-> Start {Name} data transition");
            
            Tracer.Indent();
            
            var srcDataSet = Source.GetData();

            foreach (var sourceObject in srcDataSet)
            {
                if (sourceObject == null)
                    continue;

                var target = TransitObject(sourceObject);
                
                if (target==null)
                    continue;
                
                
                Saver.Push(new[] {target});
                TraceLine($"<- {Name} object transition completedn\\n");
                // TraceLine($"Completed {completedObjects / totalObjects:P1} ({completedObjects} of {totalObjects})");
            }

            Saver.TrySave();
            
            Tracer.IndentBack();
            Migrator.Current.Tracer.TraceLine($"<- End {Name} data transition\\n");
        }

        private IValuesObject TransitObject(IValuesObject sourceObject)
        {
            var target = GetTargetObject(sourceObject.Key);

            //target can be empty when using TransitMode = OnlyExitedObjects
            if (target == null)
                return null;

            var ctx = new ValueTransitContext(sourceObject, target, null);

            TraceLine($"-> Start {Name} object transition, key [{sourceObject.Key}], IsNew:  {target.IsNew}");

            ctx.TransitDataCommand = this;

            var valuesTransitResult = TransitValues(ctx);

            switch (valuesTransitResult.Flow)
            {
                case TransitionFlow.SkipObject:
                    return null;
                case TransitionFlow.Stop:
                    throw new Exception("Transition stopped - " + valuesTransitResult.Message);
                default:
                    return target;
            }
        }

        protected TransitResult TransitValues(ValueTransitContext ctx)
        {
            try
            {
                // Migrator.Current.Tracer.Indent();
                foreach (var childTransition in Transitions)
                {
                    //Every time after value transition finishes - reset current value to Source object
                    ctx.SetCurrentValue("DataTransitCommand", ctx.Source);
                    
                    childTransition.TraceColor = ConsoleColor.Yellow;
                    
                    var valueTransitResult =  childTransition.Transit(ctx);

                    if (valueTransitResult.Flow == TransitionFlow.SkipValue)
                    {
                        TraceLine($"<- Breaking value");
                        continue;
                    }

                    if (valueTransitResult.Flow != TransitionFlow.Continue)
                    {
                        TraceLine($"<- Breaking {this.GetType().Name}");
                        return valueTransitResult;
                    }
                }
            }
            finally
            {
                // Migrator.Current.Tracer.IndentBack();
            }

            return new TransitResult(ctx.TransitValue);
        }
        
        private  IValuesObject GetTargetObject(string key)
        {
            var targetObject = Target.GetObjectsByKey(key)?.SingleOrDefault();

            switch (TransitMode)
            {
                case ObjectTransitMode.OnlyExistedObjects:
                    return targetObject;
                case ObjectTransitMode.OnlyNewObjects when targetObject != null:
                    TraceLine($"Object already exist, skipping, because TransitMode = TransitMode.OnlyNewObjects");
                    return null;
            }

            if (targetObject != null)
                return targetObject;

            targetObject = ((ITargetProvider)Target).CreateObject(key);
            return targetObject;
        }
        
        protected virtual void TraceLine(string message)
        {
           // if (MapConfig.Current.TraceObjectTransition)
                Migrator.Current.Tracer.TraceLine(message);
        }
    }
}