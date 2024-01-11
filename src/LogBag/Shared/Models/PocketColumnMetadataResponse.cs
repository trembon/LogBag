using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogBag.Shared.Models
{
    public class PocketColumnMetadataResponse
    {
        public required List<string> ConfiguredColumns { get; set; }

        public required List<string> ColumnSuggestions { get; set; }
    }
}
