using System;
using System.Collections.Generic;
using System.Text;

namespace Pipeline
{
    public static class PipelineVariableExtensions
    {
        public static DateTime GetStartDate(this Variables source)
        {
            return source.Get<DateTime>(PipelineRunner.DATE_START);
        }
    }
}
