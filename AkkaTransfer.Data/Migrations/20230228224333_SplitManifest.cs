using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkkaTransfer.Data.Migrations
{
    /// <inheritdoc />
    public partial class SplitManifest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ManifestFiles");

            migrationBuilder.DropTable(
                name: "Manifests");

            migrationBuilder.CreateTable(
                name: "ReceiveManifests",
                columns: table => new
                {
                    ReceiveManifestId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiveManifests", x => x.ReceiveManifestId);
                });

            migrationBuilder.CreateTable(
                name: "SendManifests",
                columns: table => new
                {
                    SendManifestId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SendManifests", x => x.SendManifestId);
                });

            migrationBuilder.CreateTable(
                name: "ReceiveManifestFiles",
                columns: table => new
                {
                    ReceiveManifestFileId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Filename = table.Column<string>(type: "TEXT", nullable: true),
                    FileHash = table.Column<string>(type: "TEXT", nullable: true),
                    ReceiveManifestId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiveManifestFiles", x => x.ReceiveManifestFileId);
                    table.ForeignKey(
                        name: "FK_ReceiveManifestFiles_ReceiveManifests_ReceiveManifestId",
                        column: x => x.ReceiveManifestId,
                        principalTable: "ReceiveManifests",
                        principalColumn: "ReceiveManifestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SendManifestFiles",
                columns: table => new
                {
                    SendManifestFileId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Filename = table.Column<string>(type: "TEXT", nullable: true),
                    FileHash = table.Column<string>(type: "TEXT", nullable: true),
                    SendManifestId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SendManifestFiles", x => x.SendManifestFileId);
                    table.ForeignKey(
                        name: "FK_SendManifestFiles_SendManifests_SendManifestId",
                        column: x => x.SendManifestId,
                        principalTable: "SendManifests",
                        principalColumn: "SendManifestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReceiveManifestFiles_ReceiveManifestId",
                table: "ReceiveManifestFiles",
                column: "ReceiveManifestId");

            migrationBuilder.CreateIndex(
                name: "IX_SendManifestFiles_SendManifestId",
                table: "SendManifestFiles",
                column: "SendManifestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReceiveManifestFiles");

            migrationBuilder.DropTable(
                name: "SendManifestFiles");

            migrationBuilder.DropTable(
                name: "ReceiveManifests");

            migrationBuilder.DropTable(
                name: "SendManifests");

            migrationBuilder.CreateTable(
                name: "ManifestFiles",
                columns: table => new
                {
                    ManifestFileId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileHash = table.Column<string>(type: "TEXT", nullable: true),
                    Filename = table.Column<string>(type: "TEXT", nullable: true),
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
        }
    }
}
