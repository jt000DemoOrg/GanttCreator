using System;

namespace GanttCreator.AdoModels
{
    public class TeamIteration
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public TeamIterationAttributes Attributes { get; set; }
        public Uri Url { get; set; }
    }
}