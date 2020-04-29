using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using XQ.DataMigration.Data;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.Trace;

namespace XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions
{
    /// <summary>
    /// Transition which transit objects data from DataSet of source system to DataSet of target system
    /// </summary>
    public class TransitDataCommand : ComplexTransition
    {
        public IDataSource Source { get; set; }
        public ITargetProvider Target { get; set; }

        public TargetObjectsSaver Saver { get; set; }

        #region Members


        private MigrationTracer Tracer => Migrator.Current.Tracer;

        private IValuesObject _currentSourceObject;

        #endregion

        #region Methods

        public override void Initialize(TransitionNode parent)
        {
            Color = ConsoleColor.Magenta;

            //if (string.IsNullOrEmpty(QueryToSource))
            //    throw new Exception($"{nameof(QueryToSource)} can't be empty in {nameof(TransitDataCommand)}");
            if (Saver == null)
                throw new ArgumentNullException();
            base.Initialize(parent);
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
                
                
        //        var dataProvider = Migrator.Current.MapConfig.GetDefaultDataProvider();

        //        //if (SourceProviderName.IsNotEmpty())
        //        //    dataProvider = Migrator.Current.MapConfig.GetDataProvider(SourceProviderName);

        //        return dataProvider.GetDataSet(queryToSource);
        //    }
        //    catch (Exception ex)
        //    {
        //        Tracer.TraceError("Error while trying to get source datat set." + ex, this, null);
        //        return null;
        //    }
        //}

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            var srcDataSet = Source.GetData();

            int completedObjects = 0;
            var totalObjects = srcDataSet.Count();

            foreach (var sourceObject in srcDataSet)
            {
                _currentSourceObject = sourceObject;

                ctx.Target = Target.GetObjectByKey(sourceObject.Key);;
                
                var result = TransitChildren(ctx);

                if (result.Continuation == TransitContinuation.SkipObject)
                    continue;

                if (result.Continuation == TransitContinuation.SkipObjectSet)
                    return new TransitResult(ctx.TransitValue);

                if (result.Continuation != TransitContinuation.Continue)
                {
                    TraceLine($"Breaking {nameof(TransitDataCommand)}");
                    return result;
                }
                completedObjects++;

                TraceLine($"Completed {completedObjects / totalObjects:P1} ({completedObjects} of {totalObjects})");
            }

           // Saver.Save();

            return new TransitResult(null);
        }

        protected override TransitResult TransitChild(TransitionNode childNode, ValueTransitContext ctx)
        {
            ctx.Source = _currentSourceObject;
            //reset cached source key because different nesetd transitions 
            //can use different source key evaluation logic
            //ctx.Source.Key = String.Empty;

            var result =  base.TransitChild(childNode, ctx);

            if (result.Continuation == TransitContinuation.SkipObject || result.Continuation == TransitContinuation.SkipObjectSet)
                return result;

            var targetObjects = new List<IValuesObject>();

            var target = ctx.Target;
            if (target is IEnumerable<IValuesObject>)
            {
                targetObjects.AddRange((IEnumerable<IValuesObject>)target);
            }
            else
            {
                if (target != null)//target can be null if SkipObject activated
                    targetObjects.Add((IValuesObject)target);
            }
            Saver.Push(targetObjects);
            
            return result;
        }

        #endregion
    }
}