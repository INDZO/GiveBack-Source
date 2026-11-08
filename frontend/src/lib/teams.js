import { TeamMemberStatus } from './enums.js';

/* Whether a team can apply to a team-based opportunity. Mirrors the backend
   ApplyTeam rules exactly:
   - no still-pending member invitations, and
   - at least one CONFIRMED member other than the captain (the captain is stored
     as a confirmed member, so a captain-only team is NOT a valid team).
   Pure + testable. */
export function confirmedNonCaptainCount(team) {
  const members = team?.members || [];
  return members.filter(
    (m) => m.status === TeamMemberStatus.Confirmed && m.volunteerId !== team.captainVolunteerId,
  ).length;
}

export function hasPendingMembers(team) {
  return (team?.members || []).some((m) => m.status === TeamMemberStatus.Pending);
}

export function isTeamApplyEligible(team) {
  return !hasPendingMembers(team) && confirmedNonCaptainCount(team) >= 1;
}

/* Teams the given volunteer may apply with (captain + eligible). */
export function eligibleTeamsForApply(teams, myVolunteerId) {
  if (myVolunteerId == null) return [];
  return (teams || []).filter((t) => t.captainVolunteerId === myVolunteerId && isTeamApplyEligible(t));
}
