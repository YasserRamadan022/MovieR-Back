using Application.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class AddActorDTOValidator: AbstractValidator<AddActorDTO>
    {
        public AddActorDTOValidator()
        {
            RuleFor(a => a.Name)
                .NotEmpty()
                .WithMessage("Actor name is required")
                .MaximumLength(100)
                .WithMessage("Actor name cannot exceed 100 characters")
                .Matches(@"^[A-Za-z\s\-'\.]+$")
                .WithMessage("Actor name contains invalid characters");

            RuleFor(a => a.BirthDate)
                .Must(BeValidBirthDate)
                .WithMessage("Birth date must be a valid date");

            RuleFor(a => a.ImageUrl)
                .Must(BeValidUrl).WithMessage("Image URL must be a valid URL")
                .When(a => !string.IsNullOrEmpty(a.ImageUrl));
        }
        private bool BeValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
        private bool BeValidBirthDate(DateOnly birthDate)
        {
            return birthDate <= DateOnly.FromDateTime(DateTime.Today);
        }
    }
}
