namespace Mechavian.GanttControls.Models
{
    public class GanttDescriptor
    {
        public GanttRange[] Ranges { get; set; }
        public GanttWork[] Work { get; set; }
    }

    public class GanttRange
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class GanttWork
    {
        public string Name { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
    }
}