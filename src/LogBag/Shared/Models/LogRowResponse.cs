using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogBag.Shared.Models
{
    public class LogRowResponse
    {
        public required string Id { get; set; }

        public DateTime Timestamp { get; set; }

        public required Dictionary<string, object> Data { get; set; }
    }
}
