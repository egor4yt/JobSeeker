using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace JobSeeker.WebScraper.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReorganisationScrapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"ScrapTasks\"");
            
            migrationBuilder.DropTable(
                name: "ScrapTaskToSourceMap");

            migrationBuilder.DropColumn(
                name: "ExcludeWordsList",
                table: "ScrapTasks");

            migrationBuilder.DropColumn(
                name: "SearchText",
                table: "ScrapTasks");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "ScrapTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ScrapTasks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EntrypointPath",
                table: "ScrapTasks",
                type: "varchar(2048)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ErrorDetails",
                table: "ScrapTasks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "ScrapTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ScrapGroupId",
                table: "ScrapTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ScrapSourceId",
                table: "ScrapTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "ScrapTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ScrapGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    OccupationGroup = table.Column<int>(type: "integer", nullable: false),
                    Occupation = table.Column<int>(type: "integer", nullable: true),
                    Specialization = table.Column<int>(type: "integer", nullable: true),
                    SkillTag = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapGroups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScrapTask_Priority1",
                table: "ScrapTasks",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_ScrapTasks_ScrapGroupId",
                table: "ScrapTasks",
                column: "ScrapGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ScrapTasks_ScrapSourceId",
                table: "ScrapTasks",
                column: "ScrapSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ScrapTask_Priority",
                table: "ScrapGroups",
                column: "Priority");

            migrationBuilder.AddForeignKey(
                name: "FK_ScrapTasks_ScrapGroups_ScrapGroupId",
                table: "ScrapTasks",
                column: "ScrapGroupId",
                principalTable: "ScrapGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ScrapTasks_ScrapSources_ScrapSourceId",
                table: "ScrapTasks",
                column: "ScrapSourceId",
                principalTable: "ScrapSources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScrapTasks_ScrapGroups_ScrapGroupId",
                table: "ScrapTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_ScrapTasks_ScrapSources_ScrapSourceId",
                table: "ScrapTasks");

            migrationBuilder.DropTable(
                name: "ScrapGroups");

            migrationBuilder.DropIndex(
                name: "IX_ScrapTask_Priority1",
                table: "ScrapTasks");

            migrationBuilder.DropIndex(
                name: "IX_ScrapTasks_ScrapGroupId",
                table: "ScrapTasks");

            migrationBuilder.DropIndex(
                name: "IX_ScrapTasks_ScrapSourceId",
                table: "ScrapTasks");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "ScrapTasks");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ScrapTasks");

            migrationBuilder.DropColumn(
                name: "EntrypointPath",
                table: "ScrapTasks");

            migrationBuilder.DropColumn(
                name: "ErrorDetails",
                table: "ScrapTasks");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "ScrapTasks");

            migrationBuilder.DropColumn(
                name: "ScrapGroupId",
                table: "ScrapTasks");

            migrationBuilder.DropColumn(
                name: "ScrapSourceId",
                table: "ScrapTasks");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "ScrapTasks");

            migrationBuilder.AddColumn<string>(
                name: "ExcludeWordsList",
                table: "ScrapTasks",
                type: "varchar(256)",
                nullable: true,
                comment: "List of words separated with ','");

            migrationBuilder.AddColumn<string>(
                name: "SearchText",
                table: "ScrapTasks",
                type: "varchar(64)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ScrapTaskToSourceMap",
                columns: table => new
                {
                    ScrapTaskId = table.Column<int>(type: "integer", nullable: false),
                    ScrapSourceId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapTaskToSourceMap", x => new { x.ScrapTaskId, x.ScrapSourceId });
                    table.ForeignKey(
                        name: "FK_ScrapTaskToSourceMap_ScrapSources_ScrapSourceId",
                        column: x => x.ScrapSourceId,
                        principalTable: "ScrapSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScrapTaskToSourceMap_ScrapTasks_ScrapTaskId",
                        column: x => x.ScrapTaskId,
                        principalTable: "ScrapTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScrapTaskToSourceMap_ScrapSourceId",
                table: "ScrapTaskToSourceMap",
                column: "ScrapSourceId");
        }
    }
}
