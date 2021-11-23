﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using RoleMenuPlugin.Database;

namespace RoleMenuPlugin.Migrations
{
    [DbContext(typeof(RolemenuContext))]
    [Migration("20211123090828_ChannelId")]
    partial class ChannelId
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.11")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("RoleMenuPlugin.Database.RoleMenuModel", b =>
                {
                    b.Property<decimal>("MessageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("MessageId");

                    b.ToTable("RoleMenus");
                });

            modelBuilder.Entity("RoleMenuPlugin.Database.RoleMenuOptionModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("ComponentId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("EmojiName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("MessageId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("RoleId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("RoleMenuId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal?>("RoleMenuModelMessageId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("RoleMenuModelMessageId");

                    b.ToTable("RoleMenuOptionModel");
                });

            modelBuilder.Entity("RoleMenuPlugin.Database.RoleMenuOptionModel", b =>
                {
                    b.HasOne("RoleMenuPlugin.Database.RoleMenuModel", null)
                        .WithMany("Options")
                        .HasForeignKey("RoleMenuModelMessageId");
                });

            modelBuilder.Entity("RoleMenuPlugin.Database.RoleMenuModel", b =>
                {
                    b.Navigation("Options");
                });
#pragma warning restore 612, 618
        }
    }
}
