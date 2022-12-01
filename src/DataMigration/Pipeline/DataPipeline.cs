using System;
using System.Collections.Generic;
using System.Linq;
using DataMigration.Data;
using DataMigration.Data.Interfaces;
using DataMigration.Enums;
using DataMigration.Pipeline.Operations;
using DataMigration.Trace;
using DataMigration.Utils;

namespace DataMigration.Pipeline;

/// <summary>
/// Transition which transit data from DataSet of source system to DataSet of target system
/// </summary>
public class DataPipeline<TSource, TTarget> : IDataPipeline
    where TSource : IDataObject
    where TTarget : IDataObject
{
    public bool Enabled { get; set; } = true;
    public string Name { get; set; }

    public IDataSource<TSource> Source { get; set; }
    public IDataTarget<TTarget> Target { get; set; }

    public ObjectTransitMode TransitMode { get; set; }

    public int SaveCount { get; set; } = 50;

    private TargetObjectsSaver<TTarget> Saver { get; set; }

    private List<IPipe> _pipes { get; } = new List<IPipe>();

    private IMigrationTracer _tracer;

    public DataPipeline(IMigrationTracer tracer)
    {
        _tracer = tracer;
    }

    public void Initialize(IMigrationTracer tracer)
    {
        if (Enabled == false)
            return;

        if (Source == null)
            throw new InvalidOperationException($"{nameof(Source)} must be set");

        Saver ??= new TargetObjectsSaver<TTarget>(tracer)
        {
            SaveCount = SaveCount,
            TargetSource = Target
        };
    }

    public void Run()
    {
        if (!Enabled)
            return;

        TraceLine($"\nPIPELINE '{Name}' started ", null, level: TraceMode.Pipeline, ConsoleColor.Magenta);
        _tracer.Indent();

        var srcDataSet = GetSourceDataObjects();
        uint rowCounter = 0;
        foreach (var sourceObject in srcDataSet)
        {
            if (sourceObject == null)
                continue;

            TraceLine(
                $"PIPELINE '{Name}' SOURCE OBJECT: Row {rowCounter++}, Key [{sourceObject.Key}]",
                null,
                level: TraceMode.Object,
                ConsoleColor.Magenta);

            var target = TransitSourceObject(sourceObject);

            if (target == null)
                continue;

            Saver.Push((TTarget)target);
        }

        Saver.TrySave();

        _tracer.IndentBack();

        TraceLine($"\nPIPELINE '{Name}' finished ", null, level: TraceMode.Pipeline, ConsoleColor.Magenta);
    }

    private IEnumerable<IDataObject> GetSourceDataObjects()
    {
        TraceLine($"DataSource ({Source}) - Get data...", null, level: TraceMode.Pipeline);
        var sourceDataObjects = Source.GetData();

        foreach (var dataObject in sourceDataObjects)
        {
            var key = Source.GetObjectKey(dataObject);

            if (key.IsEmpty())
                continue;

            dataObject.Key = key;
            yield return dataObject;
        }
    }

    private IDataObject TransitSourceObject(IDataObject sourceObject)
    {
        var ctx = new ValueTransitContext(sourceObject, null);

        try
        {
            SetTargetObject(ctx);
            
            if (ctx.FlowControl == PipelineFlowControl.SkipObject)
                return null;

            RunPipes(ctx);

            if (ctx.FlowControl == PipelineFlowControl.SkipObject && ctx.Target != null)
            {
                //If object just created and skipped by migration logic - need to remove it from cache
                //because it's invalid and must be removed from cache to avoid any referencing to this object
                //by any migration logic (lookups, key transitions, etc.)
                //If object is not new, it means that it's already saved and passed by migration validation
                if (ctx.Target.IsNew)
                    Target.InvalidateObject((TTarget)ctx.Target);

                TraceLine("Source object skipped", ctx, level: TraceMode.Object);

                return null;
            }
        }
        catch (Exception e)
        {
            throw new DataMigrationException("Error occured while object processing", ctx, e);
        }

        return ctx.Target;
    }

    private void RunPipes(ValueTransitContext ctx)
    {
        foreach (var pipe in _pipes)
        {
            // Every time after value transition finishes - reset current value to Source object
            ctx.ResetCurrentValue();

            TraceLine("", ctx, level: TraceMode.Pipes);
            pipe.Execute(ctx);

            if (ctx.FlowControl == PipelineFlowControl.SkipValue)
            {
                ctx.FlowControl = PipelineFlowControl.Continue;
                //Tracer.TraceEvent(MigrationEvent.ValueSkipped, ctx,"Value skipped");
                continue;
            }

            if (ctx.FlowControl != PipelineFlowControl.Continue)
                break;
        }
    }

    private void SetTargetObject(ValueTransitContext ctx)
    {
        var target = GetObjectByKeyOrCreate(ctx.Source.Key);

        // Target can be empty when using TransitMode = OnlyExitedObjects
        if (target == null)
        {
            ctx.FlowControl = PipelineFlowControl.SkipObject;
            TraceLine("Skipping object because TransitMode = OnlyExitedObjects",
                ctx,
                level: TraceMode.Object,
                ConsoleColor.Magenta);
            return;
        }

        TraceLine(
            $"\t\tTARGET OBJECT: IsNew=" + target.IsNew,
            null,
            level: TraceMode.Object,
            ConsoleColor.Magenta);

        ctx.Target = target;
    }

    public IDataObject GetObjectByKeyOrCreate(string key)
    {
        var targetObject = Target.GetObjectsByKey(key).SingleOrDefault();

        switch (TransitMode)
        {
            case ObjectTransitMode.OnlyExistedObjects:
                return targetObject;
            case ObjectTransitMode.OnlyNewObjects when targetObject != null:
                return null;
        }

        if (targetObject != null)
            return targetObject;

        targetObject = Target.GetNewObject(key);
        return targetObject;
    }

    private void TraceLine(string message, ValueTransitContext ctx, TraceMode level,
        ConsoleColor color = ConsoleColor.White)
    {
        _tracer.TraceLine(message, ctx, level, color);
    }

    public IOperation<PipeContext<TSource, TTarget, object>, object> START(string pipeName)
    {
        var context = new PipeContext<TSource, TTarget, object>(null);
        var operation = new GetOperation<PipeContext<TSource, TTarget, object>, object>(_ => null, context);

        var pipe = new Pipe<TSource, TTarget>(operation, _tracer);
        _pipes.Add(pipe);

        return operation;
    }
}