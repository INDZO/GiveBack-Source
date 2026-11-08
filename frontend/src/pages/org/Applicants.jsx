import { useState } from 'react';
import Icon from '../../components/Icon';
import Modal from '../../components/Modal';
import { Loading, ErrorState, EmptyState, Spinner } from '../../components/States';
import { ApplicationStatusPill, HourLogStatusPill } from '../../components/StatusPill';
import { useAsync } from '../../lib/useAsync';
import { applicationsApi, hourLogsApi } from '../../api/services';
import { useToast } from '../../components/Toast';
import { ApplicationStatus, ApplicationType } from '../../lib/enums';
import { canMarkAttendance } from '../../lib/applications';
import { formatDateOnly } from '../../lib/dates';
import { ApiError } from '../../api/client';

export default function OrgApplicants() {
  const toast = useToast();
  const [tab, setTab] = useState('applicants');
  // Two independent datasets (one per tab). We capture each request's outcome so
  // a failure surfaces as an ErrorState in its tab instead of a misleading empty
  // list. The page only hard-fails if BOTH primary requests fail.
  const { data, loading, error, reload } = useAsync(async () => {
    const [apps, pending] = await Promise.allSettled([applicationsApi.forOrganization(), hourLogsApi.pending()]);
    if (apps.status === 'rejected' && pending.status === 'rejected') throw apps.reason;
    return {
      apps: apps.status === 'fulfilled' ? apps.value : [],
      appsError: apps.status === 'rejected' ? apps.reason : null,
      pending: pending.status === 'fulfilled' ? pending.value : [],
      pendingError: pending.status === 'rejected' ? pending.reason : null,
    };
  }, []);
  const [busy, setBusy] = useState(null);

  // Correction modal state (replaces the old window.prompt).
  const [correctFor, setCorrectFor] = useState(null);
  const [correctVal, setCorrectVal] = useState('');
  const [correctErr, setCorrectErr] = useState('');
  const [correctBusy, setCorrectBusy] = useState(false);

  if (loading) return <main className="page"><Loading label="Loading applicants..." /></main>;
  if (error) return <main className="page"><ErrorState error={error} onRetry={reload} /></main>;

  const { apps, appsError, pending, pendingError } = data;

  const act = async (key, fn, label) => {
    setBusy(key);
    try { await fn(); toast.ok(label); reload(); }
    catch (err) {
      // Refresh state so stale rows reflect what the backend actually allows.
      toast.err('Action failed', err.message);
      reload();
    } finally { setBusy(null); }
  };

  const openCorrect = (h) => {
    setCorrectFor(h);
    setCorrectVal(String(h.submittedHours));
    setCorrectErr('');
  };

  const submitCorrection = async (e) => {
    e?.preventDefault();
    const val = Number(correctVal);
    if (!correctVal || Number.isNaN(val) || val <= 0) { setCorrectErr('Enter a positive number of hours.'); return; }
    setCorrectBusy(true);
    setCorrectErr('');
    try {
      await hourLogsApi.correct(correctFor.id, val);
      toast.ok('Hours corrected & approved');
      setCorrectFor(null);
      reload();
    } catch (err) {
      setCorrectErr(err instanceof ApiError ? err.message : 'Could not correct hours. Please try again.');
    } finally {
      setCorrectBusy(false);
    }
  };

  return (
    <main className="page">
      <div className="page-head">
        <div>
          <div className="ph-eyebrow">Manage</div>
          <h1>Applicants &amp; Hours</h1>
          <p className="ph-sub">Review applications, confirm attendance, and verify volunteer hours.</p>
        </div>
      </div>

      <div className="tabs">
        <button className={tab === 'applicants' ? 'active' : ''} onClick={() => setTab('applicants')}>Applicants <span className="cnt">{apps.length}</span></button>
        <button className={tab === 'hours' ? 'active' : ''} onClick={() => setTab('hours')}>Hours to verify <span className="cnt">{pending.length}</span></button>
      </div>

      {tab === 'applicants' && (
        <div className="panel">
          {appsError ? <ErrorState error={appsError} onRetry={reload} />
            : apps.length === 0 ? <EmptyState icon="users" title="No applicants yet" message="Applications to your opportunities will appear here." /> : (
            <div className="tbl-wrap">
              <table className="tbl">
                <thead><tr><th>Volunteer</th><th>Opportunity</th><th>Applied</th><th>Status</th><th /></tr></thead>
                <tbody>
                  {apps.map((a) => (
                    <tr key={a.id}>
                      <td>
                        <div className="cell-user">
                          <span className="mini-ava">{(a.volunteerName || a.teamName || '').slice(0, 2).toUpperCase()}</span>
                          <div>
                            <div className="td-main">{a.volunteerName || a.teamName}</div>
                            <div className="td-sub">{a.applicationType === ApplicationType.Team ? 'Team application' : 'Individual'}</div>
                            {a.message && <div className="td-sub" style={{ marginTop: 4, maxWidth: 320, whiteSpace: 'normal', lineHeight: 1.45 }}>&quot;{a.message}&quot;</div>}
                          </div>
                        </div>
                      </td>
                      <td className="muted">{a.opportunityTitle}</td>
                      <td>{new Date(a.submittedAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}</td>
                      <td><ApplicationStatusPill status={a.status} /></td>
                      <td>
                        <div className="row-actions">
                          {a.status === ApplicationStatus.Submitted && (
                            <>
                              <button className="btn btn-success btn-sm" disabled={busy === `a-${a.id}`} onClick={() => act(`a-${a.id}`, () => applicationsApi.accept(a.id), 'Applicant accepted')}>Accept</button>
                              <button className="btn btn-danger btn-sm" disabled={busy === `a-${a.id}`} onClick={() => act(`a-${a.id}`, () => applicationsApi.reject(a.id), 'Applicant rejected')}>Reject</button>
                            </>
                          )}
                          {/* Attendance can only be marked once the opportunity has ended
                              (matches the backend rule). Until then, show when it unlocks. */}
                          {a.status === ApplicationStatus.Accepted && (
                            canMarkAttendance(a) ? (
                              <>
                                <button className="btn btn-secondary btn-sm" disabled={busy === `a-${a.id}`} onClick={() => act(`a-${a.id}`, () => applicationsApi.attended(a.id), 'Marked as attended')}>Attended</button>
                                <button className="icon-act no" aria-label="No-show" disabled={busy === `a-${a.id}`} onClick={() => act(`a-${a.id}`, () => applicationsApi.noShow(a.id), 'Marked as no-show')}><Icon name="x" size={16} stroke={2} /></button>
                              </>
                            ) : (
                              <span className="muted" style={{ fontSize: '.8rem' }}>Attendance opens after {formatDateOnly(a.opportunityEndAt || a.opportunityStartAt)}</span>
                            )
                          )}
                          {![ApplicationStatus.Submitted, ApplicationStatus.Accepted].includes(a.status) && <span className="muted" style={{ fontSize: '.82rem' }}>—</span>}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {tab === 'hours' && (
        <div className="panel">
          {pendingError ? <ErrorState error={pendingError} onRetry={reload} />
            : pending.length === 0 ? <EmptyState icon="clock" title="No hours to verify" message="Submitted hour logs awaiting verification will appear here." /> : (
            <div className="tbl-wrap">
              <table className="tbl">
                <thead><tr><th>Volunteer</th><th>Opportunity</th><th>Date</th><th>Submitted</th><th>Status</th><th /></tr></thead>
                <tbody>
                  {pending.map((h) => (
                    <tr key={h.id}>
                      <td><div className="td-main">{h.volunteerName}</div><div className="td-sub">{h.description}</div></td>
                      <td className="muted">{h.opportunityTitle}</td>
                      <td>{new Date(h.workDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}</td>
                      <td className="tnum">{h.submittedHours} h</td>
                      <td><HourLogStatusPill status={h.status} /></td>
                      <td>
                        <div className="row-actions">
                          <button className="btn btn-success btn-sm" disabled={busy === `h-${h.id}`} onClick={() => act(`h-${h.id}`, () => hourLogsApi.approve(h.id), 'Hours approved')}>Approve</button>
                          <button className="btn btn-secondary btn-sm" disabled={busy === `h-${h.id}`} onClick={() => openCorrect(h)}>Adjust</button>
                          <button className="icon-act no" aria-label="Reject" disabled={busy === `h-${h.id}`} onClick={() => act(`h-${h.id}`, () => hourLogsApi.reject(h.id), 'Hours rejected')}><Icon name="x" size={16} stroke={2} /></button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      <Modal
        open={!!correctFor} onClose={() => setCorrectFor(null)} width={460}
        title="Adjust & approve hours"
        footer={(
          <>
            <button className="btn btn-secondary btn-sm" onClick={() => setCorrectFor(null)}>Cancel</button>
            <button className="btn btn-primary btn-sm" onClick={submitCorrection} disabled={correctBusy}>{correctBusy ? <><Spinner size={15} /> Saving...</> : 'Correct & approve'}</button>
          </>
        )}
      >
        {correctFor && (
          <form onSubmit={submitCorrection}>
            <div className="lrow" style={{ padding: '0 0 14px' }}>
              <div className="lrow-body">
                <div className="lrow-title">{correctFor.volunteerName}</div>
                <div className="muted" style={{ fontSize: '.9rem' }}>{correctFor.opportunityTitle}</div>
                {correctFor.description && <div className="muted" style={{ fontSize: '.85rem', marginTop: 4 }}>{correctFor.description}</div>}
              </div>
            </div>
            <p className="muted" style={{ fontSize: '.9rem', marginBottom: 12 }}>
              Volunteer submitted <b style={{ color: 'var(--ink)' }}>{correctFor.submittedHours} h</b>. Approving with a different value records the log as <b style={{ color: 'var(--ink)' }}>Corrected</b> (still counts as verified).
            </p>
            <div className={`field ${correctErr ? 'invalid' : ''}`}>
              <label htmlFor="correct-hours">Approved hours <span className="req">*</span></label>
              <input
                id="correct-hours" type="number" min="0.5" step="0.5" autoFocus
                value={correctVal} onChange={(e) => { setCorrectVal(e.target.value); setCorrectErr(''); }}
                aria-invalid={!!correctErr} aria-describedby={correctErr ? 'correct-hours-err' : undefined}
              />
              <span className="error-msg" id="correct-hours-err" style={correctErr ? { display: 'flex' } : undefined}>
                <Icon name="alert" size={16} stroke={2} /><span className="em-text">{correctErr}</span>
              </span>
            </div>
          </form>
        )}
      </Modal>
    </main>
  );
}
