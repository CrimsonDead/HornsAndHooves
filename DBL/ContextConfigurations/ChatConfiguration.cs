using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using DBL.Models;

namespace DBL.ContextConfigurations
{
    public class ChatConfiguration : IEntityTypeConfiguration<Chat>
    {
        private Guid[] _ids;

        public ChatConfiguration(Guid[] ids)
        {
            _ids = ids;
        }

        private void SeedData(EntityTypeBuilder<Chat> builder)
        {
        }

        public void Configure(EntityTypeBuilder<Chat> builder)
        {
            builder
                .HasKey(i => new { i.Id });

            builder
                .HasMany(i => i.Messages)
                .WithOne(i => i.Chat)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(i => i.ChatUsers)
                .WithOne(i => i.Chat)
                .OnDelete(DeleteBehavior.Cascade);

            SeedData(builder);
        }
    }
}
