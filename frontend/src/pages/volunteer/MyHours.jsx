import { useState } from 'react';
import Icon from '../../components/Icon';
import Modal from '../../components/Modal';
import { Loading, ErrorState, Spinner, EmptyState } from '../../components/States';
import { HourLogStatusPill } from '../../components/StatusPill';
import { useAsync } from '../../lib/useAsync';
import { hourLogsApi, certificatesApi, applicationsApi, volunteersApi } from '../../api/services';
import { useToast } from '../../components/Toast';
import { useAuth } from '../../auth/AuthContext';
import { ApplicationStatus, HourLogStatus } from '../../lib/enums';
import { sumVerifiedHours } from '../../lib/hours';
import { todayLocalDateString, toDateOnly, formatDateOnly } from '../../lib/dates';
import { ApiError } from '../../api/client';

const YEAR = new Date().getFullYear();

function dateOnlyToLocalTime(value) {
  const [year, month, day] = toDateOnly(value).split('-').map(Number);
  if (!year || !month || !day) return null;
  return new Date(year, month - 1, day).getTime();
}

function applicationEndsAtDateOnly(application) {
  if (application.opportunityEndAt) return toDateOnly(application.opportunityEndAt);
  if (!application.opportunityStartAt) return '';
  const start = new Date(application.opportunityStartAt);
  const hours = Number(application.opportunityDurationHours) || 0;
  return toDateOnly(new Date(start.getTime() + hours * 3600 * 1000).toISOString());
}

function canLogApplicationOnDate(application, workDate) {
  const work = dateOnlyToLocalTime(workDate);
  const start = dateOnlyToLocalTime(application.opportunityStartAt);
  const end = dateOnlyToLocalTime(applicationEndsAtDateOnly(application));
  if (!work || !start || !end) return false;
  return work >= start && work <= end;
}

function localEarnedBadges(hours) {
  const verified = hours.filter((h) => h.approvedHours != null);
  const total = verified.reduce((sum, h) => sum + (h.approvedHours || 0), 0);
  const badges = [];
  if (total > 0) {
    badges.push({
      id: 'local-first-hour',
      badgeName: 'First hour',
      badgeDescription: 'Earned after your first verified volunteer hour.',
    });
  }
  if (total >= 100) {
    badges.push({
      id: 'local-100-hours',
      badgeName: '100 hours',
      badgeDescription: 'Earned after reaching 100 verified volunteer hours.',
    });
  }

  const now = new Date();
  const currentMonthDays = new Set(
    verified
      .map((h) => toDateOnly(h.workDate))
      .filter((date) => {
        const [year, month] = date.split('-').map(Number);
        return year === now.getFullYear() && month === now.getMonth() + 1;
      }),
  );
  if (currentMonthDays.size >= 4) {
    badges.push({
      id: 'local-monthly-regular',
      badgeName: 'Monthly regular',
      badgeDescription: 'Earned for volunteering on four different days in one month.',
    });
  }
  return badges;
}

