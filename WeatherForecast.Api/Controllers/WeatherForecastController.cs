using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeatherForecast.CommonData.Models;
using WeatherForecast.CommonData.RabbitQueue;

namespace WeatherForecast.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IBus _busControl;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IBus busControl)
        {
            _logger = logger;
            _busControl = busControl;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]List<WeatherForecastRequest> request)
        {
            if (request == null)
            {
                _logger.LogDebug("BadRequest - ConfigurationDto is null or empty");
                return BadRequest();
            }

            await _busControl.SendAsync(Queue.Processing, request);
            await _busControl.SendAsync(Queue.SecondQueue, request);
            
            return Ok();
        }

    }
}
