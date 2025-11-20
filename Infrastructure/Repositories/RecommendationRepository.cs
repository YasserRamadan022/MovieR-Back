using Core.Domain.Common;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Models;
using Core.Helper;
using Core.Ports;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Linq;

namespace Infrastructure.Repositories
{
    public class RecommendationRepository : IRecommendationRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RecommendationRepository> _logger;
        private readonly IMemoryCache _cache;
        public RecommendationRepository(AppDbContext context, ILogger<RecommendationRepository> logger, IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }
        private async Task<UserPreferences> GetUserPreferencesAsync(string userId)
        {
            var ratings = await _context.Ratings
                .AsNoTracking()
                .Where(r => r.UserId == userId && r.RatingValue > 4)
                .Select(r => new { r.MovieId, r.RatingValue })
                .ToListAsync();

            var upvotes = await _context.Votes
                .AsNoTracking()
                .Where(v => v.UserId == userId && v.VoteType == VoteType.Upvote)
                .Select(v => v.MovieId)
                .ToListAsync();

            var favorites = await _context.Favorites
                .AsNoTracking()
                .Where(f => f.UserId == userId)
                .Select(f => f.MovieId)
                .ToListAsync();

            var interests = await _context.Interests
                .AsNoTracking()
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
                .AsNoTracking()
                .Where(ma => likedMovieIds.Contains(ma.MovieId))
                .Select(ma => ma.ActorId)
                .Distinct()
                .ToListAsync();

            var preferredDirectors = await _context.Movies
                .AsNoTracking()
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
        private async Task<List<string>> GetCandidateUserIds(string targetUserId, HashSet<int> targetUserPreferences)
        {
            var ratings = await _context.Ratings
                .AsNoTracking()
                .Where(r => targetUserPreferences.Contains(r.MovieId) && r.RatingValue > 4 && r.UserId != targetUserId)
                .Select(r => new { r.UserId, r.MovieId })
                .ToListAsync();
            
            var upvotes = await _context.Votes
                .AsNoTracking()
                .Where(v => targetUserPreferences.Contains(v.MovieId) && v.VoteType == VoteType.Upvote && v.UserId != targetUserId)
                .Select(v => new { v.UserId, v.MovieId })
                .ToListAsync();

            var favorites = await _context.Favorites
                .AsNoTracking()
                .Where(f => targetUserPreferences.Contains(f.MovieId) && f.UserId != targetUserId)
                .Select(f => new { f.UserId, f.MovieId })
                .ToListAsync();

            var interests = await _context.Interests
                .AsNoTracking()
                .Where(i => targetUserPreferences.Contains(i.MovieId) && i.UserId != targetUserId)
                .Select(i => new { i.UserId, i.MovieId })
                .ToListAsync();

            var allCandidateUsers = ratings
                .Union(upvotes)
                .Union(favorites)
                .Union(interests)
                .ToList();

            var allActiveUsers = allCandidateUsers
                .GroupBy(u => u.UserId)
                .Where(g => g.Count() >= 5)
                .Select(g => g.Key)
                .ToList();

            return allActiveUsers;
        }
        private async Task<Dictionary<string, Dictionary<int, double>>> BuildCandidateUserVectors(List<string> candidateUserIds)
        {
            var candidateUserVectors = new Dictionary<string, Dictionary<int, double>>();

            var candidateRatings = await _context.Ratings
                .AsNoTracking()
                .Where(r => candidateUserIds.Contains(r.UserId) && r.RatingValue > 4)
                .Select(r => new { r.UserId, r.MovieId, r.RatingValue })
                .ToListAsync();

            var candidateUpvotes = await _context.Votes
                .AsNoTracking()
                .Where(v => candidateUserIds.Contains(v.UserId) && v.VoteType == VoteType.Upvote)
                .Select(v => new { v.UserId, v.MovieId })
                .ToListAsync();

            var candidateFavorites = await _context.Favorites
                .AsNoTracking()
                .Where(f => candidateUserIds.Contains(f.UserId))
                .Select(f => new { f.UserId, f.MovieId })
                .ToListAsync();

            var candidateInterests = await _context.Interests
                .AsNoTracking()
                .Where(i => candidateUserIds.Contains(i.UserId))
                .Select(i => new { i.UserId, i.MovieId })
                .ToListAsync();

            var uniqueCandidateUserIds = candidateRatings.Select(r => r.UserId)
                .Union(candidateUpvotes.Select(v => v.UserId))
                .Union(candidateFavorites.Select(f => f.UserId))
                .Union(candidateInterests.Select(i => i.UserId))
                .Distinct()
                .ToList();

            foreach (var candidateUserId in uniqueCandidateUserIds)
            {
                var userVector = new Dictionary<int, double>();

                var userRatings = candidateRatings
                    .Where(r => r.UserId == candidateUserId)
                    .ToList();

                foreach (var rating in userRatings)
                {
                    userVector[rating.MovieId] = (double)rating.RatingValue / 10.0;
                }

                var userFavorites = candidateFavorites
                    .Where(f => f.UserId == candidateUserId)
                    .Select(f => f.MovieId)
                    .ToList();

                foreach (var movieId in userFavorites)
                {
                    if (!userVector.ContainsKey(movieId))
                    {
                        userVector[movieId] = 0.9;
                    }
                }

                var userUpvotes = candidateUpvotes
                    .Where(v => v.UserId == candidateUserId)
                    .Select(v => v.MovieId)
                    .ToList();

                foreach (var movieId in userUpvotes)
                {
                    if (!userVector.ContainsKey(movieId))
                    {
                        userVector[movieId] = 0.7;
                    }
                }

                var userInterests = candidateInterests
                    .Where(i => i.UserId == candidateUserId)
                    .Select(i => i.MovieId)
                    .ToList();

                foreach (var movieId in userInterests)
                {
                    if (!userVector.ContainsKey(movieId))
                    {
                        userVector[movieId] = 0.5;
                    }
                }

                candidateUserVectors[candidateUserId] = userVector;
            }

            return candidateUserVectors;
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

            var allMovieGenresForLikedMovies = await _context.MovieGenres
                    .AsNoTracking()
                    .Where(mg => allLikedMovieIds.Contains(mg.MovieId))
                    .Select(mg => new { mg.MovieId, mg.GenreId })
                    .ToListAsync();

            var uniqueGenreIdsInLikedMovies = allMovieGenresForLikedMovies
                    .Select(ma => ma.GenreId)
                    .Distinct()
                    .ToList();

            var userGenreVector = new Dictionary<int, double>();
            foreach (var genreId in uniqueGenreIdsInLikedMovies)
            {
                double genreWeight = 0;

                var moviesWithGenre = allMovieGenresForLikedMovies
                    .Where(mg => mg.GenreId == genreId)
                    .Select(mg => mg.MovieId)
                    .Distinct()
                    .ToList();

                if (moviesWithGenre.Any())
                {
                    genreWeight = SimilarityCalculator.CalculateWeightForMovies(moviesWithGenre, preferences);
                }

                userGenreVector[genreId] = genreWeight;
            }

            var allMovieActorsForLikedMovies = await _context.MovieActors
                    .AsNoTracking()
                    .Where(ma => allLikedMovieIds.Contains(ma.MovieId))
                    .Select(ma => new { ma.MovieId, ma.ActorId })
                    .Distinct()
                    .ToListAsync();

            var uniqueActorIdsInLikedMovies = allMovieActorsForLikedMovies
                .Select(ma => ma.ActorId)
                .Distinct()
                .ToList();

            // Build user actor vector
            var userActorVector = new Dictionary<int, double>();
            foreach (var actorId in uniqueActorIdsInLikedMovies)
            {
                double actorWeight = 0;
                var movieWithActor = allMovieActorsForLikedMovies
                    .Where(ma => ma.ActorId == actorId)
                    .Select(ma => ma.MovieId)
                    .Distinct()
                    .ToList();

                if (movieWithActor.Any())
                {
                    actorWeight = SimilarityCalculator.CalculateWeightForMovies(movieWithActor, preferences);
                }

                userActorVector[actorId] = actorWeight;
            }

            var allMoviesWithDirectors = await _context.Movies
                    .AsNoTracking()
                    .Where(m => allLikedMovieIds.Contains(m.Id))
                    .Select(m => new { m.Id, m.DirectorId })
                    .Distinct()
                    .ToListAsync();

            var uniqueDirectorIdsInLikedMovies = allMoviesWithDirectors
                    .Select(ma => ma.DirectorId)
                    .Distinct()
                    .ToList();

            // Build user director vector
            var userDirectorVector = new Dictionary<int, double>();
            foreach (var directorId in uniqueDirectorIdsInLikedMovies)
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

            var currentMovieCount = await _context.Movies.CountAsync();
            var cacheKey = $"candidate_movies_{preferences.UserId}";

            List<int> candidateMovieIds = new List<int>();
            bool useCache = false;

            if (_cache.TryGetValue(cacheKey, out CachedCandidateMovies cachedData))
            {
                int newMoviesCount = Math.Max(0, currentMovieCount - cachedData.MovieCountAtCacheTime);

                if (newMoviesCount < 30)
                {
                    candidateMovieIds = cachedData.CandidateMovieIds;
                    useCache = true;
                    _logger.LogInformation("Using cached candidate movies. {NewMovies} new movies since cache (threshold: 20). Cached count: {CachedCount}",
                        newMoviesCount, candidateMovieIds.Count);
                }
                else
                {
                    _cache.Remove(cacheKey);
                    _logger.LogInformation("Cache invalidated. {NewMovies} new movies added (threshold: 20). Rebuilding cache...",
                        newMoviesCount);
                }
            }
            else
            {
                _logger.LogInformation("No cache found for user {UserId}. Building new cache...", preferences.UserId);
            }

            if (!useCache)
            {
                var totalPreferences = userGenreVector.Count + userActorVector.Count + userDirectorVector.Count;
                var candidateMovieIdsQuery = Enumerable.Empty<int>().AsQueryable();

                // Handle edge case: No preferences
                if (totalPreferences == 0)
                {
                    candidateMovieIdsQuery = _context.Movies
                        .AsNoTracking()
                        .Where(m => !allLikedMovieIds.Contains(m.Id))
                        .OrderByDescending(m => m.ReleaseYear)
                        .Take(pageSize * 3)
                        .Select(m => m.Id);
                }
                else
                {
                    // Determine minimum matches required
                    int minMatchesRequired = totalPreferences > 30 ? 2 : 1;

                    if (minMatchesRequired == 2)
                    {
                        candidateMovieIdsQuery = _context.Movies
                            .AsNoTracking()
                            .Where(m => !allLikedMovieIds.Contains(m.Id))
                            .Where(m =>
                                ((userGenreVector.Count > 0 &&
                                  m.MovieGenres.Any(mg => userGenreVector.ContainsKey(mg.GenreId))) ? 1 : 0) +
                                ((userActorVector.Count > 0 &&
                                  m.MovieActors.Any(ma => userActorVector.ContainsKey(ma.ActorId))) ? 1 : 0) +
                                ((userDirectorVector.Count > 0 &&
                                  userDirectorVector.ContainsKey(m.DirectorId)) ? 1 : 0)
                                >= 2
                            )
                            .Select(m => m.Id);
                    }
                    else
                    {
                        candidateMovieIdsQuery = _context.Movies
                            .AsNoTracking()
                            .Where(m => !allLikedMovieIds.Contains(m.Id))
                            .Where(m =>
                                (userGenreVector.Count > 0 &&
                                 m.MovieGenres.Any(mg => userGenreVector.ContainsKey(mg.GenreId))) ||
                                (userActorVector.Count > 0 &&
                                 m.MovieActors.Any(ma => userActorVector.ContainsKey(ma.ActorId))) ||
                                (userDirectorVector.Count > 0 &&
                                 userDirectorVector.ContainsKey(m.DirectorId))
                            )
                            .Select(m => m.Id);
                    }
                }

                candidateMovieIds = await candidateMovieIdsQuery.ToListAsync();

                if (candidateMovieIds.Any())
                {
                    var newCachedData = new CachedCandidateMovies
                    {
                        CandidateMovieIds = candidateMovieIds,
                        MovieCountAtCacheTime = currentMovieCount,
                        CachedAt = DateTime.UtcNow
                    };

                    //var cacheOptions = new MemoryCacheEntryOptions
                    //{
                    //    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24),
                    //    SlidingExpiration = TimeSpan.FromHours(1)
                    //};

                    _cache.Set(cacheKey, newCachedData);
                    _logger.LogInformation("Cached {Count} candidate movies for user {UserId}. Total movies in DB: {TotalMovies}",
                        candidateMovieIds.Count, preferences.UserId, currentMovieCount);
                }
            }

            if (!candidateMovieIds.Any())
            {
                return await GetPopularMoviesAsync(pageNumber, pageSize);
            }

            var candidateMoviesBasic = await _context.Movies
                    .AsNoTracking()
                    .Where(m => candidateMovieIds.Contains(m.Id))
                    .Select(m => new
                    {
                        m.Id,
                        m.DirectorId,
                        GenreIds = m.MovieGenres.Select(mg => mg.GenreId).ToList(),
                        ActorIds = m.MovieActors.Select(ma => ma.ActorId).ToList()
                    })
                    .ToListAsync();

            // ============================================
            // STEP 3: BUILD MOVIE VECTORS & CALCULATE COSINE SIMILARITY
            // ============================================

            var availableCores = Environment.ProcessorCount;
            var movieScores = new ConcurrentBag<(int MovieId, double SimilarityScore)>();

            _logger.LogInformation("Processing {Count} candidate movies using {Cores} CPU core(s)",
                    candidateMoviesBasic.Count, availableCores);

            var preferredGenreIds = userGenreVector.Keys.ToHashSet();
            var preferredActorIds = userActorVector.Keys.ToHashSet();
            var preferredDirectorIds = userDirectorVector.Keys.ToHashSet();

            if (availableCores > 1 && candidateMoviesBasic.Count > 10)
            {
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = availableCores
                };

                Parallel.ForEach(candidateMoviesBasic, parallelOptions, movie =>
                {
                    var movieSimilarity = new MovieSimilarity
                    {
                        Movie = movie,
                        PreferredGenreIds = preferredGenreIds,
                        PreferredActorIds = preferredActorIds,
                        PreferredDirectorIds = preferredDirectorIds,
                        UserGenreVector = userGenreVector,
                        UserActorVector = userActorVector,
                        UserDirectorVector = userDirectorVector
                    };
                    double finalSimilarity = SimilarityCalculator.CalculateMovieSimilarity(movieSimilarity);

                    movieScores.Add((movie.Id, finalSimilarity));
                });

                _logger.LogInformation("Completed parallel processing using {Threads} thread(s)",
                        parallelOptions.MaxDegreeOfParallelism);
            }
            else
            {
                foreach (var movie in candidateMoviesBasic)
                {
                    var movieSimilarity = new MovieSimilarity
                    {
                        Movie = movie,
                        PreferredGenreIds = preferredGenreIds,
                        PreferredActorIds = preferredActorIds,
                        PreferredDirectorIds = preferredDirectorIds,
                        UserGenreVector = userGenreVector,
                        UserActorVector = userActorVector,
                        UserDirectorVector = userDirectorVector
                    };
                    double finalSimilarity = SimilarityCalculator.CalculateMovieSimilarity(movieSimilarity);

                    movieScores.Add((movie.Id, finalSimilarity));
                }

                _logger.LogInformation("Completed sequential processing");
            }

            // ============================================
            // STEP 4: GET TOP MOVIE IDs
            // ============================================

            var topMovieIds = movieScores
                .OrderByDescending(ms => ms.SimilarityScore)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(ms => ms.MovieId)
                .ToList();

            // ============================================
            // STEP 5: LOAD FULL ENTITIES ONLY FOR TOP RESULTS
            // ============================================

            var topMovies = await _context.Movies
                .AsNoTracking()
                .Where(m => topMovieIds.Contains(m.Id))
                .Include(m => m.MovieGenres)
                .Include(m => m.MovieActors)
                .Include(m => m.Director)
                .ToListAsync();

            var topMoviesDict = topMovies.ToDictionary(m => m.Id);
            var topMoviesOrdered = topMovieIds
                .Select(id => topMoviesDict[id])
                .ToList();

            return topMoviesOrdered;
        }

