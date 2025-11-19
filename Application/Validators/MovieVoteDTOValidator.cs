using Application.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class MovieVoteDTOValidator: AbstractValidator<MovieVoteDTO>
    {
        public MovieVoteDTOValidator()
        {
            RuleFor(v => v.MovieId)
                .NotEmpty().WithMessage("MovieId is required.")
                .GreaterThan(0).WithMessage("Valid movieId is required.");

            RuleFor(v => v.VoteType)
                .NotEmpty().WithMessage("VoteType is required.")
                .IsInEnum().WithMessage("Invalid VoteType.");
        }
    }
}
