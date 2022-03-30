using System;
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
}