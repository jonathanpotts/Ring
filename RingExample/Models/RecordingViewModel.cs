using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RingExample.Models
{
    public class RecordingViewModel
    {
        public Uri RecordingUri { get; set; }
        public string Type { get; set; }
        public string DeviceName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
