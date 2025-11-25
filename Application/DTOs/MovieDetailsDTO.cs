using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class MovieDetailsDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ReleaseYear { get; set; }
        public string PosterUrl { get; set; }
        public string TrailerUrl { get; set; }
        public List<ActorDTO> Actors { get; set; }
        public DirectorDTO Director { get; set; }
        public List<GenresDTO> Genres { get; set; }
        public double? AverageRating { get; set; }
        public int UpVotes { get; set; }
        public int DownVotes { get; set; }
    }
}
