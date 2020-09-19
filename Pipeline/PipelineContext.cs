using System;
using System.Collections.Generic;
using System.Text;

namespace Pipeline
{
    public class PipelineContext
    {
        public Guid Id { get; set; }

        public Variables GlobalVariables { get; private set; }
        public Variables LocalVariables { get; private set; }

        public PipelineContext()
        {
            GlobalVariables = new Variables();
            Id = Guid.NewGuid();
        }

        internal void SetLocalVariable(Variables localVariable) 
        {
            LocalVariables = localVariable;
        }

        internal void ClearLocalVariable() 
        {
            LocalVariables = new Variables();
        }

        /// <summary>
        /// Try get variable from local variables. If not found try get from global variables.
        /// </summary>
        /// <typeparam name="T">Type of variable</typeparam>
        /// <param name="key">Variable key</param>
        /// <returns>Value of variable</returns>
        public T GetVariable<T>(string key)
        {
            if(LocalVariables.TryGet<T>(key, out T @lv))
            {
                return lv;
            }

            if (GlobalVariables.TryGet<T>(key, out T @gv))
            {
                return gv;
            }

            throw new KeyNotFoundException($"Key: {key}");
        }
    }
}
