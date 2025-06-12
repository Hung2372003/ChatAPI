using FakeFacebook.Commom;
using FakeFacebook.Models;
using Microsoft.EntityFrameworkCore;

namespace FakeFacebook.Data
{
    public class FakeFacebookDbContext : DbContext
    {
        public FakeFacebookDbContext(DbContextOptions<FakeFacebookDbContext> options) : base(options) { }

        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<UserInformation> UserInformations { get; set; }
        public DbSet<FriendDouble> FriendDoubles { get; set; }
        public DbSet<FileInformation> FileInformations { get; set; }
        public DbSet<ChatContent> ChatContents { get; set; }
        public DbSet<FileChat> FileChats { get; set; }
        public DbSet<ChatGroups> ChatGroups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<Posts> Posts { get; set; }
        public DbSet<PostComment> PostComments { get; set; }
        public DbSet<FeelingPost> FeelingPosts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Thay đổi tên bảng
                entity.SetTableName(entity?.GetTableName()?.ToSnakeCase(true));

                // Thay đổi tên cột
                foreach (var property in entity!.GetProperties())
                {
                    property.SetColumnName(property.Name.ToSnakeCase(true));
                }

                // Thay đổi tên khóa
                foreach (var key in entity.GetKeys())
                {
                    key.SetName(key?.GetName()?.ToSnakeCase(true));
                }

                // Thay đổi tên khóa ngoại
                foreach (var foreignKey in entity.GetForeignKeys())
                {
                    foreignKey.SetConstraintName(foreignKey?.GetConstraintName()?.ToSnakeCase(true));
                }

                // Thay đổi tên chỉ mục
                foreach (var index in entity.GetIndexes())
                {
                    index.SetDatabaseName(index?.GetDatabaseName()?.ToSnakeCase(true));
                }
            }
        }

    }
}
