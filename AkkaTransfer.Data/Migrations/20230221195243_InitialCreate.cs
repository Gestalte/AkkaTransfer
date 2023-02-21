using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkkaTransfer.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileHeaders",
                columns: table => new
                {
                    FileHeaderId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    PieceCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileHeaders", x => x.FileHeaderId);
                });

            migrationBuilder.CreateTable(
                name: "FilePieces",
                columns: table => new
                {
                    FilePieceId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    FileHeaderId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilePieces", x => x.FilePieceId);
                    table.ForeignKey(
                        name: "FK_FilePieces_FileHeaders_FileHeaderId",
                        column: x => x.FileHeaderId,
                        principalTable: "FileHeaders",
                        principalColumn: "FileHeaderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FilePieces_FileHeaderId",
                table: "FilePieces",
                column: "FileHeaderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FilePieces");

            migrationBuilder.DropTable(
                name: "FileHeaders");
        }
    }
}
