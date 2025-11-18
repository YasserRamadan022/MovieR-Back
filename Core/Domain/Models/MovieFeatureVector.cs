using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Models
{
    public class MovieFeatureVector
    {
        public int MovieId { get; set; }

        // Genres this movie belongs to (binary: 1 if has genre, 0 if not)
        public Dictionary<int, double> Genres { get; set; } = new();

        // Actors in this movie
        public Dictionary<int, double> Actors { get; set; } = new();

        // Director of this movie
        public Dictionary<int, double> Director { get; set; } = new();
    }
}
