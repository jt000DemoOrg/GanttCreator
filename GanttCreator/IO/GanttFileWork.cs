using Mechavian.GanttControls.Json;
using Newtonsoft.Json;

namespace GanttCreator.IO
{
    public class GanttFileWork
    {
        public string Name { get; set; }
        public string Start { get; set; }
        public string End { get; set; }

        [JsonConverter(typeof(PercentageConverter))]
        public double Progress { get; set; }
    }
}