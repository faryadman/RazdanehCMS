using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Persistence.Migrations
{
    public partial class updateappsettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAcAllowed",
                table: "AppSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSpAllowed",
                table: "AppSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTimeToStopAllowed",
                table: "AppSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TimeToStop",
                table: "AppSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAcAllowed",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "IsSpAllowed",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "IsTimeToStopAllowed",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "TimeToStop",
                table: "AppSettings");
        }
    }
}
