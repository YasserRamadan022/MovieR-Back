using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helper
{
    public class MovieSimilarity
    {
        public dynamic Movie { get; set; }
        public Dictionary<int, double> UserGenreVector { get; set; }
        public Dictionary<int, double> UserActorVector { get; set; }
        public Dictionary<int, double> UserDirectorVector { get; set; }
        public HashSet<int> PreferredGenreIds { get; set; }
        public HashSet<int> PreferredActorIds { get; set; }
        public HashSet<int> PreferredDirectorIds { get; set; }
    }
}
