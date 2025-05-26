using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyTreeAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FamilyMembers",
                columns: table => new
                {
                    FamilyMemberId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateOfDeath = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<bool>(type: "bit", nullable: false),
                    FamilyTreeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyMembers", x => x.FamilyMemberId);
                });

            migrationBuilder.CreateTable(
                name: "Registrations",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfilePicture = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registrations", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "FamilyTree",
                columns: table => new
                {
                    FamilyTreeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FamilyTreeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FamilyTreeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    DateOfCreation = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyTree", x => x.FamilyTreeId);
                    table.ForeignKey(
                        name: "FK_FamilyTree_Registrations_UserID",
                        column: x => x.UserID,
                        principalTable: "Registrations",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Relationships",
                columns: table => new
                {
                    RelationshipId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromFamilyMemberId = table.Column<int>(type: "int", nullable: false),
                    ToFamilyMemberId = table.Column<int>(type: "int", nullable: false),
                    RelationshipType = table.Column<int>(type: "int", nullable: false),
                    FamilyTreeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relationships", x => x.RelationshipId);
                    table.ForeignKey(
                        name: "FK_Relationships_FamilyMembers_FromFamilyMemberId",
                        column: x => x.FromFamilyMemberId,
                        principalTable: "FamilyMembers",
                        principalColumn: "FamilyMemberId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Relationships_FamilyMembers_ToFamilyMemberId",
                        column: x => x.ToFamilyMemberId,
                        principalTable: "FamilyMembers",
                        principalColumn: "FamilyMemberId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Relationships_FamilyTree_FamilyTreeId",
                        column: x => x.FamilyTreeId,
                        principalTable: "FamilyTree",
                        principalColumn: "FamilyTreeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyTree_UserID",
                table: "FamilyTree",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_FamilyTreeId",
                table: "Relationships",
                column: "FamilyTreeId");

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_FromFamilyMemberId",
                table: "Relationships",
                column: "FromFamilyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_ToFamilyMemberId",
                table: "Relationships",
                column: "ToFamilyMemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Relationships");

            migrationBuilder.DropTable(
                name: "FamilyMembers");

            migrationBuilder.DropTable(
                name: "FamilyTree");

            migrationBuilder.DropTable(
                name: "Registrations");
        }
    }
}
