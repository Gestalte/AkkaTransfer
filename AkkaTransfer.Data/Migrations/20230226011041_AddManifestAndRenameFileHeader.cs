using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkkaTransfer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddManifestAndRenameFileHeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FilePieces_FileHeaders_FileHeaderId",
                table: "FilePieces");

            migrationBuilder.DropIndex(
                name: "IX_FilePieces_FileHeaderId",
                table: "FilePieces");

            migrationBuilder.RenameColumn(
                name: "FileHeaderId",
                table: "FileHeaders",
                newName: "ReceiveFileHeaderId");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "FilePieces",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "ReceiveFileHeaderId",
                table: "FilePieces",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "FileHeaders",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateTable(
                name: "ManifestFiles",
                columns: table => new
                {
                    ManifestFileId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Filename = table.Column<string>(type: "TEXT", nullable: true),
                    FileHash = table.Column<string>(type: "TEXT", nullable: true),
                    ManifestId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManifestFiles", x => x.ManifestFileId);
                });

            migrationBuilder.CreateTable(
                name: "Manifests",
                columns: table => new
                {
                    ManifestId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manifests", x => x.ManifestId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FilePieces_ReceiveFileHeaderId",
                table: "FilePieces",
                column: "ReceiveFileHeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_FilePieces_FileHeaders_ReceiveFileHeaderId",
                table: "FilePieces",
                column: "ReceiveFileHeaderId",
                principalTable: "FileHeaders",
                principalColumn: "ReceiveFileHeaderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FilePieces_FileHeaders_ReceiveFileHeaderId",
                table: "FilePieces");

            migrationBuilder.DropTable(
                name: "ManifestFiles");

            migrationBuilder.DropTable(
                name: "Manifests");

            migrationBuilder.DropIndex(
                name: "IX_FilePieces_ReceiveFileHeaderId",
                table: "FilePieces");

            migrationBuilder.DropColumn(
                name: "ReceiveFileHeaderId",
                table: "FilePieces");

            migrationBuilder.RenameColumn(
                name: "ReceiveFileHeaderId",
                table: "FileHeaders",
                newName: "FileHeaderId");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "FilePieces",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "FileHeaders",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FilePieces_FileHeaderId",
                table: "FilePieces",
                column: "FileHeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_FilePieces_FileHeaders_FileHeaderId",
                table: "FilePieces",
                column: "FileHeaderId",
                principalTable: "FileHeaders",
                principalColumn: "FileHeaderId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
