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
    internal class LookupValueTransitUnit: TransitionNode
    {

        /// <summary>
        /// DataSet id in which current transition will try to find a particular object by Key or by LookupPredicate
        /// </summary>
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

 

        //Set this poperty to true to allow search in data sets where multiple objects can have same search lookup expression
        //NOTE: used only when LookupAlternativeExpr is used
        public bool FindFirstOccurence { get; set; }


        /// <summary>
        /// Searches item by this expression instead of object Key (slow)
        /// </summary>
        public MigrationExpression<bool> LookupPredicate { get; set; }

        /// <summary>
        /// Specifies migration behavior in case when lookup object wasn't found
        /// </summary>
        public TransitionFlow OnNotFound { get; set; } = TransitionFlow.Stop;

        public override void Initialize(TransitionNode parent)
        {
            if (Source == null)
                throw new Exception($"{ nameof(Source)}  is required");

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
                    message = $"Lookup ({ Source }) object not found by value '{valueToFind}'\n";
                    continuation = this.OnNotFound;
                }
            }

            return new TransitResult(continuation, lookupObject, message);
        }
        private IValuesObject FindLookupObject(string searchValue, ValueTransitContext ctx)
        {
            if (LookupPredicate != null)
            {
                var foundObject = Source
                    .GetData()
                    .Where(i => LookupPredicate.Evaluate(new ValueTransitContext(i, null, ctx.TransitValue)))
                    .SingleOrDefault();

                return foundObject;
            }

            var foundObects = Source.GetObjectsByKey(searchValue);
            
            return FindFirstOccurence ? foundObects?.FirstOrDefault() : foundObects?.SingleOrDefault();
        }
    }
}