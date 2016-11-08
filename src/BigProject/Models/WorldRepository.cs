using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

namespace BigProject.Models
{
    public class WorldRepository : IWorldRepository
    {
        private readonly WorldContext _context;
        private readonly ILogger<WorldRepository> _logger;
        
        public WorldRepository(WorldContext context,
            ILogger<WorldRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void AddStop(string tripName, Stop newStop, string userName)
        {
            try
            {
                var trip = GetUserTripByName(tripName, userName);

                trip.Stops.Add(newStop);

                _context.Stops.Add(newStop);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void AddTrip(Trip trip)
        {
            _context.Add(trip);
        }

        public IEnumerable<Trip> GetAllTrips()
        {
            _logger.LogInformation("Getting All Trips form the Database");

            return _context.Trips.ToList();
        }

        public IEnumerable<Trip> GetTripsByUsername(string name)
        {
            return _context
                .Trips
                .Include(t => t.Stops)
                .Where(t => t.UserName == name)
                .ToList();
        }

        public Trip GetTripByName(string tripName)
        {
            return _context.Trips
                .Include(t => t.Stops)
                .Single(t => string.Equals(t.Name, tripName, StringComparison.OrdinalIgnoreCase));
        }

        public Trip GetUserTripByName(string tripName, string userName)
        {
            return _context.Trips
                .Include(t => t.Stops)
                .Single(t => string.Equals(t.Name, tripName, StringComparison.OrdinalIgnoreCase) &&
                t.UserName == userName);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync()) > 0;
        }
    }
}
