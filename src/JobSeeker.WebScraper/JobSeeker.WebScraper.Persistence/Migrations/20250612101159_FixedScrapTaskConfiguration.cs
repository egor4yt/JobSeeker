using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSeeker.WebScraper.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixedScrapTaskConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScrapTaskConfiguration_ScrapGroups_ScrapGroupId",
                table: "ScrapTaskConfiguration");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScrapTaskConfiguration",
                table: "ScrapTaskConfiguration");

            migrationBuilder.RenameTable(
                name: "ScrapTaskConfiguration",
                newName: "ScrapTaskConfigurations");

            migrationBuilder.RenameIndex(
                name: "IX_ScrapTaskConfiguration_ScrapGroupId",
                table: "ScrapTaskConfigurations",
                newName: "IX_ScrapTaskConfigurations_ScrapGroupId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScrapTaskConfigurations",
                table: "ScrapTaskConfigurations",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ScrapTaskConfigurations_ScrapGroups_ScrapGroupId",
                table: "ScrapTaskConfigurations",
                column: "ScrapGroupId",
                principalTable: "ScrapGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScrapTaskConfigurations_ScrapGroups_ScrapGroupId",
                table: "ScrapTaskConfigurations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScrapTaskConfigurations",
                table: "ScrapTaskConfigurations");

            migrationBuilder.RenameTable(
                name: "ScrapTaskConfigurations",
                newName: "ScrapTaskConfiguration");

            migrationBuilder.RenameIndex(
                name: "IX_ScrapTaskConfigurations_ScrapGroupId",
                table: "ScrapTaskConfiguration",
                newName: "IX_ScrapTaskConfiguration_ScrapGroupId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScrapTaskConfiguration",
                table: "ScrapTaskConfiguration",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ScrapTaskConfiguration_ScrapGroups_ScrapGroupId",
                table: "ScrapTaskConfiguration",
                column: "ScrapGroupId",
                principalTable: "ScrapGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
