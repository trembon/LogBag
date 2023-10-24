using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogBag.Services
{
    public interface IAddLogsService
    {
        Task<string?> AddLogItem(string pocket, string itemJson, CancellationToken cancellationToken);
    }

    public class AddLogsService : IAddLogsService
    {
        private readonly IMongoService _mongoService;

        public AddLogsService(IMongoService mongoService)
        {
            _mongoService = mongoService;
        }

        public async Task<string?> AddLogItem(string pocket, string itemJson, CancellationToken cancellationToken)
        {
            try
            {
                BsonDocument item = BsonDocument.Parse(itemJson);
                item.Add("_timestamp", DateTime.UtcNow);
                await _mongoService.GetCollection(pocket).InsertOneAsync(item, null, cancellationToken);
                return item.GetValue("_id").AsObjectId.ToString();
            }
            catch
            {
                return null;
            }
        }
    }
}
