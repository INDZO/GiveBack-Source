import { useState } from 'react';
import Icon from '../../components/Icon';
import { AsyncBoundary } from '../../components/States';
import { useAsync } from '../../lib/useAsync';
import { leaderboardApi } from '../../api/services';

const now = new Date();

export default function Leaderboard() {
  const [scope, setScope] = useState('monthly');
  const { data, loading, error, reload } = useAsync(
    () => (scope === 'monthly'
      ? leaderboardApi.monthly(now.getMonth() + 1, now.getFullYear())
      : leaderboardApi.yearly(now.getFullYear())),
    [scope],
  );
  const rows = data || [];
  const medal = (rank) => ['#FFD37A', '#D8DCE3', '#E6B98A'][rank - 1];

  return (
    <main className="page narrow">
      <div className="page-head">
        <div>
          <div className="ph-eyebrow">Recognition</div>
          <h1>Volunteer Leaderboard</h1>
          <p className="ph-sub">Top volunteers by verified hours — {scope === 'monthly' ? now.toLocaleDateString('en-US', { month: 'long', year: 'numeric' }) : now.getFullYear()}.</p>
        </div>
      </div>

      <div className="tabs" style={{ marginBottom: 24 }}>
        <button className={scope === 'monthly' ? 'active' : ''} onClick={() => setScope('monthly')}>This month</button>
        <button className={scope === 'yearly' ? 'active' : ''} onClick={() => setScope('yearly')}>This year</button>
      </div>

      <AsyncBoundary
        loading={loading} error={error} onRetry={reload}
        empty={!loading && !error && rows.length === 0}
        emptyProps={{ icon: 'trophy', title: 'No ranking yet', message: 'Verified hours will populate the leaderboard.' }}
        loadingLabel="Loading leaderboard…"
      >
        <div className="panel">
          <div className="panel-body flush">
            {rows.map((r) => (
              <div className="lrow" key={r.volunteerId}>
                <div className="lrow-ico" style={r.rank <= 3 ? { background: medal(r.rank), color: '#7a5b18' } : { background: 'var(--purple-50)', color: 'var(--purple)' }}>
                  {r.rank <= 3 ? <Icon name="trophy" size={19} stroke={1.9} /> : <b style={{ fontFamily: 'Poppins' }}>{r.rank}</b>}
                </div>
                <div className="lrow-body">
                  <div className="lrow-title">{r.firstName} {r.lastName}</div>
                  <div className="lrow-meta"><span>{r.numberOfOpportunities} opportunit{r.numberOfOpportunities === 1 ? 'y' : 'ies'}</span></div>
                </div>
                <div className="lrow-end"><span className="tnum" style={{ color: 'var(--purple)', fontSize: '1.1rem' }}>{r.totalHours} h</span></div>
              </div>
            ))}
          </div>
        </div>
      </AsyncBoundary>
    </main>
  );
}
