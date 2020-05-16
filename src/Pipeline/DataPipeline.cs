using System;
using System.Collections.Generic;
using System.Linq;
using XQ.DataMigration.Data;
using XQ.DataMigration.Data.DataSources;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Pipeline.Commands;
using XQ.DataMigration.Pipeline.Trace;

namespace XQ.DataMigration.Pipeline
{
    /// <summary>
    /// Transition which transit objects data from DataSet of source system to DataSet of target system
    /// </summary>
    public class DataPipeline
    {
        public bool Enabled { get; set; } = true;
        public string Name { get; set; }
        public bool TraceObjects { get; set; }

        public IDataSource Source { get; set; }
        public ITargetSource Target { get; set; }

        public int SaveCount { get; set; } = 50;

        public TargetObjectsSaver Saver { get; set; }

        public List<ComplexCommand<CommandBase>> Transitions { get; set; }

        private MigrationTracer Tracer => Migrator.Current.Tracer;

        public void Initialize()
        {
            if (Target == null)
                throw new ArgumentNullException();

            if (Source == null)
                throw new ArgumentNullException();

            if (Saver == null)
            {
                Saver = new TargetObjectsSaver();
                Saver.SaveCount = SaveCount;
            }

            Saver.TargetSource = (ITargetSource)Target;//NEED REFACTOR TO AVOID EXPLICIT CONVERTION

            Transitions.ForEach(i => i.Initialize(null));
        }

        public void Run()
        {
            Tracer.TraceLine("");
            Tracer.TraceLine($"-> Start {Name} data transition", ConsoleColor.Magenta);
            Tracer.Indent();

            var srcDataSet = Source.GetData();
            long objectsCount = srcDataSet.Count();
            long completedCount = 0;
            foreach (var sourceObject in srcDataSet)
            {
                completedCount++;
                if (sourceObject == null)
                    continue;

                var target = TransitObject(sourceObject);

                if (target == null)
                    continue;

                Saver.Push(target);


                TraceLine($"<- {Name} object transition completed {completedCount / objectsCount:P1} ({completedCount} of {objectsCount}) \\n");
            }

            Saver.TrySave();

            Tracer.IndentBack();
            Tracer.TraceLine($"<- End {Name} data transition\\n", ConsoleColor.Magenta);
        }

        private IValuesObject TransitObject(IValuesObject sourceObject)
        {
            var target = Target.GetObjectByKeyOrCreate(sourceObject.Key);

            //target can be empty when using TransitMode = OnlyExitedObjects
            if (target == null)
                return null;

            var ctx = new ValueTransitContext(sourceObject, target, null);

            TraceLine($"-> Start {Name} object transition, key [{sourceObject.Key}], IsNew:  {target.IsNew}");

            ctx.DataPipeline = this;

            TransitValues(ctx);

            if (ctx.Flow == TransitionFlow.SkipObject)
            {
                //If object just created and skipped by migration logic - need to remove it from cache
                //becaus it's invalid and must be removed from cache to avoid any referencing to this object
                //by any migration logic (lookups, key ytansitions, etc.)
                //If object is not new, it means that it's already saved and passed by migration validation
                if (target.IsNew)
                    Target.InvalidateObject(target);

                return null;
            }

            if (ctx.Flow == TransitionFlow.Stop)
            {
                Migrator.Current.Tracer.TraceError("Transition stopped by TransitionFlow.Stop", ctx);
                throw new Exception("Transition stopped by TransitionFlow.Stop");
            }

            return target;
        }

        protected void TransitValues(ValueTransitContext ctx)
        {
            foreach (var childTransition in Transitions)
            {
                //every time after value transition finishes - reset current value to Source object
                ctx.SetCurrentValue(ctx.Source);

                childTransition.TraceColor = ConsoleColor.Yellow;

                if (MapConfig.Current.TraceValueTransition == true)
                    ctx.Trace = true;

                childTransition.Execute(ctx);

                if (ctx.Flow == TransitionFlow.SkipValue)
                {
                    ctx.Flow = TransitionFlow.Continue;
                    TraceLine($"<- Breaking value");
                    continue;
                }

                if (ctx.Flow != TransitionFlow.Continue)
                {
                    TraceLine($"<- Breaking {this.GetType().Name}");
                    break;
                }
            }
        }

        protected virtual void TraceLine(string message)
        {
            if (TraceObjects)
                Migrator.Current.Tracer.TraceLine(message, ConsoleColor.Gray);
        }
    }
}