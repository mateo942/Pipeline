using System;
using System.Collections.Generic;
using System.Text;

namespace Pipeline
{
    public class PipelineContext
    {
        public Variables GlobalVariables { get; private set; }
        public Variables LocalVariables { get; private set; }

        public PipelineContext()
        {
            GlobalVariables = new Variables();
        }

        public void SetLocalVariable(Variables localVariable) 
        {
            LocalVariables = localVariable;
        }

        public void ClearLocalVariable() 
        {
            LocalVariables = new Variables();
        }
    }
}
