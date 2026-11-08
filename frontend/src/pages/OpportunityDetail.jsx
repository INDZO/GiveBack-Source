import { useEffect, useRef, useState } from 'react';
import { Link, useLocation, useNavigate, useParams } from 'react-router-dom';
import Icon from '../components/Icon';
import { AsyncBoundary } from '../components/States';
import { useAsync } from '../lib/useAsync';
import { opportunitiesApi, applicationsApi, teamsApi, impactApi } from '../api/services';
import { useAuth } from '../auth/AuthContext';
import { useToast } from '../components/Toast';
import { ApiError } from '../api/client';
import {
  causeTone, toneIcon, OpportunityFormat, OpportunityTypeLabel,
  OpportunityStatus, OpportunityStatusLabel, OpportunityStatusTone, UserRole,
} from '../lib/enums';
import { eligibleTeamsForApply } from '../lib/teams';

function fmtRange(o) {
  if (o.format === OpportunityFormat.Online && !o.startAt) return o.periodDescription || 'Flexible · Remote';
  try {
    const s = new Date(o.startAt);
    const opts = { weekday: 'short', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' };
    if (o.endAt) {
      const e = new Date(o.endAt);
      const sameDay = s.toDateString() === e.toDateString();
      return `${s.toLocaleString('en-US', opts)} – ${e.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })}${sameDay ? '' : ` (${e.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })})`}`;
    }
    return s.toLocaleString('en-US', opts);
  } catch { return o.periodDescription || 'Flexible'; }
}

