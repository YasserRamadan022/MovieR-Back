using Application.DTOs;
using AutoMapper;
using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappings
{
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            CreateMap<AddMovieDTO, Movie>()
                .ForMember(dest => dest.MovieGenres, opt => opt.Ignore())
                .ForMember(dest => dest.MovieActors, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    if (src.MovieGenres != null && src.MovieGenres.Count > 0)
                    {
                        dest.MovieGenres = src.MovieGenres.Distinct().Select(genreId => new MovieGenre
                        {
                            GenreId = genreId
                        }).ToList();
                    }
                    else
                    {
                        dest.MovieGenres = new List<MovieGenre>();
                    }

                    if(src.MovieActors != null &&  src.MovieActors.Count > 0)
                    {
                        dest.MovieActors = src.MovieActors.Distinct().Select(actorId => new MovieActor
                        {
                            ActorId = actorId
                        }).ToList();
                    }
                    else
                    {
                        dest.MovieActors = new List<MovieActor>();
                    }
                });

            CreateMap<AddActorDTO, Actor>();
            CreateMap<AddDirectorDTO, Director>();
        }
    }
}