        public async Task<List<Movie>> GetMoviesBySimilarUsersAsync(string userId, int pageNumber, int pageSize)
        {
            var targetUserPreferences = await GetUserPreferencesAsync(userId);

            var targetUserMovieIds = targetUserPreferences.RatedMovies.Keys
                                    .Union(targetUserPreferences.UpvotedMovies)
                                    .Union(targetUserPreferences.FavoritedMovies)
                                    .Union(targetUserPreferences.InterestedMovies)
                                    .Distinct()
                                    .ToHashSet();

            if (targetUserMovieIds.Count == 0)
            {
                return await GetPopularMoviesAsync(pageNumber, pageSize);
            }

            var candidateUserIds = await GetCandidateUserIds(userId, targetUserMovieIds);

            // ============================================
            // STEP 1: BUILD TARGET USER VECTOR
            // ============================================

            var targetUserVector = new Dictionary<int, double>();
            foreach (var movieId in targetUserMovieIds)
            {
                if (targetUserPreferences.RatedMovies.ContainsKey(movieId))
                {
                    var rating = targetUserPreferences.RatedMovies[movieId];
                    targetUserVector[movieId] = (double)rating / 10.0;
                }
                else if (targetUserPreferences.FavoritedMovies.Contains(movieId))
                {
                    targetUserVector[movieId] = 0.9;
                }
                else if (targetUserPreferences.UpvotedMovies.Contains(movieId))
                {
                    targetUserVector[movieId] = 0.7;
                }
                else if (targetUserPreferences.InterestedMovies.Contains(movieId))
                {
                    targetUserVector[movieId] = 0.5;
                }
            }

            // ============================================
            // STEP 2: BUILD CANDITADE USERS VECTOR
            // ============================================

            var candidateUserVectors = await BuildCandidateUserVectors(candidateUserIds);

            _logger.LogInformation("Built vectors for {Count} candidate users", candidateUserVectors.Count);

            if (candidateUserVectors.Count == 0)
            {
                _logger.LogInformation("No candidate users found. Falling back to popular movies.");
                return await GetPopularMoviesAsync(pageNumber, pageSize);
            }

            // ============================================
            // STEP 3: CALCULATE USER-TO-USER SIMILARITY
            // ============================================

            var availableCores = Environment.ProcessorCount;
            var userSimilarities = new ConcurrentBag<(string UserId, double SimilarityScore)>();

            _logger.LogInformation("Calculating similarity for {Count} candidate users using {Cores} CPU core(s)",
                candidateUserVectors.Count, availableCores);

            if (availableCores > 1 && candidateUserVectors.Count > 10)
            {
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = availableCores
                };

                Parallel.ForEach(candidateUserVectors, parallelOptions, kvp =>
                {
                    var candidateUserId = kvp.Key;
                    var candidateUserVector = kvp.Value;

                    var similarity = SimilarityCalculator.CalculateCosineSimilarity(
                        targetUserVector,
                        candidateUserVector
                    );

                    if (similarity > 0)
                    {
                        userSimilarities.Add((candidateUserId, similarity));
                    }
                });

                _logger.LogInformation("Parallel processing completed. Found {Count} similar users (similarity > 0)",
                    userSimilarities.Count);
            }
            else
            {
                foreach (var kvp in candidateUserVectors)
                {
                    var candidateUserId = kvp.Key;
                    var candidateUserVector = kvp.Value;

                    var similarity = SimilarityCalculator.CalculateCosineSimilarity(
                        targetUserVector,
                        candidateUserVector
                    );

                    if (similarity > 0)
                    {
                        userSimilarities.Add((candidateUserId, similarity));
                    }
                }

                _logger.LogInformation("Sequential processing completed. Found {Count} similar users (similarity > 0)",
                    userSimilarities.Count);
            }

