using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Xml.Serialization;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.Trace;
using XQ.DataMigration.Utils;
using TraceLevel = XQ.DataMigration.Enums.TraceLevel;

namespace XQ.DataMigration.Mapping.TransitionNodes
{
    /// <summary>
    /// Base class for all transition elements in Map configuration
    /// </summary>
    public abstract class TransitionNode
    {
        public string Name { get; set; }


        public bool Trace { get; set; }

        public bool TraceWarnings { get; set; } = true;

        public virtual ConsoleColor Color { get; set; } = ConsoleColor.White;

        /// <summary>
        /// Mark element by this attribute to fast debug particular TransitionNode
        /// </summary>
        public bool Break { get; set; }

        internal bool ActualTrace => (Parent?.ActualTrace ?? false) || Trace;

        public TransitionNode Parent { get; private set; }

        /// <summary>
        /// Specify what to do if some error occured while current transition processing
        /// </summary>
        public TransitionFlow OnError { get; set; } = TransitionFlow.Stop;

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
            if (ctx == null)
                throw new ArgumentNullException(nameof(ctx));

            Migrator.Current.Tracer.Indent();

            TraceStart(ctx);

            if (Break)
                Debugger.Break();

            object resultValue = null;
            TransitionFlow continuation;
            string message = "";
            try
            {
                var result = TransitInternal(ctx);
                resultValue = result.Value;
                continuation = result.Flow;
                message = result.Message;
            }
            catch (Exception ex)
            {
                continuation = OnError;
                Migrator.Current.Tracer.TraceWarning(ex.ToString());
            }

            if (continuation == TransitionFlow.Stop)
            {
                message = $"Transition stopped, message: {message}";
                Migrator.Current.Tracer.TraceError(message, this, ctx);
                return new TransitResult(continuation, null);
            }

            ctx.SetCurrentValue(Name, resultValue);

            TraceEnd(ctx);

            Migrator.Current.Tracer.IndentBack();


            return new TransitResult(continuation, ctx.TransitValue, message);
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

        protected virtual void TraceStart(ValueTransitContext ctx, string attributes = "")
        {
            var tagName = GetType().Name;


            if (!string.IsNullOrEmpty(attributes))
                attributes = " " + attributes;

            var incomingValue = ctx.TransitValue?.ToString();
            var incomingValueType = ctx.TransitValue?.GetType().Name;

            var traceMsg = $"-> {tagName}{attributes}";
            TraceLine(traceMsg, ctx);

            TraceLine($"   Input: ({incomingValueType.Truncate(30)}){incomingValue}",ctx);
        }

        protected virtual void TraceEnd(ValueTransitContext ctx)
        {
            var tagName = GetType().Name;
            var returnValue = ctx.TransitValue?.ToString();
            var returnValueType = ctx.TransitValue?.GetType().Name;

            var traceMsg = $"   Output: ({returnValueType.Truncate(30)}){returnValue}\n";
            TraceLine(traceMsg,ctx);
            //traceMsg = $"<- {tagName}";
            //TraceLine(traceMsg);
        }

        protected virtual void TraceLine(string message, ValueTransitContext ctx)
        {
            Migrator.Current.Tracer.TraceLine(message, this, ctx);
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

        public override string ToString()
        {
            return $"Type={GetType().Name}";
        }
    }
}