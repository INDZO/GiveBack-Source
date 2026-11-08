import { Link } from 'react-router-dom';
import Icon from '../../components/Icon';
import { Loading, ErrorState } from '../../components/States';
import { ApplicationStatusPill } from '../../components/StatusPill';
import { useAsync } from '../../lib/useAsync';
import { useAuth } from '../../auth/AuthContext';
import { volunteersApi, applicationsApi, opportunitiesApi } from '../../api/services';
import {
  ApplicationStatus,
  causeTone, toneIcon, OpportunityFormat,
} from '../../lib/enums';

const MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
const YEAR = new Date().getFullYear();

export default function VolunteerDashboard() {
  const { profile } = useAuth();

  const { data, loading, error, reload } = useAsync(async () => {
    // The volunteer profile (hours/badges) is primary data: a failure is a full
    // ErrorState. The applications/matching/report widgets load independently and
    // surface their own partial errors instead of silently showing empty lists.
    const prof = await volunteersApi.me();
    const [apps, matching, report] = await Promise.allSettled([
      applicationsApi.mine(),
      opportunitiesApi.matchingMe(),
      volunteersApi.annualReport(YEAR),
    ]);
    return {
      profile: prof,
      apps: apps.status === 'fulfilled' ? apps.value : [],
      appsError: apps.status === 'rejected' ? apps.reason : null,
      matching: matching.status === 'fulfilled' ? matching.value : [],
      matchingError: matching.status === 'rejected' ? matching.reason : null,
      report: report.status === 'fulfilled' ? report.value : null,
    };
  }, []);

  if (loading) return <main className="page"><Loading label="Loading your dashboard..." /></main>;
  if (error) return <main className="page"><ErrorState error={error} onRetry={reload} /></main>;

  const p = data.profile || profile || {};
  const apps = data.apps || [];
  const { appsError, matchingError } = data;
  const matching = data.matching || [];
  const report = data.report;

  const completed = apps.filter((a) => a.status === ApplicationStatus.Attended).length;
  const active = apps.filter((a) => [ApplicationStatus.Submitted, ApplicationStatus.Accepted].includes(a.status));
  const firstName = p.firstName || 'there';

  // Bar chart from annual report (last 6 months with data, scaled).
  const monthly = report?.hoursByMonth || [];
  const recent = MONTHS.map((m, i) => ({ label: m, hours: monthly.find((x) => x.month === i + 1)?.totalHours || 0 }))
    .slice(Math.max(0, new Date().getMonth() - 5), new Date().getMonth() + 1);
  const maxH = Math.max(1, ...recent.map((r) => r.hours));

  // Real hours logged this year (sum of the annual report's monthly buckets).
  // There is no "annual goal" concept in the backend, so we never fabricate one.
  const yearHours = monthly.reduce((s, x) => s + (x.totalHours || 0), 0);

  const KPIS = [
    { icon: 'clock', val: p.totalHours ?? 0, lbl: 'Verified volunteer hours' },
    { icon: 'checkCircle', tone: 'green', val: completed, lbl: 'Completed actions' },
    { icon: 'award', tone: 'orange', val: (p.badges?.length ?? 0), lbl: 'Badges earned' },
    { icon: 'users', tone: 'blue', val: (p.causes?.length ?? report?.causesCount ?? 0), lbl: 'Causes supported' },
  ];

  return (
    <main className="page">
      <div className="page-head">
        <div>
          <div className="ph-eyebrow">{new Date().toLocaleDateString('en-US', { weekday: 'long', month: 'long', day: 'numeric', year: 'numeric' })}</div>
          <h1>Welcome back, {firstName} 👋</h1>
          <p className="ph-sub">You have <b style={{ color: 'var(--ink)' }}>{active.length} active application{active.length === 1 ? '' : 's'}</b> and {p.totalHours ?? 0} verified hours so far.</p>
        </div>
        <div className="ph-actions">
          <Link to="/app/hours" className="btn btn-secondary btn-sm">Log hours</Link>
          <Link to="/app/opportunities" className="btn btn-primary btn-sm">Find opportunities <Icon name="arrowRight" size={16} /></Link>
        </div>
      </div>

      <div className="kpi-grid">
        {KPIS.map((k) => (
          <div className="kpi" key={k.lbl}>
            <div className="kpi-top"><div className={`kpi-ico ${k.tone || ''}`}><Icon name={k.icon} size={21} stroke={1.9} /></div></div>
            <div className="kpi-num">{k.val}</div>
            <div className="kpi-lbl">{k.lbl}</div>
          </div>
        ))}
      </div>

      <div className="col-2">
        <div className="stack">
          <section className="panel">
            <div className="panel-head">
              <h3><span className="ph-ico"><Icon name="calendar" size={16} stroke={2} /></span>Your applications</h3>
              <Link to="/app/applications" className="link">View all <Icon name="arrowRight" size={15} /></Link>
            </div>
            <div className="panel-body flush">
              {appsError && <ErrorState error={appsError} onRetry={reload} />}
              {!appsError && active.length === 0 && <div className="empty" style={{ padding: '40px 24px' }}><div className="empty-ico"><Icon name="calendar" size={24} /></div><h3>No active applications</h3><p>Browse opportunities and apply to get started.</p></div>}
              {!appsError && active.slice(0, 5).map((a) => (
                <div className="lrow" key={a.id}>
                  <div className="lrow-ico edu"><Icon name="briefcase" size={19} stroke={1.9} /></div>
                  <div className="lrow-body">
                    <div className="lrow-title">{a.opportunityTitle} <ApplicationStatusPill status={a.status} /></div>
                    <div className="lrow-meta"><span>Submitted {new Date(a.submittedAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}</span>{a.teamName && <span>Team: {a.teamName}</span>}</div>
                  </div>
                </div>
              ))}
            </div>
          </section>

          <section className="panel">
            <div className="panel-head">
              <h3><span className="ph-ico"><Icon name="star" size={16} stroke={2} /></span>Recommended for you</h3>
              <Link to="/app/opportunities" className="link">See all <Icon name="arrowRight" size={15} /></Link>
            </div>
            <div className="panel-body flush">
              {matchingError && <ErrorState error={matchingError} onRetry={reload} />}
              {!matchingError && matching.length === 0 && <div className="empty" style={{ padding: '40px 24px' }}><div className="empty-ico"><Icon name="search" size={24} /></div><h3>No matches yet</h3><p>Add skills and interests to your profile for sharper matches.</p></div>}
              {!matchingError && matching.slice(0, 4).map((o) => {
                const tone = causeTone(o.causeName);
                return (
                  <div className="lrow" key={o.id}>
                    <div className={`lrow-ico ${tone}`}><Icon name={toneIcon[tone] || 'leaf'} size={19} stroke={1.9} /></div>
                    <div className="lrow-body">
                      <div className="lrow-title">{o.title}</div>
                      <div className="lrow-meta"><span className={`cat ${tone}`}>{o.causeName}</span><span><Icon name="mapPin" size={12} stroke={2} />{o.format === OpportunityFormat.Online ? 'Online' : o.location}</span></div>
                    </div>
                    <div className="lrow-end"><Link to={`/app/opportunities/${o.id}`} className="btn btn-primary btn-sm">View</Link></div>
                  </div>
                );
              })}
            </div>
          </section>
        </div>

        <div className="stack">
          <section className="panel">
            <div className="panel-head"><h3><span className="ph-ico"><Icon name="barChart" size={16} stroke={2} /></span>Hours this year</h3></div>
            <div className="panel-body">
              <div className="flex items-center gap-16" style={{ marginBottom: 18 }}>
                <div>
                  <div className="muted" style={{ fontSize: '.86rem' }}>Verified hours in {YEAR}</div>
                  <div className="tnum" style={{ fontSize: '2rem', color: 'var(--purple)', margin: '4px 0 2px' }}>{yearHours}</div>
                  <div className="muted" style={{ fontSize: '.84rem' }}>{p.totalHours ?? 0} hours verified all-time</div>
                </div>
              </div>
              <div className="barchart" style={{ height: 120 }}>
                {recent.map((r, i) => {
                  const h = Math.max(6, Math.round((r.hours / maxH) * 92));
                  const hot = i === recent.length - 1;
                  return (
                    <div className="bc" key={r.label}>
                      <div className={`bc-bar${hot ? ' hot' : ''}`} style={{ height: h }}>{hot && r.hours > 0 && <span className="bc-val">{r.hours}h</span>}</div>
                      <div className="bc-lbl">{r.label}</div>
                    </div>
                  );
                })}
              </div>
            </div>
          </section>

          <section className="panel">
            <div className="panel-head"><h3><span className="ph-ico"><Icon name="award" size={16} stroke={2} /></span>Your badges</h3></div>
            <div className="panel-body">
              {(p.badges?.length ?? 0) === 0 && <p className="muted" style={{ fontSize: '.9rem' }}>Log your first verified hour to start earning badges.</p>}
              <div className="flex gap-8" style={{ flexWrap: 'wrap' }}>
                {(p.badges || []).map((b) => (
                  <span key={b.id} className="opp-skill" style={{ margin: 0 }}><Icon name="award" size={13} stroke={2} />{b.badgeName}</span>
                ))}
              </div>
              <div className="alert purple mt-16" style={{ padding: '13px 15px' }}>
                <Icon name="info" size={20} />
                <span>Keep logging verified hours to unlock the next milestone badge.</span>
              </div>
            </div>
          </section>
        </div>
      </div>
    </main>
  );
}
