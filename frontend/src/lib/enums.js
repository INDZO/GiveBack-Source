/* Backend enums serialize as integers (no JsonStringEnumConverter configured).
   These maps keep the frontend in sync with GiveBack.Domain.Enums. */

export const UserRole = { Volunteer: 1, Organization: 2, Admin: 3 };
export const RoleName = { 1: 'Volunteer', 2: 'Organization', 3: 'Admin' };

export const ApplicationStatus = {
  Submitted: 1, Accepted: 2, Rejected: 3, Attended: 4, NoShow: 5, Withdrawn: 6,
};
export const ApplicationStatusLabel = {
  1: 'Submitted', 2: 'Accepted', 3: 'Rejected', 4: 'Attended', 5: 'No-show', 6: 'Withdrawn',
};
/* maps a status to a status-pill modifier class (app.css .st--*) */
export const ApplicationStatusTone = {
  1: 'amber', 2: 'green', 3: 'red', 4: 'blue', 5: 'slate', 6: 'slate',
};

export const ApplicationType = { Individual: 1, Team: 2 };

export const HourLogStatus = { Pending: 1, Approved: 2, Corrected: 3, Rejected: 4 };
export const HourLogStatusLabel = { 1: 'Pending', 2: 'Approved', 3: 'Corrected', 4: 'Rejected' };
export const HourLogStatusTone = { 1: 'amber', 2: 'green', 3: 'blue', 4: 'red' };

export const InvitationStatus = { Pending: 1, Accepted: 2, Declined: 3 };
export const InvitationStatusLabel = { 1: 'Pending', 2: 'Accepted', 3: 'Declined' };

export const OpportunityFormat = { OnSite: 1, Online: 2 };
export const OpportunityFormatLabel = { 1: 'On-site', 2: 'Online' };

export const OpportunityStatus = { Open: 1, Filled: 2, Cancelled: 3, Completed: 4 };
export const OpportunityStatusLabel = { 1: 'Open', 2: 'Filled', 3: 'Cancelled', 4: 'Completed' };
export const OpportunityStatusTone = { 1: 'green', 2: 'blue', 3: 'red', 4: 'slate' };

export const OpportunityType = { OneTime: 1, Periodic: 2, LongTerm: 3 };
export const OpportunityTypeLabel = { 1: 'One-time', 2: 'Recurring', 3: 'Long-term' };

export const OpportunityVisibility = { Public: 1, Private: 2 };
export const OpportunityVisibilityLabel = { 1: 'Public', 2: 'Private' };

export const TeamMemberStatus = { Pending: 1, Confirmed: 2, Declined: 3 };
export const TeamMemberStatusLabel = { 1: 'Pending', 2: 'Confirmed', 3: 'Declined' };

export const NotificationType = {
  NewMatchingOpportunity: 1, Reminder24h: 2, Milestone: 3, ProjectEnded: 4,
  ApplicationAccepted: 5, ApplicationRejected: 6, HoursVerified: 7,
  OpportunityCancelled: 8, InvitationReceived: 9, InvitationAccepted: 10,
  InvitationDeclined: 11, OpportunityCompleted: 12,
};

export const DayOfWeek = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

/* Maps a cause name to the design's category color family
   (env=green, ani=orange, edu=purple, health=red, tech=blue). */
export function causeTone(name = '') {
  const n = name.toLowerCase();
  if (/(eco|environ|nature|park|clean|climate)/.test(n)) return 'env';
  if (/(animal|shelter|pet|wildlife)/.test(n)) return 'ani';
  if (/(health|blood|medical|care|crisis|humanitar|refugee|elder)/.test(n)) return 'health';
  if (/(tech|cod|program|software|web|it\b|digital)/.test(n)) return 'tech';
  if (/(child|educat|school|teach|book|read|translat|language|student)/.test(n)) return 'edu';
  return 'edu';
}

/* Lucide icon name for a cause tone (used on cards). */
export const toneIcon = { env: 'leaf', ani: 'paw', edu: 'book', health: 'heartHand', tech: 'code' };
