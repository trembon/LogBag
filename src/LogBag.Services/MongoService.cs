using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogBag.Services
{
    public interface IMongoService
    {
        IMongoCollection<BsonDocument> GetCollection(string pocket);
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

        public IMongoCollection<BsonDocument> GetCollection(string pocket)
        {
            return _client.GetDatabase(_configuration["MongoDB:DatabaseName"]).GetCollection<BsonDocument>(pocket);
        }
    }
}
