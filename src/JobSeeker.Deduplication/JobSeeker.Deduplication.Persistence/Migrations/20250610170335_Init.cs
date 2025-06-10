using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace JobSeeker.Deduplication.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RawVacancies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Location = table.Column<string>(type: "varchar(64)", nullable: true),
                    Title = table.Column<string>(type: "varchar(256)", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Company = table.Column<string>(type: "varchar(128)", nullable: false),
                    SourceDomain = table.Column<string>(type: "varchar(64)", nullable: false),
                    DeduplicationCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    DownloadKey = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawVacancies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RawVacancy_DownloadKey",
                table: "RawVacancies",
                column: "DownloadKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RawVacancies");
        }
    }
}
