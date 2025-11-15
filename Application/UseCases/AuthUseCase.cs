using Application.DTOs.Authentication;
using Application.Interfaces;
using Application.Settings;
using Core.Domain.Common;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class AuthUseCase: IAuthUseCase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IEmailService _emailService;
        private readonly AppSettings _appSettings;
        private readonly ILogger<AuthUseCase> _logger;
        public AuthUseCase(UserManager<ApplicationUser> userManager, IJwtTokenService jwtTokenService, ILogger<AuthUseCase> logger, IEmailService emailService, AppSettings appSettings)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
            _emailService = emailService;
            _appSettings = appSettings;
        }
        public async Task<OpResult> SignUpAsync(SignUpDTO signUpDTO)
        {
            if (signUpDTO == null)
            {
                _logger.LogWarning("SignUpAsync called with null SignUpDTO");
                return new OpResult() { Success = false, Message = "Invalid sign up data", StatusCode = 400 };
            }

            try
            {
                var existingUser = await _userManager.FindByEmailAsync(signUpDTO.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration attempt with existing email: {Email}", signUpDTO.Email);
                    return new OpResult() { Success = false, Message = "Email is in use", StatusCode = 409 };
                }

                var newUser = new ApplicationUser()
                {
                    ApplicationUserName = signUpDTO.UserName,
                    UserName = signUpDTO.Email,
                    Email = signUpDTO.Email,
                    EmailConfirmed = false
                };

                var result = await _userManager.CreateAsync(newUser, signUpDTO.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("User creation failed for email {Email}: {Errors}", signUpDTO.Email, errors);
                    return new OpResult() { Success = false, Message = $"User creation failed: {errors}", StatusCode = 400 };
                }

                var roleResult = await _userManager.AddToRoleAsync(newUser, "User");
                if (!roleResult.Succeeded)
                {
                    _logger.LogWarning("Failed to assign User role to user: {Email}", signUpDTO.Email);

                    var deleteResult = await _userManager.DeleteAsync(newUser);
                    if (!deleteResult.Succeeded)
                    {
                        _logger.LogError("Failed to rollback user creation for: {Email}", signUpDTO.Email);
                        // a7a tb hn3mel eh
                    }

                    return new OpResult() { Success = false, Message = "Failed to assign role to user", StatusCode = 500 };
                }

                var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);

                // Encode token for URL (Identity tokens can contain special characters)
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailConfirmationToken));

                var confirmationLink = $"{_appSettings.BaseUrl}/api/Authentication/ConfirmEmail?userId={newUser.Id}&token={encodedToken}";

                await _emailService.SendEmailConfirmationAsync(newUser.Email, newUser.ApplicationUserName, confirmationLink);

                _logger.LogInformation("Email confirmation token generated for: {Email}. Token: {Token}",
                    signUpDTO.Email, emailConfirmationToken);

                _logger.LogInformation("User created successfully with email: {Email}", signUpDTO.Email);
                return new OpResult() { Success = true, Message = "User created successfully", StatusCode = 201 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during user creation for email: {Email}", signUpDTO.Email);
                return new OpResult() { Success = false, Message = "An error occurred during user creation", StatusCode = 500 };
            }
        }
        public async Task<OpResult> SignInAsync(SignInDTO signInDTO)
        {
            if(signInDTO == null)
            {
                _logger.LogWarning("SignInAsync called with null SignInDTO");
                return new OpResult() { Success = false, Message = "Invalid sign in data", StatusCode = 400 };
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(signInDTO.Email);
                if (user == null)
                {
                    _logger.LogWarning("Sign in attempt with non-existing email: {Email}", signInDTO.Email);
                    return new OpResult() { Success = false, Message = "Invalid credentials", StatusCode = 401 };
                }

                if (!user.EmailConfirmed)
                {
                    _logger.LogWarning("Login attempt with unconfirmed email: {Email}", signInDTO.Email);
                    return new OpResult() { Success = false, Message = "Please confirm your email before logging in. Check your inbox for the confirmation link.", StatusCode = 403 };
                }

                var isPasswordValid = await _userManager.CheckPasswordAsync(user, signInDTO.Password);
                if (!isPasswordValid)
                {
                    _logger.LogWarning("Invalid password attempt for email: {Email}", signInDTO.Email);
                    return new OpResult() { Success = false, Message = "Invalid credentials", StatusCode = 401 };
                }

                if(await _userManager.IsLockedOutAsync(user))
                {
                    _logger.LogWarning("Sign in attempt for locked out user: {Email}", signInDTO.Email);
                    return new OpResult() { Success = false, Message = "Account is locked. Please try again later.", StatusCode = 423 };
                }

                var token = await _jwtTokenService.GenerateTokenAsync(user);

                var authResponse = new AuthResponseDTO
                {
                    Token = token,
                    Email = user.Email ?? string.Empty,
                    UserName = user.UserName ?? string.Empty,
                    UserId = user.Id
                };

                _logger.LogInformation("User logged in successfully: {Email}", signInDTO.Email);
                return new OpResult() { Success = true, Message = "Login successful", StatusCode = 200, Data = authResponse };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during sign in for email: {Email}", signInDTO.Email);
                return new OpResult() { Success = false, Message = "An error occurred during sign in", StatusCode = 500 };
            }
        }
        public async Task<OpResult> ConfirmEmailAsync(string userId, string encodedToken)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(encodedToken))
            {
                return new OpResult() { Success = false, Message = "Invalid confirmation link", StatusCode = 400 };
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if(user == null)
                {
                    return new OpResult() { Success = false, Message = "Invalid confirmation link", StatusCode = 400 };
                }

                if(user.EmailConfirmed)
                {
                    return new OpResult() { Success = true, Message = "Email already confirmed", StatusCode = 200 };
                }

                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encodedToken));

                var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Email confirmation failed for userId {UserId}: {Errors}", userId, errors);
                    return new OpResult() { Success = false, Message = $"Email confirmation failed: {errors}", StatusCode = 400 };
                }

                _logger.LogInformation("Email confirmed successfully for user: {Email}", user.Email);
                return new OpResult() { Success = true, Message = "Email confirmed successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during email confirmation for userId: {UserId}", userId);
                return new OpResult() { Success = false, Message = "An error occurred during email confirmation", StatusCode = 500 };
            }
        }
        public async Task<OpResult> ResendConfirmationEmailAsync(ResendConfirmationEmailDTO dto)
        {
            if (dto == null)
            {
                _logger.LogWarning("ResendConfirmationEmailAsync called with null DTO");
                return new OpResult() { Success = false, Message = "Invalid data", StatusCode = 400 };
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    _logger.LogWarning("Resend confirmation requested for non-existent email: {Email}", dto.Email);
                    return new OpResult() { Success = false, Message = "confirmation email has been sent.", StatusCode = 200 };
                }

                if (user.EmailConfirmed)
                {
                    _logger.LogInformation("Resend confirmation requested for already confirmed email: {Email}", dto.Email);
                    return new OpResult() { Success = true, Message = "Email is already confirmed", StatusCode = 200 };
                }

                var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailConfirmationToken));

                var confirmationLink = $"{_appSettings.BaseUrl}/api/Authentication/ConfirmEmail?userId={user.Id}&token={encodedToken}";

                await _emailService.SendEmailConfirmationAsync(dto.Email, user.ApplicationUserName,confirmationLink);

                _logger.LogInformation("Confirmation email resent to: {Email}", dto.Email);

                return new OpResult { Success = true, Message = "Confirmation email has been sent. Please check your inbox.", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while resending confirmation email");
                return new OpResult { Success = false, Message = "An unexpected error occurred", StatusCode = 500 };
            }
        }
    }
}
