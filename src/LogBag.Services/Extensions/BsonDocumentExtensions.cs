using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogBag.Services.Extensions
{
    internal static class BsonDocumentExtensions
    {
        public static Dictionary<string, object> ToFlattenedDictionary(this BsonDocument? document, string[] ignoreProperties = null)
        {
            if (document is null)
                return [];

            Dictionary<string, object> result = [];
            foreach (var prop in document)
            {
                if (ignoreProperties != null && ignoreProperties.Contains(prop.Name))
                    continue;

                ProcessBsonElement(result, prop, null);
            }
            return result;
        }

        private static void ProcessBsonElement(Dictionary<string, object> data, BsonElement element, string? namePrefix)
        {
            string name = element.Name;
            if(!string.IsNullOrEmpty(namePrefix))
                name = $"{namePrefix}.{name}";

            if (element.Value.IsBsonDocument)
            {
                foreach (var prop in element.Value.AsBsonDocument)
                    ProcessBsonElement(data, prop, name);
            }
            else if (element.Value.IsBsonArray)
            {
                var array = element.Value.AsBsonArray;
                for (int i = 0; i < array.Count; i++)
                    ProcessBsonElement(data, new BsonElement($"{name}[{i}]", array[i]), null);
            }
            else
            {
                data[name] = BsonTypeMapper.MapToDotNetValue(element.Value);
            }
        }
    }
}
