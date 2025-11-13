using Application.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class AddMovieDTOValidator: AbstractValidator<AddMovieDTO>
    {
        public AddMovieDTOValidator()
        {
            RuleFor(m => m.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(50).WithMessage("Title cannot exceed 50 characters");

            RuleFor(m => m.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

            RuleFor(m => m.ReleaseYear)
                .InclusiveBetween(1970, DateTime.Now.Year)
                .WithMessage("Release year must be between 1970 and current year");

            RuleFor(m => m.PosterUrl)
                .Must(BeValidUrl).WithMessage("Poster URL must be a valid URL")
                .When(x => !string.IsNullOrEmpty(x.PosterUrl));

            RuleFor(m => m.TrailerUrl)
                .Must(BeValidUrl).WithMessage("Trailer URL must be a valid URL")
                .When(x => !string.IsNullOrEmpty(x.TrailerUrl));

            RuleFor(m => m.MovieGenres)
                .Must(genres => genres != null && genres.Count > 0)
                .WithMessage("At least one genre must be selected");

            RuleFor(m => m.MovieActors)
                .Must(actors => actors != null && actors.Count > 0)
                .WithMessage("At least one actor must be selected");

            RuleFor(m => m.DirectorId)
                .GreaterThan(0)
                .WithMessage("Director Id is invalid");
        }
        private bool BeValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}
