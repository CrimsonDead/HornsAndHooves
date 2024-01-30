using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using DBL.Models;

namespace DBL.ContextConfigurations
{
    public class ChatUserConfiguration : IEntityTypeConfiguration<ChatUser>
    {
        private readonly Guid[] _ids;

        public ChatUserConfiguration(Guid[] ids)
        {
            _ids = ids;
        }

        private void SeedData(EntityTypeBuilder<ChatUser> builder)
        {
        }

        public void Configure(EntityTypeBuilder<ChatUser> builder)
        {
            builder
                .HasKey(i => new { i.UserId, i.ChatId });

            builder
                .HasOne(i => i.User)
                .WithMany(i => i.ChatUsers)
                .OnDelete(DeleteBehavior.Cascade)
                .HasForeignKey(i => i.UserId);

            builder
                .HasOne(i => i.Chat)
                .WithMany(i => i.ChatUsers)
                .OnDelete(DeleteBehavior.Cascade)
                .HasForeignKey(i => i.ChatId);

            SeedData(builder);
        }
    }
}
