using Core.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public class AppDbContext: IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<ApplicationUser> ApplicationUsers {  get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Interest> Interests { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieGenre> MovieGenres { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<MovieActor> MovieActors { get; set; }
        public DbSet<Director> Directors { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Movie>(entity =>
            {
                entity.Property(m => m.RowVersion)
                    .IsRowVersion();

                entity.HasOne(m => m.Director)
                      .WithMany(d => d.Movies)
                      .HasForeignKey(m => m.DirectorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(m => new { m.Title, m.ReleaseYear, m.DirectorId })
                    .IsUnique();
            });

            List<Genre> genres = new List<Genre>()
            {
                new Genre(){ Id = 1, Name = "Action"},
                new Genre(){ Id = 2, Name = "Comedy"},
                new Genre(){ Id = 3, Name = "Horror"},
                new Genre(){ Id = 4, Name = "Drama"},
                new Genre(){ Id = 5, Name = "Western"},
                new Genre(){ Id = 6, Name = "Science fiction"}
            };
            modelBuilder.Entity<Genre>()
                .HasData(genres);

            modelBuilder.Entity<MovieGenre>(entity =>
            {
                entity.HasIndex(mg => new { mg.MovieId, mg.GenreId })
                      .IsUnique();

                entity.HasIndex(mg => mg.MovieId);
                entity.HasIndex(mg => mg.GenreId);

                entity.HasOne(mg => mg.Movie)
                      .WithMany(m => m.MovieGenres)
                      .HasForeignKey(mg => mg.MovieId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(mg => mg.Genre)
                      .WithMany(g => g.MovieGenres)
                      .HasForeignKey(mg => mg.GenreId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Rating>(entity =>
            {
                entity.HasIndex(r => new { r.UserId, r.MovieId })
                    .IsUnique();

                entity.HasIndex(r => r.MovieId);
                entity.HasIndex(r => r.UserId);

                entity.HasOne(r => r.Movie)
                      .WithMany( m => m.Ratings)
                      .HasForeignKey(r => r.MovieId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Actor>(entity =>
            {
                entity.HasIndex(a => a.Name)
                    .IsUnique();
            });

            modelBuilder.Entity<MovieActor>(entity =>
            {
                entity.HasIndex(ma => ma.MovieId);
                entity.HasIndex(ma => ma.ActorId);
            });

            modelBuilder.Entity<MovieActor>()
                .HasOne(ma => ma.Movie)
                .WithMany(m => m.MovieActors)
                .HasForeignKey(ma => ma.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MovieActor>()
                .HasOne(ma => ma.Actor)
                .WithMany(a => a.MovieActors)
                .HasForeignKey(ma => ma.ActorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Director>(entity =>
            {
                entity.HasIndex(d => d.Name)
                    .IsUnique();
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasIndex(c => c.MovieId);
                entity.HasIndex(c => c.UserId);

                entity.HasIndex(c => c.ParentCommentId);

                entity.HasIndex(c => new { c.ParentCommentId, c.CreatedAt });

                entity.HasOne(c => c.Movie)
                      .WithMany(m => m.Comments)
                      .HasForeignKey(c => c.MovieId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.ParentComment)
                    .WithMany(c => c.Replies)
                    .HasForeignKey(c => c.ParentCommentId)
                    .OnDelete(DeleteBehavior.ClientCascade);
            });

            modelBuilder.Entity<Favorite>(entity =>
            {
                entity.HasIndex(f => new { f.UserId, f.MovieId })
                      .IsUnique();

                entity.HasIndex(f => f.UserId);
                entity.HasIndex(f => f.MovieId);

                entity.HasOne(f => f.Movie)
                      .WithMany(m => m.Favorites)
                      .HasForeignKey(f => f.MovieId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Interest>(entity =>
            {
                entity.HasIndex(i => new { i.UserId, i.MovieId })
                      .IsUnique();

                entity.HasIndex(i => i.UserId);
                entity.HasIndex(i => i.MovieId);

                entity.HasOne(i => i.Movie)
                      .WithMany(m => m.Interests)
                      .HasForeignKey(i => i.MovieId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Vote>(entity =>
            {
                entity.HasIndex(v => new { v.UserId, v.MovieId })
                    .IsUnique();

                // Index on MovieId - For getting all votes for a movie (popularity calculation)
                entity.HasIndex(v => v.MovieId);

                // Index on UserId - For collaborative filtering (get all votes by a user)
                entity.HasIndex(v => v.UserId);

                // Composite index on (MovieId, VoteType) - For aggregating upvotes/downvotes per movie
                // This is crucial for recommendation: "Get all upvotes for movie X"
                entity.HasIndex(v => new { v.MovieId, v.VoteType });

                // Composite index on (UserId, VoteType) - For user preference analysis
                // Useful for: "Get all movies this user upvoted"
                entity.HasIndex(v => new { v.UserId, v.VoteType });

                // Index on CreatedAt - For time-based recommendations (trending, recent activity)
                entity.HasIndex(v => v.CreatedAt);

                entity.HasOne(v => v.Movie)
                      .WithMany(m => m.Votes)
                      .HasForeignKey(v => v.MovieId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
