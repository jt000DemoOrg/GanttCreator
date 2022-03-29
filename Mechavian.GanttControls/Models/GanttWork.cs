using System;
using Mechavian.GanttControls.Json;
using Newtonsoft.Json;

namespace Mechavian.GanttControls.Models
{
    public class GanttWork
    {
        [JsonIgnore]
        public Guid Id { get; } = Guid.NewGuid();
        
        public string Name { get; set; }
        public GanttRange Start { get; set; }
        public GanttRange End { get; set; }

        [JsonConverter(typeof(PercentageConverter))]
        public double Progress { get; set; }
    }
}