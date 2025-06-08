using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GP.Migrations
{
    /// <inheritdoc />
    public partial class AppoitmentsEnhance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Breed",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PetName",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PetType",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReasonForVisit",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Breed",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "PetName",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "PetType",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ReasonForVisit",
                table: "Appointments");
        }
    }
}
