import { OpportunityStatus, OpportunityVisibility } from './enums.js';

/* Capability guards for organization opportunity actions.

   The backend now returns authoritative flags (canEdit/canDelete/canInvite/
   canCancel/canComplete/canCreateImpactReport/hasImpactReport/verifiedHoursCount)
   on the organization "my opportunities" payload. We use those when present and
   fall back to a conservative status/visibility heuristic only for older data
   that lacks them. Pure + testable. */
export function oppCapabilities(o, { verified } = {}) {
  // Prefer backend-provided flags when the payload includes them.
  if (o && typeof o.canEdit === 'boolean') {
    return {
      canEdit: o.canEdit,
      canDelete: o.canDelete,
      canComplete: o.canComplete,
      canCancel: o.canCancel,
      canInvite: o.canInvite,
      canCreateImpactReport: !!o.canCreateImpactReport,
      hasImpactReport: !!o.hasImpactReport,
      verifiedHoursCount: o.verifiedHoursCount ?? 0,
      get none() { return !(this.canEdit || this.canDelete || this.canComplete || this.canCancel || this.canInvite); },
    };
  }

  // Fallback heuristic (no backend flags available).
  const status = o?.status;
  const isOpen = status === OpportunityStatus.Open;
  const isFilled = status === OpportunityStatus.Filled;
  const isCompleted = status === OpportunityStatus.Completed;
  const isCancelled = status === OpportunityStatus.Cancelled;
  const isTerminal = isCompleted || isCancelled;
  const isPrivate = o?.visibility === OpportunityVisibility.Private;
  return {
    canEdit: !isTerminal,
    canDelete: !isCompleted,
    canComplete: isOpen || isFilled,
    canCancel: isOpen || isFilled,
    canInvite: !!verified && isPrivate && isOpen,
    canCreateImpactReport: isCompleted,
    hasImpactReport: false,
    verifiedHoursCount: o?.verifiedHoursCount ?? 0,
    get none() { return !(this.canEdit || this.canDelete || this.canComplete || this.canCancel || this.canInvite); },
  };
}

/* Whether an unverified organization may publish/create/invite at all. */
export function canPublish(profile) {
  return !!profile?.isVerified;
}
