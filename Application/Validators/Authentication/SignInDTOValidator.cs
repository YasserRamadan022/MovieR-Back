using Application.DTOs.Authentication;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.Authentication
{
    public class SignInDTOValidator: AbstractValidator<SignInDTO>
    {
        public SignInDTOValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("email is required.")
                .EmailAddress().WithMessage("email must be a valid email address.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }
}
