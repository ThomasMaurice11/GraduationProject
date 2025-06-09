using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GP.Migrations
{
    /// <inheritdoc />
    public partial class MarriageAdoption1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Slots_SlotId1",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_SlotId1",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "SlotId1",
                table: "Appointments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SlotId1",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_SlotId1",
                table: "Appointments",
                column: "SlotId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Slots_SlotId1",
                table: "Appointments",
                column: "SlotId1",
                principalTable: "Slots",
                principalColumn: "SlotId");
        }
    }
}
