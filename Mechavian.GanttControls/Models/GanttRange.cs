using System;
using Newtonsoft.Json;

namespace Mechavian.GanttControls.Models
{
    public class GanttRange
    {
        [JsonIgnore]
        public Guid Id { get; } = Guid.NewGuid();

        public string Name { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public GanttRange[] Children { get; set; }
    }
}