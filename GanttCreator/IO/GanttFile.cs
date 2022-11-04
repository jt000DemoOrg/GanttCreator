using System;
using System.Collections.Generic;
using System.Linq;
using Mechavian.GanttControls.Models;

namespace GanttCreator.IO
{
    public class GanttFile
    {
        public string AzureDevOpsUri { get; set; }
        public string User { get; set; }
        public string PersonalAccessToken { get; set; }
        public string Organization { get; set; }
        public string Project { get; set; }
        public string Team { get; set; }

        public List<GanttRange> Ranges { get; set; }

        public List<GanttFileWork> Work { get; set; }

        private IEnumerable<GanttRange> EnumerateAllRanges(IEnumerable<GanttRange> ranges)
        {
            var outputRanges = Enumerable.Empty<GanttRange>();

            foreach (var range in ranges)
            {
                outputRanges = outputRanges.Append(range);
                if (range.Children != null)
                {
                    outputRanges = outputRanges.Concat(EnumerateAllRanges(range.Children));
                }
            }

            return outputRanges;
        }

        public GanttDescriptor ToDescriptor()
        {
            var rangeById = EnumerateAllRanges(Ranges).ToDictionary(r => r.Name, r => r);
            var descriptor = new GanttDescriptor()
            {
                Ranges = Ranges?.ToArray() ?? Array.Empty<GanttRange>(),
                Work = Work?.Select(w => new GanttWork()
                {
                    Name = w.Name,
                    Progress = w.Progress,
                    Start = w.Start == null ? null : rangeById?[w.Start],
                    End = w.End == null ? null : rangeById?[w.End]
                }).ToArray() ?? Array.Empty<GanttWork>()
            };

            return descriptor;
        }

        public void FromDescriptor(GanttDescriptor descriptor)
        {
            Ranges = new List<GanttRange>(descriptor.Ranges);
            Work = descriptor.Work?.Select(w =>
            {
                return new GanttFileWork()
                {
                    Name = w.Name,
                    Progress = w.Progress,
                    Start = w.Start?.Name,
                    End = w.End?.Name
                };
            }).ToList();
        }
    }
}