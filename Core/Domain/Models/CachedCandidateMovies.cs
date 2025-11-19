using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Models
{
    public class CachedCandidateMovies
    {
        public List<int> CandidateMovieIds { get; set; } = new();
        public int MovieCountAtCacheTime { get; set; }
        public DateTime CachedAt { get; set; }
    }
}
