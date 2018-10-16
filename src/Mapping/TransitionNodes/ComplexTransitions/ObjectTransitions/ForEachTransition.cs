using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using XQ.DataMigration.Data;
using XQ.DataMigration.Enums;
using XQ.DataMigration.Mapping.Logic;
using XQ.DataMigration.Mapping.Trace;

namespace XQ.DataMigration.Mapping.TransitionNodes.ComplexTransitions.ObjectTransitions
{
    /// <summary>
    /// Transition which transit objects data from DataSet of source system to DataSet of target system
    /// </summary>
    public class ForEachTransition : ComplexTransition
    {
        #region XmlAttributes

        [XmlElement]
        public TransitionNode DataProvider { get; set; }

        [XmlAttribute]
        public override ConsoleColor Color { get; set; } = ConsoleColor.Magenta;

        #endregion

        #region Members

        private MigrationTracer Tracer => Migrator.Current.Tracer;

        private IValuesObject _currentSourceObject;

        private int _currentRowsCount;

        #endregion

        #region Methods

        public override void Initialize(TransitionNode parent)
        {
            if (!(DataProvider is IDataProvider))
                throw new Exception($"{nameof(DataProvider)} property for object {nameof(ForEachTransition)} should be filled by object of type {nameof(IDataProvider)}");

            DataProvider.Initialize(this);

            base.Initialize(parent);
        }

        public override TransitResult Transit(ValueTransitContext ctx)
        {
            var srcDataSet = (IEnumerable<IValuesObject>)DataProvider.TransitInternal(ctx).Value;

            if (srcDataSet == null)
                 return new TransitResult(null);

            _currentRowsCount = srcDataSet.Count();

            if (_currentRowsCount == 0)
                Tracer.TraceWarning("Source objects collection is empty!", this);

            var rowNumber = 0;

            
            foreach (var sourceObject in srcDataSet)
            {
                rowNumber++;

                sourceObject.SetValue("RowNumber", rowNumber);

                _currentSourceObject = sourceObject;
                
                var result = TransitChildren(ctx);

                if (result.Continuation == TransitContinuation.SkipObject)
                    continue;

                if (result.Continuation != TransitContinuation.Continue)
                {
                    TraceLine($"Breaking {nameof(ForEachTransition)}");
                    return result;
                }

                TraceLine($"Completed {(double)rowNumber / _currentRowsCount:P1} ({rowNumber} of {_currentRowsCount})");
            }

            return new TransitResult(null);
        }

        protected override TransitResult TransitChild(TransitionNode childNode, ValueTransitContext ctx)
        {
            ctx.Source = _currentSourceObject;
            //reset cached source key because different nesetd transitions 
            //can use different source key evaluation logic
            ctx.Source.Key = String.Empty;

            return base.TransitChild(childNode, ctx);
        }

        #endregion
    }
}