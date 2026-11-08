import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import Icon from '../../components/Icon';
import { AsyncBoundary } from '../../components/States';
import { ApplicationStatusPill } from '../../components/StatusPill';
import { useAsync } from '../../lib/useAsync';
import { applicationsApi, teamsApi } from '../../api/services';
import { useToast } from '../../components/Toast';
import { useAuth } from '../../auth/AuthContext';
import { ApplicationStatus, ApplicationType } from '../../lib/enums';

const TABS = [
  { key: 'all', label: 'All' },
  { key: 'active', label: 'Active', match: (a) => [ApplicationStatus.Submitted, ApplicationStatus.Accepted].includes(a.status) },
  { key: 'completed', label: 'Completed', match: (a) => a.status === ApplicationStatus.Attended },
  { key: 'closed', label: 'Not selected', match: (a) => [ApplicationStatus.Rejected, ApplicationStatus.NoShow, ApplicationStatus.Withdrawn].includes(a.status) },
];

export default function MyApplications() {
  const toast = useToast();
  const { profile } = useAuth();
  const { data, loading, error, reload } = useAsync(() => applicationsApi.mine(), []);
  // Teams are needed to tell whether a *team* application can be withdrawn by
  // the current user (only the captain may withdraw — members would get a 403).
  const { data: teamsData } = useAsync(() => teamsApi.mine().catch(() => []), []);
  const [tab, setTab] = useState('all');
  const [busyId, setBusyId] = useState(null);

  const apps = useMemo(() => data || [], [data]);
  const myVolunteerId = profile?.id;
  const captainTeamIds = useMemo(
    () => new Set((teamsData || []).filter((t) => t.captainVolunteerId === myVolunteerId).map((t) => t.id)),
    [teamsData, myVolunteerId],
  );

  /* A withdrawable application: must be Submitted AND the current user must be
     the one allowed to withdraw it — the individual applicant, or the captain
     of the applying team. Members of a team application cannot withdraw. */
  const canWithdraw = (a) => {
    if (a.status !== ApplicationStatus.Submitted) return false;
    if (a.applicationType === ApplicationType.Team) return captainTeamIds.has(a.teamId);
    return a.volunteerId == null || a.volunteerId === myVolunteerId;
  };
  const counts = useMemo(() => ({
    all: apps.length,
    active: apps.filter(TABS[1].match).length,
    completed: apps.filter(TABS[2].match).length,
    closed: apps.filter(TABS[3].match).length,
  }), [apps]);

  const current = TABS.find((t) => t.key === tab);
  const rows = tab === 'all' ? apps : apps.filter(current.match);

  const withdraw = async (a) => {
    if (!window.confirm(`Withdraw your application to "${a.opportunityTitle}"`)) return;
    setBusyId(a.id);
    try {
      await applicationsApi.withdraw(a.id);
      toast.info('Application withdrawn', 'You can re-apply any time.');
      reload();
    } catch (err) {
      toast.err('Could not withdraw', err.message);
    } finally {
      setBusyId(null);
    }
  };

  return (
    <main className="page">
      <div className="page-head">
        <div>
          <div className="ph-eyebrow">Your activity</div>
          <h1>My Applications</h1>
          <p className="ph-sub">Track the status of every opportunity you've applied to.</p>
        </div>
        <div className="ph-actions"><Link to="/app/opportunities" className="btn btn-primary btn-sm">Find more</Link></div>
      </div>

      <div className="tabs">
        {TABS.map((t) => (
          <button key={t.key} className={tab === t.key ? 'active' : ''} onClick={() => setTab(t.key)}>
            {t.label} <span className="cnt">{counts[t.key]}</span>
          </button>
        ))}
      </div>

      <AsyncBoundary
        loading={loading} error={error} onRetry={reload}
        empty={!loading && !error && rows.length === 0}
        emptyProps={{ icon: 'calendar', title: 'No applications here', message: 'Browse opportunities and apply to see them listed.' }}
        loadingLabel="Loading your applications…"
      >
        <div className="panel">
          <div className="tbl-wrap">
            <table className="tbl">
              <thead><tr><th>Opportunity</th><th>Type</th><th>Submitted</th><th>Reviewed</th><th>Status</th><th /></tr></thead>
              <tbody>
                {rows.map((a) => (
                  <tr key={a.id}>
                    <td>
                      <div className="cell-user">
                        <span className="mini-ava"><Icon name="briefcase" size={17} stroke={2} /></span>
                        <div><div className="td-main">{a.opportunityTitle}</div><div className="td-sub">{a.applicationType === ApplicationType.Team ? `Team${a.teamName ? ` · ${a.teamName}` : ''}` : 'Individual'}</div></div>
                      </div>
                    </td>
                    <td className="muted">{a.applicationType === ApplicationType.Team ? 'Team' : 'Individual'}</td>
                    <td>{new Date(a.submittedAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}</td>
                    <td className="muted">{a.reviewedAt ? new Date(a.reviewedAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric' }) : '—'}</td>
                    <td><ApplicationStatusPill status={a.status} /></td>
                    <td>
                      <div className="row-actions">
                        <Link to={`/app/opportunities/${a.opportunityId}`} className="icon-act" aria-label="View"><Icon name="eye" size={17} stroke={2} /></Link>
                        {canWithdraw(a) && (
                          <button className="icon-act no" aria-label="Withdraw" disabled={busyId === a.id} onClick={() => withdraw(a)}><Icon name="x" size={17} stroke={2} /></button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </AsyncBoundary>
    </main>
  );
}
