using System;
using System.Collections.Generic;

namespace Pipeline
{
    public class PipelineContext
    {
        public Guid Id { get; set; }

        public Variables GlobalVariables { get; private set; }
        public Variables LocalVariables { get; private set; }

        public PipelineScope Scope { get; internal set; }

        internal readonly IDictionary<string, PipelineScope> Scopes;

        public PipelineContext()
        {
            GlobalVariables = new Variables();
            Id = Guid.NewGuid();
            Scopes = new Dictionary<string, PipelineScope>();
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

        #region Scopes
        public void AddObjectToScope(string name, object value)
        {
            if(!Scopes.TryGetValue(name, out PipelineScope scope))
            {
                scope = new PipelineScope(name);
                Scopes.Add(name, scope);
            }

            scope.AddObject(value);
        }

        internal void SetCurrentScope(string name)
        {
            if (!Scopes.TryGetValue(name, out PipelineScope scope))
            {
                scope = new PipelineScope(name);
                Scopes.Add(name, scope);
            }

            Scope = scope;
        }
        #endregion
    }
}
