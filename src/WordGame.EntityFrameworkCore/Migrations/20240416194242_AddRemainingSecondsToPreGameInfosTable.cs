using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordGame.Migrations
{
    /// <inheritdoc />
    public partial class AddRemainingSecondsToPreGameInfosTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReceiverUserRemainingSeconds",
                schema: "WordGame",
                table: "PreGameInfos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SenderUserRemainingSeconds",
                schema: "WordGame",
                table: "PreGameInfos",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiverUserRemainingSeconds",
                schema: "WordGame",
                table: "PreGameInfos");

            migrationBuilder.DropColumn(
                name: "SenderUserRemainingSeconds",
                schema: "WordGame",
                table: "PreGameInfos");
        }
    }
}
