using LogBag.Services.Extensions;
using LogBag.Services.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LogBag.Services
{
    public interface IPocketService
    {
        Task<IEnumerable<string>> GetPocketNames();

        Task<List<string>> GetColumnSuggestions(string pocket, CancellationToken cancellationToken);

        Task<List<string>> GetColumns(string pocket, CancellationToken cancellationToken);
    }

    public class PocketService(IMongoService mongoService) : IPocketService
    {
        private const string CONFIGURATION_COLLECTION = "__pocket_configuration__";

        public async Task<IEnumerable<string>> GetPocketNames()
        {
            return await mongoService.GetCollectionNames();
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
    }
}
