using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkkaTransfer.Data.Migrations
{
    /// <inheritdoc />
    public partial class SplitFileHeaderIntoSendAndReceive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FilePieces");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FileHeaders",
                table: "FileHeaders");

            migrationBuilder.RenameTable(
                name: "FileHeaders",
                newName: "ReceiveFileHeaders");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReceiveFileHeaders",
                table: "ReceiveFileHeaders",
                column: "ReceiveFileHeaderId");

            migrationBuilder.CreateTable(
                name: "ReceiveFilePieces",
                columns: table => new
                {
                    ReceiveFilePieceId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    ReceiveFileHeaderId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiveFilePieces", x => x.ReceiveFilePieceId);
                    table.ForeignKey(
                        name: "FK_ReceiveFilePieces_ReceiveFileHeaders_ReceiveFileHeaderId",
                        column: x => x.ReceiveFileHeaderId,
                        principalTable: "ReceiveFileHeaders",
                        principalColumn: "ReceiveFileHeaderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SendFileHeaders",
                columns: table => new
                {
                    SendFileHeaderId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", nullable: true),
                    PieceCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SendFileHeaders", x => x.SendFileHeaderId);
                });

            migrationBuilder.CreateTable(
                name: "SendFilePieces",
                columns: table => new
                {
                    SendFilePieceId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    SendFileHeaderId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SendFilePieces", x => x.SendFilePieceId);
                    table.ForeignKey(
                        name: "FK_SendFilePieces_SendFileHeaders_SendFileHeaderId",
                        column: x => x.SendFileHeaderId,
                        principalTable: "SendFileHeaders",
                        principalColumn: "SendFileHeaderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReceiveFilePieces_ReceiveFileHeaderId",
                table: "ReceiveFilePieces",
                column: "ReceiveFileHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_SendFilePieces_SendFileHeaderId",
                table: "SendFilePieces",
                column: "SendFileHeaderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReceiveFilePieces");

            migrationBuilder.DropTable(
                name: "SendFilePieces");

            migrationBuilder.DropTable(
                name: "SendFileHeaders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReceiveFileHeaders",
                table: "ReceiveFileHeaders");

            migrationBuilder.RenameTable(
                name: "ReceiveFileHeaders",
                newName: "FileHeaders");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FileHeaders",
                table: "FileHeaders",
                column: "ReceiveFileHeaderId");

            migrationBuilder.CreateTable(
                name: "FilePieces",
                columns: table => new
                {
                    FilePieceId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    FileHeaderId = table.Column<int>(type: "INTEGER", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    ReceiveFileHeaderId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilePieces", x => x.FilePieceId);
                    table.ForeignKey(
                        name: "FK_FilePieces_FileHeaders_ReceiveFileHeaderId",
                        column: x => x.ReceiveFileHeaderId,
                        principalTable: "FileHeaders",
                        principalColumn: "ReceiveFileHeaderId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FilePieces_ReceiveFileHeaderId",
                table: "FilePieces",
                column: "ReceiveFileHeaderId");
        }
    }
}
