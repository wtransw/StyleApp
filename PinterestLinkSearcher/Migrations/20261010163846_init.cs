using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PinterestLinkSearcher.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PinterestPins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    datetime = table.Column<DateTime>(type: "datetime", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(2048)", nullable: false),
                    Board = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UrlHash = table.Column<byte[]>(type: "binary(32)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PinterestPins", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PinterestPins_UrlHash",
                table: "PinterestPins",
                column: "UrlHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PinterestPins");
        }
    }
}