            // ============================================
            // STEP 4: SELECT TOP-K SIMILAR USERS
            // ============================================

            if (!userSimilarities.Any())
            {
                return await GetPopularMoviesAsync(pageNumber, pageSize);
            }

            var sortedSimilarities = userSimilarities
                    .OrderByDescending(s => s.SimilarityScore)
                    .ToList();

            const int topKUsers = 20;
            var topSimilarUsers = sortedSimilarities.Take(topKUsers).ToList();

            var topSimilarUsersDict = topSimilarUsers
                .ToDictionary(u => u.UserId, u => u.SimilarityScore);

            var topSimilarUserIds = topSimilarUsersDict.Keys.ToHashSet();

            // ============================================
            // STEP 5: AGGREGATE MOVIE SCORES
            // ============================================

            var movieScores = new Dictionary<int, double>();

            foreach (var similarUser in topSimilarUsers)
            {
                var similarUserId = similarUser.UserId;
                var similarityScore = topSimilarUsersDict[similarUserId];
                var userVector = candidateUserVectors[similarUserId];

                foreach (var kvp in userVector)
                {
                    var movieId = kvp.Key;
                    var interactionWeight = kvp.Value;

                    if (!targetUserMovieIds.Contains(movieId))
                    {
                        var contribution = similarityScore * interactionWeight;

                        if (!movieScores.ContainsKey(movieId))
                            movieScores[movieId] = 0;

                        movieScores[movieId] += contribution;
                    }
                }
            }

            // ============================================
            // STEP 6: RETURN TOP RECOMMENDATIONS
            // ============================================

            var sortedMovieIds = movieScores
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();

            var skip = (pageNumber - 1) * pageSize;
            var pagedMovieIds = sortedMovieIds
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            var movies = await _context.Movies
                .AsNoTracking()
                .Where(m => pagedMovieIds.Contains(m.Id))
                .ToListAsync();

            var orderedMovies = pagedMovieIds
                .Select(id => movies.First(m => m.Id == id))
                .ToList();

            _logger.LogInformation("Returning {Count} movies for page {Page} (page size: {PageSize})",
                orderedMovies.Count, pageNumber, pageSize);

            return orderedMovies;
        }

        public Task<PagedResult<Movie>> GetHybridRecommendationsAsync(string userId, int pageNumber = 1, int pageSize = 20)
        {
            throw new NotImplementedException();
        }

        public Task<List<Movie>> GetMoviesByMatrixFactorizationAsync(string userId, int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<List<Movie>> GetPopularMoviesAsync(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}
