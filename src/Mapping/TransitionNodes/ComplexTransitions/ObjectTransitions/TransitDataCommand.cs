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
        public ITargetProvider Target { get; set; }

        public int SaveCount { get; set; } = 50;

        public TargetObjectsSaver Saver { get; set; }
        
        public ObjectTransitMode TransitMode { get; set; }
        
        public List<ComplexTransition<TransitionNode>> Transitions { get; set; }

        private MigrationTracer Tracer => Migrator.Current.Tracer;

        public void Initialize()
        {
            //if (string.IsNullOrEmpty(QueryToSource))
            //    throw new Exception($"{nameof(QueryToSource)} can't be empty in {nameof(TransitDataCommand)}");
            
            if (Target == null)
                throw new ArgumentNullException();
            
            if (Source == null)
                throw new ArgumentNullException();

            Saver = new TargetObjectsSaver(Target)
            {
                TargetProvider = Target,
                SaveCount = SaveCount
            };
            
            
            Transitions.ForEach(i=>i.Initialize(null));
        }

        //protected override void TraceStart(ValueTransitContext ctx, string attributes = "")
        //{
        //    //var queryToSource = ExpressionEvaluator.EvaluateString(QueryToSource, ctx);
                
        //    //attributes = $"{nameof(Name)}=\"{Name}\" {nameof(QueryToSource)}=\"{queryToSource}\"";
        //    //base.TraceStart(ctx, attributes);
        //}

        //protected virtual IEnumerable<IValuesObject> GetSourceObjects(ValueTransitContext ctx)
        //{
        //    try
        //    {
        //        //need to fix this later and recognize expression by more smart way
        //        var queryToSource = ExpressionEvaluator.EvaluateString(QueryToSource, ctx);

        //        if (FetchMode == FetchMode.SourceObject)
        //            return ((IValueObjectsCollecion)ctx.Source).GetObjects(queryToSource);
                
                
        //        var dataProvider = MapConfig.Current.GetDefaultDataProvider();

        //        //if (SourceProviderName.IsNotEmpty())
        //        //    dataProvider = MapConfig.Current.GetDataProvider(SourceProviderName);

        //        return dataProvider.GetDataSet(queryToSource);
        //    }
        //    catch (Exception ex)
        //    {
        //        Tracer.TraceError("Error while trying to get source datat set." + ex, this, null);
        //        return null;
        //    }
        //}

        public virtual void Run()
        {
            
            Migrator.Current.Tracer.TraceLine($"-> Start {Name} data transition");
            
            Tracer.Indent();
            
            var srcDataSet = Source.GetData();

            foreach (var sourceObject in srcDataSet)
            {
                if (sourceObject == null)
                    continue;

                var target = GetTargetObject(sourceObject.Key);

                //target can be empty when using TransitMode = OnlyExitedObjects
                if (target == null)
                    continue;

                var ctx = new ValueTransitContext(sourceObject, target, null);

                TraceLine($"-> Start {Name} object transition, key [{sourceObject.Key}], IsNew:  {target.IsNew}");

                ctx.TransitDataCommand = this;

                var result = TransitChildren(ctx);

                if (result.Flow == TransitionFlow.SkipObject)
                {
                    //If object just created and skipped by migration logic - need to remove it from cache
                    //becaus it's invalid and must be removed from cache to avoid any referencing to this object
                    //by any migration logic (lookups, key ytansitions, etc.)
                    //If object is not new, it means that it's already saved and passed by migration validation
                    if (ctx.Target?.IsNew == true)
                        Target.RemoveObjectFromCache(ctx.Target.Key);

                    continue;
                }

                if (result.Flow != TransitionFlow.Continue)
                {
                    TraceLine($"Breaking {nameof(TransitDataCommand)}");
                    break;
                }

                Saver.Push(new[] {ctx.Target});
                TraceLine($"<- {Name} object transition completedn\\n");

                // TraceLine($"Completed {completedObjects / totalObjects:P1} ({completedObjects} of {totalObjects})");
            }

            Saver.TrySave();
            
            Tracer.IndentBack();
            Migrator.Current.Tracer.TraceLine($"<- End {Name} data transition\\n");

        }
        
        protected TransitResult TransitChildren(ValueTransitContext ctx)
        {
            try
            {
                Migrator.Current.Tracer.Indent();

                foreach (var childTransition in Transitions)
                {
                    //Every time after value transition finishes - reset current value to Source object
                    ctx.SetCurrentValue("DataTransitCommand", ctx.Source);
                    
                    childTransition.Trace = MapConfig.Current.TraceValueTransition;
                    
                    var childTransitResult = TransitChild(childTransition, ctx);
                    
                    if (childTransitResult.Flow == TransitionFlow.SkipValue)
                    {
                        TraceLine($"<- Breaking value");
                        continue;
                    }

                    if (childTransitResult.Flow != TransitionFlow.Continue)
                    {
                        TraceLine($"<- Breaking {this.GetType().Name}");
                        return childTransitResult;
                    }
                }
            }
            finally
            {
                Migrator.Current.Tracer.IndentBack();
            }

            return new TransitResult(ctx.TransitValue);
        }
        
        protected virtual TransitResult TransitChild(TransitionNode childNode, ValueTransitContext ctx)
        {
            childNode.Color = ConsoleColor.Yellow;
            var childTransitResult =  childNode.TransitCore(ctx);
          //  childTransitResult = EndTransitChild(childTransitResult, ctx);
            return childTransitResult;
        }
        
        //protected virtual TransitResult EndTransitChild(TransitResult result, ValueTransitContext ctx)
        //{
        //    if (result.Flow == TransitionFlow.SkipUnit)
        //        return new TransitResult(result.Value);

        //    return result;
        //}
        
        
        private  IValuesObject GetTargetObject(string key)
        {
           // var provider = MapConfig.Current.GetTargetProvider();

            var existedObject = Target.GetObjectByKey(key);


            if (TransitMode == ObjectTransitMode.OnlyExistedObjects)
            {
                return existedObject;
            }

            if (TransitMode == ObjectTransitMode.OnlyNewObjects && existedObject != null)
            {
                TraceLine($"Object already exist, skipping, because TransitMode = TransitMode.OnlyNewObjects");
                return null;
            }

            if (existedObject != null)
                return existedObject;

            var newObject = Target.CreateObject(key);
            return newObject;
        }
        
      

        protected virtual void TraceLine(string message)
        {
            if (MapConfig.Current.TraceObjectTransition)
                Migrator.Current.Tracer.TraceLine(message);
        }


        // protected override TransitResult TransitChild(TransitionNode childNode, ValueTransitContext ctx)
        // {
        //     ctx.Source = _currentSourceObject;
        //     //reset cached source key because different nesetd transitions 
        //     //can use different source key evaluation logic
        //     //ctx.Source.Key = String.Empty;
        //
        //     var result =  base.TransitChild(childNode, ctx);
        //
        //     if (result.Flow == TransitionFlow.SkipObject || result.Flow == TransitionFlow.SkipObjectSet)
        //         return result;
        //
        //     var targetObjects = new List<IValuesObject>();
        //
        //     var target = ctx.Target;
        //     if (target is IEnumerable<IValuesObject>)
        //     {
        //         targetObjects.AddRange((IEnumerable<IValuesObject>)target);
        //     }
        //     else
        //     {
        //         if (target != null)//target can be null if SkipObject activated
        //             targetObjects.Add((IValuesObject)target);
        //     }
        //     Saver.Push(targetObjects);
        //     
        //     return result;
        // }
    }
}