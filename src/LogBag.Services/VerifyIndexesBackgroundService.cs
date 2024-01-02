using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogBag.Services
{
    public class VerifyIndexesBackgroundService(IMongoService mongoService, IConfiguration configuration) : BackgroundService, IHostedService
    {
        private readonly TimeSpan _period = TimeSpan.FromHours(1);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new(_period);
            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var collections = await mongoService.GetCollectionNames();
                    foreach(var collection in collections)
                    {
                        await mongoService.CreateIndex(collection, LogsService.TIMESTAMP_COLUMN, true, stoppingToken);

                        int retention = int.Parse(configuration["PocketSettings:DefaultRetentionInDays"] ?? "7");
                        await mongoService.CreateTtlIndex(collection, LogsService.TIMESTAMP_COLUMN, TimeSpan.FromDays(retention), stoppingToken);
                    }
                }
                catch
                {
                    // unknown error, catch so loop aint broken
                }

                await timer.WaitForNextTickAsync(stoppingToken);
            }
        }
    }
}
