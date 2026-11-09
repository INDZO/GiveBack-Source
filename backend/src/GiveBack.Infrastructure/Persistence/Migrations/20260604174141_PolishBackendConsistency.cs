using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiveBack.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PolishBackendConsistency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Applications_OpportunityId",
                table: "Applications");

            migrationBuilder.AddColumn<DateTime>(
                name: "WorkDate",
                table: "VolunteerHourLogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationRejectionReason",
                table: "OrganizationProfiles",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndAt",
                table: "Opportunities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PeriodDescription",
                table: "Opportunities",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartAt",
                table: "Opportunities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE VolunteerHourLogs
                SET WorkDate = CAST(SubmittedAt AS date)
                WHERE WorkDate IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE Opportunities
                SET StartAt = COALESCE(TRY_CONVERT(datetime2, DateTimeOrPeriod, 126), CreatedAt)
                WHERE StartAt IS NULL;
                """);

            migrationBuilder.AlterColumn<DateTime>(
                name: "WorkDate",
                table: "VolunteerHourLogs",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartAt",
                table: "Opportunities",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "DateTimeOrPeriod",
                table: "Opportunities");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Opportunity_EndAfterStart",
                table: "Opportunities",
                sql: "[EndAt] IS NULL OR [EndAt] > [StartAt]");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_OpportunityId_TeamId",
                table: "Applications",
                columns: new[] { "OpportunityId", "TeamId" },
                unique: true,
                filter: "[TeamId] IS NOT NULL AND [Status] IN (1, 2)");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_OpportunityId_VolunteerId",
                table: "Applications",
                columns: new[] { "OpportunityId", "VolunteerId" },
                unique: true,
                filter: "[VolunteerId] IS NOT NULL AND [Status] IN (1, 2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Opportunity_EndAfterStart",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Applications_OpportunityId_TeamId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_OpportunityId_VolunteerId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "WorkDate",
                table: "VolunteerHourLogs");

            migrationBuilder.DropColumn(
                name: "VerificationRejectionReason",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "EndAt",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "PeriodDescription",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "StartAt",
                table: "Opportunities");

            migrationBuilder.AddColumn<string>(
                name: "DateTimeOrPeriod",
                table: "Opportunities",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_OpportunityId",
                table: "Applications",
                column: "OpportunityId");
        }
    }
}
