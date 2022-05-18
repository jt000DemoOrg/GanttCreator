using Newtonsoft.Json;

namespace GanttCreator.AdoModels
{
    public class WorkItemFields
    {
        [JsonProperty("System.Id")]
        public int Id { get; set; }

        [JsonProperty("System.Title")]
        public string Title { get; set; }

        [JsonProperty("System.BoardColumn")]
        public string BoardColumn { get; set; }

        [JsonProperty("System.State")]
        public string State { get; set; }

    }
}