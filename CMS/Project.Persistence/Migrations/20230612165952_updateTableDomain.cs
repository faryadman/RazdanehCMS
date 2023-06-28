using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Persistence.Migrations
{
    public partial class updateTableDomain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DomainIP",
                schema: "dbo",
                table: "Domains");

            migrationBuilder.DropColumn(
                name: "DomainType",
                schema: "dbo",
                table: "Domains");

            migrationBuilder.RenameTable(
                name: "Domains",
                schema: "dbo",
                newName: "Domains");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.RenameTable(
                name: "Domains",
                newName: "Domains",
                newSchema: "dbo");

            migrationBuilder.AddColumn<string>(
                name: "DomainIP",
                schema: "dbo",
                table: "Domains",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DomainType",
                schema: "dbo",
                table: "Domains",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
