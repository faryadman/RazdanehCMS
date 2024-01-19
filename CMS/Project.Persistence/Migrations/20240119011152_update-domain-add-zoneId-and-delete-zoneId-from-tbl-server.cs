using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Persistence.Migrations
{
    public partial class updatedomainaddzoneIdanddeletezoneIdfromtblserver : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ZoneId",
                table: "Servers");

            migrationBuilder.AddColumn<string>(
                name: "ZoneId",
                table: "Domains",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ZoneId",
                table: "Domains");

            migrationBuilder.AddColumn<string>(
                name: "ZoneId",
                table: "Servers",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);
        }
    }
}
