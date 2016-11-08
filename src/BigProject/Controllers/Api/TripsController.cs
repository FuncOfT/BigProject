using AutoMapper;
using BigProject.Models;
using BigProject.ViewModels;
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
    [Route("api/trips")]
    [Authorize]
    public class TripsController : Controller
    {
        private readonly ILogger<TripsController> _logger;
        private readonly IWorldRepository _repository;

        public TripsController(IWorldRepository repository,
            ILogger<TripsController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet("")]
        public IActionResult Get()
        {
            //if (true)
            //{
            //    return BadRequest("Help");
            //}
            try
            {
                var results = _repository.GetTripsByUsername(User.Identity.Name);

                return Ok(Mapper.Map<IEnumerable<TripViewModel>>(results));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not fetch all trips from the database..: {ex.Message}");
                return BadRequest("Could not fetch all trips from the database..");
            }
        }

        [HttpPost("")]
        public async Task<IActionResult> Post([FromBody]TripViewModel tripModel)
        {
            if (ModelState.IsValid)
            {
                // Save to the Database
                var trip = Mapper.Map<Trip>(tripModel);

                trip.UserName = User.Identity.Name;

                _repository.AddTrip(trip);

                if (await _repository.SaveChangesAsync())
                {
                    return Created($"api/trips/{trip.Name}", Mapper.Map<TripViewModel>(trip));
                }               
            }

            return BadRequest("Failed to save the trip");
        }
    }
}
