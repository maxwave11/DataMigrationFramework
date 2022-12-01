using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DataMigration.Enums;
using DataMigration.Pipeline.Operations;
using DataMigration.Trace;
using DataMigration.Utils;

namespace DataMigration.Pipeline;

public class Pipe<TSource, TTarget>: IPipe
{
    private readonly IOperation _operation;
    private readonly IMigrationTracer _tracer;

    public Pipe(IOperation operation, IMigrationTracer tracer)
    {
        _operation = operation;
        _tracer = tracer;
    }

    public void Execute(ValueTransitContext ctx)
    {
        var allOperations = GetAllOperations();
        
        foreach (var operation in allOperations)
        {
            var isFirstOperation = allOperations.First() == operation;
            
            if (!isFirstOperation)
                _tracer.Indent();
            
            _tracer.TraceLine($"{ operation }", ctx, level:TraceMode.Pipes, ConsoleColor.Green);
            _tracer.Indent();
            operation.Context.Set(ctx.TransitValue, ctx.Source, ctx.Target);
            var newValue = operation.Execute();
            ctx.FlowControl = operation.Context.FlowControl;
            ctx.SetCurrentValue(newValue);
            _tracer.IndentBack();
            
            TraceValueInfo(newValue, ctx);
            
            if (!isFirstOperation)
                _tracer.IndentBack();
            
            if (ctx.FlowControl == PipelineFlowControl.Debug)
                Debugger.Break();
            
            if (ctx.FlowControl == PipelineFlowControl.Stop)
                throw new DataMigrationException("Migration stopped " + operation.Context.Message, ctx, null);
            
            if (ctx.FlowControl == PipelineFlowControl.SkipObject)
            {
                _tracer.TraceLine($"FLOW: {ctx.FlowControl}, Message: {operation.Context.Message}", ctx, level:TraceMode.Object);
                break;
            }

            if (ctx.FlowControl != PipelineFlowControl.Continue)
            {
                _tracer.TraceLine($"FLOW: {ctx.FlowControl}, Message: {operation.Context.Message}", ctx, level:TraceMode.Pipes);
                break;
            }
        }
    }

    private IReadOnlyCollection<IOperation> GetAllOperations()
    {
        var pipesSequence = new List<IOperation>();
        var nextOperation = _operation;
             
        do
        {
            pipesSequence.Add(nextOperation);
            nextOperation = nextOperation.NextOperation;
        } while (nextOperation != null);

        return pipesSequence;
    }
    
    private void TraceValueInfo(object value, ValueTransitContext ctx)
    {
        var builder = new StringBuilder();
        var valueType = value?.GetType().Name.Truncate(30);

        builder.Append($" => ({valueType}){value?.ToString().Truncate(240) ?? "null"}");
        
        if (value is not string && value is IEnumerable enumerable )
        {
            builder.Append("\n ITEMS: [");

            foreach (var item in enumerable)
            {
                builder.Append(" - ");
                builder.Append(item);
            }
            
            builder.Append("]");
        }
        
        _tracer.TraceLine(builder.ToString(), ctx, level:TraceMode.Pipes, ConsoleColor.DarkGray);
    }
}