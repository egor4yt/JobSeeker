using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSeeker.Deduplication.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangedTypeRawVacancyDownloadKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DownloadKey",
                table: "RawVacancies",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Fingerprint",
                table: "RawVacancies",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fingerprint",
                table: "RawVacancies");

            migrationBuilder.AlterColumn<int>(
                name: "DownloadKey",
                table: "RawVacancies",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
