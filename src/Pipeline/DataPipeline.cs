using System;
using System.Collections.Generic;
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
        
        public TraceMode TraceMode { get; set; }

        public List<CommandSet<CommandBase>> Commands { get; set; }

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

            Saver.TargetSource = Target;
        }

        public void Run()
        {
            TraceLine($"\nPIPELINE '{Name}' start ", null);
            Tracer.Indent();

            var srcDataSet = Source.GetData();
           //long objectsCount = srcDataSet.Count();
           // long completedCount = 0;
            foreach (var sourceObject in srcDataSet)
            {
                //completedCount++;
                if (sourceObject == null)
                    continue;

                var target = TransitObject(sourceObject);

                if (target == null)
                    continue;

                Saver.Push(target);
            }

            Saver.TrySave();

            Tracer.IndentBack();
        }

        private IValuesObject TransitObject(IValuesObject sourceObject)
        {
            var target = Target.GetObjectByKeyOrCreate(sourceObject.Key);

            //target can be empty when using TransitMode = OnlyExitedObjects
            if (target == null)
                return null;

            var ctx = new ValueTransitContext(sourceObject, target, null);

            TraceLine($"PIPELINE '{Name}' OBJECT, Row {sourceObject.RowNumber}, Key [{sourceObject.Key}], IsNew:  {target.IsNew}", ctx);

            ctx.DataPipeline = this;

            try
            {
                TransitValues(ctx);

                if (ctx.Flow == TransitionFlow.SkipObject)
                {
                    //If object just created and skipped by migration logic - need to remove it from cache
                    //becaus it's invalid and must be removed from cache to avoid any referencing to this object
                    //by any migration logic (lookups, key ytansitions, etc.)
                    //If object is not new, it means that it's already saved and passed by migration validation
                    if (target.IsNew)
                        Target.InvalidateObject(target);

                    Tracer.TraceEvent(MigrationEvent.ObjectSkipped, ctx, "Object skipped");

                    return null;
                }

            }
            catch (Exception e)
            {
                Tracer.TraceError("Error occured while pipeline processing", ctx);
                throw;
            }

            return target;
        }

        protected void TransitValues(ValueTransitContext ctx)
        {
            foreach (var childTransition in Commands)
            {
                //every time after value transition finishes - reset current value to Source object
                ctx.ResetCurrentValue();

                childTransition.TraceColor = ConsoleColor.Yellow;
                
                ctx.Trace = (TraceMode | MapConfig.Current.TraceMode).HasFlag(TraceMode.Commands) ;
                
                if(ctx.Trace)
                    TraceLine("", ctx);
                
                childTransition.Execute(ctx);

                if (ctx.Flow == TransitionFlow.SkipValue)
                {
                    ctx.Flow = TransitionFlow.Continue;
                    Tracer.TraceEvent(MigrationEvent.ValueSkipped, ctx,"Value skipped");
                    continue;
                }

                if (ctx.Flow != TransitionFlow.Continue)
                    break;
            }
        }

        protected virtual void TraceLine(string message, ValueTransitContext ctx)
        { 
            if ((TraceMode | MapConfig.Current.TraceMode).HasFlag(TraceMode.Objects))
                Tracer.TraceLine(message, ctx, ConsoleColor.Magenta);
        }
    }
}