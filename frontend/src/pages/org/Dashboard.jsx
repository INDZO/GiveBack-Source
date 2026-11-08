import { Link } from 'react-router-dom';
import Icon from '../../components/Icon';
import { Loading, ErrorState, EmptyState } from '../../components/States';
import { ApplicationStatusPill, OpportunityStatusPill } from '../../components/StatusPill';
import { useAsync } from '../../lib/useAsync';
import { organizationsApi, applicationsApi, hourLogsApi } from '../../api/services';
import { ApplicationStatus } from '../../lib/enums';

export default function OrgDashboard() {
  const { data, loading, error, reload } = useAsync(async () => {
    // The organization profile is the page's primary data: if it fails, the whole
    // page is an ErrorState. The dashboard widgets load independently and surface
    // their own partial errors rather than silently showing zeros/empty lists.
    const me = await organizationsApi.me();
    const [stats, opps, apps, pending] = await Promise.allSettled([
      organizationsApi.statistics(), organizationsApi.myOpportunities(),
      applicationsApi.forOrganization(), hourLogsApi.pending(),
    ]);
    return {
      me,
      stats: stats.status === 'fulfilled' ? stats.value : null,
      statsError: stats.status === 'rejected' ? stats.reason : null,
      opps: opps.status === 'fulfilled' ? opps.value : [],
      oppsError: opps.status === 'rejected' ? opps.reason : null,
      apps: apps.status === 'fulfilled' ? apps.value : [],
      appsError: apps.status === 'rejected' ? apps.reason : null,
      pending: pending.status === 'fulfilled' ? pending.value : [],
    };
  }, []);

  if (loading) return <main className="page"><Loading label="Loading your dashboard..." /></main>;
  if (error) return <main className="page"><ErrorState error={error} onRetry={reload} /></main>;

  const { me, stats, statsError, opps, oppsError, apps, appsError, pending } = data;
  const newApps = apps.filter((a) => a.status === ApplicationStatus.Submitted);

  const KPIS = [
    { icon: 'briefcase', val: stats?.totalPublishedOpportunities ?? opps.length, lbl: 'Published opportunities' },
    { icon: 'users', tone: 'green', val: stats?.totalVolunteers ?? 0, lbl: 'Volunteers engaged' },
    { icon: 'clock', tone: 'orange', val: stats?.totalApprovedHours ?? 0, lbl: 'Approved hours' },
    { icon: 'heartHand', tone: 'blue', val: stats?.totalBeneficiariesHelped ?? 0, lbl: 'Beneficiaries helped' },
  ];

  return (
    <main className="page">
      <div className="page-head">
        <div>
          <div className="ph-eyebrow">Organization</div>
          <h1>{me?.name || 'Your organization'}</h1>
          <p className="ph-sub">{me?.workArea}{me?.location ? ` · ${me.location}` : ''}</p>
        </div>
        <div className="ph-actions">
          <Link to="/org/applicants" className="btn btn-secondary btn-sm">Review applicants</Link>
          <Link to="/org/opportunities" className="btn btn-primary btn-sm"><Icon name="plus" size={16} /> New opportunity</Link>
        </div>
      </div>

      {me && !me.isVerified && (
        <div className="alert amber" style={{ marginBottom: 24 }}>
          <Icon name="alert" size={20} />
          <span>
            <span className="a-title">Verification pending</span>
            {me.verificationRejectionReason
              ? `Your verification was rejected: ${me.verificationRejectionReason}`
              : 'Your organization is awaiting admin verification. Your opportunities become public once verified.'}
          </span>
        </div>
      )}
      {me?.isVerified && (
        <div className="alert green" style={{ marginBottom: 24 }}>
          <Icon name="shieldCheck" size={20} />
          <span><span className="a-title">Verified organization</span>Your profile is verified and visible to all volunteers.</span>
        </div>
      )}

      {statsError ? (
        <div className="alert" style={{ background: 'var(--red-bg)', border: '1px solid var(--red-bd)', color: 'var(--red)', marginBottom: 24, display: 'flex', gap: 10, alignItems: 'center', padding: '14px 16px', borderRadius: 12 }}>
          <Icon name="alert" size={20} /><span>Could not load your statistics. <button type="button" className="link" onClick={reload}>Retry</button></span>
        </div>
      ) : (
        <div className="kpi-grid">
          {KPIS.map((k) => (
            <div className="kpi" key={k.lbl}>
              <div className="kpi-top"><div className={`kpi-ico ${k.tone || ''}`}><Icon name={k.icon} size={21} stroke={1.9} /></div></div>
              <div className="kpi-num">{k.val}</div>
              <div className="kpi-lbl">{k.lbl}</div>
            </div>
          ))}
        </div>
      )}

      <div className="col-2">
        <div className="stack">
          <section className="panel">
            <div className="panel-head">
              <h3><span className="ph-ico"><Icon name="users" size={16} stroke={2} /></span>New applicants</h3>
              <Link to="/org/applicants" className="link">Manage <Icon name="arrowRight" size={15} /></Link>
            </div>
            <div className="panel-body flush">
              {appsError && <ErrorState error={appsError} onRetry={reload} />}
              {!appsError && newApps.length === 0 && <EmptyState icon="users" title="No new applicants" message="New applications will appear here for review." />}
              {!appsError && newApps.slice(0, 6).map((a) => (
                <div className="lrow" key={a.id}>
                  <div className="lrow-ico edu"><Icon name="users" size={19} stroke={1.9} /></div>
                  <div className="lrow-body">
                    <div className="lrow-title">{a.volunteerName || a.teamName} <ApplicationStatusPill status={a.status} /></div>
                    <div className="lrow-meta"><span>{a.opportunityTitle}</span><span>{new Date(a.submittedAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}</span></div>
                  </div>
                  <div className="lrow-end"><Link to="/org/applicants" className="btn btn-secondary btn-sm">Review</Link></div>
                </div>
              ))}
            </div>
          </section>
        </div>

        <div className="stack">
          <section className="panel">
            <div className="panel-head">
              <h3><span className="ph-ico"><Icon name="briefcase" size={16} stroke={2} /></span>Your opportunities</h3>
              <Link to="/org/opportunities" className="link">All <Icon name="arrowRight" size={15} /></Link>
            </div>
            <div className="panel-body flush">
              {oppsError && <ErrorState error={oppsError} onRetry={reload} />}
              {!oppsError && opps.length === 0 && <EmptyState icon="briefcase" title="No opportunities yet" message="Publish your first opportunity to start recruiting." />}
              {!oppsError && opps.slice(0, 5).map((o) => (
                <div className="lrow" key={o.id}>
                  <div className="lrow-body">
                    <div className="lrow-title">{o.title} <OpportunityStatusPill status={o.status} /></div>
                    <div className="lrow-meta"><span>{o.acceptedVolunteersCount}/{o.requiredVolunteersCount} spots</span><span>{o.causeName}</span></div>
                  </div>
                </div>
              ))}
            </div>
          </section>

          {pending.length > 0 && (
            <section className="panel">
              <div className="panel-head"><h3><span className="ph-ico"><Icon name="clock" size={16} stroke={2} /></span>Hours to verify</h3><Link to="/org/applicants" className="link">Verify <Icon name="arrowRight" size={15} /></Link></div>
              <div className="panel-body">
                <div className="alert purple"><Icon name="info" size={20} /><span><b>{pending.length}</b> hour log{pending.length === 1 ? '' : 's'} awaiting your verification.</span></div>
              </div>
            </section>
          )}
        </div>
      </div>
    </main>
  );
}
