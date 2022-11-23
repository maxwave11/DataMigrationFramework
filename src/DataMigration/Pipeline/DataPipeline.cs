using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using DataMigration.Data;
using DataMigration.Data.DataSources;
using DataMigration.Enums;
using DataMigration.Pipeline.Commands;
using DataMigration.Pipeline.Pipes;
using DataMigration.Pipeline.Trace;
using DataMigration.Utils;

namespace DataMigration.Pipeline
{
    /// <summary>
    /// Transition which transit data from DataSet of source system to DataSet of target system
    /// </summary>
    public class DataPipeline<TSource, TTarget>: IDataPipeline 
        where TSource : IDataObject
    {
        public bool Enabled { get; set; } = true;
        public string Name { get; set; }

        public IDataSource<TSource> Source { get; set; }
        public IDataTarget Target { get; set; }
        
        private IDataTarget _targetSystem { get; set; }

        public int SaveCount { get; set; } = 50;

        public TargetObjectsSaver Saver { get; set; }
        
        public TraceMode TraceMode { get; set; }

        public IEnumerable<CommandSet<CommandBase>> Commands { get; set; }
        
        public IEnumerable<IEnumerable<CommandBase>> Commands2 { get; set; }

        public IEnumerable<IPipe> Pipes { get; set; } = new List<IPipe>();

        
        public IEnumerable<IEnumerable<Func<ValueTransitContext, object>>> Commands3 { get; set; }

        private MigrationTracer Tracer => Migrator.Current.Tracer;
        

        public void Initialize()
        {
            if (Enabled == false)
                return;
            
            if (Source == null)
                throw new InvalidOperationException($"{nameof(Source)} must be set");

            if (Saver == null)
            {
                Saver = new TargetObjectsSaver();
                Saver.SaveCount = SaveCount;
            }
            //shitty workaround, need to refactor!
            //_targetSystem = Commands.SelectMany(i => i.Commands).OfType<GetTargetCommand>().Single().Target;
            _targetSystem = Target;
            Saver.TargetSource =  _targetSystem;
        }

        public void Run()
        {
            if (!Enabled)
                return;
            
            TraceLine($"\nPIPELINE '{Name}' started ", null);
            Tracer.Indent();

            TraceLine($"DataSource ({ Source }) - Get data...", null);
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
           
            try
            {
                SetTargetObject(ctx);
                RunPipes(ctx);

                if (ctx.Flow == TransitionFlow.SkipObject && ctx.Target != null)
                {
                    //If object just created and skipped by migration logic - need to remove it from cache
                    //because it's invalid and must be removed from cache to avoid any referencing to this object
                    //by any migration logic (lookups, key transitions, etc.)
                    //If object is not new, it means that it's already saved and passed by migration validation
                    if (ctx.Target.IsNew)
                        _targetSystem.InvalidateObject(ctx.Target);

                    //Tracer.TraceEvent(MigrationEvent.ObjectSkipped, ctx, "Source object skipped");

                    return null;
                }
            }
            catch (Exception e) 
            {
                throw new DataMigrationException("Error occured while object processing", ctx, e);
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
        
        private void RunCommands2(ValueTransitContext ctx)
        {
            foreach (var childTransition in Commands2)
            {
                //every time after value transition finishes - reset current value to Source object
                ctx.ResetCurrentValue();

                //childTransition.TraceColor = ConsoleColor.Yellow;
                
                TraceLine("", ctx);
                
                ExecuteInternal(ctx, childTransition);

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
        
        private void RunPipes(ValueTransitContext ctx)  
        {
            foreach (var pipe in Pipes)
            {
                //every time after value transition finishes - reset current value to Source object
                ctx.ResetCurrentValue();

                //childTransition.TraceColor = ConsoleColor.Yellow;
                
                TraceLine("", ctx);
                
                //ExecuteInternal(ctx, childTransition);
                ExecutePipe(ctx, pipe);

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

        private void SetTargetObject(ValueTransitContext ctx)
        {
            var target = _targetSystem.GetObjectByKeyOrCreate(ctx.Source.Key);

            // Target can be empty when using TransitMode = OnlyExitedObjects
            if (target == null)
            {
                ctx.Flow = TransitionFlow.SkipObject;
                return;
            }

            ctx.Target = target;
            
            ctx.TraceLine($"PIPELINE '{ Name }' OBJECT, Row {ctx.Source.RowNumber}, Key [{ctx.Source.Key}], IsNew:  {target.IsNew}", ConsoleColor.Magenta);
        }
        
         void ExecuteInternal(ValueTransitContext ctx, IEnumerable<CommandBase> commands)
        {
            foreach (var childCommand in commands)
            {
                ctx.Execute(childCommand);

                if (ctx.Flow != TransitionFlow.Continue)
                {
                    ctx.TraceLine($"Breaking {this.GetType().Name}");
                    break;
                }
            }
        }
         
         void ExecutePipe(ValueTransitContext ctx, IPipe pipe)
         {
             var pipesSequence = new List<IPipe>();
             var previousPipe = pipe;
             
             do
             {
                 pipesSequence.Add(previousPipe);
                 previousPipe = previousPipe.PreviousPipe;
             } while (previousPipe != null);

             pipesSequence.Reverse();
              
             foreach (var nextPipe in pipesSequence)
             {
               
                 
                 // TraceLine($"{ CommandUtils.GetCommandYamlName(cmd.GetType()) } { cmd.GetParametersInfo() }");
                 
                 TraceLine($"PIPE { nextPipe.ToString() }", ctx);
                 Migrator.Current.Tracer.Indent();
            
                 var result = nextPipe.Execute(ctx.TransitValue, ctx.Source, ctx.Target);
          
                 Migrator.Current.Tracer.IndentBack();
                 
                 
                 ctx.SetCurrentValue(result);
                 
                // ctx.Execute(childCommand);

                 if (ctx.Flow != TransitionFlow.Continue)
                 {
                     ctx.TraceLine($"Breaking {this.GetType().Name}");
                     break;
                 }
             }
         }
    }
}