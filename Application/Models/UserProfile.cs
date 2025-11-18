using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public class UserProfile
    {
        public Dictionary<int, int> PreferredGenres { get; set; } = new();
        // Key: GenreId, Value: Count (how many times user interacted with this genre)

        public Dictionary<int, int> PreferredActors { get; set; } = new();
        // Key: ActorId, Value: Count

        public Dictionary<int, int> PreferredDirectors { get; set; } = new();
        // Key: DirectorId, Value: Count

        public int TotalInteractions { get; set; }
        // Total movies user has interacted with (for normalization)
    }
}