export default function OpportunityDetail({ shell = false }) {
  const { id } = useParams();
  const { role, isAuthenticated, profile } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const toast = useToast();
  const basePath = shell ? '/app/opportunities' : '/opportunities';

  /* Back to wherever the user came from (e.g. a filtered list). location.key is
     'default' only when this page was opened directly (no in-app history to pop),
     so fall back to the opportunities list in that case. */
  const goBack = () => {
    if (location.key !== 'default') navigate(-1);
    else navigate(basePath);
  };

  const { data: o, loading, error, reload } = useAsync(() => opportunitiesApi.get(id), [id]);
  const { data: similarData } = useAsync(() => opportunitiesApi.list({ pageSize: 6 }), []);
  const { data: teamsData } = useAsync(
    () => (isAuthenticated && role === UserRole.Volunteer ? teamsApi.mine() : Promise.resolve([])),
    [isAuthenticated, role],
  );
  const { data: impactReport } = useAsync(() => impactApi.opportunityReport(id).catch(() => null), [id]);

  const [state, setState] = useState('idle'); // idle | pending | success | busy
  const [message, setMessage] = useState('');
  const [teamId, setTeamId] = useState('');
  const [retry, setRetry] = useState(0);
  const [result, setResult] = useState(null);
  const timerRef = useRef(null);

  useEffect(() => () => clearInterval(timerRef.current), []);

  useEffect(() => { window.scrollTo(0, 0); }, [id]);

  const startBusyCountdown = (seconds) => {
    setRetry(seconds);
    clearInterval(timerRef.current);
    timerRef.current = setInterval(() => {
      setRetry((s) => {
        if (s <= 1) { clearInterval(timerRef.current); return 0; }
        return s - 1;
      });
    }, 1000);
  };

  const submit = async (teamBased) => {
    if (!isAuthenticated) { navigate('/login', { state: { from: `${basePath}/${id}` } }); return; }
    if (role !== UserRole.Volunteer) { toast.info('Volunteers only', 'Switch to a volunteer account to apply.'); return; }
    if (state === 'pending') return; // prevent duplicate submissions
    if (teamBased && !Number(teamId)) {
      toast.err('Select a team', 'Choose a team you captain that has at least one confirmed member.');
      return;
    }
    setState('pending');
    try {
      const app = teamBased
        ? await applicationsApi.applyTeam(id, Number(teamId), message || null)
        : await applicationsApi.apply(id, message || null);
      setResult(app);
      setState('success');
      toast.ok('Application submitted', 'The organizer will review it shortly.');
      reload();
    } catch (err) {
      if (err instanceof ApiError && err.isBusy) {
        startBusyCountdown(err.retryAfter || 15);
        setState('busy');
        toast.err('Server busy (503)', `Retry-After: ${err.retryAfter || 15}s — your spot was not lost`);
      } else if (err instanceof ApiError && err.status === 409) {
        toast.err('Already applied', err.message);
        setState('idle');
      } else {
        toast.err('Could not apply', err.message || 'Please try again.');
        setState('idle');
      }
    }
  };

  const content = (oData) => {
    const tone = causeTone(oData.causeName);
  const filled = oData.requiredVolunteersCount ? Math.min(100, Math.round((oData.acceptedVolunteersCount / oData.requiredVolunteersCount) * 100)) : 0;
  const spotsLeft = Math.max(0, oData.requiredVolunteersCount - oData.acceptedVolunteersCount);
  const impactBars = impactReport ? [
    { label: 'Volunteers', value: impactReport.volunteersCount },
    { label: 'Hours', value: impactReport.totalHours },
    { label: 'Helped', value: impactReport.beneficiariesHelpedCount },
  ] : [];
  const maxImpact = Math.max(1, ...impactBars.map((b) => Number(b.value) || 0));
    const isOpen = oData.status === OpportunityStatus.Open;
    const teams = teamsData || [];
    const myVolunteerId = profile?.id;
    /* Teams the current volunteer can actually apply with: captain, no pending
       invitations, and at least one CONFIRMED member besides the captain —
       exactly what the backend ApplyTeam rules enforce (captain-only teams are
       rejected). */
    const eligibleTeams = eligibleTeamsForApply(teams, myVolunteerId);
    const captainTeamsCount = teams.filter((t) => t.captainVolunteerId === myVolunteerId).length;
    const similar = (Array.isArray(similarData) ? similarData : similarData?.items || []).filter((s) => s.id !== oData.id).slice(0, 3);

    return (
      <main className="page">
        <button type="button" className="back-btn" onClick={goBack}>
          <Icon name="arrowLeft" size={17} stroke={2.2} /> Back
        </button>
        <div className="crumb">
          <Link to={basePath}>Opportunities</Link>
          <Icon name="chevronDown" size={14} style={{ transform: 'rotate(-90deg)' }} />
          <span className="cur">{oData.title}</span>
        </div>

        <div className="detail-grid">
          <div>
            <div className="detail-hero">
              <div className={`dh-cover ${tone}`}>
                <span className={`ocard-cat cat ${tone}`} style={{ top: 16, left: 16 }}>{oData.causeName}</span>
                <Icon name={toneIcon[tone] || 'leaf'} size={64} stroke={1.4} />
              </div>
            </div>

            <div className="flex items-center gap-12" style={{ margin: '18px 0 10px' }}>
              <span className={`mini-ava ${tone === 'env' ? 'g2' : ''}`}>{(oData.organizationName || 'OR').slice(0, 2).toUpperCase()}</span>
              <div>
                <div style={{ fontWeight: 600, display: 'flex', alignItems: 'center', gap: 7 }}>
                  {oData.organizationName}
                  <span className="st verified" style={{ padding: '3px 9px' }}><Icon name="shieldCheck" size={12} stroke={2.4} />Verified org</span>
                </div>
                <div className="muted" style={{ fontSize: '.82rem' }}>{oData.causeName} · {oData.format === OpportunityFormat.Online ? 'Online' : oData.location}</div>
              </div>
            </div>
            <h1 style={{ fontSize: '2rem', letterSpacing: '-.025em' }}>{oData.title}</h1>

            <div className="detail-meta">
              <div className="dm-item"><div className="dm-ico"><Icon name="mapPin" size={18} stroke={2} /></div><div><div className="dm-l">Location</div><div className="dm-v">{oData.format === OpportunityFormat.Online ? 'Online' : oData.location}</div></div></div>
              <div className="dm-item"><div className="dm-ico"><Icon name="clock" size={18} stroke={2} /></div><div><div className="dm-l">Date &amp; time</div><div className="dm-v">{fmtRange(oData)}</div></div></div>
              <div className="dm-item"><div className="dm-ico"><Icon name="users" size={18} stroke={2} /></div><div><div className="dm-l">Format</div><div className="dm-v">{oData.format === OpportunityFormat.Online ? 'Online' : 'In person'}{oData.isTeamBased ? ' · Team friendly' : ''}</div></div></div>
              <div className="dm-item"><div className="dm-ico"><Icon name="zap" size={18} stroke={2} /></div><div><div className="dm-l">Duration</div><div className="dm-v">≈ {oData.durationHours} volunteer hours · {OpportunityTypeLabel[oData.type]}</div></div></div>
            </div>

            <div className="prose">
              <p>{oData.description}</p>
            </div>

            {(oData.skillNames?.length > 0) && (
              <div className="flex gap-8" style={{ flexWrap: 'wrap', marginTop: 16 }}>
                {oData.skillNames.map((s) => <span key={s} className="opp-skill" style={{ margin: 0 }}>{s}</span>)}
              </div>
            )}

            {impactReport && (
              <section className="panel mt-24">
                <div className="panel-head"><h3><span className="ph-ico"><Icon name="barChart" size={16} /></span>Impact report</h3></div>
                <div className="panel-body">
                  <div className="impact-report-grid">
                    <div>
                      <div className="badge-grid impact-metrics">
                        <div className="bdg" style={{ textAlign: 'left' }}><div className="tnum" style={{ fontSize: '1.45rem', color: 'var(--purple)' }}>{impactReport.volunteersCount}</div><div className="muted" style={{ fontSize: '.82rem' }}>Volunteers</div></div>
                        <div className="bdg" style={{ textAlign: 'left' }}><div className="tnum" style={{ fontSize: '1.45rem', color: 'var(--green)' }}>{impactReport.totalHours}</div><div className="muted" style={{ fontSize: '.82rem' }}>Verified hours</div></div>
                        <div className="bdg" style={{ textAlign: 'left' }}><div className="tnum" style={{ fontSize: '1.45rem', color: 'var(--orange)' }}>{impactReport.beneficiariesHelpedCount}</div><div className="muted" style={{ fontSize: '.82rem' }}>People helped</div></div>
                      </div>
                      <p className="muted" style={{ lineHeight: 1.7, marginTop: 16 }}>{impactReport.summary}</p>
                    </div>
                    <div className="barchart impact-chart" style={{ height: 210 }}>
                      {impactBars.map((b) => {
                        const value = Number(b.value) || 0;
                        return (
                          <div className="bc" key={b.label}>
                            <div className={`bc-bar${value > 0 ? ' hot' : ''}`} style={{ height: Math.max(10, Math.round((value / maxImpact) * 160)) }}>
                              {value > 0 && <span className="bc-val">{value}</span>}
                            </div>
                            <div className="bc-lbl">{b.label}</div>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                </div>
              </section>
            )}
          </div>

          {/* Apply sidebar */}
          <aside className="apply-box">
            <div className="ab-head">
              <div className="cap-meter">
                <div className="cm-top">
                  <span className="cm-num">{oData.acceptedVolunteersCount}<small>/{oData.requiredVolunteersCount} spots</small></span>
                  <span className={`st ${OpportunityStatusTone[oData.status] === 'green' ? 'open' : OpportunityStatusTone[oData.status] === 'red' ? 'rejected' : 'noshow'}`} style={{ padding: '4px 10px' }}><span className="stdot" />{OpportunityStatusLabel[oData.status]}</span>
                </div>
                <div className="progress"><span style={{ width: `${filled}%` }} /></div>
                <div className="cm-lbl mt-8">{spotsLeft} spot{spotsLeft === 1 ? '' : 's'} left</div>
              </div>
            </div>
            <div className="ab-body">
              {!isOpen && state === 'idle' && (
                <div className="alert amber"><Icon name="alert" size={20} /><span>This opportunity is {OpportunityStatusLabel[oData.status].toLowerCase()} and is no longer accepting applications.</span></div>
              )}

              {state === 'idle' && isOpen && (
                <div className="qstate show">
                  {oData.isTeamBased && (
                    <div className="alert purple" style={{ padding: '11px 13px', marginBottom: 14 }}>
                      <Icon name="users" size={18} />
                      <span style={{ fontSize: '.84rem' }}>This is a <b>team opportunity</b>. Only a team captain can apply, on behalf of a team with confirmed members.</span>
                    </div>
                  )}
                  <div className="field" style={{ marginBottom: 14 }}>
                    <label>Message to organizer <span className="opt">optional</span></label>
                    <textarea placeholder={`Tell ${oData.organizationName} why you'd like to join…`} style={{ minHeight: 80 }} value={message} onChange={(e) => setMessage(e.target.value)} />
                  </div>

                  {/* Team-based opportunities have NO individual apply path. */}
                  {oData.isTeamBased ? (
                    isAuthenticated && role === UserRole.Volunteer ? (
                      eligibleTeams.length > 0 ? (
                        <>
                          <div className="field" style={{ marginBottom: 14 }}>
                            <label>Apply with team</label>
                            <select value={teamId} onChange={(e) => setTeamId(e.target.value)}>
                              <option value="">Select a team…</option>
                              {eligibleTeams.map((t) => <option key={t.id} value={t.id}>{t.name}</option>)}
                            </select>
                            <span className="hint">Only teams you captain with at least one confirmed member besides you (and no pending invites) are listed.</span>
                          </div>
                          <button className="btn btn-primary" style={{ width: '100%' }} disabled={!teamId} onClick={() => submit(true)}>
                            Apply as team <Icon name="arrowRight" size={18} />
                          </button>
                        </>
                      ) : (
                        <div className="field">
                          <div className="alert amber" style={{ padding: '11px 13px' }}>
                            <Icon name="alert" size={18} />
                            <span style={{ fontSize: '.84rem' }}>
                              {captainTeamsCount > 0
                                ? 'Add at least one confirmed team member (besides you) before applying — a captain-only team cannot apply.'
                                : "You don't captain a team yet. Create a team, invite members, and have at least one confirm before applying."}
                            </span>
                          </div>
                          <Link to="/app/teams" className="btn btn-primary" style={{ width: '100%', marginTop: 12 }}>Go to My Teams <Icon name="arrowRight" size={18} /></Link>
                        </div>
                      )
                    ) : (
                      <button className="btn btn-primary" style={{ width: '100%' }} onClick={() => submit(true)}>
                        {isAuthenticated ? 'Volunteers only' : 'Log in to apply'} <Icon name="arrowRight" size={18} />
                      </button>
                    )
                  ) : (
                    <>
                      <button className="btn btn-primary" style={{ width: '100%' }} onClick={() => submit(false)}>
                        {isAuthenticated ? 'Apply now' : 'Log in to apply'} <Icon name="arrowRight" size={18} />
                      </button>
                      <p className="muted" style={{ fontSize: '.8rem', textAlign: 'center', marginTop: 12 }}>You'll get a confirmation when the organizer reviews your application.</p>
                    </>
                  )}
                </div>
              )}

              {state === 'pending' && (
                <div className="qstate show">
                  <div className="q-center">
                    <div className="q-spinner" />
                    <div className="muted" style={{ fontSize: '.86rem', marginBottom: 4 }}>Submitting your application…</div>
                    <div className="muted" style={{ fontSize: '.86rem', marginTop: 6 }}>Holding your spot — this is a popular opportunity</div>
                  </div>
                  <div className="alert purple mt-16" style={{ padding: '12px 14px' }}>
                    <Icon name="code" size={20} />
                    <span style={{ fontSize: '.84rem' }}>Many people may be applying at once. The concurrency limiter processes a fixed number of requests at a time so no spot is ever double-booked.</span>
                  </div>
                </div>
              )}

              {state === 'success' && (
                <div className="qstate show">
                  <div className="q-center">
                    <div className="q-success-ico"><Icon name="check" size={28} stroke={2.4} /></div>
                    <h3 style={{ fontSize: '1.25rem' }}>You're in!</h3>
                    <p className="muted" style={{ fontSize: '.9rem', marginTop: 6 }}>Your application was submitted{result.teamName ? ` for team ${result.teamName}` : ''}. {oData.organizationName} will confirm shortly.</p>
                  </div>
                  <Link to="/app/applications" className="btn btn-primary" style={{ width: '100%', marginTop: 18 }}>Track my application</Link>
                  <button className="btn btn-secondary" style={{ width: '100%', marginTop: 10 }} onClick={() => setState('idle')}>Close</button>
                </div>
              )}

              {state === 'busy' && (
                <div className="qstate show">
                  <div className="q-center">
                    <div className="q-warn-ico"><Icon name="alert" size={26} stroke={2} /></div>
                    <h3 style={{ fontSize: '1.25rem' }}>We're at capacity right now</h3>
                    <p className="muted" style={{ fontSize: '.9rem', marginTop: 6 }}>Too many applications are being processed at once. Your request was safely declined rather than risk an overbooking — please try again in a moment.</p>
                    <span className="retry-badge">HTTP 503 · Retry-After: {retry}s</span>
                  </div>
                  <button className="btn btn-primary" style={{ width: '100%', marginTop: 14 }} disabled={retry > 0} onClick={() => submit(oData.isTeamBased)}>
                    {retry > 0 ? `Retry in ${retry}s` : 'Retry now'}
                  </button>
                </div>
              )}
            </div>
          </aside>
        </div>

        {similar.length > 0 && (
          <>
            <h2 className="section-label" style={{ marginTop: 52 }}>More like this</h2>
            <div className="opp-grid">
              {similar.map((s) => {
                const t = causeTone(s.causeName);
                return (
                  <article className="ocard" key={s.id}>
                    <div className={`ocard-top ${t}`}><span className={`ocard-cat cat ${t}`}>{s.causeName}</span><Icon name={toneIcon[t] || 'leaf'} size={46} stroke={1.5} /></div>
                    <div className="ocard-body">
                      <div className="ocard-org">{s.organizationName}</div>
                      <h3>{s.title}</h3>
                      <div className="ocard-meta"><div><Icon name="mapPin" size={15} stroke={2} /><span className="ml">{s.format === OpportunityFormat.Online ? 'Online' : s.location}</span></div></div>
                      <div className="ocard-foot"><Link to={`${basePath}/${s.id}`} className="btn btn-primary btn-sm">View</Link></div>
                    </div>
                  </article>
                );
              })}
            </div>
          </>
        )}
      </main>
    );
  };

  return (
    <AsyncBoundary loading={loading} error={error} onRetry={reload} loadingLabel="Loading opportunity…">
      {o ? content(o) : null}
    </AsyncBoundary>
  );
}
