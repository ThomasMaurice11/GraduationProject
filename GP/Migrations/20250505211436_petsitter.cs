using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GP.Migrations
{
    /// <inheritdoc />
    public partial class petsitter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Age",
                table: "PetSitterRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Breed",
                table: "PetSitterRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "PetSitterRequests");

            migrationBuilder.DropColumn(
                name: "Breed",
                table: "PetSitterRequests");
        }
    }
}
