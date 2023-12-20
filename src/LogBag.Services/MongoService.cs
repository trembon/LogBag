﻿using Microsoft.Extensions.Configuration;
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

        Task<IEnumerable<string>> GetCollectionNames();

        Task CreateIndex(string collectionName, string column, bool descending, CancellationToken cancellationToken);
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
            return _client.GetDatabase(_configuration["MongoDB:DatabaseName"]).GetCollection<BsonDocument>(name);
        }

        public async Task CreateIndex(string collectionName, string column, bool descending, CancellationToken cancellationToken)
        {
            var collection = GetCollection(collectionName);

            var indexDefinition = Builders<BsonDocument>.IndexKeys.Ascending(column);
            if(descending)
                indexDefinition = Builders<BsonDocument>.IndexKeys.Descending(column);

            await collection.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(indexDefinition), new CreateOneIndexOptions(), cancellationToken);
        }
    }
}
