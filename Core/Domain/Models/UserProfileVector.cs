using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Models
{
    public class UserProfileVector
    {
        // Genre preferences (weighted by how much user likes each genre)
        public Dictionary<int, double> GenreWeights { get; set; } = new();

        // Actor preferences
        public Dictionary<int, double> ActorWeights { get; set; } = new();

        // Director preferences
        public Dictionary<int, double> DirectorWeights { get; set; } = new();
    }
}
