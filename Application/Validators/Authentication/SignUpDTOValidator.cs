using Application.DTOs.Authentication;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.Authentication
{
    public class SignUpDTOValidator: AbstractValidator<SignUpDTO>
    {
        public SignUpDTOValidator()
        {
            RuleFor(e => e.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email is required.")
                .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

            RuleFor(e => e.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character");

            RuleFor(e => e.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm Password is required.")
                .Equal(e => e.Password).WithMessage("Passwords do not match.");

            RuleFor(e => e.UserName)
                .NotEmpty().WithMessage("username is required.")
                .MinimumLength(3).WithMessage("username must be at least 3 characters.")
                .MaximumLength(50).WithMessage("username must not exceed 50 characters.")
                .Matches(@"^[a-zA-Z0-9_ ]+$").WithMessage("Username can only contain letters, numbers, spaces, and underscores");
        }
    }
}
