using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardBloom.Migrations
{
    public partial class communities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommunityId",
                table: "Blooms",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Communities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Communities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Communities_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ModeratorCommunity",
                columns: table => new
                {
                    CommunityId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModeratorCommunity", x => new { x.CommunityId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ModeratorCommunity_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModeratorCommunity_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCommunity",
                columns: table => new
                {
                    CommunityId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCommunity", x => new { x.CommunityId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserCommunity_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCommunity_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Blooms_CommunityId",
                table: "Blooms",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_Communities_CreatedBy",
                table: "Communities",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ModeratorCommunity_UserId",
                table: "ModeratorCommunity",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCommunity_UserId",
                table: "UserCommunity",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Blooms_Communities_CommunityId",
                table: "Blooms",
                column: "CommunityId",
                principalTable: "Communities",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blooms_Communities_CommunityId",
                table: "Blooms");

            migrationBuilder.DropTable(
                name: "ModeratorCommunity");

            migrationBuilder.DropTable(
                name: "UserCommunity");

            migrationBuilder.DropTable(
                name: "Communities");

            migrationBuilder.DropIndex(
                name: "IX_Blooms_CommunityId",
                table: "Blooms");

            migrationBuilder.DropColumn(
                name: "CommunityId",
                table: "Blooms");
        }
    }
}
