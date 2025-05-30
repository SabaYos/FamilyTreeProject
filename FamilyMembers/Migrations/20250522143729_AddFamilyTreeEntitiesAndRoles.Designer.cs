﻿// <auto-generated />
using System;
using FamilyTreeAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace FamilyTreeAPI.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250522143729_AddFamilyTreeEntitiesAndRoles")]
    partial class AddFamilyTreeEntitiesAndRoles
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("FamilyTreeAPI.Models.FamilyMember", b =>
                {
                    b.Property<int>("FamilyMemberId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("FamilyMemberId"));

                    b.Property<DateTime?>("DateOfBirth")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateOfDeath")
                        .HasColumnType("datetime2");

                    b.Property<int>("FamilyTreeId")
                        .HasColumnType("int");

                    b.Property<int?>("FatherId")
                        .HasColumnType("int");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<bool>("Gender")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int?>("MotherId")
                        .HasColumnType("int");

                    b.HasKey("FamilyMemberId");

                    b.HasIndex("FamilyTreeId");

                    b.HasIndex("FatherId");

                    b.HasIndex("MotherId");

                    b.ToTable("FamilyMembers");
                });

            modelBuilder.Entity("FamilyTreeAPI.Models.FamilyTree", b =>
                {
                    b.Property<int>("FamilyTreeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("FamilyTreeId"));

                    b.Property<string>("FamilyTreeName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsPublic")
                        .HasColumnType("bit");

                    b.Property<string>("OwnerId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("FamilyTreeId");

                    b.ToTable("FamilyTrees");
                });

            modelBuilder.Entity("FamilyTreeAPI.Models.FamilyTreeUserRole", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("FamilyTreeId")
                        .HasColumnType("int");

                    b.Property<string>("FamilyTreeName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(450)
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("FamilyTreeId");

                    b.ToTable("FamilyTreeUserRoles", (string)null);
                });

            modelBuilder.Entity("FamilyTreeAPI.Models.Relationship", b =>
                {
                    b.Property<int>("RelationshipId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RelationshipId"));

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("FromPersonId")
                        .HasColumnType("int");

                    b.Property<string>("RelationshipType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("StartDate")
                        .IsRequired()
                        .HasColumnType("datetime2");

                    b.Property<int>("ToPersonId")
                        .HasColumnType("int");

                    b.HasKey("RelationshipId");

                    b.HasIndex("FromPersonId");

                    b.HasIndex("ToPersonId");

                    b.ToTable("Relationships");
                });

            modelBuilder.Entity("FamilyTreeAPI.Models.FamilyMember", b =>
                {
                    b.HasOne("FamilyTreeAPI.Models.FamilyTree", "Tree")
                        .WithMany("FamilyMembers")
                        .HasForeignKey("FamilyTreeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FamilyTreeAPI.Models.FamilyMember", "Father")
                        .WithMany("ChildrenAsFather")
                        .HasForeignKey("FatherId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("FamilyTreeAPI.Models.FamilyMember", "Mother")
                        .WithMany("ChildrenAsMother")
                        .HasForeignKey("MotherId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("Father");

                    b.Navigation("Mother");

                    b.Navigation("Tree");
                });

            modelBuilder.Entity("FamilyTreeAPI.Models.FamilyTreeUserRole", b =>
                {
                    b.HasOne("FamilyTreeAPI.Models.FamilyTree", null)
                        .WithMany()
                        .HasForeignKey("FamilyTreeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("FamilyTreeAPI.Models.Relationship", b =>
                {
                    b.HasOne("FamilyTreeAPI.Models.FamilyMember", "FromPerson")
                        .WithMany("RelationshipsAsFromPerson")
                        .HasForeignKey("FromPersonId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("FamilyTreeAPI.Models.FamilyMember", "ToPerson")
                        .WithMany("RelationshipsAsToPerson")
                        .HasForeignKey("ToPersonId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("FromPerson");

                    b.Navigation("ToPerson");
                });

            modelBuilder.Entity("FamilyTreeAPI.Models.FamilyMember", b =>
                {
                    b.Navigation("ChildrenAsFather");

                    b.Navigation("ChildrenAsMother");

                    b.Navigation("RelationshipsAsFromPerson");

                    b.Navigation("RelationshipsAsToPerson");
                });

            modelBuilder.Entity("FamilyTreeAPI.Models.FamilyTree", b =>
                {
                    b.Navigation("FamilyMembers");
                });
#pragma warning restore 612, 618
        }
    }
}
