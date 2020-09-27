using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Pipeline
{
    internal class PipelineException : Exception
    {
        public string StepId { get; }
        public string StepType { get; }

        public PipelineException(string message, string stepId, string stepType, Exception innerException) : base(message, innerException)
        {
            StepId = stepId;
            StepType = stepType;
        }
    }

    internal class AggregatePipelineException : AggregateException
    {
        public AggregatePipelineException(IEnumerable<Exception> innerExceptions) : base(innerExceptions)
        {
        }
    }
}
