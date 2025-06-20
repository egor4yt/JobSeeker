using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSeeker.Deduplication.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedLinksToOriginalsVacancies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OriginalRawVacancyId",
                table: "RawVacancies",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RawVacancies_OriginalRawVacancyId",
                table: "RawVacancies",
                column: "OriginalRawVacancyId");

            migrationBuilder.AddForeignKey(
                name: "FK_RawVacancies_RawVacancies_OriginalRawVacancyId",
                table: "RawVacancies",
                column: "OriginalRawVacancyId",
                principalTable: "RawVacancies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RawVacancies_RawVacancies_OriginalRawVacancyId",
                table: "RawVacancies");

            migrationBuilder.DropIndex(
                name: "IX_RawVacancies_OriginalRawVacancyId",
                table: "RawVacancies");

            migrationBuilder.DropColumn(
                name: "OriginalRawVacancyId",
                table: "RawVacancies");
        }
    }
}
