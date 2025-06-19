using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSeeker.Deduplication.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RawVacancyCreationTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RawVacancy_DownloadKey",
                table: "RawVacancies");

            migrationBuilder.DropColumn(
                name: "DownloadKey",
                table: "RawVacancies");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "RawVacancies",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RawVacancies");

            migrationBuilder.AddColumn<string>(
                name: "DownloadKey",
                table: "RawVacancies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_RawVacancy_DownloadKey",
                table: "RawVacancies",
                column: "DownloadKey");
        }
    }
}
