using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class AddActorDTO
    {
        public string Name { get; set; }
        public DateOnly BirthDate { get; set; }
        public string ImageUrl { get; set; }
    }
}
