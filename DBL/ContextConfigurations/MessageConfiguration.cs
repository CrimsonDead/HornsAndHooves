using DBL.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace DBL.ContextConfigurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        private Guid[] _ids;

        public MessageConfiguration(Guid[] ids)
        {
            _ids = ids;
        }

        private void SeedData(EntityTypeBuilder<Message> builder)
        {
        }

        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder
                .HasKey(i => new { i.Id });

            builder
                .HasOne(i => i.User)
                .WithMany(i => i.Messages)
                .OnDelete(DeleteBehavior.Cascade)
                .HasForeignKey(i => i.UserId);

            builder
                .HasOne(i => i.Chat)
                .WithMany(i => i.Messages)
                .OnDelete(DeleteBehavior.Cascade)
                .HasForeignKey(i => i.ChatId);

            SeedData(builder);
        }
    }
}
