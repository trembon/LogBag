using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogBag.Services.Models
{
    internal class BaseMongoDocument
    {
        public object? Id { get; set; }
    }
}
