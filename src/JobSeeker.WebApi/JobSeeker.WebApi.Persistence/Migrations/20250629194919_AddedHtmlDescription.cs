using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSeeker.WebApi.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedHtmlDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HtmlDescription",
                table: "Vacancies",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HtmlDescription",
                table: "Vacancies");
        }
    }
}
