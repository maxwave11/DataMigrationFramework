using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DataMigration.Data;
using DataMigration.Data.DataSources;
using DataMigration.Enums;
using DataMigration.Pipeline.Expressions;
using DataMigration.Utils;

namespace DataMigration.Pipeline.Commands
{
    public enum LookupMode
    {
        //Find single object by key from data source
        Single, 
        //Find first object by key from data source (when data source have have multiple objects with same search key)
        First,
        //Use this mode to find all objects by search key
        All
    }
    
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

        public LookupMode Mode { get; set; }
        
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
            object lookupObject = null;

            var valueToFind = ctx.TransitValue?.ToString();

            if (valueToFind.IsNotEmpty())
            {
                lookupObject = FindLookupObject(valueToFind, ctx);

                if (lookupObject == null)
                {
                    if (TraceNotFound)
                    {
                        string message = $"Lookup ({Source}) object not found by value '{valueToFind}'\nSource row: { ctx.Source.RowNumber}, Source key: {ctx.Source.Key}";
                        Migrator.Current.Tracer.TraceEvent(MigrationEvent.LookupFailed, ctx, message);
                    }

                    ctx.Execute(OnNotFound);
                }
            }
            
            ctx.SetCurrentValue(lookupObject);
        }
        private object FindLookupObject(string searchValue, ValueTransitContext ctx)
        {
            if (LookupPredicate != null)
            {
                var foundObject = Source
                    .GetCachedData()
                    .SingleOrDefault(i => LookupPredicate.Evaluate(new ValueTransitContext(i, ctx.TransitValue)));

                return foundObject;
            }

            switch (Mode)
            {
                case LookupMode.Single:
                    return Source.GetObjectsByKey(searchValue).SingleOrDefault();
                case LookupMode.First:
                    return Source.GetObjectsByKey(searchValue).FirstOrDefault();
                case LookupMode.All:
                    var result =  Source.GetObjectsByKey(searchValue);
                    return result.Any() ? result : null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string GetParametersInfo() => $"Source: {Source}";
    }
}