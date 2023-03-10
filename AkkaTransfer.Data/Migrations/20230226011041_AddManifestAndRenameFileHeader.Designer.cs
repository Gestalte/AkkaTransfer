// <auto-generated />
using System;
using AkkaTransfer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AkkaTransfer.Data.Migrations
{
    [DbContext(typeof(ReceiveDbContext))]
    [Migration("20230226011041_AddManifestAndRenameFileHeader")]
    partial class AddManifestAndRenameFileHeader
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.3");

            modelBuilder.Entity("AkkaTransfer.Data.FilePiece", b =>
                {
                    b.Property<int>("FilePieceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Content")
                        .HasColumnType("TEXT");

                    b.Property<int>("FileHeaderId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Position")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ReceiveFileHeaderId")
                        .HasColumnType("INTEGER");

                    b.HasKey("FilePieceId");

                    b.HasIndex("ReceiveFileHeaderId");

                    b.ToTable("FilePieces");
                });

            modelBuilder.Entity("AkkaTransfer.Data.Manifest", b =>
                {
                    b.Property<int>("ManifestId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("TEXT");

                    b.HasKey("ManifestId");

                    b.ToTable("Manifests");
                });

            modelBuilder.Entity("AkkaTransfer.Data.ManifestFile", b =>
                {
                    b.Property<int>("ManifestFileId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("FileHash")
                        .HasColumnType("TEXT");

                    b.Property<string>("Filename")
                        .HasColumnType("TEXT");

                    b.Property<int>("ManifestId")
                        .HasColumnType("INTEGER");

                    b.HasKey("ManifestFileId");

                    b.ToTable("ManifestFiles");
                });

            modelBuilder.Entity("AkkaTransfer.Data.ReceiveFileHeader", b =>
                {
                    b.Property<int>("ReceiveFileHeaderId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("FileName")
                        .HasColumnType("TEXT");

                    b.Property<int>("PieceCount")
                        .HasColumnType("INTEGER");

                    b.HasKey("ReceiveFileHeaderId");

                    b.ToTable("FileHeaders");
                });

            modelBuilder.Entity("AkkaTransfer.Data.FilePiece", b =>
                {
                    b.HasOne("AkkaTransfer.Data.ReceiveFileHeader", null)
                        .WithMany("FilePieces")
                        .HasForeignKey("ReceiveFileHeaderId");
                });

            modelBuilder.Entity("AkkaTransfer.Data.ReceiveFileHeader", b =>
                {
                    b.Navigation("FilePieces");
                });
#pragma warning restore 612, 618
        }
    }
}
