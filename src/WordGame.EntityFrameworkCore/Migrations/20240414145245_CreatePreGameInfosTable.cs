using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordGame.Migrations
{
    /// <inheritdoc />
    public partial class CreatePreGameInfosTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChannelId",
                schema: "WordGame",
                table: "ChallengeRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PreGameInfos",
                schema: "WordGame",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChallengeRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    GameTypeId = table.Column<int>(type: "integer", nullable: false),
                    WordLength = table.Column<int>(type: "integer", nullable: false),
                    SenderPlayerWord = table.Column<string>(type: "text", nullable: true),
                    ReceiverPlayerWord = table.Column<string>(type: "text", nullable: true),
                    PreGameStatusId = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreGameInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreGameInfos_ChallengeRequests_ChallengeRequestId",
                        column: x => x.ChallengeRequestId,
                        principalSchema: "WordGame",
                        principalTable: "ChallengeRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PreGameInfos_ChallengeRequestId",
                schema: "WordGame",
                table: "PreGameInfos",
                column: "ChallengeRequestId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PreGameInfos",
                schema: "WordGame");

            migrationBuilder.DropColumn(
                name: "ChannelId",
                schema: "WordGame",
                table: "ChallengeRequests");
        }
    }
}
