﻿// <auto-generated />
using System;
using AcfunTools.CommentXuMing.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AcfunTools.CommentXuMing.Model.Migrations
{
    [DbContext(typeof(CommentXuMingDbContext))]
    [Migration("20200110142845_initial_1th")]
    partial class initial_1th
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.0");

            modelBuilder.Entity("AcfunTools.CommentXuMing.Model.Entity.Article", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AcNo")
                        .HasColumnType("TEXT");

                    b.Property<string>("AvatarImageUrl")
                        .HasColumnType("TEXT");

                    b.Property<int>("ChannelId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ParentChannelId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Raw")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ReleaseTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("SubTitle")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserName")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Articles");
                });

            modelBuilder.Entity("AcfunTools.CommentXuMing.Model.Entity.Comment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AcNo")
                        .HasColumnType("TEXT");

                    b.Property<int?>("ArticleId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("AvatarImageUrl")
                        .HasColumnType("TEXT");

                    b.Property<int>("CId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Content")
                        .HasColumnType("TEXT");

                    b.Property<int>("Floor")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Raw")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ReleaseTime")
                        .HasColumnType("TEXT");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserName")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ArticleId");

                    b.ToTable("Comments");
                });

            modelBuilder.Entity("AcfunTools.CommentXuMing.Model.Entity.Comment", b =>
                {
                    b.HasOne("AcfunTools.CommentXuMing.Model.Entity.Article", null)
                        .WithMany("Comments")
                        .HasForeignKey("ArticleId");
                });
#pragma warning restore 612, 618
        }
    }
}
