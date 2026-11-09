using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiveBack.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SemanticHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VolunteerHourLogs_ApplicationId",
                table: "VolunteerHourLogs");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_OpportunityId_TeamId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_OpportunityId_VolunteerId",
                table: "Applications");

            migrationBuilder.AddColumn<int>(
                name: "RelatedEntityId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedEntityType",
                table: "Notifications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerHourLogs_ApplicationId_VolunteerId_WorkDate",
                table: "VolunteerHourLogs",
                columns: new[] { "ApplicationId", "VolunteerId", "WorkDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationProfiles_RegistrationNumber",
                table: "OrganizationProfiles",
                column: "RegistrationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_NotificationType_RelatedEntityType_RelatedEntityId",
                table: "Notifications",
                columns: new[] { "UserId", "NotificationType", "RelatedEntityType", "RelatedEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_OpportunityId_TeamId",
                table: "Applications",
                columns: new[] { "OpportunityId", "TeamId" },
                unique: true,
                filter: "[TeamId] IS NOT NULL AND [Status] IN (1, 2, 4, 5)");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_OpportunityId_VolunteerId",
                table: "Applications",
                columns: new[] { "OpportunityId", "VolunteerId" },
                unique: true,
                filter: "[VolunteerId] IS NOT NULL AND [Status] IN (1, 2, 4, 5)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VolunteerHourLogs_ApplicationId_VolunteerId_WorkDate",
                table: "VolunteerHourLogs");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationProfiles_RegistrationNumber",
                table: "OrganizationProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_NotificationType_RelatedEntityType_RelatedEntityId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_OpportunityId_TeamId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_OpportunityId_VolunteerId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "RelatedEntityId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RelatedEntityType",
                table: "Notifications");

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerHourLogs_ApplicationId",
                table: "VolunteerHourLogs",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

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
    }
}
