using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GP.Migrations
{
    /// <inheritdoc />
    public partial class AnimalMarriage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HealthStatus",
                table: "Pets",
                newName: "HealthIssues");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Pets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AnimalMarriageRequests",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderPetId = table.Column<int>(type: "int", nullable: false),
                    ReceiverAnimalId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalMarriageRequests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_AnimalMarriageRequests_Animals_ReceiverAnimalId",
                        column: x => x.ReceiverAnimalId,
                        principalTable: "Animals",
                        principalColumn: "AnimalId");
                    table.ForeignKey(
                        name: "FK_AnimalMarriageRequests_Pets_SenderPetId",
                        column: x => x.SenderPetId,
                        principalTable: "Pets",
                        principalColumn: "PetId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnimalMarriageRequests_ReceiverAnimalId",
                table: "AnimalMarriageRequests",
                column: "ReceiverAnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalMarriageRequests_SenderPetId",
                table: "AnimalMarriageRequests",
                column: "SenderPetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnimalMarriageRequests");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Pets");

            migrationBuilder.RenameColumn(
                name: "HealthIssues",
                table: "Pets",
                newName: "HealthStatus");
        }
    }
}
