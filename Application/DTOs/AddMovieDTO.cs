using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class AddMovieDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int ReleaseYear { get; set; }
        public string PosterUrl { get; set; }
        public string TrailerUrl { get; set; }
        public List<int> MovieGenres { get; set; } = new List<int>();
        public List<int> MovieActors { get; set; } = new List<int>();
        public int DirectorId { get; set; }
    }
}
