using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace JobSeeker.WebScraper.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemovedScrapSources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScrapTasks_ScrapSources_ScrapSourceId",
                table: "ScrapTasks");

            migrationBuilder.DropTable(
                name: "ScrapSources");

            migrationBuilder.DropIndex(
                name: "IX_ScrapTasks_ScrapSourceId",
                table: "ScrapTasks");

            migrationBuilder.DropColumn(
                name: "ScrapSourceId",
                table: "ScrapTasks");

            migrationBuilder.RenameColumn(
                name: "EntrypointPath",
                table: "ScrapTasks",
                newName: "Entrypoint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Entrypoint",
                table: "ScrapTasks",
                newName: "EntrypointPath");

            migrationBuilder.AddColumn<int>(
                name: "ScrapSourceId",
                table: "ScrapTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ScrapSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Domain = table.Column<string>(type: "varchar(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapSources", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScrapTasks_ScrapSourceId",
                table: "ScrapTasks",
                column: "ScrapSourceId");

            migrationBuilder.CreateIndex(
                name: "UX_ScrapSource_Domain",
                table: "ScrapSources",
                column: "Domain",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ScrapTasks_ScrapSources_ScrapSourceId",
                table: "ScrapTasks",
                column: "ScrapSourceId",
                principalTable: "ScrapSources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
