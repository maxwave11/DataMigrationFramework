using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        var srcDataSet = GetSourceDataWithKeys();
        uint rowCounter = 0;
        foreach ((string key, TSource sourceObject)  in srcDataSet)
        {
            TraceLine(
                $"PIPELINE '{Name}' SOURCE OBJECT: Row {rowCounter++}, Key [{key}]",
                null,
                level: TraceMode.Object,
                ConsoleColor.Magenta);

            var target = TransitSourceObject(key, sourceObject);

            if (target == null)
                continue;

            Saver.Push(target);
        }

        Saver.TrySave();

        _tracer.IndentBack();

        TraceLine($"\nPIPELINE '{Name}' finished ", null, level: TraceMode.Pipeline, ConsoleColor.Magenta);
    }

    private IEnumerable<(string key, TSource srcObject)> GetSourceDataWithKeys()
    {
        TraceLine($"DataSource ({Source}) - Get data...", null, level: TraceMode.Pipeline);
        var sourceDataObjects = Source.GetData();

        foreach (var dataObject in sourceDataObjects)
        {
            var key = Source.GetObjectKey(dataObject);

            if (key.IsEmpty())
                continue;
            
            yield return (key, dataObject);
        }
    }

    private TTarget TransitSourceObject(string key,  TSource sourceObject)
    {
        var ctx = new ValueTransitContext(sourceObject, null);

        TTarget target;

        try
        {
            target = GetObjectByKeyOrCreate(key);

            // Target can be empty when using TransitMode = OnlyExitedObjects
            if (target == null)
            {
                TraceLine("Skipping object because TransitMode = OnlyExitedObjects",
                    ctx,
                    level: TraceMode.Object,
                    ConsoleColor.Magenta);
                
                return default;
            }

            TraceLine(
                $"\t\tTARGET OBJECT: IsNew=" + Target.IsObjectNew(key),
                null,
                level: TraceMode.Object,
                ConsoleColor.Magenta);

            ctx.Target = target;
            
            RunPipes(ctx);

            if (ctx.FlowControl == PipelineFlowControl.SkipObject && ctx.Target != null)
            {
                //If object just created and skipped by migration logic - need to remove it from cache
                //because it's invalid and must be removed from cache to avoid any referencing to this object
                //by any migration logic (like lookups)
                //If object is not new, it means that it's already saved and passed by migration validation
                if(Target.IsObjectNew(key))
                    Target.InvalidateObject(key);

                TraceLine("Source object skipped", ctx, level: TraceMode.Object);

                return default;
            }
        }
        catch (Exception e)
        {
            throw new DataMigrationException("Error occured while object processing", ctx, e);
        }

        return target;
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
    
    public TTarget GetObjectByKeyOrCreate(string key)
    {
        var targetObject = Target.GetObjectsByKey(key).SingleOrDefault();

        switch (TransitMode)
        {
            case ObjectTransitMode.OnlyExistedObjects:
                return targetObject;
            case ObjectTransitMode.OnlyNewObjects when targetObject != null:
                return default;
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