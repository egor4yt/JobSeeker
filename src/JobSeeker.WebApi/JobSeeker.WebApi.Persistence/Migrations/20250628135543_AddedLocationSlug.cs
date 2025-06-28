using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSeeker.WebApi.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedLocationSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VacancySource_OuterSources_SourceId",
                table: "VacancySource");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OuterSources",
                table: "OuterSources");

            migrationBuilder.RenameTable(
                name: "OuterSources",
                newName: "Sources");

            migrationBuilder.AlterColumn<int>(
                name: "LocationId",
                table: "VacancySource",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Locations",
                type: "varchar(256)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sources",
                table: "Sources",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "UX_Location_Slug",
                table: "Locations",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_VacancySource_Sources_SourceId",
                table: "VacancySource",
                column: "SourceId",
                principalTable: "Sources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VacancySource_Sources_SourceId",
                table: "VacancySource");

            migrationBuilder.DropIndex(
                name: "UX_Location_Slug",
                table: "Locations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sources",
                table: "Sources");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Locations");

            migrationBuilder.RenameTable(
                name: "Sources",
                newName: "OuterSources");

            migrationBuilder.AlterColumn<int>(
                name: "LocationId",
                table: "VacancySource",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OuterSources",
                table: "OuterSources",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VacancySource_OuterSources_SourceId",
                table: "VacancySource",
                column: "SourceId",
                principalTable: "OuterSources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
