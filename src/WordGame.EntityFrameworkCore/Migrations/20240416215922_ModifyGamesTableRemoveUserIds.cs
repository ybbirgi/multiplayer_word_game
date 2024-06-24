using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordGame.Migrations
{
    /// <inheritdoc />
    public partial class ModifyGamesTableRemoveUserIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiverUserId",
                schema: "WordGame",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "SenderUserId",
                schema: "WordGame",
                table: "Games");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReceiverUserId",
                schema: "WordGame",
                table: "Games",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SenderUserId",
                schema: "WordGame",
                table: "Games",
                type: "uuid",
                nullable: true);
        }
    }
}
