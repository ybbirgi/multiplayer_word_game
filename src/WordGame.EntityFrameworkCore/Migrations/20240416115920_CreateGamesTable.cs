using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordGame.Migrations
{
    /// <inheritdoc />
    public partial class CreateGamesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Games",
                schema: "WordGame",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreGameInfoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentGameId = table.Column<Guid>(type: "uuid", nullable: true),
                    SenderUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceiverUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SenderUserAnswer = table.Column<string>(type: "text", nullable: true),
                    ReceiverUserAnswer = table.Column<string>(type: "text", nullable: true),
                    SenderUserGuessCount = table.Column<int>(type: "integer", nullable: false),
                    ReceiverUserGuessCount = table.Column<int>(type: "integer", nullable: false),
                    GameResultId = table.Column<int>(type: "integer", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Games_Games_ParentGameId",
                        column: x => x.ParentGameId,
                        principalSchema: "WordGame",
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Games_PreGameInfos_PreGameInfoId",
                        column: x => x.PreGameInfoId,
                        principalSchema: "WordGame",
                        principalTable: "PreGameInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Games_ParentGameId",
                schema: "WordGame",
                table: "Games",
                column: "ParentGameId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_PreGameInfoId",
                schema: "WordGame",
                table: "Games",
                column: "PreGameInfoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Games",
                schema: "WordGame");
        }
    }
}
