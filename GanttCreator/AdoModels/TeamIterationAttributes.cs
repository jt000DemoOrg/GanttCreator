using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GanttCreator.AdoModels
{
    public class TeamIterationAttributes
    {
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset FinishDate { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public IterationTimeFrame TimeFrame { get; set; }
    }
}