using System.Collections.Generic;
using System.Threading.Tasks;

namespace BigProject.Models
{
    public interface IWorldRepository
    {
        IEnumerable<Trip> GetAllTrips();
        IEnumerable<Trip> GetTripsByUsername(string name);

        void AddTrip(Trip trip);
        void AddStop(string tripName, Stop newStop, string userName);

        Task<bool> SaveChangesAsync();

        Trip GetTripByName(string tripName);
        Trip GetUserTripByName(string tripName, string userName);
    }
}