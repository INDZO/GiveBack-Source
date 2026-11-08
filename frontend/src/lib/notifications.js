import { NotificationType, UserRole } from './enums.js';

/* Maps a notification to a role-aware action link, or null when there is no
   meaningful/safe destination (the UI then renders an informational row rather
   than a dead link). Pure function so it can be unit-tested without a DOM.

   - Volunteer paths live under /app, Organization under /org, Admin under /admin.
   - Opportunity-scoped notifications use relatedEntityId; if it is missing for a
     volunteer-only opportunity link we return null instead of a broken URL. */
export function notificationAction(n, role) {
  const oppLink = (id) => {
    if (role === UserRole.Volunteer) return id ? { to: `/app/opportunities/${id}`, label: 'View opportunity' } : null;
    if (role === UserRole.Organization) return { to: '/org/opportunities', label: 'View opportunities' };
    return id ? { to: `/opportunities/${id}`, label: 'View opportunity' } : null;
  };
  switch (n?.notificationType) {
    case NotificationType.InvitationReceived:
      return role === UserRole.Volunteer ? { to: '/app/invitations', label: 'View invitation' } : null;
    case NotificationType.NewMatchingOpportunity:
    case NotificationType.Reminder24h:
    case NotificationType.OpportunityCompleted:
    case NotificationType.OpportunityCancelled:
    case NotificationType.ProjectEnded:
      return oppLink(n.relatedEntityId);
    case NotificationType.ApplicationAccepted:
    case NotificationType.ApplicationRejected:
      return role === UserRole.Volunteer ? { to: '/app/applications', label: 'View applications' } : null;
    case NotificationType.HoursVerified:
      return role === UserRole.Volunteer ? { to: '/app/hours', label: 'View hours' } : null;
    case NotificationType.Milestone:
      return role === UserRole.Volunteer ? { to: '/app/profile', label: 'View profile' } : null;
    case NotificationType.InvitationAccepted:
    case NotificationType.InvitationDeclined:
      return role === UserRole.Organization ? { to: '/org/applicants', label: 'View applicants' } : null;
    default:
      return null;
  }
}
