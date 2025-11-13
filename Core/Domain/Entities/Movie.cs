using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domain.Entities
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ReleaseYear { get; set; }
        public string PosterUrl { get; set; }
        public string TrailerUrl { get; set; }
        public byte[] RowVersion { get; set; }
        [ForeignKey("Director")]
        public int DirectorId { get; set; }
        public virtual Director Director { get; set; }
        public virtual ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
        public virtual ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public virtual ICollection<Interest> Interests { get; set; } = new List<Interest>();
        public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}