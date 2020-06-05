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

        public IDataSource Source { get; set; }
        
        private IDataTarget _targetSystem { get; set; }

        public int SaveCount { get; set; } = 50;

        public TargetObjectsSaver Saver { get; set; }
        
        public TraceMode TraceMode { get; set; }

        public List<CommandSet<CommandBase>> Commands { get; set; }

        private MigrationTracer Tracer => Migrator.Current.Tracer;
        

        public void Initialize()
        {
            if (Source == null)
                throw new ArgumentNullException();

            if (Saver == null)
            {
                Saver = new TargetObjectsSaver();
                Saver.SaveCount = SaveCount;
            }
            //shitty workaround, need to refactor!
            _targetSystem = Commands.SelectMany(i => i.Commands).OfType<GetTargetCommand>().Single().Target;
            Saver.TargetSource = _targetSystem;
        }

        public void Run()
        {
            TraceLine($"\nPIPELINE '{Name}' start ", null);
            Tracer.Indent();

            var srcDataSet = Source.GetData();
        
            foreach (var sourceObject in srcDataSet)
            {
                if (sourceObject == null)
                    continue;

                var target = TransitSourceObject(sourceObject);

                if (target == null)
                    continue;

                Saver.Push(target);
            }

            Saver.TrySave();

            Tracer.IndentBack();
        }

        private IDataObject TransitSourceObject(IDataObject sourceObject)
        {
            var ctx = new ValueTransitContext(sourceObject, null);
            ctx.Trace = (TraceMode | MapConfig.Current.TraceMode).HasFlag(TraceMode.Commands);
            ctx.DataPipeline = this;
            
            try
            {
                RunCommands(ctx);

                if (ctx.Flow == TransitionFlow.SkipObject && ctx.Target != null)
                {
                    //If object just created and skipped by migration logic - need to remove it from cache
                    //becaus it's invalid and must be removed from cache to avoid any referencing to this object
                    //by any migration logic (lookups, key ytansitions, etc.)
                    //If object is not new, it means that it's already saved and passed by migration validation
                    if (ctx.Target.IsNew)
                        _targetSystem.InvalidateObject(ctx.Target);

                    Tracer.TraceEvent(MigrationEvent.ObjectSkipped, ctx, "Source object skipped");

                    return null;
                }

            }
            catch (Exception e)
            {
                Tracer.TraceError("Error occured while pipeline processing", ctx);
                throw;
            }

            return ctx.Target;
        }

        private void RunCommands(ValueTransitContext ctx)
        {
            foreach (var childTransition in Commands)
            {
                //every time after value transition finishes - reset current value to Source object
                ctx.ResetCurrentValue();

                childTransition.TraceColor = ConsoleColor.Yellow;
                
                TraceLine("", ctx);
                
                ctx.Execute(childTransition);

                if (ctx.Flow == TransitionFlow.SkipValue)
                {
                    ctx.Flow = TransitionFlow.Continue;
                    //Tracer.TraceEvent(MigrationEvent.ValueSkipped, ctx,"Value skipped");
                    continue;
                }

                if (ctx.Flow != TransitionFlow.Continue)
                    break;
            }
        }

        private void TraceLine(string message, ValueTransitContext ctx)
        { 
            if ((TraceMode | MapConfig.Current.TraceMode).HasFlag(TraceMode.Objects) || ctx?.Trace == true)
                Tracer.TraceLine(message, ctx, ConsoleColor.Magenta);
        }
    }
}