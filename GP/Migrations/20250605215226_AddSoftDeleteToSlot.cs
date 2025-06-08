using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GP.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Disabled",
                table: "Slots",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DisabledDate",
                table: "Slots",
                type: "datetime2",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Slots_SlotId1",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_SlotId1",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Disabled",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "DisabledDate",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "SlotId1",
                table: "Appointments");
        }
    }
}
