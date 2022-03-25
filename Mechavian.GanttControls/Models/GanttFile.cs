using System;
using Mechavian.GanttControls.Json;
using Newtonsoft.Json;

namespace Mechavian.GanttControls.Models
{
    public class GanttDescriptor
    {
        [JsonIgnore]
        public Guid Id { get; } = Guid.NewGuid();

        public GanttRange[] Ranges { get; set; }
        public GanttWork[] Work { get; set; }
    }

    public class GanttRange
    {
        [JsonIgnore]
        public Guid Id { get; } = Guid.NewGuid();

        public string Name { get; set; }

        public GanttRange[] Children { get; set; }
    }

    public class GanttWork
    {
        [JsonIgnore]
        public Guid Id { get; } = Guid.NewGuid();
        
        public string Name { get; set; }
        public int Start { get; set; }
        public int End { get; set; }

        [JsonConverter(typeof(PercentageConverter))]
        public double Progress { get; set; }
    }
}