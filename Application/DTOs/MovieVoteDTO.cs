using Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class MovieVoteDTO
    {
        public int MovieId { get; set; }
        public VoteType VoteType { get; set; }
    }
}
