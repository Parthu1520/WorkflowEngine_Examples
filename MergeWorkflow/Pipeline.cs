using System.Collections.Generic;

namespace MergeWorkflow
{
    public class Pipeline 
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public List<StageInfo> Stages { get; internal set; }
    }
}
