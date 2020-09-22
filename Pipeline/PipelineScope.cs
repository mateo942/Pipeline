using System;
using System.Collections.Generic;
using System.Text;

namespace Pipeline
{
    public class PipelineScope
    {
        public string Id { get; }
        internal List<object> Objects;

        public PipelineScope(string id)
        {
            Id = id;
            Objects = new List<object>();
        }

        internal void AddObject(object @object)
        {
            Objects.Add(@object);
        }

        public IReadOnlyCollection<object> GetObjects()
        {
            return Objects.AsReadOnly();
        }
    }
}
