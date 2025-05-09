using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace JobSeeker.WebScraper.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "ScrapTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExcludeWordsList = table.Column<string>(type: "varchar(256)", nullable: true, comment: "List of words separated with ','"),
                    SearchText = table.Column<string>(type: "varchar(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapTasks", x => x.Id);
                });

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
                name: "UX_ScrapSource_Domain",
                table: "ScrapSources",
                column: "Domain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScrapTaskToSourceMap_ScrapSourceId",
                table: "ScrapTaskToSourceMap",
                column: "ScrapSourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScrapTaskToSourceMap");

            migrationBuilder.DropTable(
                name: "ScrapSources");

            migrationBuilder.DropTable(
                name: "ScrapTasks");
        }
    }
}
