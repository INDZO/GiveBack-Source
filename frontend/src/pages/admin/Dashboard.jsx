import { Link } from 'react-router-dom';
import Icon from '../../components/Icon';
import { Loading, ErrorState } from '../../components/States';
import { useAsync } from '../../lib/useAsync';
import { adminApi } from '../../api/services';

export default function AdminDashboard() {
  const { data, loading, error, reload } = useAsync(async () => {
    const stats = await adminApi.statistics();
    const [pending] = await Promise.allSettled([adminApi.pendingOrganizations()]);
    return {
      stats,
      pending: pending.status === 'fulfilled' ? pending.value : [],
    };
  }, []);

  if (loading) return <main className="page"><Loading label="Loading platform statistics..." /></main>;
  if (error) return <main className="page"><ErrorState error={error} onRetry={reload} /></main>;

  const s = data.stats || {};
  const pending = data.pending || [];

  const KPIS = [
    { icon: 'users', val: s.usersCount ?? 0, lbl: 'Total users' },
    { icon: 'building', tone: 'green', val: s.organizationsCount ?? 0, lbl: 'Organizations' },
    { icon: 'briefcase', tone: 'orange', val: s.opportunitiesCount ?? 0, lbl: 'Opportunities' },
    { icon: 'clock', tone: 'blue', val: s.totalVerifiedHours ?? 0, lbl: 'Verified hours' },
  ];

  const BREAKDOWN = [
    { lbl: 'Verified orgs', val: s.verifiedOrganizationsCount, tone: 'green' },
    { lbl: 'Pending orgs', val: s.pendingOrganizationsCount, tone: 'amber' },
    { lbl: 'Open opportunities', val: s.openOpportunitiesCount, tone: 'green' },
    { lbl: 'Filled', val: s.filledOpportunitiesCount, tone: 'blue' },
    { lbl: 'Completed', val: s.completedOpportunitiesCount, tone: 'slate' },
    { lbl: 'Cancelled', val: s.cancelledOpportunitiesCount, tone: 'red' },
    { lbl: 'Urgent', val: s.urgentOpportunitiesCount, tone: 'amber' },
    { lbl: 'Applications', val: s.applicationsCount, tone: 'blue' },
    { lbl: 'Certificates', val: s.certificatesCount, tone: 'green' },
  ];

  return (
    <main className="page">
      <div className="page-head">
        <div>
          <div className="ph-eyebrow">Administration</div>
          <h1>Platform overview</h1>
          <p className="ph-sub">Monitor activity, verify organizations, and keep the platform healthy.</p>
        </div>
        <div className="ph-actions">
          <Link to="/admin/verify" className="btn btn-primary btn-sm">Verification &amp; moderation {pending.length > 0 && <span className="cnt" style={{ marginLeft: 4 }}>{pending.length}</span>}</Link>
        </div>
      </div>

      {pending.length > 0 && (
        <div className="alert amber" style={{ marginBottom: 24 }}>
          <Icon name="alert" size={20} />
          <span><b>{pending.length}</b> organization{pending.length === 1 ? '' : 's'} awaiting verification. <Link to="/admin/verify" style={{ color: 'var(--purple)', fontWeight: 600 }}>Review now</Link></span>
        </div>
      )}

      <div className="kpi-grid">
        {KPIS.map((k) => (
          <div className="kpi" key={k.lbl}>
            <div className="kpi-top"><div className={`kpi-ico ${k.tone || ''}`}><Icon name={k.icon} size={21} stroke={1.9} /></div></div>
            <div className="kpi-num">{k.val}</div>
            <div className="kpi-lbl">{k.lbl}</div>
          </div>
        ))}
      </div>

      <section className="panel">
        <div className="panel-head"><h3><span className="ph-ico"><Icon name="barChart" size={16} stroke={2} /></span>Platform breakdown</h3></div>
        <div className="panel-body">
          <div className="badge-grid">
            {BREAKDOWN.map((b) => (
              <div className="bdg" key={b.lbl} style={{ textAlign: 'left' }}>
                <div className="tnum" style={{ fontSize: '1.6rem', color: `var(--${b.tone === 'amber' ? 'amber' : b.tone === 'red' ? 'red' : b.tone === 'green' ? 'green' : b.tone === 'blue' ? 'blue' : 'slate'})` }}>{b.val ?? 0}</div>
                <div className="muted" style={{ fontSize: '.84rem' }}>{b.lbl}</div>
              </div>
            ))}
          </div>
        </div>
      </section>
    </main>
  );
}
