import { ApplicationStatus } from './enums.js';

/* When may an organization mark an accepted application as Attended / NoShow?
   Only after the opportunity has ended. Mirrors the backend rule
   (OpportunityTimeRules.HasEnded + status == Accepted) using the opportunity
   timing now exposed on ApplicationDto. Pure + testable. */
export function opportunityEndsAt(a, now = new Date()) {
  if (!a) return now;
  if (a.opportunityEndAt) return new Date(a.opportunityEndAt);
  if (a.opportunityStartAt) {
    const start = new Date(a.opportunityStartAt);
    const hours = Number(a.opportunityDurationHours) || 0;
    return new Date(start.getTime() + hours * 3600 * 1000);
  }
  return now;
}

export function canMarkAttendance(a, now = new Date()) {
  if (!a || a.status !== ApplicationStatus.Accepted) return false;
  return opportunityEndsAt(a, now).getTime() <= now.getTime();
}
