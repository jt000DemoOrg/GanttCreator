using System.Collections.Generic;

namespace GanttCreator.AdoModels
{
    public class TeamIterationCollection
    {
        public long Count { get; set; }
        public List<TeamIteration> Value { get; set; }
    }
}