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
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public bool Enabled { get; set; } = true;

        [XmlAttribute]
        public TraceLevel TraceLevel { get; set; }

        [XmlAttribute]
        public bool TraceWarnings { get; set; } = true;

        [XmlAttribute]
        public virtual ConsoleColor Color { get; set; } = ConsoleColor.White;

        /// <summary>
        /// Mark element by this attribute to fast debug particular TransitionNode
        /// </summary>
        [XmlAttribute]
        public bool Break { get; set; }

        internal TraceLevel ActualTrace => TraceLevel == TraceLevel.Auto ? Parent?.ActualTrace ?? TraceLevel : TraceLevel;

        //protected Expressions.ExpressionEvaluator ExpressionEvaluator { get; } = new Expressions.ExpressionEvaluator();

        [XmlIgnore]
        public TransitionNode Parent { get; private set; }

        /// <summary>
        /// Specify what to do if some error occured while current transition processing
        /// </summary>
        [XmlAttribute]
        public TransitContinuation OnError { get; set; } = TransitContinuation.RaiseError;

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
        internal TransitResult TransitCore(ValueTransitContext ctx)
        {
            if (ctx == null)
                throw new ArgumentNullException(nameof(ctx));

            TraceStart(ctx);

            if (Break)
                Debugger.Break();

            object resultValue = null;
            TransitContinuation continuation;
            string message = "";
            try
            {
                var result = Transit(ctx);
                resultValue = result.Value;
                continuation = result.Continuation;
                message = result.Message;
            }
            catch (Exception ex)
            {
                continuation = OnError;
                Migrator.Current.Tracer.TraceWarning(ex.ToString(), this);
            }

            if (continuation == TransitContinuation.RaiseError)
            {
                message = $"Transition stopped, message: {message}";
                continuation = Migrator.Current.Tracer.TraceError(message, this, ctx);
            }

            ctx.SetCurrentValue(Name, resultValue);

            TraceEnd(ctx);

            return new TransitResult(continuation, ctx.TransitValue, message);
        }

        /// <summary>
        /// Method to override in client's code for custom transitions. Allow to use custom logic inside own transitino nodes
        /// inherited from TransitionNode class
        /// Don't use this method inside XQ.DataMigration tool - use
        /// <code>TransitCore</code> instead
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public abstract TransitResult Transit(ValueTransitContext ctx);

        protected virtual void TraceStart(ValueTransitContext ctx, string attributes = "")
        {
            var tagName = GetType().Name;


            if (!string.IsNullOrEmpty(attributes))
                attributes = " " + attributes;

            var incomingValue = ctx.TransitValue?.ToString();
            var incomingValueType = ctx.TransitValue?.GetType().Name;

            var traceMsg = $"-> {tagName}{attributes}";
            TraceLine(traceMsg);

            TraceLine($"{MigrationTracer.IndentUnit} Input: ({incomingValueType.Truncate(30)}){incomingValue}");
        }

        protected virtual void TraceEnd(ValueTransitContext ctx)
        {
            var tagName = GetType().Name;
            var returnValue = ctx.TransitValue?.ToString();
            var returnValueType = ctx.TransitValue?.GetType().Name;

            var traceMsg = $"{MigrationTracer.IndentUnit} Output: ({returnValueType.Truncate(30)}){returnValue}";
            TraceLine(traceMsg);

            traceMsg = $"<- {tagName}";
            TraceLine(traceMsg);
        }

        protected virtual void TraceLine(string message)
        {
            Migrator.Current.Tracer.TraceLine(message, this);
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

        public virtual bool CanTransit(ValueTransitContext ctx)
        {
            return Enabled;
        }

        public override string ToString()
        {
            return $"Type={GetType().Name} Name={Name}";
        }
    }
}