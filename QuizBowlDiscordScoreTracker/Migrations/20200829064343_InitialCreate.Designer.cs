﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QuizBowlDiscordScoreTracker.Database;

namespace QuizBowlDiscordScoreTracker.Migrations
{
    [DbContext(typeof(BotConfigurationContext))]
    [Migration("20200829064343_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.7");

            modelBuilder.Entity("QuizBowlDiscordScoreTracker.Database.GuildSetting", b =>
                {
                    b.Property<ulong>("GuildSettingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TeamRolePrefix")
                        .HasColumnType("TEXT");

                    b.HasKey("GuildSettingId");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("QuizBowlDiscordScoreTracker.Database.TextChannelSetting", b =>
                {
                    b.Property<ulong>("TextChannelSettingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("GuildSettingId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong?>("TeamMessageId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong?>("VoiceChannelId")
                        .HasColumnType("INTEGER");

                    b.HasKey("TextChannelSettingId");

                    b.HasIndex("GuildSettingId");

                    b.ToTable("TextChannels");
                });

            modelBuilder.Entity("QuizBowlDiscordScoreTracker.Database.TextChannelSetting", b =>
                {
                    b.HasOne("QuizBowlDiscordScoreTracker.Database.GuildSetting", null)
                        .WithMany("TextChannels")
                        .HasForeignKey("GuildSettingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}