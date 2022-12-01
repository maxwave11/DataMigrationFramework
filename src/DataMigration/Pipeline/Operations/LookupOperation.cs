using System;
using System.Linq;
using DataMigration.Data;
using DataMigration.Data.Interfaces;
using DataMigration.Enums;
using DataMigration.Utils;

namespace DataMigration.Pipeline.Operations;

/// <summary>
/// Operation allows to find a specific object/value from some data source (defined by Source property). 
/// For example, find an asset and get its name by asset ID or find a city id by city name or 
/// by any other condition. Lookup condition defined by <c>LookupKeyExpr</c> or 
/// <c>LookupAlternativeExpr</c> migration expression.
/// </summary>
public class LookupOperation<TContext, TEntity, TOutput>: IOperation<TContext,TOutput> 
    where TEntity : IDataObject
    where TContext: IPipeContext
{
    public IPipeContext Context { get; }
    public IOperation NextOperation { get; set; }
    
    /// <summary>
    /// DataSet id in which current transition will try to find a particular object by Key or by LookupPredicate
    /// </summary>
    public ICachedDataSource<TEntity> Source { get; }

    public bool TraceNotFound { get; } = true;
    
    public LookupMode Mode { get; }
    
    /// <summary>
    /// Specifies migration behavior in case when lookup object wasn't found
    /// </summary>
    public PipelineFlowControl OnNotFound { get; set; } = PipelineFlowControl.Stop;
        
    /// <summary>
    /// Searches item by this expression instead of object Key (slow)
    /// </summary>
    // public MigrationExpression<bool> LookupPredicate { get; set; }
    public Predicate<TOutput> LookupPredicate { get; set; }

    public bool CreateIfNotFound { get; }

    public LookupOperation(TContext context, ICachedDataSource<TEntity> dataSource, LookupMode lookupMode,
        Predicate<TOutput> lookupPredicate, bool createIfNotFound = false)
    {
        Source = dataSource;
        Mode = lookupMode;
        LookupPredicate = lookupPredicate;
        CreateIfNotFound = createIfNotFound;
        Context = context;
    }
    
    public object Execute()
    {
        var keyToFind = Context.GetValue()?.ToString();

        if (keyToFind.IsEmpty()) 
            return null;

        object lookupObject = FindLookupObjectsByKey(keyToFind);

        if (lookupObject == null)
        {
            if (TraceNotFound)
                Migrator.Current.Tracer.TraceLine($"Lookup ({Source}) object not found by key '{keyToFind}'\n", 
                    level: TraceMode.Pipes);
                
            if (CreateIfNotFound)
            {
                Migrator.Current.Tracer.TraceLine($"Creating new lookup object with key '{keyToFind}'\n", 
                    level: TraceMode.Pipes);
                var newLookupObject = Source.GetNewObject(keyToFind);
                return newLookupObject;
            }

            if (OnNotFound == PipelineFlowControl.Stop)
                Context.Message = "Required lookup object not found!";

            Context.FlowControl = OnNotFound;
        }

        return (TOutput)lookupObject;
    }

    private object FindLookupObjectsByKey(string key)
    {
        // if (LookupPredicate != null)
        // {
        //     var foundObject = Source
        //         .GetCachedData()
        //         .SingleOrDefault(i => LookupPredicate.Evaluate(new ValueTransitContext(i, ctx.TransitValue)));
        //
        //     return foundObject;
        // }

        // Ugly workaround, need to fix

        var foundObjects = Source.GetObjectsByKey(key);
        switch (Mode)
        {
            case LookupMode.Single:
            {
                if (foundObjects.Count() > 1)
                    throw new InvalidOperationException(
                        $"LOOKUP found multiple objects by key {key}, but expected single one");
                
                return foundObjects.SingleOrDefault();
            }
            case LookupMode.First:
                return foundObjects.FirstOrDefault();
            case LookupMode.All:
                return foundObjects.Any() ? foundObjects : null;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override string ToString()
    {
        return "LOOKUP";
    }
}

