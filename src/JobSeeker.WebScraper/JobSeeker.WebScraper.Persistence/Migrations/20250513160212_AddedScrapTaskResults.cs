using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace JobSeeker.WebScraper.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedScrapTaskResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScrapTaskResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Link = table.Column<string>(type: "varchar(256)", nullable: false),
                    Uploaded = table.Column<bool>(type: "boolean", nullable: false, comment: "Indicates whether the result of the scraping task has been uploaded to S3 or not"),
                    ScrapTaskId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapTaskResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScrapTaskResults_ScrapTasks_ScrapTaskId",
                        column: x => x.ScrapTaskId,
                        principalTable: "ScrapTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScrapTaskResults_ScrapTaskId",
                table: "ScrapTaskResults",
                column: "ScrapTaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScrapTaskResults");
        }
    }
}
