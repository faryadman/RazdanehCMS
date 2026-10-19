using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNativeJobScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ConfigurationId = table.Column<int>(type: "int", nullable: false),
                    SettingsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkerId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Trigger = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ScheduledForUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    ErrorCode = table.Column<string>(type: "nvarchar(48)", maxLength: 48, nullable: true),
                    NeedsReview = table.Column<bool>(type: "bit", nullable: false),
                    ReviewedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobExecutions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobSchedules",
                columns: table => new
                {
                    JobName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ConfigurationId = table.Column<int>(type: "int", nullable: false),
                    ConfigurationHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    MinuteStep = table.Column<int>(type: "int", nullable: false),
                    NextRunUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RunRequested = table.Column<bool>(type: "bit", nullable: false),
                    RequestedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastStartedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastCompletedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastStatus = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    ValidationError = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SuccessfulExecutions = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSchedules", x => x.JobName);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutions_JobName_StartedUtc",
                table: "JobExecutions",
                columns: new[] { "JobName", "StartedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutions_Status_NeedsReview",
                table: "JobExecutions",
                columns: new[] { "Status", "NeedsReview" });

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutions_NeedsReview",
                table: "JobExecutions",
                column: "NeedsReview",
                filter: "[NeedsReview] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutions_StartedUtc",
                table: "JobExecutions",
                column: "StartedUtc",
                descending: new[] { true });

            migrationBuilder.CreateIndex(
                name: "IX_JobSchedules_Enabled_NextRunUtc",
                table: "JobSchedules",
                columns: new[] { "Enabled", "NextRunUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobExecutions");

            migrationBuilder.DropTable(
                name: "JobSchedules");
        }
    }
}
