using Application.Extensions;
using Application.Mappings;
using Application.Validators;
using Core.Ports;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Extensions;
using System;

namespace MovieRecommendation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddInfrastructure(builder.Configuration)
                .AddApplication()
                .AddAutoMapper(typeof(MappingProfile))
                .AddValidatorsFromAssemblyContaining<AddMovieDTOValidator>()
                .AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
