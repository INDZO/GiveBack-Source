import { HourLogStatus } from './enums.js';

/* Hours that count as recognized/verified. The backend treats both Approved and
   Corrected logs as verified (they carry an approvedHours value and feed totals,
   reports and certificates), so the frontend must match that. */
export const VERIFIED_HOUR_STATUSES = [HourLogStatus.Approved, HourLogStatus.Corrected];

export function isVerifiedHour(status) {
  return VERIFIED_HOUR_STATUSES.includes(status);
}

/* Sums approvedHours across verified logs. */
export function sumVerifiedHours(logs = []) {
  return logs.filter((h) => isVerifiedHour(h.status)).reduce((s, h) => s + (h.approvedHours || 0), 0);
}
