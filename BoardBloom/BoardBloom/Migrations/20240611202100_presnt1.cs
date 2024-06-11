using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardBloom.Migrations
{
    public partial class presnt1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BloomBoards_Blooms_BloomId",
                table: "BloomBoards");

            migrationBuilder.DropForeignKey(
                name: "FK_BloomBoards_Boards_BoardId",
                table: "BloomBoards");

            migrationBuilder.AddForeignKey(
                name: "FK_BloomBoards_Blooms_BloomId",
                table: "BloomBoards",
                column: "BloomId",
                principalTable: "Blooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BloomBoards_Boards_BoardId",
                table: "BloomBoards",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BloomBoards_Blooms_BloomId",
                table: "BloomBoards");

            migrationBuilder.DropForeignKey(
                name: "FK_BloomBoards_Boards_BoardId",
                table: "BloomBoards");

            migrationBuilder.AddForeignKey(
                name: "FK_BloomBoards_Blooms_BloomId",
                table: "BloomBoards",
                column: "BloomId",
                principalTable: "Blooms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BloomBoards_Boards_BoardId",
                table: "BloomBoards",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id");
        }
    }
}