export default function MyHours() {
  const toast = useToast();
  const { profile } = useAuth();

  const { data, loading, error, reload } = useAsync(async () => {
    // Logged hours is the primary data for this page: if it fails the whole
    // page shows an ErrorState (not a misleading empty list). The secondary
    // widgets (certs/apps/badges) are allowed to fail independently.
    const hours = await hourLogsApi.mine();
    const [certs, apps, badges] = await Promise.allSettled([
      certificatesApi.mine(), applicationsApi.mine(), volunteersApi.badges(),
    ]);
    return {
      hours,
      certs: certs.status === 'fulfilled' ? certs.value : [],
      apps: apps.status === 'fulfilled' ? apps.value : [],
      badges: badges.status === 'fulfilled' ? badges.value : [],
    };
  }, []);

  const [modal, setModal] = useState(false);
  const [form, setForm] = useState({ applicationId: '', workDate: todayLocalDateString(), hours: '', description: '' });
  const [errors, setErrors] = useState({});
  const [saving, setSaving] = useState(false);
  const [genBusy, setGenBusy] = useState(false);
  const [dlId, setDlId] = useState(null);
  const [certModal, setCertModal] = useState(false);
  const [certForm, setCertForm] = useState({ from: `${YEAR}-01-01`, to: todayLocalDateString() });
  const [certError, setCertError] = useState('');

  if (loading) return <main className="page"><Loading label="Loading your hours…" /></main>;
  if (error) return <main className="page"><ErrorState error={error} onRetry={reload} /></main>;

  const { hours, certs, apps, badges } = data;
  // Verified = Approved + Corrected (both carry an approvedHours value).
  const totalVerified = sumVerifiedHours(hours);
  const pending = hours.filter((h) => h.status === HourLogStatus.Pending).reduce((s, h) => s + (h.submittedHours || 0), 0);
  const loggedApplicationIdsForDate = new Set(
    hours
      .filter((h) => toDateOnly(h.workDate) === form.workDate)
      .map((h) => h.applicationId),
  );
  const loggable = apps.filter((a) => (
    [ApplicationStatus.Accepted, ApplicationStatus.Attended].includes(a.status) &&
    canLogApplicationOnDate(a, form.workDate) &&
    !loggedApplicationIdsForDate.has(a.id)
  ));

  const set = (k) => (e) => { setForm((f) => ({ ...f, [k]: e.target.value })); setErrors((x) => ({ ...x, [k]: undefined })); };

  const submitHours = async (e) => {
    e.preventDefault();
    const er = {};
    if (!form.applicationId) er.applicationId = 'Select the activity you volunteered for.';
    else if (!loggable.some((a) => String(a.id) === String(form.applicationId))) {
      er.applicationId = 'This activity cannot be logged for the selected date.';
    }
    if (!form.hours || Number(form.hours) <= 0) er.hours = 'Enter the number of hours.';
    if (!form.description.trim()) er.description = 'Add a short description.';
    setErrors(er);
    if (Object.keys(er).length) return;
    setSaving(true);
    try {
      await hourLogsApi.create({
        applicationId: Number(form.applicationId),
        // Date-only field: submit the plain local calendar day (YYYY-MM-DD). The
        // backend binds it to a DateTime at unspecified midnight, so the day is
        // preserved regardless of timezone (no UTC round-trip / off-by-one).
        workDate: form.workDate,
        submittedHours: Number(form.hours),
        description: form.description.trim(),
      });
      toast.ok('Hours submitted', 'The organization will review and verify them.');
      setModal(false);
      setForm({ applicationId: '', workDate: new Date().toISOString().slice(0, 10), hours: '', description: '' });
      reload();
    } catch (err) {
      if (err instanceof ApiError && err.isBusy) toast.err('Service busy', 'Please try again shortly.');
      else if (err instanceof ApiError && err.status === 409) {
        const message = err.message && err.message !== 'Conflict'
          ? err.message
          : 'This activity cannot be logged for the selected date, or an hour log already exists for it.';
        toast.err('Could not submit hours', message);
        reload();
      } else toast.err('Could not submit hours', err.message);
    } finally {
      setSaving(false);
    }
  };

  const generateCert = async (e) => {
    e.preventDefault();
    if (!certForm.from || !certForm.to) { setCertError('Choose a start and end date.'); return; }
    if (new Date(certForm.from) > new Date(certForm.to)) { setCertError('The start date must be before the end date.'); return; }
    setCertError('');
    setGenBusy(true);
    try {
      await certificatesApi.generate({
        // Date-only period: submit plain local calendar days (YYYY-MM-DD). The
        // backend truncates with .Date and compares inclusively, so the full
        // selected "to" day is covered with no timezone shift.
        periodFrom: certForm.from,
        periodTo: certForm.to,
      });
      toast.ok('Certificate generated', 'Your participation certificate is ready to download.');
      setCertModal(false);
      reload();
    } catch (err) {
      // Backend returns 409 (ConflictException) when no verified hours exist in
      // the period, and 400 for invalid input — surface both inline.
      if (err instanceof ApiError && (err.status === 409 || err.status === 400)) setCertError(err.message || 'No verified hours in this period.');
      else toast.err('Could not generate certificate', err.message);
    } finally {
      setGenBusy(false);
    }
  };

  const download = async (cert) => {
    setDlId(cert.id);
    try {
      const res = await certificatesApi.download(cert.id);
      const blob = await res.blob();
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `giveback-certificate-${cert.id}.pdf`;
      document.body.appendChild(a); a.click(); a.remove();
      URL.revokeObjectURL(url);
    } catch (err) {
      toast.err('Could not download', err.message);
    } finally {
      setDlId(null);
    }
  };

  const allBadges = badges.length ? badges : ((profile?.badges?.length ? profile.badges : localEarnedBadges(hours)) || []);

  return (
    <main className="page">
      <div className="page-head">
        <div>
          <div className="ph-eyebrow">Your record</div>
          <h1>Hours &amp; Certificates</h1>
          <p className="ph-sub">Log completed hours, track verification, and download your certificates.</p>
        </div>
        <div className="ph-actions">
          <button className="btn btn-secondary btn-sm" onClick={() => { setCertError(''); setCertModal(true); }} disabled={genBusy}>
            <Icon name="award" size={16} /> Generate certificate
          </button>
          <button className="btn btn-primary btn-sm" onClick={() => setModal(true)}><Icon name="plus" size={16} /> Log hours</button>
        </div>
      </div>

      <div className="kpi-grid">
        <div className="kpi"><div className="kpi-top"><div className="kpi-ico"><Icon name="clock" size={21} stroke={1.9} /></div></div><div className="kpi-num">{totalVerified}</div><div className="kpi-lbl">Verified hours</div></div>
        <div className="kpi"><div className="kpi-top"><div className="kpi-ico orange"><Icon name="clock" size={21} stroke={1.9} /></div></div><div className="kpi-num">{pending}</div><div className="kpi-lbl">Pending verification</div></div>
        <div className="kpi"><div className="kpi-top"><div className="kpi-ico green"><Icon name="fileText" size={21} stroke={1.9} /></div></div><div className="kpi-num">{certs.length}</div><div className="kpi-lbl">Certificates</div></div>
        <div className="kpi"><div className="kpi-top"><div className="kpi-ico blue"><Icon name="award" size={21} stroke={1.9} /></div></div><div className="kpi-num">{allBadges.length}</div><div className="kpi-lbl">Badges earned</div></div>
      </div>

      <div className="col-2">
        <div className="stack">
          <section className="panel">
            <div className="panel-head"><h3><span className="ph-ico"><Icon name="clock" size={16} stroke={2} /></span>Logged hours</h3></div>
            <div className="panel-body flush">
              {hours.length === 0 && <EmptyState icon="clock" title="No hours logged yet" message="Log hours for activities you've attended." />}
              {hours.length > 0 && (
                <div className="tbl-wrap">
                  <table className="tbl">
                    <thead><tr><th>Activity</th><th>Date</th><th>Submitted</th><th>Approved</th><th>Status</th></tr></thead>
                    <tbody>
                      {hours.map((h) => (
                        <tr key={h.id}>
                          <td><div className="td-main">{h.opportunityTitle}</div><div className="td-sub">{h.description}</div></td>
                          <td>{formatDateOnly(h.workDate)}</td>
                          <td className="tnum">{h.submittedHours} h</td>
                          <td className="tnum" style={{ color: 'var(--purple)' }}>{h.approvedHours != null ? `${h.approvedHours} h` : '—'}</td>
                          <td><HourLogStatusPill status={h.status} /></td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          </section>

          <section className="panel">
            <div className="panel-head"><h3><span className="ph-ico"><Icon name="fileText" size={16} stroke={2} /></span>Certificates</h3></div>
            <div className="panel-body flush">
              {certs.length === 0 && <EmptyState icon="fileText" title="No certificates yet" message="Generate a certificate once you have verified hours." />}
              {certs.map((c) => (
                <div className="lrow" key={c.id}>
                  <div className="lrow-ico edu"><Icon name="award" size={19} stroke={1.9} /></div>
                  <div className="lrow-body">
                    <div className="lrow-title">{c.totalHours} Volunteer Hours</div>
                    <div className="lrow-meta"><span>{new Date(c.periodFrom).toLocaleDateString('en-US', { month: 'short', year: 'numeric' })} – {new Date(c.periodTo).toLocaleDateString('en-US', { month: 'short', year: 'numeric' })}</span><span>Generated {new Date(c.generatedAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}</span></div>
                  </div>
                  <div className="lrow-end"><button className="btn btn-secondary btn-sm" disabled={dlId === c.id} onClick={() => download(c)}>{dlId === c.id ? <Spinner size={15} /> : <><Icon name="download" size={15} /> PDF</>}</button></div>
                </div>
              ))}
            </div>
          </section>
        </div>

        <div className="stack">
          <section className="panel">
            <div className="panel-head"><h3><span className="ph-ico"><Icon name="award" size={16} stroke={2} /></span>Badges</h3></div>
            <div className="panel-body">
              {allBadges.length === 0 && <p className="muted" style={{ fontSize: '.9rem' }}>Earn your first badge by logging a verified hour.</p>}
              <div className="badge-grid">
                {allBadges.map((b) => (
                  <div className="bdg" key={b.id}>
                    <div className="bdg-medal"><Icon name="award" size={22} stroke={1.9} /></div>
                    <div className="bdg-name">{b.badgeName}</div>
                    <div className="bdg-desc">{b.badgeDescription}</div>
                    {b.awardedAt && <div className="bdg-date">{new Date(b.awardedAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })}</div>}
                  </div>
                ))}
              </div>
            </div>
          </section>
        </div>
      </div>

      <Modal
        open={modal} onClose={() => setModal(false)} title="Log volunteer hours"
        footer={(
          <>
            <button className="btn btn-secondary btn-sm" onClick={() => setModal(false)}>Cancel</button>
            <button className="btn btn-primary btn-sm" onClick={submitHours} disabled={saving}>{saving ? <><Spinner size={15} /> Submitting…</> : 'Submit hours'}</button>
          </>
        )}
      >
        <form onSubmit={submitHours}>
          <div className={`field ${errors.applicationId ? 'invalid' : ''}`}>
            <label>Activity <span className="req">*</span></label>
            <select value={form.applicationId} onChange={set('applicationId')}>
              <option value="">Select an activity you joined</option>
              {loggable.map((a) => <option key={a.id} value={a.id}>{a.opportunityTitle}</option>)}
            </select>
            <span className="error-msg"><Icon name="alert" size={16} stroke={2} /><span className="em-text">{errors.applicationId}</span></span>
            {loggable.length === 0 && (
              <span className="hint">
                {apps.some((a) => [ApplicationStatus.Accepted, ApplicationStatus.Attended].includes(a.status))
                  ? 'All accepted activities already have an hour log for this date. Pick another date if the activity happened on multiple days.'
                  : 'You can log hours once an organization accepts your application.'}
              </span>
            )}
          </div>
          <div className="field">
            <label>Date volunteered</label>
            <input
              type="date"
              value={form.workDate}
              onChange={(e) => {
                setForm((f) => ({ ...f, workDate: e.target.value, applicationId: '' }));
                setErrors((x) => ({ ...x, workDate: undefined, applicationId: undefined }));
              }}
            />
          </div>
          <div className={`field ${errors.hours ? 'invalid' : ''}`}>
            <label>Hours worked <span className="req">*</span></label>
            <input type="number" min="0.5" step="0.5" placeholder="e.g. 4" value={form.hours} onChange={set('hours')} />
            <span className="error-msg"><Icon name="alert" size={16} stroke={2} /><span className="em-text">{errors.hours}</span></span>
          </div>
          <div className={`field ${errors.description ? 'invalid' : ''}`}>
            <label>Short description <span className="req">*</span></label>
            <textarea placeholder="What did you do during this activity" value={form.description} onChange={set('description')} />
            <span className="error-msg"><Icon name="alert" size={16} stroke={2} /><span className="em-text">{errors.description}</span></span>
          </div>
        </form>
      </Modal>

      <Modal
        open={certModal} onClose={() => setCertModal(false)} title="Generate participation certificate"
        footer={(
          <>
            <button className="btn btn-secondary btn-sm" onClick={() => setCertModal(false)}>Cancel</button>
            <button className="btn btn-primary btn-sm" onClick={generateCert} disabled={genBusy}>{genBusy ? <><Spinner size={15} /> Generating…</> : 'Generate'}</button>
          </>
        )}
      >
        <form onSubmit={generateCert}>
          <p className="muted" style={{ fontSize: '.9rem', marginBottom: 14 }}>Choose the period to cover. The certificate totals your approved volunteer hours within these dates.</p>
          <div className="form-grid">
            <div className="field">
              <label>From <span className="req">*</span></label>
              <input type="date" value={certForm.from} max={certForm.to || undefined} onChange={(e) => { setCertForm((f) => ({ ...f, from: e.target.value })); setCertError(''); }} />
            </div>
            <div className="field">
              <label>To <span className="req">*</span></label>
              <input type="date" value={certForm.to} min={certForm.from || undefined} onChange={(e) => { setCertForm((f) => ({ ...f, to: e.target.value })); setCertError(''); }} />
            </div>
          </div>
          {certError && <span className="error-msg" style={{ display: 'flex' }}><Icon name="alert" size={16} stroke={2} /><span className="em-text">{certError}</span></span>}
          {totalVerified === 0 && <span className="hint">You currently have no approved hours — generation may return no certificate until hours are verified.</span>}
        </form>
      </Modal>
    </main>
  );
}
