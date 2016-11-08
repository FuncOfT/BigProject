using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;

namespace BigProject.Models
{
    public class WorldUser : IdentityUser
    {
        public DateTime FirstTrip { get; set; }
    }
}