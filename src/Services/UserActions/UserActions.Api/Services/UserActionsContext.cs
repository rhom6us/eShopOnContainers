using Microsoft.EntityFrameworkCore;

namespace UserActions.Api.Services {
    public class UserActionsContext : DbContext {
        public DbSet<AccessTokenData> AccessTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);


            builder.Entity<AccessTokenData>()
                .HasKey(p => new {p.Provider, p.SurrogateId});
            builder.Entity<AccessTokenData>()
                .Property(p => p.UserId)
                .IsRequired();

            builder.Entity<AccessTokenData>()
                .Property(p => p.AccessToken)
                .IsRequired();

        }
    }
}