import { useMemo, useState } from 'react';
import Icon from '../../components/Icon';
import { Loading, ErrorState } from '../../components/States';
import { useAsync } from '../../lib/useAsync';
import { volunteersApi } from '../../api/services';

const MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

export default function VolunteerReport() {
  const [year, setYear] = useState(new Date().getFullYear());
  const { data, loading, error, reload } = useAsync(() => volunteersApi.annualReport(year), [year]);

  const monthly = data?.hoursByMonth || [];
  const total = monthly.reduce((sum, m) => sum + (m.totalHours || 0), 0);
  const max = Math.max(1, ...monthly.map((m) => m.totalHours || 0));
  const years = useMemo(() => Array.from({ length: 5 }, (_, i) => new Date().getFullYear() - i), []);

  if (loading) return <main className="page"><Loading label="Loading yearly report..." /></main>;
  if (error) return <main className="page"><ErrorState error={error} onRetry={reload} /></main>;

  return (
    <main className="page">
      <div className="page-head">
        <div>
          <div className="ph-eyebrow">Personal impact</div>
          <h1>{year} yearly report</h1>
          <p className="ph-sub">A SQL-backed summary of verified hours, opportunities, causes, and badges earned.</p>
        </div>
        <div className="ph-actions">
          <select aria-label="Report year" value={year} onChange={(e) => setYear(Number(e.target.value))}>
            {years.map((y) => <option key={y} value={y}>{y}</option>)}
          </select>
        </div>
      </div>

      <div className="kpi-grid">
        <div className="kpi"><div className="kpi-top"><div className="kpi-ico"><Icon name="clock" size={21} /></div></div><div className="kpi-num">{total}</div><div className="kpi-lbl">Verified hours</div></div>
        <div className="kpi"><div className="kpi-top"><div className="kpi-ico green"><Icon name="briefcase" size={21} /></div></div><div className="kpi-num">{data?.opportunitiesCount ?? 0}</div><div className="kpi-lbl">Opportunities completed</div></div>
        <div className="kpi"><div className="kpi-top"><div className="kpi-ico orange"><Icon name="heartHand" size={21} /></div></div><div className="kpi-num">{data?.causesCount ?? 0}</div><div className="kpi-lbl">Causes supported</div></div>
        <div className="kpi"><div className="kpi-top"><div className="kpi-ico blue"><Icon name="award" size={21} /></div></div><div className="kpi-num">{data?.earnedBadgesCount ?? 0}</div><div className="kpi-lbl">Badges earned</div></div>
      </div>

      <section className="panel">
        <div className="panel-head"><h3><span className="ph-ico"><Icon name="barChart" size={16} /></span>Hours by month</h3></div>
        <div className="panel-body">
          <div className="barchart" style={{ height: 240 }}>
            {MONTHS.map((label, i) => {
              const hours = monthly.find((m) => m.month === i + 1)?.totalHours || 0;
              return (
                <div className="bc" key={label}>
                  <div className={`bc-bar${hours > 0 ? ' hot' : ''}`} style={{ height: Math.max(8, Math.round((hours / max) * 190)) }}>
                    {hours > 0 && <span className="bc-val">{hours}h</span>}
                  </div>
                  <div className="bc-lbl">{label}</div>
                </div>
              );
            })}
          </div>
        </div>
      </section>
    </main>
  );
}
