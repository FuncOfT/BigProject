using AutoMapper;
using BigProject.Models;
using BigProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigProject.Controllers.Api
{
    [Route("/api/trips/{tripName}/stops")]
    [Authorize]
    public class StopsController : Controller
    {
        private readonly IWorldRepository _repository;
        private readonly ILogger<StopsController> _logger;
        private readonly GeoCoordsService _coordsService;

        public StopsController(IWorldRepository repository,
            ILogger<StopsController> logger,
            GeoCoordsService coordsSerice)
        {
            _repository = repository;
            _logger = logger;
            _coordsService = coordsSerice;
        }

        [HttpGet("")]
        public IActionResult Get(string tripName)
        {
            try
            {
                var trip = _repository.GetUserTripByName(tripName, User.Identity.Name);

                var stops = trip.Stops.OrderBy(s => s.Order);

                var models = Mapper.Map<IEnumerable<StopViewModel>>(stops);

                return Ok(models);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get stops: {ex.Message}");
            }

            return BadRequest($"Failed to get stops for {tripName}");
        }

        [HttpPost("")]
        public async Task<IActionResult> Post(string tripName, [FromBody]StopViewModel stopModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var newStop = Mapper.Map<Stop>(stopModel);

                    var result = await _coordsService.GetCoordsAsync(newStop.Name);

                    if (!result.Success)
                    {
                        _logger.LogError(result.Message);
                    }
                    else
                    {
                        newStop.Latitude = result.Latitude;
                        newStop.Longitude = result.Longitude;

                        _repository.AddStop(tripName, newStop, User.Identity.Name);

                        if (await _repository.SaveChangesAsync())
                        {
                            return Created($"/api/trips/{tripName}/stops/{newStop.Name}",
                                Mapper.Map<StopViewModel>(newStop));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save new Stop: {0}", ex);
            }

            return BadRequest("Failed to save new stop");
        }
    }
}
