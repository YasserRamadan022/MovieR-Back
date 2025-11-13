using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domain.Entities
{
    public class Interest
    {
        public int Id { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        [ForeignKey("Movie")]
        public int MovieId { get; set; }
        public DateOnly AddedAt { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual Movie Movie { get; set; }
    }
}
