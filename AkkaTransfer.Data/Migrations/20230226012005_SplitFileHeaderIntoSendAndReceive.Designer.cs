﻿// <auto-generated />
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
    [Migration("20230226012005_SplitFileHeaderIntoSendAndReceive")]
    partial class SplitFileHeaderIntoSendAndReceive
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.3");

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

                    b.ToTable("ReceiveFileHeaders");
                });

            modelBuilder.Entity("AkkaTransfer.Data.ReceiveFilePiece", b =>
                {
                    b.Property<int>("ReceiveFilePieceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Content")
                        .HasColumnType("TEXT");

                    b.Property<int>("Position")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ReceiveFileHeaderId")
                        .HasColumnType("INTEGER");

                    b.HasKey("ReceiveFilePieceId");

                    b.HasIndex("ReceiveFileHeaderId");

                    b.ToTable("ReceiveFilePieces");
                });

            modelBuilder.Entity("AkkaTransfer.Data.SendFileHeader", b =>
                {
                    b.Property<int>("SendFileHeaderId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("FileName")
                        .HasColumnType("TEXT");

                    b.Property<int>("PieceCount")
                        .HasColumnType("INTEGER");

                    b.HasKey("SendFileHeaderId");

                    b.ToTable("SendFileHeaders");
                });

            modelBuilder.Entity("AkkaTransfer.Data.SendFilePiece", b =>
                {
                    b.Property<int>("SendFilePieceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Content")
                        .HasColumnType("TEXT");

                    b.Property<int>("Position")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SendFileHeaderId")
                        .HasColumnType("INTEGER");

                    b.HasKey("SendFilePieceId");

                    b.HasIndex("SendFileHeaderId");

                    b.ToTable("SendFilePieces");
                });

            modelBuilder.Entity("AkkaTransfer.Data.ReceiveFilePiece", b =>
                {
                    b.HasOne("AkkaTransfer.Data.ReceiveFileHeader", null)
                        .WithMany("ReceiveFilePieces")
                        .HasForeignKey("ReceiveFileHeaderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("AkkaTransfer.Data.SendFilePiece", b =>
                {
                    b.HasOne("AkkaTransfer.Data.SendFileHeader", null)
                        .WithMany("SendFilePieces")
                        .HasForeignKey("SendFileHeaderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("AkkaTransfer.Data.ReceiveFileHeader", b =>
                {
                    b.Navigation("ReceiveFilePieces");
                });

            modelBuilder.Entity("AkkaTransfer.Data.SendFileHeader", b =>
                {
                    b.Navigation("SendFilePieces");
                });
#pragma warning restore 612, 618
        }
    }
}
