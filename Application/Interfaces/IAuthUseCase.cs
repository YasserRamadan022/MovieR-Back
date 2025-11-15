using Application.DTOs.Authentication;
using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAuthUseCase
    {
        Task<OpResult> SignUpAsync(SignUpDTO signUpDTO);
        Task<OpResult> SignInAsync(SignInDTO signInDTO);
        Task<OpResult> ConfirmEmailAsync(string userId, string encodedToken);
        Task<OpResult> ResendConfirmationEmailAsync(ResendConfirmationEmailDTO dto);
    }
}
