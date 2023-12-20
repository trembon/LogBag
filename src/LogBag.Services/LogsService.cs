using LogBag.Shared.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogBag.Services
{
    public interface ILogsService
    {
        Task<string?> AddLogItem(string pocket, string itemJson, CancellationToken cancellationToken);

        Task<long> GetLogCount(string pocket, CancellationToken cancellationToken);

        Task<List<LogRowResponse>> GetLogs(string pocket, int page, int pageSize, CancellationToken cancellationToken);
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
    }
}
