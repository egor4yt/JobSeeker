using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSeeker.WebApi.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixedLinksIsVacancySource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VacancySource_Locations_SourceId",
                table: "VacancySource");

            migrationBuilder.DropForeignKey(
                name: "FK_VacancySource_Vacancies_SourceId",
                table: "VacancySource");

            migrationBuilder.CreateIndex(
                name: "IX_VacancySource_LocationId",
                table: "VacancySource",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_VacancySource_VacancyId",
                table: "VacancySource",
                column: "VacancyId");

            migrationBuilder.AddForeignKey(
                name: "FK_VacancySource_Locations_LocationId",
                table: "VacancySource",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VacancySource_Vacancies_VacancyId",
                table: "VacancySource",
                column: "VacancyId",
                principalTable: "Vacancies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VacancySource_Locations_LocationId",
                table: "VacancySource");

            migrationBuilder.DropForeignKey(
                name: "FK_VacancySource_Vacancies_VacancyId",
                table: "VacancySource");

            migrationBuilder.DropIndex(
                name: "IX_VacancySource_LocationId",
                table: "VacancySource");

            migrationBuilder.DropIndex(
                name: "IX_VacancySource_VacancyId",
                table: "VacancySource");

            migrationBuilder.AddForeignKey(
                name: "FK_VacancySource_Locations_SourceId",
                table: "VacancySource",
                column: "SourceId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VacancySource_Vacancies_SourceId",
                table: "VacancySource",
                column: "SourceId",
                principalTable: "Vacancies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
