using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardBloom.Migrations
{
    public partial class init_24_4_11 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BloomBoards_Blooms_BoardId",
                table: "BloomBoards");

            migrationBuilder.DropForeignKey(
                name: "FK_BloomBoards_Boards_BoardId",
                table: "BloomBoards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BloomBoards",
                table: "BloomBoards");

            migrationBuilder.AlterColumn<int>(
                name: "BoardId",
                table: "BloomBoards",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "BloomId",
                table: "BloomBoards",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BloomBoards",
                table: "BloomBoards",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BloomBoards_BloomId",
                table: "BloomBoards",
                column: "BloomId");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BloomBoards_Blooms_BloomId",
                table: "BloomBoards");

            migrationBuilder.DropForeignKey(
                name: "FK_BloomBoards_Boards_BoardId",
                table: "BloomBoards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BloomBoards",
                table: "BloomBoards");

            migrationBuilder.DropIndex(
                name: "IX_BloomBoards_BloomId",
                table: "BloomBoards");

            migrationBuilder.AlterColumn<int>(
                name: "BoardId",
                table: "BloomBoards",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BloomId",
                table: "BloomBoards",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BloomBoards",
                table: "BloomBoards",
                columns: new[] { "Id", "BloomId", "BoardId" });

            migrationBuilder.AddForeignKey(
                name: "FK_BloomBoards_Blooms_BoardId",
                table: "BloomBoards",
                column: "BoardId",
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
    }
}
