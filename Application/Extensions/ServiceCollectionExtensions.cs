using Application.Interfaces;
using Application.Services;
using Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAuthUseCase, AuthUseCase>();

            services.AddScoped<IMovieUseCase, MovieUseCase>();
            services.AddScoped<IActorUseCase, ActorUseCase>();
            services.AddScoped<IDirectorUseCase, DirectorUseCase>();

            return services;
        }
    }
}
