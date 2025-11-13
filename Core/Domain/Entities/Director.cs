using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Entities
{
    public class Director
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateOnly BirthDate { get; set; }
        public string ImageUrl { get; set; }
        public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();
    }
}
