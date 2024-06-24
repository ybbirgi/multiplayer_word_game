using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordGame.Migrations
{
    /// <inheritdoc />
    public partial class ModiftGamesTableAddGameResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GameResultId",
                schema: "WordGame",
                table: "Games",
                newName: "GameStatusId");

            migrationBuilder.AddColumn<string>(
                name: "ReceiverUserGameResult",
                schema: "WordGame",
                table: "Games",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderUserGameResult",
                schema: "WordGame",
                table: "Games",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiverUserGameResult",
                schema: "WordGame",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "SenderUserGameResult",
                schema: "WordGame",
                table: "Games");

            migrationBuilder.RenameColumn(
                name: "GameStatusId",
                schema: "WordGame",
                table: "Games",
                newName: "GameResultId");
        }
    }
}
