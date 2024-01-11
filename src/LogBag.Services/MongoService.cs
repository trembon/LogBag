using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LogBag.Services
{
    public interface IMongoService
    {
        IMongoCollection<BsonDocument> GetCollection(string pocket);

        IMongoCollection<T> GetCollection<T>(string name);

        Task<IEnumerable<string>> GetCollectionNames();

        Task CreateIndex(string collectionName, string column, bool descending, CancellationToken cancellationToken);

        Task CreateTtlIndex(string collectionName, string column, TimeSpan expireAfter, CancellationToken cancellationToken);
    }

    public class MongoService : IMongoService
    {
        private readonly IConfiguration _configuration;
        private readonly MongoClient _client;

        public MongoService(IConfiguration configuration)
        {
            _configuration = configuration;
            _client = new MongoClient(_configuration.GetConnectionString("LogDB"));
        }

        public async Task<IEnumerable<string>> GetCollectionNames()
        {
            var collections = await _client.GetDatabase(_configuration["MongoDB:DatabaseName"]).ListCollectionNamesAsync();
            return await collections.ToListAsync();
        }

        public IMongoCollection<BsonDocument> GetCollection(string name)
        {
            return GetCollection<BsonDocument>(name);
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _client.GetDatabase(_configuration["MongoDB:DatabaseName"]).GetCollection<T>(name);
        }

        public async Task CreateIndex(string collectionName, string column, bool descending, CancellationToken cancellationToken)
        {
            var collection = GetCollection(collectionName);

            var definition = Builders<BsonDocument>.IndexKeys.Ascending(column);
            if(descending)
                definition = Builders<BsonDocument>.IndexKeys.Descending(column);

            await collection.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(definition), new CreateOneIndexOptions(), cancellationToken);
        }

        public async Task CreateTtlIndex(string collectionName, string column, TimeSpan expireAfter, CancellationToken cancellationToken)
        {
            var collection = GetCollection(collectionName);

            var definition = Builders<BsonDocument>.IndexKeys.Ascending(column);
            var options = new CreateIndexOptions<BsonDocument>
            {
                ExpireAfter = expireAfter,
                Name = "ExpireAtIndex"
            };

            await collection.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(definition, options), new CreateOneIndexOptions(), cancellationToken);
        }
    }
}
