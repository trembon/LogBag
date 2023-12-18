using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogBag.Shared.Models
{
    public class LogRowsResponse
    {
        public required List<string> Columns { get; set; }

        public required List<LogRowResponse> Rows { get; set; }
    }
}
