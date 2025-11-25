using Application.DTOs;
using Application.DTOs.Dashboard;
using AutoMapper;
using Core.Domain.Entities;
using Core.Domain.Enums;
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
            CreateMap<AddGenreDTO, Genre>();
            CreateMap<Movie, MoviesDTO>()
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => 
                    src.Ratings != null && src.Ratings.Any()
                        ? (double?)src.Ratings.Average(r => (double)r.RatingValue)
                        : null));
            CreateMap<Genre, GenresDTO>();
            CreateMap<Actor, ActorDTO>();
            CreateMap<Director, DirectorDTO>();
            CreateMap<Movie, MovieDetailsDTO>()
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src =>
                    src.Ratings != null && src.Ratings.Any()
                        ? (double?)src.Ratings.Average(r => (double)r.RatingValue)
                        : null))
                .ForMember(dest => dest.Actors, opt => opt.MapFrom(src =>
                    src.MovieActors != null && src.MovieActors.Any()
                        ? src.MovieActors.Select(ma => ma.Actor).ToList()
                        : new List<Actor>()))
                .ForMember(dest => dest.Director, opt => opt.MapFrom(src => src.Director))
                .ForMember(dest => dest.Genres, opt => opt.MapFrom(src =>
                    src.MovieGenres != null && src.MovieGenres.Any()
                        ? src.MovieGenres.Select(mg => mg.Genre).ToList()
                        : new List<Genre>()))
                .ForMember(dest => dest.UpVotes, opt => opt.MapFrom(src =>
                    src.Votes != null
                        ? src.Votes.Count(v => v.VoteType == VoteType.Upvote)
                        : 0))
                .ForMember(dest => dest.DownVotes, opt => opt.MapFrom(src =>
                    src.Votes != null
                        ? src.Votes.Count(v => v.VoteType == VoteType.Downvote)
                        : 0));
        }
    }
}
