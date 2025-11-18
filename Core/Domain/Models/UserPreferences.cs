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

        // Movies the user rated
        public Dictionary<int, decimal> RatedMovies { get; set; } = new();

        // Movies the user upvoted
        public List<int> UpvotedMovies { get; set; } = new();

        // Movies the user favorited
        public List<int> FavoritedMovies { get; set; } = new();

        // Genres the user is interested in
        public List<int> InterestedMovies { get; set; } = new();

        // Actors the user likes (from rated/favorited movies)
        public List<int> PreferredActors { get; set; } = new();

        // Directors the user likes
        public List<int> PreferredDirectors { get; set; } = new();
    }
}
