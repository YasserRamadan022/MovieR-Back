using Application.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class MovieRateDTOValidator: AbstractValidator<MovieRateDTO>
    {
        public MovieRateDTOValidator()
        {
            RuleFor(v => v.MovieId)
                .NotEmpty().WithMessage("MovieId is required.")
                .GreaterThan(0).WithMessage("Valid movieId is required.");

            RuleFor(v => v.RatingValue)
                .InclusiveBetween(1, 10).WithMessage("RatingValue must be between 0 and 10.");
        }
    }
}
