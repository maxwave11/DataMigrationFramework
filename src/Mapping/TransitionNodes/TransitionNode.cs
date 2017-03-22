using System;
using System.Xml.Serialization;
using XQ.DataMigration.MapConfig;
using XQ.DataMigration.Mapping.Logic;

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

        internal TransitResult TransitInternal(ValueTransitContext ctx)
        {
            Migrator.Current.Tracer.TraceTransitionNodeStart(this, ctx);

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

            Migrator.Current.Tracer.TraceTransitionNodeEnd(this, ctx);

            return new TransitResult(continuation, ctx.TransitValue);
        }

        public override string ToString()
        {
            return $"({Name ?? this.GetType().Name})";
        }
    }
}