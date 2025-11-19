using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Models
{
    public class UserPreferences
    {
        public string UserId { get; set; } = string.Empty;
        public Dictionary<int, decimal> RatedMovies { get; set; } = new();
        public List<int> UpvotedMovies { get; set; } = new();
        public List<int> FavoritedMovies { get; set; } = new();
        public List<int> InterestedMovies { get; set; } = new();
        public List<int> PreferredActors { get; set; } = new();
        public List<int> PreferredDirectors { get; set; } = new();
    }
}
