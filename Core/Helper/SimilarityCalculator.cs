using Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helper
{
    public static class SimilarityCalculator
    {
        /// <summary>
        /// Calculates cosine similarity between vectors
        /// Formula: (Vector One · Vector Two) / (|Vector One| × |Vector Two|)
        /// </summary>
        public static double CalculateCosineSimilarity(Dictionary<int, double> vectorOne, Dictionary<int, double> vectorTwo)
        {
            var allKeys = vectorOne.Keys.Union(vectorTwo.Keys).ToList();

            if (allKeys.Count == 0)
                return 0;

            // Calculate dot product (Vector One · Vector Two)
            double dotProduct = 0;
            foreach (var key in allKeys)
            {
                var userValue = vectorOne.GetValueOrDefault(key, 0);
                var movieValue = vectorTwo.GetValueOrDefault(key, 0);
                dotProduct += userValue * movieValue;
            }

            // Calculate magnitude of Vector One |Vector One|
            double vectorOneMagnitude = 0;
            foreach (var value in vectorOne.Values)
            {
                vectorOneMagnitude += value * value;
            }
            vectorOneMagnitude = Math.Sqrt(vectorOneMagnitude);

            // Calculate magnitude of Vector Two |Vector Two|
            double vectorTwoMagnitude = 0;
            foreach (var value in vectorTwo.Values)
            {
                vectorTwoMagnitude += value * value;
            }
            vectorTwoMagnitude = Math.Sqrt(vectorTwoMagnitude);

            // Avoid division by zero
            if (vectorOneMagnitude == 0 || vectorTwoMagnitude == 0)
                return 0;

            // Cosine Similarity = Dot Product / (Vector One Magnitude × Vector Two Magnitude)
            return dotProduct / (vectorOneMagnitude * vectorTwoMagnitude);
        }

        /// <summary>
        /// Combines multiple similarity scores (genres, actors, directors) with weights
        /// </summary>
        public static double CalculateWeightedSimilarity(
            double genreSimilarity,
            double actorSimilarity,
            double directorSimilarity,
            double genreWeight = 0.5,
            double actorWeight = 0.3,
            double directorWeight = 0.2)
        {
            return (genreSimilarity * genreWeight) +
                   (actorSimilarity * actorWeight) +
                   (directorSimilarity * directorWeight);
        }

        public static double CalculateMovieSimilarity(MovieSimilarity data)
        {
            var movieGenreIds = data.Movie.GenreIds.ToHashSet();
            var movieActorIds = data.Movie.ActorIds.ToHashSet();

            var movieGenreVector = data.PreferredGenreIds.ToDictionary(
                g => g,
                g => movieGenreIds.Contains(g) ? 1.0 : 0.0);

            var movieActorVector = data.PreferredActorIds.ToDictionary(
                a => a,
                a => movieActorIds.Contains(a) ? 1.0 : 0.0);

            var movieDirectorVector = data.PreferredDirectorIds.ToDictionary(
                d => d,
                d => (data.Movie.DirectorId == d) ? 1.0 : 0.0);


            // Calculate similarity for each dimension
            double genreSimilarity = CalculateCosineSimilarity(
                data.UserGenreVector,
                movieGenreVector);

            double actorSimilarity = CalculateCosineSimilarity(
                data.UserActorVector,
                movieActorVector);

            double directorSimilarity = CalculateCosineSimilarity(
                data.UserDirectorVector,
                movieDirectorVector);

            // Combine similarities with weights
            double finalSimilarity = CalculateWeightedSimilarity(
                genreSimilarity,
                actorSimilarity,
                directorSimilarity,
                genreWeight: 0.5,
                actorWeight: 0.3,
                directorWeight: 0.2
            );

            return finalSimilarity;
        }

        /// <summary>
        /// Calculates the weighted preference for a list of movies based on user interactions.
        /// </summary>
        /// <param name="movieIds">List of movie IDs to calculate weight for</param>
        /// <param name="preferences">User's preferences (ratings, favorites, upvotes, interests)</param>
        /// <returns>Weighted preference score (0 if no interactions)</returns>
        public static double CalculateWeightForMovies(IEnumerable<int> movieIds, UserPreferences preferences)
        {
            double totalWeight = 0;
            int interactionCount = 0;

            foreach (var movieId in movieIds)
            {
                double movieWeight = 0;
                bool shouldCount = false;

                if (preferences.RatedMovies.ContainsKey(movieId))
                {
                    var rating = preferences.RatedMovies[movieId];
                    movieWeight = (double)rating / 10.0;
                    shouldCount = true;
                }
                else if (preferences.FavoritedMovies.Contains(movieId))
                {
                    movieWeight = 0.9;
                    shouldCount = true;
                }
                else if (preferences.UpvotedMovies.Contains(movieId))
                {
                    movieWeight = 0.7;
                    shouldCount = true;
                }
                else if (preferences.InterestedMovies.Contains(movieId))
                {
                    movieWeight = 0.5;
                    shouldCount = true;
                }

                if (shouldCount)
                {
                    totalWeight += movieWeight;
                    interactionCount++;
                }
            }

            if (interactionCount == 0)
                return 0;

            double avgWeight = totalWeight / interactionCount;
            return avgWeight * Math.Log(interactionCount + 1);
        }
    }
}
