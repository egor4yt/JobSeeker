using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSeeker.WebScraper.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedVacancyKeyIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_ScrapTask_VacancyKey",
                table: "ScrapGroups",
                columns: new[] { "Occupation", "OccupationGroup", "Specialization", "SkillTag" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_ScrapTask_VacancyKey",
                table: "ScrapGroups");
        }
    }
}
