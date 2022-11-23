using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DataMigration.Data;
using DataMigration.Data.DataSources;
using DataMigration.Enums;
using DataMigration.Pipeline.Commands;
using DataMigration.Pipeline.Expressions;
using DataMigration.Utils;

namespace DataMigration.Pipeline.Pipes;


/// <summary>
/// Command allows to find a specific value from some data source(defined by Source property). 
/// For example, find an asset and get its name by asset ID or find a city id by city name or 
/// by any other condition. Lookup condition defined by <c>LookupKeyExpr</c> or 
/// <c>LookupAlternativeExpr</c> migration expression.
/// </summary>
public class LookupPipe<TInput,TOutput>: IPipe<TInput,TOutput>
{
    public LookupPipe(IPipe previousPipe, ICachedDataSource dataSource, LookupMode lookupMode, Predicate<TOutput> lookupPredicate)
    {
        PreviousPipe = previousPipe;
        Source = dataSource;
        Mode = lookupMode;
        LookupPredicate = lookupPredicate;
    }

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
    // public MigrationExpression<bool> LookupPredicate { get; set; }
    public Predicate<TOutput> LookupPredicate { get; set; }


    /// <summary>
    /// Specifies migration behavior in case when lookup object wasn't found
    /// </summary>
    public SetFlowCommand OnNotFound { get; set; } = TransitionFlow.RiseError.ToString();
    
    
    //public string GetParametersInfo() => $"Source: {Source}";
    
    public IPipe PreviousPipe { get; }
    public object Execute(object pipeValue, IDataObject source, IDataObject target)
    {
        object lookupObject = null;

        var valueToFind = pipeValue?.ToString();

        if (valueToFind.IsNotEmpty())
        {
            lookupObject = FindLookupObject(valueToFind);

            if (lookupObject == null)
            {
                if (TraceNotFound)
                {
                    string message = $"Lookup ({Source}) object not found by value '{valueToFind}'\n";
                    Migrator.Current.Tracer.TraceLine(message);
                }

                //ctx.Execute(OnNotFound);
            }
        }

        return (TOutput)lookupObject;
    }
    
    private object FindLookupObject(string searchValue)
    {
        // if (LookupPredicate != null)
        // {
        //     var foundObject = Source
        //         .GetCachedData()
        //         .SingleOrDefault(i => LookupPredicate.Evaluate(new ValueTransitContext(i, ctx.TransitValue)));
        //
        //     return foundObject;
        // }

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

}