using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSeeker.Deduplication.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedSourceVacancyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Fingerprint",
                table: "RawVacancies",
                type: "varchar(64)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "SourceId",
                table: "RawVacancies",
                type: "varchar(128)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "RawVacancies");

            migrationBuilder.AlterColumn<string>(
                name: "Fingerprint",
                table: "RawVacancies",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)");
        }
    }
}
