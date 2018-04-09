using System;
using System.CodeDom;
using System.Diagnostics;
using System.Xml.Serialization;
using XQ.DataMigration.Enums;
using XQ.DataMigration.MapConfig;
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
        public string TraceMessage { get; set; }

        [XmlAttribute]
        public ConsoleColor Color { get; set; }  = ConsoleColor.White;

        /// <summary>
        /// Mark element by this attribute to fast debug particular TransitionNode
        /// </summary>
        [XmlAttribute]
        public bool Break { get; set; }

        internal TraceLevel ActualTrace => TraceLevel == TraceLevel.Auto ? Parent?.ActualTrace ?? TraceLevel : TraceLevel;

        protected Expressions.ExpressionEvaluator ExpressionEvaluator { get; } = new Expressions.ExpressionEvaluator();

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
        }

        public abstract TransitResult Transit(ValueTransitContext ctx);

        protected virtual void TraceStart(ValueTransitContext ctx, string attributes = "")
        {
            var tagName = this.GetType().Name;
            

            if (!string.IsNullOrEmpty(attributes))
                attributes = " " + attributes;

            var incomingValue = ctx.TransitValue?.ToString();
            var incomingValueType = ctx.TransitValue?.GetType().Name;

            var traceMsg = $"<{tagName}{attributes}>";
            TraceLine(traceMsg);

            TraceLine($"{MigrationTracer.IndentUnit}<Input Value=\"({incomingValueType.Truncate(30)}){incomingValue}\"/>");

            if (TraceMessage.IsNotEmpty())
            {
                var userMessage = $"Trace Massage: '{ ExpressionEvaluator.Evaluate(TraceMessage, ctx) }'";
                TraceLine(userMessage);
            }
        }

        protected virtual void TraceEnd(ValueTransitContext ctx)
        {
            var tagName = this.GetType().Name;
            var returnValue = ctx.TransitValue?.ToString();
            var returnValueType = ctx.TransitValue?.GetType().Name;

            var traceMsg = $"{MigrationTracer.IndentUnit}<Output Value=\"({returnValueType.Truncate(30)}){returnValue}\"/>";
            TraceLine(traceMsg);

            traceMsg = $"</{tagName}>";
            TraceLine(traceMsg);
        }

        protected virtual void TraceLine(string message)
        {
            Migrator.Current.Tracer.TraceLine(message, this);
        }

        internal TransitResult TransitInternal(ValueTransitContext ctx)
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
                continuation = this.OnError;
                Migrator.Current.Tracer.TraceWarning(ex.ToString(), this);
            }

            if (continuation == TransitContinuation.RaiseError)
            {
                message = $"Transition stopped, message: {message}";
                continuation = Migrator.Current.Tracer.TraceError(message, this, ctx);
            }

            ctx.SetCurrentValue(this.Name, resultValue);

            TraceEnd(ctx);

            return new TransitResult(continuation, ctx.TransitValue, message);
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
            return this.Enabled;
        }

        public override string ToString()
        {
            return $"{Name ?? this.GetType().Name}";
        }
    }
}