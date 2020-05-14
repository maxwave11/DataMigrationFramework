using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Xml.Serialization;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.Trace;
using XQ.DataMigration.Mapping.TransitionNodes.TransitUnits;
using XQ.DataMigration.Utils;
using TraceLevel = XQ.DataMigration.Enums.TraceLevel;

namespace XQ.DataMigration.Mapping.TransitionNodes
{
    /// <summary>
    /// Base class for all transition elements in Map configuration
    /// </summary>
    public abstract class TransitionNode:
    {
        public string Name { get; set; }

        public bool TraceWarnings { get; set; } = true;

        public virtual ConsoleColor TraceColor { get; set; } = ConsoleColor.White;

        public TransitionNode Parent { get; private set; }

        public virtual void Initialize(TransitionNode parent)
        {
            Parent = parent;
            Validate();
        }

        void Validate()
        {
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(this, new ValidationContext(this), results, true))
            {
                var firstError = results[0];
                throw new ValidationException(firstError, null, this);
            }
        }

        /// <summary>
        /// Main (core) transition method which wraps node transition logic by logging and next flow control
        /// Generally used by DataMigration classes to construct transition flow
        /// </summary>
        public TransitResult Transit(ValueTransitContext ctx)
        {
            ctx.CurrentNode = this;

            TraceStart(ctx);

            TransitResult result = null;
            try
            {
                result = TransitInternal(ctx);
            }
            catch (Exception ex)
            {
                Migrator.Current.Tracer.TraceError(ex.ToString(), ctx);
                throw;
            }
            
            ctx.SetCurrentValue(Name, result.Value);

            TraceEnd(ctx);

            return new TransitResult(result.Flow, ctx.TransitValue);
        }

        /// <summary>
        /// Method to override in client's code for custom transitions. Allow to use custom logic inside own transitino nodes
        /// inherited from TransitionNode class
        /// Don't use this method inside XQ.DataMigration tool - use
        /// <code>Transit</code> instead
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        protected abstract TransitResult TransitInternal(ValueTransitContext ctx);

        private void TraceStart(ValueTransitContext ctx)
        {
            Migrator.Current.Tracer.Indent();

            var tagName = GetType().Name;

            var incomingValue = ctx.TransitValue?.ToString();
            var incomingValueType = ctx.TransitValue?.GetType().Name;

            var traceMsg = $"-> {tagName} { this.ToString() }";
            TraceLine(traceMsg, ctx);

            TraceLine($"   Input: ({incomingValueType.Truncate(30)}){incomingValue}",ctx);
        }

        private void TraceEnd(ValueTransitContext ctx)
        {
            var returnValue = ctx.TransitValue?.ToString();
            var returnValueType = ctx.TransitValue?.GetType().Name;

            var traceMsg = $"   Output: ({returnValueType.Truncate(30)}){returnValue}\n";
            TraceLine(traceMsg,ctx);

            Migrator.Current.Tracer.IndentBack();
        }

        protected virtual void TraceLine(string message, ValueTransitContext ctx)
        {
            Migrator.Current.Tracer.TraceLine(message, this.TraceColor, ctx);
        }

        public bool HasParent(TransitionNode node)
        {
            if (Parent == node)
                return true;

            return Parent?.HasParent(node) ?? false;
        }

        public bool HasParentOfType<T>()
        {
            if (Parent is T)
                return true;

            return Parent?.HasParentOfType<T>() ?? false;
        }

        public static implicit operator TransitionNode(string expression)
        {
            return new ReadTransitUnit() { Expression = expression };
        }

        public override string ToString()
        {
            return $"Type={GetType().Name}";
        }
    }
}