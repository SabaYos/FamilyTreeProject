using Microsoft.EntityFrameworkCore.Migrations;

namespace FamilyTreeAPI.Migrations
{
    public partial class FixMissingColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Relationships",
                columns: table => new
                {
                    RelationshipId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromPersonId = table.Column<int>(type: "int", nullable: false),
                    ToPersonId = table.Column<int>(type: "int", nullable: false),
                    RelationshipType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relationships", x => x.RelationshipId);
                    table.ForeignKey(
                        name: "FK_Relationships_FamilyMembers_FromPersonId",
                        column: x => x.FromPersonId,
                        principalTable: "FamilyMembers",
                        principalColumn: "FamilyMemberId",
                        onDelete: ReferentialAction.NoAction); // Changed to NoAction
                    table.ForeignKey(
                        name: "FK_Relationships_FamilyMembers_ToPersonId",
                        column: x => x.ToPersonId,
                        principalTable: "FamilyMembers",
                        principalColumn: "FamilyMemberId",
                        onDelete: ReferentialAction.NoAction); // Changed to NoAction
                });

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_FromPersonId",
                table: "Relationships",
                column: "FromPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_ToPersonId",
                table: "Relationships",
                column: "ToPersonId");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "FamilyMembers");

            migrationBuilder.AddColumn<int>(
                name: "MotherId",
                table: "FamilyMembers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FatherId",
                table: "FamilyMembers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FamilyMembers_MotherId",
                table: "FamilyMembers",
                column: "MotherId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyMembers_FatherId",
                table: "FamilyMembers",
                column: "FatherId");

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyMembers_FamilyMembers_MotherId",
                table: "FamilyMembers",
                column: "MotherId",
                principalTable: "FamilyMembers",
                principalColumn: "FamilyMemberId",
                onDelete: ReferentialAction.NoAction); // Changed to NoAction

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyMembers_FamilyMembers_FatherId",
                table: "FamilyMembers",
                column: "FatherId",
                principalTable: "FamilyMembers",
                principalColumn: "FamilyMemberId",
                onDelete: ReferentialAction.NoAction); // Changed to NoAction
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Relationships_FamilyMembers_FromPersonId",
                table: "Relationships");

            migrationBuilder.DropForeignKey(
                name: "FK_Relationships_FamilyMembers_ToPersonId",
                table: "Relationships");

            migrationBuilder.DropForeignKey(
                name: "FK_FamilyMembers_FamilyMembers_MotherId",
                table: "FamilyMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_FamilyMembers_FamilyMembers_FatherId",
                table: "FamilyMembers");

            migrationBuilder.DropTable(
                name: "Relationships");

            migrationBuilder.DropIndex(
                name: "IX_FamilyMembers_MotherId",
                table: "FamilyMembers");

            migrationBuilder.DropIndex(
                name: "IX_FamilyMembers_FatherId",
                table: "FamilyMembers");

            migrationBuilder.DropColumn(
                name: "MotherId",
                table: "FamilyMembers");

            migrationBuilder.DropColumn(
                name: "FatherId",
                table: "FamilyMembers");

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "FamilyMembers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}