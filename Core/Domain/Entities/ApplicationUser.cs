using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Entities
{
    public class ApplicationUser: IdentityUser
    {
        public string ApplicationUserName { get; set; } = string.Empty;
        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
        public virtual ICollection<Favorite> Favorites { get; set;} = new List<Favorite>();
        public virtual ICollection<Interest> Interests { get; set; } = new List<Interest>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}
