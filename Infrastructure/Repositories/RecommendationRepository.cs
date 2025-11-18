using Core.Domain.Common;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Models;
using Core.Helper;
using Core.Ports;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories
{
    public class RecommendationRepository : IRecommendationRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RecommendationRepository> _logger;
        public RecommendationRepository(AppDbContext context, ILogger<RecommendationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<UserPreferences> GetUserPreferencesAsync(string userId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.UserId == userId)
                .Select(r => new { r.MovieId, r.RatingValue })
                .ToListAsync();

            var upvotes = await _context.Votes
                .Where(v => v.UserId == userId && v.VoteType == VoteType.Upvote)
                .Select(v => v.MovieId)
                .ToListAsync();

            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Select(f => f.MovieId)
                .ToListAsync();

            var interests = await _context.Interests
                .Where(i => i.UserId == userId)
                .Select(i => i.MovieId)
                .ToListAsync();

            var likedMovieIds = ratings.Select(r => r.MovieId)
                .Union(upvotes)
                .Union(favorites)
                .Union(interests)
                .Distinct()
                .ToHashSet();

            var preferredActors = await _context.MovieActors
                .Where(ma => likedMovieIds.Contains(ma.MovieId))
                .Select(ma => ma.ActorId)
                .Distinct()
                .ToListAsync();

            var preferredDirectors = await _context.Movies
                .Where(m => likedMovieIds.Contains(m.Id))
                .Select(m => m.DirectorId)
                .Distinct()
                .ToListAsync();

            return new UserPreferences
            {
                UserId = userId,
                RatedMovies = ratings.ToDictionary(r => r.MovieId, r => r.RatingValue),
                UpvotedMovies = upvotes,
                FavoritedMovies = favorites,
                InterestedMovies = interests,
                PreferredActors = preferredActors,
                PreferredDirectors = preferredDirectors
            };
        }
        public async Task<List<Movie>> GetSimilarMoviesByContentAsync(UserPreferences preferences, int pageNumber, int pageSize)
        {
            var allLikedMovieIds = preferences.RatedMovies.Keys
                .Union(preferences.UpvotedMovies)
                .Union(preferences.FavoritedMovies)
                .Union(preferences.InterestedMovies)
                .Distinct()
                .ToHashSet();

            if (allLikedMovieIds.Count == 0)
            {
                return await GetPopularMoviesAsync(pageNumber, pageSize);
            }

            // ============================================
            // STEP 1: BUILD USER PROFILE VECTOR
            // ============================================

            var allGenres = await _context.Genres.Select(g => g.Id).ToListAsync();
            var allActors = await _context.Actors.Select(a => a.Id).ToListAsync();
            var allDirectors = await _context.Directors.Select(d => d.Id).ToListAsync();

            var allMovieGenresForLikedMovies = await _context.MovieGenres
                    .Where(mg => allLikedMovieIds.Contains(mg.MovieId))
                    .Select(mg => new { mg.MovieId, mg.GenreId })
                    .ToListAsync();

            var userGenreVector = new Dictionary<int, double>();
            foreach (var genreId in allGenres)
            {
                double genreWeight = 0;

                var moviesWithGenre = allMovieGenresForLikedMovies
                    .Where(mg => mg.GenreId == genreId)
                    .Select(mg => mg.MovieId)
                    .ToList();

                if (moviesWithGenre.Any())
                {
                    genreWeight = SimilarityCalculator.CalculateWeightForMovies(moviesWithGenre, preferences);
                }

                userGenreVector[genreId] = genreWeight;
            }

            var allMovieActorsForLikedMovies = await _context.MovieActors
                    .Where(ma => allLikedMovieIds.Contains(ma.MovieId))
                    .Select(ma => new { ma.MovieId, ma.ActorId })
                    .ToListAsync();

            // Build user actor vector
            var userActorVector = new Dictionary<int, double>();
            foreach (var actorId in allActors)
            {
                double actorWeight = 0;
                var movieWithActor = allMovieActorsForLikedMovies
                    .Where(ma => ma.ActorId == actorId)
                    .Select(ma => ma.MovieId)
                    .ToList();

                if (movieWithActor.Any())
                {
                    actorWeight = SimilarityCalculator.CalculateWeightForMovies(movieWithActor, preferences);
                }

                userActorVector[actorId] = actorWeight;
            }

            var allMoviesWithDirectors = await _context.Movies
                    .Where(m => allLikedMovieIds.Contains(m.Id))
                    .Select(m => new { m.Id, m.DirectorId })
                    .ToListAsync();

            // Build user director vector
            var userDirectorVector = new Dictionary<int, double>();
            foreach (var directorId in allDirectors)
            {
                double directorWeight = 0;
                var moviesWithDirector = allMoviesWithDirectors
                    .Where(m => m.DirectorId == directorId)
                    .Select(m => m.Id)
                    .ToList();

                if (moviesWithDirector.Any())
                {
                    directorWeight = SimilarityCalculator.CalculateWeightForMovies(moviesWithDirector, preferences);
                }

                userDirectorVector[directorId] = directorWeight;
            }

            // ============================================
            // STEP 2: GET ALL CANDIDATE MOVIES
            // ============================================

            var candidateMovies = await _context.Movies
                .Where(m => !allLikedMovieIds.Contains(m.Id)) // Exclude already liked
                .Include(m => m.MovieGenres)
                .Include(m => m.MovieActors)
                .Include(m => m.Director)
                .ToListAsync();

            // ============================================
            // STEP 3: BUILD MOVIE VECTORS & CALCULATE SIMILARITY
            // ============================================

            var movieScores = new List<(Movie Movie, double SimilarityScore)>();

            foreach (var movie in candidateMovies)
            {
                // Build movie genre vector (binary: 1 if has genre, 0 if not)
                var movieGenreVector = new Dictionary<int, double>();
                foreach (var genreId in allGenres)
                {
                    movieGenreVector[genreId] = movie.MovieGenres.Any(mg => mg.GenreId == genreId) ? 1.0 : 0.0;
                }

                // Build movie actor vector
                var movieActorVector = new Dictionary<int, double>();
                foreach (var actorId in allActors)
                {
                    movieActorVector[actorId] = movie.MovieActors.Any(ma => ma.ActorId == actorId) ? 1.0 : 0.0;
                }

                // Build movie director vector
                var movieDirectorVector = new Dictionary<int, double>();
                foreach (var directorId in allDirectors)
                {
                    movieDirectorVector[directorId] = (movie.DirectorId == directorId) ? 1.0 : 0.0;
                }

                // ============================================
                // STEP 4: CALCULATE COSINE SIMILARITY
                // ============================================

                // Calculate similarity for each dimension
                double genreSimilarity = SimilarityCalculator.CalculateCosineSimilarity(
                    userGenreVector,
                    movieGenreVector);

                double actorSimilarity = SimilarityCalculator.CalculateCosineSimilarity(
                    userActorVector,
                    movieActorVector);

                double directorSimilarity = SimilarityCalculator.CalculateCosineSimilarity(
                    userDirectorVector,
                    movieDirectorVector);

                // Combine similarities with weights
                double finalSimilarity = SimilarityCalculator.CalculateWeightedSimilarity(
                    genreSimilarity,
                    actorSimilarity,
                    directorSimilarity,
                    genreWeight: 0.5,
                    actorWeight: 0.3,
                    directorWeight: 0.2
                );

                movieScores.Add((movie, finalSimilarity));
            }

            // ============================================
            // STEP 5: SORT BY SIMILARITY & RETURN TOP RESULTS
            // ============================================

            var topMovies = movieScores
                .OrderByDescending(ms => ms.SimilarityScore)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(ms => ms.Movie)
                .ToList();

            return topMovies;
        }
        public Task<PagedResult<Movie>> GetHybridRecommendationsAsync(string userId, int pageNumber = 1, int pageSize = 20)
        {
            throw new NotImplementedException();
        }

        public Task<List<Movie>> GetMoviesByMatrixFactorizationAsync(string userId, int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<List<Movie>> GetMoviesBySimilarUsersAsync(string userId, int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<List<Movie>> GetPopularMoviesAsync(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

    }
}
