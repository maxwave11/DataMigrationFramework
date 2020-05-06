using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using XQ.DataMigration.Data;
using XQ.DataMigration.Enums;
using XQ.DataMigration.MapConfiguration;
using XQ.DataMigration.Mapping.Expressions;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions;
using XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ValueTransitions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes.TransitUnits
{
    /// <summary>
    /// Transition unit which allows to find a specific value from some reference data set. For example, find an asset and get its name 
    /// by asset's id or find a city id by city name or by any other condition. Lookup condition determined by <c>LookupKeyExpr</c> or 
    /// <c>LookupAlternativeExpr</c> migration expression.
    /// </summary>
    internal class LookupValueTransitUnit: TransitUnit
    {
        
        /// <summary>
        /// DataSet id in which current transition will try to find a particular object by expression determined in 
        /// <c>LookupKeyExpr</c> or <c>LookupAlternativeExpr</c>
        /// </summary>
        //public string LookupDataSetId { get; set; }

        public IDataSource Source { get; set; }

        /// <summary>
        /// Migration expression which determines how to evaluate key for each lookup object from <c>LookupDataSetId</c>. 
        /// This key will allow to find a specific object from lookup's DataSet. In general you should to find objects by this way 
        /// because it's very fast. 
        /// WARNING: Use this attribute carefully because all fetched objects from DataSet (if they wasn't fetched early by some another
        /// transition) will be hosted in local cache by this key.
        /// NOTE: If you want to find an object by another way (not by its key) then you should to use <c>LookupAlternativeExpr</c> attribue
        /// </summary>
        //public MigrationExpression LookupKeyExpr { get; set; }

        /// <summary>
        /// By using this attribute lookup logic will try to find a specific object by alternative expression (not by <c>LookupKeyExpr</c>). 
        /// But anyway <c>LookupKeyExpr</c> is required for correct objects caching.
        /// WARNING: Searching an object by this way is pretty slow! Use this attribute only if you have't data to find object by its key
        /// </summary>
        //public string LookupAlternativeExpr { get; set; }

        //Set this poperty to true to allow search in data sets where multiple objects can have same search lookup expression
        //NOTE: used only when LookupAlternativeExpr is used
        public bool FindFirstOccurence { get; set; }

        /// <summary>
        /// Specifies which DataProvider lookup logic will use to search particular objects
        /// Default Target provder by default
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Query to limit amout of objects for fetching
        /// </summary>
        public string QueryToTarget { get; set; }

        /// <summary>
        /// Specifies migration behavior in case when lookup object wasn't found
        /// </summary>
        public TransitionFlow OnNotFound { get; set; } = TransitionFlow.Stop;

        // private readonly ObjectTransition _currentObjectTransition;

        public override void Initialize(TransitionNode parent)
        {
            if (Expression == null)
                Expression = "{ VALUE }";


            if (Source == null)
                throw new Exception($"{ nameof(Source)}  is required");

            //if (LookupKeyExpr == null && LookupAlternativeExpr.IsEmpty())
            //    throw new Exception($"Field {nameof(LookupKeyExpr)} or {nameof(LookupAlternativeExpr)}  should be filled to search lookup object");

            // if (!LookupKeyExpr.Contains("{"))
            //     LookupKeyExpr = $"{{ VALUE[{ LookupKeyExpr }] }}";

            base.Initialize(parent);
        }

        protected  override TransitResult TransitInternal(ValueTransitContext ctx)
        {
            IValuesObject lookupObject = null;
            string message = "";
            var continuation = TransitionFlow.Continue;

            var valueToFind = ctx.TransitValue?.ToString();

            if (valueToFind.IsNotEmpty())
            {
                lookupObject = FindLookupObject(valueToFind, ctx);

                if (lookupObject == null)
                {
                    Migrator.Current.Tracer.TraceWarning($"Lookup object not found by key '{valueToFind}'\n");

                    continuation = this.OnNotFound;
                    if (continuation != TransitionFlow.Continue)
                        message = "Value transition interuppted because of empty value of transition " + this.Name;
                }
            }

            return new TransitResult(continuation, lookupObject, message);
        }

        private IValuesObject FindLookupObject(string searchValue, ValueTransitContext ctx)
        {
            //var provider = ProviderName.IsEmpty()
            //                    ? mapConfig.GetTargetProvider()
            //                    : mapConfig.GetDataProvider(ProviderName);
            //IDataSource provider = null;

           // string queryToSource = ExpressionEvaluator.EvaluateString(LookupDataSetId, ctx);

            // IValuesObject lookupObject = null;
           

            //if (provider is ITargetProvider targetProvider && LookupAlternativeExpr.IsEmpty())
            //{

            //    //QueryToTarget = ExpressionEvaluator.EvaluateString(QueryToTarget, ctx);

            //    //quick serach (from cache, O(1)) by migration key
                
            //    lookupObject = Source.Get(queryToSource, searchValue, EvaluateObjectKey, QueryToTarget);
            //    //lookupObject = targetProvider.GetObjectByKey(queryToSource, searchValue, EvaluateObjectKey, QueryToTarget);
            //}
            //else
            //{

            //    var data = provider.GetData();
            //    var unifiedSearchValue = searchValue.ToUpper().Trim();

            //    var evaluateKeyMethod = LookupAlternativeExpr.IsEmpty() 
            //        ? (Func<IValuesObject, string>)EvaluateObjectKey 
            //        : EvaluateAlternativeObjectKey;

            

            //    //slow search (simple iteration, O(N))
            //    lookupObject = searchMethod(data, i => evaluateKeyMethod(i).ToUpper().Trim() == unifiedSearchValue);
            //}
            
            var allObjects = Source.GetData();
            
            var searchMethod = FindFirstOccurence 
                ? (Func<IEnumerable<IValuesObject>, Func<IValuesObject, bool>, IValuesObject >)Enumerable.FirstOrDefault
                : Enumerable.SingleOrDefault;

            var lookupObject = searchMethod(allObjects, i => IsObjectLookupKeyEquals(i, searchValue));
            return lookupObject;
        }

       
        private bool IsObjectLookupKeyEquals(IValuesObject lookupObject, string lookupValue)
        {
            return lookupObject.Key.ToUpper().Trim() == lookupValue.ToUpper().Trim();
        }

        // private string EvaluateAlternativeObjectKey(IValuesObject lookupObject)
        // {
        //     var ctx = new ValueTransitContext(lookupObject, lookupObject, lookupObject);
        //     return ExpressionEvaluator.EvaluateString(LookupAlternativeExpr, ctx);
        // }

       
    }
}