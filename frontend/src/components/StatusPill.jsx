import Icon from './Icon';
import {
  ApplicationStatusLabel, ApplicationStatusTone, ApplicationStatus,
  HourLogStatusLabel, HourLogStatusTone,
  OpportunityStatusLabel, OpportunityStatusTone,
} from '../lib/enums';
import { isVerifiedHour } from '../lib/hours';

/* Maps a tone to the app.css `.st` modifier class. */
const TONE_CLASS = { green: 'accepted', blue: 'attended', red: 'rejected', amber: 'submitted', slate: 'noshow' };

function Pill({ tone, label, verified }) {
  const cls = verified ? 'verified' : (TONE_CLASS[tone] || 'submitted');
  return (
    <span className={`st ${cls}`}>
      {verified ? <Icon name="check" size={12} stroke={2.4} /> : <span className="stdot" />}
      {label}
    </span>
  );
}

export function ApplicationStatusPill({ status }) {
  return <Pill tone={ApplicationStatusTone[status]} label={ApplicationStatusLabel[status]} verified={status === ApplicationStatus.Attended} />;
}

export function HourLogStatusPill({ status }) {
  // Approved and Corrected are both recognized/verified hours (backend counts
  // both toward totals, reports and certificates).
  return <Pill tone={HourLogStatusTone[status]} label={HourLogStatusLabel[status]} verified={isVerifiedHour(status)} />;
}

export function OpportunityStatusPill({ status }) {
  return <Pill tone={OpportunityStatusTone[status]} label={OpportunityStatusLabel[status]} />;
}
