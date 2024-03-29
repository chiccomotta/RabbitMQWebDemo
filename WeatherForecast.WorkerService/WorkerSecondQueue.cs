﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WeatherForecast.CommonData.Models;
using WeatherForecast.CommonData.RabbitQueue;

namespace WeatherForecast.WorkerService
{
    public class WorkerSecondQueue : BackgroundService
    {
        private readonly ILogger<WorkerSecondQueue> _logger;
        private readonly IBus _busControl;
        
        public WorkerSecondQueue(ILogger<WorkerSecondQueue> logger)
        {
            _logger = logger;
            _busControl = RabbitManager.CreateBus("localhost");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _busControl.ReceiveAsync<IEnumerable<WeatherForecastRequest>>(Queue.SecondQueue, request =>
            {
                Task.Run(() =>
                {
                    foreach (var wf in request)
                    {
                        _logger.LogInformation($"Location: {wf.Location}, date: {wf.Date}, {wf.TemperatureC}°C, {wf.TemperatureF}°F, Summary: {wf.Summary}");
                    }                }, stoppingToken);
            });
        }
    }
}
