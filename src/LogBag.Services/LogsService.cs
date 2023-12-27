using LogBag.Shared.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LogBag.Services
{
    public interface ILogsService
    {
        Task<string?> AddLogItem(string pocket, string itemJson, CancellationToken cancellationToken);

        Task<long> GetLogCount(string pocket, CancellationToken cancellationToken);

        Task<List<LogRowResponse>> GetLogs(string pocket, int page, int pageSize, CancellationToken cancellationToken);

        Task<LogRowResponse?> GetDetails(string pocket, string logId, CancellationToken cancellationToken);
    }

    public class LogsService(IMongoService mongoService) : ILogsService
    {
        internal const string ID_COLUMN = "_id";
        internal const string TIMESTAMP_COLUMN = "_timestamp";

        public async Task<string?> AddLogItem(string pocket, string itemJson, CancellationToken cancellationToken)
        {
            try
            {
                BsonDocument item = BsonDocument.Parse(itemJson);
                item.Add(TIMESTAMP_COLUMN, DateTime.UtcNow);
                await mongoService.GetCollection(pocket).InsertOneAsync(item, null, cancellationToken);
                return item.GetValue(ID_COLUMN).AsObjectId.ToString();
            }
            catch
            {
                return null;
            }
        }

        public async Task<long> GetLogCount(string pocket, CancellationToken cancellationToken)
        {
            var filter = Builders<BsonDocument>.Filter.Empty;
            long count = await mongoService.GetCollection(pocket).CountDocumentsAsync(filter, null, cancellationToken);
            return count;
        }

        public async Task<List<LogRowResponse>> GetLogs(string pocket, int page, int pageSize, CancellationToken cancellationToken)
        {
            var collection = mongoService.GetCollection(pocket);

            var filter = Builders<BsonDocument>.Filter.Empty;
            var sort = Builders<BsonDocument>.Sort.Descending(TIMESTAMP_COLUMN);

            var rows = await collection.FindAsync(filter, new FindOptions<BsonDocument, BsonDocument>()
            {
                Sort = sort,
                BatchSize = pageSize,
                Skip = (page - 1) * pageSize
            }, cancellationToken);

            await rows.MoveNextAsync(cancellationToken);

            List<LogRowResponse> result = [];
            foreach (var row in rows.Current)
            {
                result.Add(new LogRowResponse
                {
                    Id = row.GetValue(ID_COLUMN).AsObjectId.ToString(),
                    Timestamp = row.GetValue(TIMESTAMP_COLUMN).AsBsonDateTime.ToUniversalTime(),
                    Data = row.ToDictionary()
                });
            }

            return result;
        }

        public async Task<LogRowResponse?> GetDetails(string pocket, string logId, CancellationToken cancellationToken)
        {
            var collection = mongoService.GetCollection(pocket);

            var item = await collection
                .Find(Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(logId)))
                .FirstOrDefaultAsync(cancellationToken);

            if (item is null)
                return null;

            LogRowResponse response = new()
            {
                Id = item.GetValue(ID_COLUMN).AsObjectId.ToString(),
                Timestamp = item.GetValue(TIMESTAMP_COLUMN).AsBsonDateTime.ToUniversalTime(),
                Data = []
            };

            foreach (var prop in item)
            {
                if (prop.Name == ID_COLUMN || prop.Name == TIMESTAMP_COLUMN)
                    continue;

                ProcessBsonElement(response.Data, null, prop);
            }

            return response;
        }

        private void ProcessBsonElement(Dictionary<string, object> data, string? namePrefix, BsonElement doc)
        {
            string name = doc.Name;
            if (!string.IsNullOrEmpty(namePrefix))
                name = $"{namePrefix}.{name}";

            if (doc.Value.IsBsonDocument)
            {
                foreach (var prop in doc.Value.AsBsonDocument)
                    ProcessBsonElement(data, name, prop);
            }
            else if (doc.Value.IsBsonArray)
            {
                var array = doc.Value.AsBsonArray;
                for (int i = 0; i < array.Count; i++)
                {
                    //ProcessBsonDocument(data, $"{name}[{i}]", array[i].);
                }
            }
            else
            {
                data[name] = doc.Value.AsString;
            }
        }
    }
}
