using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogBag.Services
{
    public interface IPocketService
    {
        Task<IEnumerable<string>> GetPocketNames();

        Task<List<string>> GetColumns(string pocket, CancellationToken cancellationToken);
    }

    public class PocketService(IMongoService mongoService) : IPocketService
    {
        public async Task<IEnumerable<string>> GetPocketNames()
        {
            return await mongoService.GetCollectionNames();
        }

        public async Task<List<string>> GetColumns(string pocket, CancellationToken cancellationToken)
        {
            var filter = Builders<BsonDocument>.Filter.Empty;
            var row = await mongoService.GetCollection(pocket).Find(filter).Limit(1).SingleAsync(cancellationToken);

            var dictionary = row.ToDictionary();

            List<string> columns = [];
            foreach(var kvp in dictionary)
            {
                if (kvp.Key == LogsService.ID_COLUMN)
                    continue;

                if (kvp.Key == LogsService.TIMESTAMP_COLUMN)
                    continue;

                columns.Add(kvp.Key);
            }

            return columns;
        }
    }
}
