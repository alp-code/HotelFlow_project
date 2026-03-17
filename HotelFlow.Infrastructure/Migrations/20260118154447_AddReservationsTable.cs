using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HousekeepingTasks_Users_HousekeepingId",
                table: "HousekeepingTasks");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "HousekeepingTasks");

            migrationBuilder.RenameColumn(
                name: "PricePerNightAtBooking",
                table: "Reservations",
                newName: "TotalPrice");

            migrationBuilder.RenameColumn(
                name: "HousekeepingId",
                table: "HousekeepingTasks",
                newName: "AssignedToUserId");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "HousekeepingTasks",
                newName: "Deadline");

            migrationBuilder.RenameIndex(
                name: "IX_HousekeepingTasks_HousekeepingId",
                table: "HousekeepingTasks",
                newName: "IX_HousekeepingTasks_AssignedToUserId");

            migrationBuilder.AddColumn<int>(
                name: "MaxGuests",
                table: "RoomTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckedInAt",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckedOutAt",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HousekeepingTaskId",
                table: "Reservations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Reservations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfGuests",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SpecialRequests",
                table: "Reservations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "HousekeepingTasks",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "HousekeepingTasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "HousekeepingTasks",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "HousekeepingTasks",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "HousekeepingTasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_HousekeepingTaskId",
                table: "Reservations",
                column: "HousekeepingTaskId",
                unique: true,
                filter: "[HousekeepingTaskId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_HousekeepingTasks_Users_AssignedToUserId",
                table: "HousekeepingTasks",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_HousekeepingTasks_HousekeepingTaskId",
                table: "Reservations",
                column: "HousekeepingTaskId",
                principalTable: "HousekeepingTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HousekeepingTasks_Users_AssignedToUserId",
                table: "HousekeepingTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_HousekeepingTasks_HousekeepingTaskId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_HousekeepingTaskId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "MaxGuests",
                table: "RoomTypes");

            migrationBuilder.DropColumn(
                name: "CheckedInAt",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CheckedOutAt",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "HousekeepingTaskId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "NumberOfGuests",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "SpecialRequests",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "HousekeepingTasks");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "HousekeepingTasks");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "HousekeepingTasks");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "HousekeepingTasks");

            migrationBuilder.RenameColumn(
                name: "TotalPrice",
                table: "Reservations",
                newName: "PricePerNightAtBooking");

            migrationBuilder.RenameColumn(
                name: "Deadline",
                table: "HousekeepingTasks",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "AssignedToUserId",
                table: "HousekeepingTasks",
                newName: "HousekeepingId");

            migrationBuilder.RenameIndex(
                name: "IX_HousekeepingTasks_AssignedToUserId",
                table: "HousekeepingTasks",
                newName: "IX_HousekeepingTasks_HousekeepingId");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Reservations",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "HousekeepingTasks",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "HousekeepingTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_HousekeepingTasks_Users_HousekeepingId",
                table: "HousekeepingTasks",
                column: "HousekeepingId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
