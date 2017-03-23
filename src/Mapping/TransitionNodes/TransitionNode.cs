using System;
using System.Xml.Serialization;
using XQ.DataMigration.MapConfig;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Utils;

namespace XQ.DataMigration.Mapping.TransitionNodes
{
    /// <summary>
    /// Base class for any transition element in Map configuration
    /// </summary>
    public abstract class TransitionNode
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public bool Enabled { get; set; } = true;

        [XmlAttribute]
        public TraceMode Trace { get; set; }

        [XmlAttribute]
        public string TraceMessage { get; set; }

        [XmlAttribute]
        public ConsoleColor Color { get; set; }  = ConsoleColor.White;

        internal TraceMode ActualTrace => Trace == TraceMode.Auto ? Parent?.ActualTrace ?? Trace : Trace;

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

        protected virtual void TraceStart(ValueTransitContext ctx)
        {
            var traceMsg =
              $"> {this.ToString()}\n    Input: ({ctx.TransitValue?.GetType().Name.Truncate(30)}){ctx.TransitValue?.ToString().Truncate(40)}";
            TraceLine(traceMsg);
        }

        protected virtual void TraceEnd(ValueTransitContext ctx)
        {
            var traceMsg = $"< =({ctx.TransitValue?.GetType().Name.Truncate(30)}){ctx.TransitValue?.ToString().Truncate(40)}";
            TraceLine(traceMsg);
        }

        protected virtual void TraceLine(string message)
        {
            Migrator.Current.Tracer.TraceText(message, this);
        }

        internal TransitResult TransitInternal(ValueTransitContext ctx)
        {
            TraceStart(ctx);

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
                Migrator.Current.Tracer.TraceText(ex.ToString(), this, ConsoleColor.Yellow);
            }

            if (continuation == TransitContinuation.RaiseError)
            {
                message = $"Transition stopped, message: {message}";
                continuation = Migrator.Current.Tracer.TraceError(message, this, ctx);
            }

            ctx.SetCurrentValue(this.Name, resultValue);

            TraceEnd(ctx);

            return new TransitResult(continuation, ctx.TransitValue);
        }

        public override string ToString()
        {
            return $"({Name ?? this.GetType().Name})";
        }
    }
}