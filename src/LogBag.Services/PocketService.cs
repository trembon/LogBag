using LogBag.Services.Extensions;
using LogBag.Services.Models;
using LogBag.Shared.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace LogBag.Services
{
    public interface IPocketService
    {
        Task<List<string>> GetPocketNames();

        Task<List<string>> GetColumnSuggestions(string pocket, CancellationToken cancellationToken);

        Task<List<string>> GetColumns(string pocket, CancellationToken cancellationToken);

        Task SaveConfiguration(string pocket, ConfigurePocketRequest configuration, CancellationToken cancellationToken);
    }

    public class PocketService(IMongoService mongoService) : IPocketService
    {
        private const string CONFIGURATION_COLLECTION = "__pocket_configuration__";

        public async Task<List<string>> GetPocketNames()
        {
            var names = await mongoService.GetCollectionNames();
            return names.Where(x => x != CONFIGURATION_COLLECTION).ToList();
        }

        public async Task<List<string>> GetColumnSuggestions(string pocket, CancellationToken cancellationToken)
        {
            var filter = Builders<BsonDocument>.Filter.Empty;
            var row = await mongoService.GetCollection(pocket).Find(filter).Limit(1).SingleOrDefaultAsync(cancellationToken);
            if (row == null)
                return [];

            var dictionary = row.ToFlattenedDictionary([LogsService.ID_COLUMN, LogsService.TIMESTAMP_COLUMN]);
            return [.. dictionary.Keys];
        }

        public async Task<List<string>> GetColumns(string pocket, CancellationToken cancellationToken)
        {
            var collection = mongoService.GetCollection<PocketConfiguration>(CONFIGURATION_COLLECTION);

            var item = await collection
                .Find(Builders<PocketConfiguration>.Filter.Eq(x => x.PocketName, pocket))
                .FirstOrDefaultAsync(cancellationToken);

            return item?.ConfiguredColumns ?? [];
        }

        public async Task SaveConfiguration(string pocket, ConfigurePocketRequest configuration, CancellationToken cancellationToken)
        {
            var collection = mongoService.GetCollection<PocketConfiguration>(CONFIGURATION_COLLECTION);

            var filter = Builders<PocketConfiguration>.Filter.Eq(x => x.PocketName, pocket);
            var item = await collection
                .Find(filter)
                .FirstOrDefaultAsync(cancellationToken);

            if(item == null)
            {
                PocketConfiguration insert = new()
                {
                    PocketName = pocket,
                    ConfiguredColumns = configuration.Columns
                };
                await collection.InsertOneAsync(insert, null, cancellationToken);
            }
            else
            {
                var update = Builders<PocketConfiguration>.Update.Set(x => x.ConfiguredColumns, configuration.Columns);
                await collection.UpdateOneAsync(filter, update, null,cancellationToken);
            }
        }
    }
}
