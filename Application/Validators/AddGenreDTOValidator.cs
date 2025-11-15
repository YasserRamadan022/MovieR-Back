using Application.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class AddGenreDTOValidator: AbstractValidator<AddGenreDTO>
    {
        public AddGenreDTOValidator()
        {
            RuleFor(g => g.Name)
                .NotEmpty()
                .WithMessage("Genre name is required")
                .MaximumLength(50)
                .WithMessage("Genre name cannot exceed 50 characters")
                .Matches(@"^[\p{L}\s\-']+$")
                .WithMessage("Genre name contains invalid characters");
        }
    }
}
