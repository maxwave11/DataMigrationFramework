using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using XQ.DataMigration.Data;
using XQ.DataMigration.Data.DataSources;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Pipeline.Expressions;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Pipeline.Commands
{
    /// <summary>
    /// Transition unit which allows to find a specific value from some reference data set. For example, find an asset and get its name 
    /// by asset's id or find a city id by city name or by any other condition. Lookup condition determined by <c>LookupKeyExpr</c> or 
    /// <c>LookupAlternativeExpr</c> migration expression.
    /// </summary>
    
    [Command("LOOKUP")]
    internal class LookupCommand: CommandBase
    {
        /// <summary>
        /// DataSet id in which current transition will try to find a particular object by Key or by LookupPredicate
        /// </summary>
        [Required]
        public ICachedDataSource Source { get; set; }

        public bool TraceNotFound { get; set; } = true;


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
        public SetFlowCommand OnNotFound { get; set; } = TransitionFlow.RiseError.ToString();

        public override void ExecuteInternal(ValueTransitContext ctx)
        {
            IDataObject lookupObject = null;

            var valueToFind = ctx.TransitValue?.ToString();

            if (valueToFind.IsNotEmpty())
            {
                lookupObject = FindLookupObject(valueToFind, ctx);

                if (lookupObject == null)
                {
                    if (TraceNotFound)
                    {
                        string message = $"Lookup ({Source}) object not found by value '{valueToFind}'";
                        Migrator.Current.Tracer.TraceEvent(MigrationEvent.LookupFailed, ctx, message);
                    }

                    ctx.Execute(OnNotFound);
                }
            }
            
            ctx.SetCurrentValue(lookupObject);
        }
        private IDataObject FindLookupObject(string searchValue, ValueTransitContext ctx)
        {
            if (LookupPredicate != null)
            {
                var foundObject = Source
                    .GetCachedData()
                    .SingleOrDefault(i => LookupPredicate.Evaluate(new ValueTransitContext(i, ctx.TransitValue)));

                return foundObject;
            }

            var foundObects = Source.GetObjectsByKey(searchValue);
            
            return FindFirstOccurence ? foundObects?.FirstOrDefault() : foundObects?.SingleOrDefault();
        }

        public override string GetParametersInfo() => $"Source: {Source}";
    }
}