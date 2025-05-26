using FamilyTreeAPI.Models;
using LoginAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeAPI
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<FamilyMember> FamilyMembers { get; set; }
        public DbSet<Relationship> Relationships { get; set; }
        public DbSet<FamilyTree> FamilyTrees { get; set; }
        public DbSet<FamilyTreeUserRole> FamilyTreeUserRoles { get; set; }
        public DbSet<FamilyTreeInvite> FamilyTreeInvites { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure FamilyTree
            builder.Entity<FamilyTree>(entity =>
            {
                entity.ToTable("FamilyTrees");
                entity.HasKey(ft => ft.FamilyTreeId);
                entity.Property(ft => ft.FamilyTreeId);  
                entity.Property(ft => ft.FamilyTreeName).IsRequired().HasMaxLength(200);
                entity.Property(ft => ft.IsPublic).IsRequired();
                entity.Property(ft => ft.OwnerId).IsRequired().HasMaxLength(450);

                entity.HasMany(ft => ft.FamilyMembers)
                    .WithOne(fm => fm.Tree)
                    .HasForeignKey(fm => fm.FamilyTreeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure FamilyMember
            builder.Entity<FamilyMember>(entity =>
            {
                entity.ToTable("FamilyMembers");
                entity.HasKey(fm => fm.FamilyMemberId);
                entity.Property(fm => fm.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(fm => fm.LastName).IsRequired().HasMaxLength(100);

                entity.HasOne(p => p.Mother)
                    .WithMany(p => p.ChildrenAsMother)
                    .HasForeignKey(p => p.MotherId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(p => p.Father)
                    .WithMany(p => p.ChildrenAsFather)
                    .HasForeignKey(p => p.FatherId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(p => p.MotherId);
                entity.HasIndex(p => p.FatherId);
                entity.HasIndex(p => p.FamilyTreeId);
            });

            // Configure Relationships
            builder.Entity<Relationship>(entity =>
            {
                entity.ToTable("Relationships");
                entity.HasKey(r => r.RelationshipId);

                entity.Property(r => r.RelationshipType)
                    .HasConversion<string>()
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(r => r.FromPerson)
                    .WithMany(p => p.RelationshipsAsFromPerson)
                    .HasForeignKey(r => r.FromPersonId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(r => r.ToPerson)
                    .WithMany(p => p.RelationshipsAsToPerson)
                    .HasForeignKey(r => r.ToPersonId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure FamilyTreeUserRole
            builder.Entity<FamilyTreeUserRole>(entity =>
            {
                entity.ToTable("FamilyTreeUserRoles");
                entity.HasKey(ut => ut.Id);
                entity.Property(ut => ut.Id).ValueGeneratedOnAdd();
                entity.Property(ut => ut.FamilyTreeId).IsRequired().HasMaxLength(450);
                entity.Property(ut => ut.UserId).IsRequired().HasMaxLength(450);
                entity.Property(ut => ut.Role).IsRequired().HasMaxLength(50);

                entity.HasOne<FamilyTree>()
                    .WithMany()
                    .HasForeignKey(ut => ut.FamilyTreeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure FamilyTreeInvite
            builder.Entity<FamilyTreeInvite>(entity =>
            {
                entity.ToTable("FamilyTreeInvites");
                entity.HasKey(fi => fi.Id);
                entity.Property(fi => fi.Id).ValueGeneratedOnAdd();
                entity.Property(fi => fi.Token).IsRequired().HasMaxLength(450);
                entity.Property(fi => fi.FamilyTreeId).IsRequired(); 
                entity.Property(fi => fi.Role).IsRequired().HasMaxLength(50);
                entity.Property(fi => fi.ExpirationDate).IsRequired();
                entity.Property(fi => fi.IsUsed).IsRequired();
                entity.Property(fi => fi.RecipientEmail).HasMaxLength(256);

                //define the foreign key relationship
                entity.HasOne<FamilyTree>()
                    .WithMany()
                    .HasForeignKey(fi => fi.FamilyTreeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(fi => fi.Token).IsUnique();
            });
        }
    }
}