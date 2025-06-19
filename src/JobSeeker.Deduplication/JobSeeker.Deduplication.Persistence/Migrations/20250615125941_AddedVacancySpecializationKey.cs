using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSeeker.Deduplication.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedVacancySpecializationKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Occupation",
                table: "RawVacancies",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OccupationGroup",
                table: "RawVacancies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SkillTag",
                table: "RawVacancies",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Specialization",
                table: "RawVacancies",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Occupation",
                table: "RawVacancies");

            migrationBuilder.DropColumn(
                name: "OccupationGroup",
                table: "RawVacancies");

            migrationBuilder.DropColumn(
                name: "SkillTag",
                table: "RawVacancies");

            migrationBuilder.DropColumn(
                name: "Specialization",
                table: "RawVacancies");
        }
    }
}
