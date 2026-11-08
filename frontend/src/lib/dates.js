/* Date-only helpers. Date-only fields (hour-log work date, certificate period)
   must never be round-tripped through UTC midnight, or the calendar day can
   shift by one in non-UTC timezones. These helpers keep everything on the local
   calendar day and submit plain `YYYY-MM-DD` strings. */

const pad = (n) => String(n).padStart(2, '0');

/* Today's local calendar date as `YYYY-MM-DD` (NOT UTC). */
export function todayLocalDateString(d = new Date()) {
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
}

/* Normalize any value to its `YYYY-MM-DD` date part for submission. */
export function toDateOnly(value) {
  if (!value) return '';
  return String(value).slice(0, 10);
}

/* Format a date-only value for display without constructing a Date from UTC
   midnight (which would shift the day). Builds a LOCAL date from the parts. */
export function formatDateOnly(value, opts = { month: 'short', day: 'numeric' }) {
  const part = toDateOnly(value);
  const [y, m, d] = part.split('-').map(Number);
  if (!y || !m || !d) return '';
  return new Date(y, m - 1, d).toLocaleDateString('en-US', opts);
}
