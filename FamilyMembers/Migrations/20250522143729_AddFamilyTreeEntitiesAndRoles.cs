using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyTreeAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddFamilyTreeEntitiesAndRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FamilyMembers_FamilyMembers_FatherId",
                table: "FamilyMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_FamilyMembers_FamilyMembers_MotherId",
                table: "FamilyMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Relationships_FamilyMembers_FromPersonId",
                table: "Relationships");

            migrationBuilder.DropForeignKey(
                name: "FK_Relationships_FamilyMembers_ToPersonId",
                table: "Relationships");

            migrationBuilder.AlterColumn<string>(
                name: "RelationshipType",
                table: "Relationships",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "FamilyMembers",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateTable(
                name: "FamilyTreeUserRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FamilyTreeId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FamilyTreeName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyTreeUserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyTreeUserRoles_FamilyTrees_FamilyTreeId",
                        column: x => x.FamilyTreeId,
                        principalTable: "FamilyTrees",
                        principalColumn: "FamilyTreeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyTreeUserRoles_FamilyTreeId",
                table: "FamilyTreeUserRoles",
                column: "FamilyTreeId");

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyMembers_FamilyMembers_FatherId",
                table: "FamilyMembers",
                column: "FatherId",
                principalTable: "FamilyMembers",
                principalColumn: "FamilyMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyMembers_FamilyMembers_MotherId",
                table: "FamilyMembers",
                column: "MotherId",
                principalTable: "FamilyMembers",
                principalColumn: "FamilyMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Relationships_FamilyMembers_FromPersonId",
                table: "Relationships",
                column: "FromPersonId",
                principalTable: "FamilyMembers",
                principalColumn: "FamilyMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Relationships_FamilyMembers_ToPersonId",
                table: "Relationships",
                column: "ToPersonId",
                principalTable: "FamilyMembers",
                principalColumn: "FamilyMemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FamilyMembers_FamilyMembers_FatherId",
                table: "FamilyMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_FamilyMembers_FamilyMembers_MotherId",
                table: "FamilyMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Relationships_FamilyMembers_FromPersonId",
                table: "Relationships");

            migrationBuilder.DropForeignKey(
                name: "FK_Relationships_FamilyMembers_ToPersonId",
                table: "Relationships");

            migrationBuilder.DropTable(
                name: "FamilyTreeUserRoles");

            migrationBuilder.AlterColumn<int>(
                name: "RelationshipType",
                table: "Relationships",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "FamilyMembers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyMembers_FamilyMembers_FatherId",
                table: "FamilyMembers",
                column: "FatherId",
                principalTable: "FamilyMembers",
                principalColumn: "FamilyMemberId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyMembers_FamilyMembers_MotherId",
                table: "FamilyMembers",
                column: "MotherId",
                principalTable: "FamilyMembers",
                principalColumn: "FamilyMemberId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Relationships_FamilyMembers_FromPersonId",
                table: "Relationships",
                column: "FromPersonId",
                principalTable: "FamilyMembers",
                principalColumn: "FamilyMemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Relationships_FamilyMembers_ToPersonId",
                table: "Relationships",
                column: "ToPersonId",
                principalTable: "FamilyMembers",
                principalColumn: "FamilyMemberId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
