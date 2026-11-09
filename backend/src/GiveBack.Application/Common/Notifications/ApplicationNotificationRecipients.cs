using GiveBack.Domain.Enums;
using VolunteerApplication = GiveBack.Domain.Entities.Application;

namespace GiveBack.Application.Common.Notifications;

public static class ApplicationNotificationRecipients
{
    public static IReadOnlyList<int> GetUserIds(VolunteerApplication application)
    {
        var userIds = new HashSet<int>();

        if (application.Volunteer is not null)
        {
            userIds.Add(application.Volunteer.UserId);
        }

        if (application.Team is not null)
        {
            if (application.Team.CaptainVolunteer is not null)
            {
                userIds.Add(application.Team.CaptainVolunteer.UserId);
            }

            foreach (var member in application.Team.Members.Where(m => m.Status == TeamMemberStatus.Confirmed))
            {
                userIds.Add(member.Volunteer.UserId);
            }
        }

        return userIds.ToList();
    }
}
