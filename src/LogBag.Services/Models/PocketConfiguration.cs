using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogBag.Services.Models
{
    internal class PocketConfiguration : BaseMongoDocument
    {
        public required string PocketName { get; set; }

        public List<string>? ConfiguredColumns { get; set; } = [];
    }
}
