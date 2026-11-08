import { useState } from 'react';
import Icon from '../../components/Icon';
import { Loading, ErrorState, EmptyState } from '../../components/States';
import { OpportunityStatusPill } from '../../components/StatusPill';
import { useAsync } from '../../lib/useAsync';
import { adminApi } from '../../api/services';
import { useToast } from '../../components/Toast';
import { RoleName } from '../../lib/enums';

export default function AdminVerify() {
  const toast = useToast();
  const [tab, setTab] = useState('orgs');
  const [busy, setBusy] = useState(null);
  const [newCause, setNewCause] = useState('');

  const { data, loading, error, reload } = useAsync(async () => {
    // Four independent moderation datasets, one per tab. Each failure surfaces as
    // an ErrorState inside its own tab rather than a misleading empty list; the
    // page only hard-fails if every dataset fails.
    const [pending, causes, users, opps] = await Promise.allSettled([
      adminApi.pendingOrganizations(), adminApi.causes(), adminApi.users(), adminApi.opportunities(),
    ]);
    if ([pending, causes, users, opps].every((r) => r.status === 'rejected')) throw pending.reason;
    return {
      pending: pending.status === 'fulfilled' ? pending.value : [],
      pendingError: pending.status === 'rejected' ? pending.reason : null,
      causes: causes.status === 'fulfilled' ? causes.value : [],
      causesError: causes.status === 'rejected' ? causes.reason : null,
      users: users.status === 'fulfilled' ? users.value : [],
      usersError: users.status === 'rejected' ? users.reason : null,
      opps: opps.status === 'fulfilled' ? opps.value : [],
      oppsError: opps.status === 'rejected' ? opps.reason : null,
    };
  }, []);

  if (loading) return <main className="page"><Loading label="Loading moderation tools..." /></main>;
  if (error) return <main className="page"><ErrorState error={error} onRetry={reload} /></main>;

  const { pending, pendingError, causes, causesError, users, usersError, opps, oppsError } = data;

  const act = async (key, fn, label) => {
    setBusy(key);
    try { await fn(); toast.ok(label); reload(); }
    catch (err) { toast.err('Action failed', err.message); }
    finally { setBusy(null); }
  };

  const reject = (org) => {
    const reason = window.prompt(`Reason for rejecting "${org.name}":`, '');
    if (reason == null) return;
    act(`o-${org.id}`, () => adminApi.rejectOrganization(org.id, reason || 'Not eligible'), 'Organization rejected');
  };

  const addCause = async (e) => {
    e.preventDefault();
    if (!newCause.trim()) return;
    await act('cause-new', () => adminApi.createCause(newCause.trim()), 'Cause added');
    setNewCause('');
  };

  const deleteCause = (c) => { if (window.confirm(`Delete cause "${c.name}"?`)) act(`c-${c.id}`, () => adminApi.deleteCause(c.id), 'Cause deleted'); };
  const editCause = (c) => {
    const name = window.prompt('Cause name:', c.name);
    if (name == null || !name.trim() || name.trim() === c.name) return;
    act(`c-${c.id}`, () => adminApi.updateCause(c.id, name.trim()), 'Cause updated');
  };

  const TABS = [
    { key: 'orgs', label: 'Organizations', cnt: pending.length },
    { key: 'causes', label: 'Causes', cnt: causes.length },
    { key: 'users', label: 'Users', cnt: users.length },
    { key: 'opps', label: 'Opportunities', cnt: opps.length },
  ];

  return (
    <main className="page">
      <div className="page-head">
        <div>
          <div className="ph-eyebrow">Administration</div>
          <h1>Verification &amp; Moderation</h1>
          <p className="ph-sub">Verify organizations, manage causes, moderate users and opportunities.</p>
        </div>
      </div>

      <div className="tabs">
        {TABS.map((t) => (
          <button key={t.key} className={tab === t.key ? 'active' : ''} onClick={() => setTab(t.key)}>{t.label} <span className="cnt">{t.cnt}</span></button>
        ))}
      </div>

      {tab === 'orgs' && (
        <div className="panel">
          {pendingError ? <ErrorState error={pendingError} onRetry={reload} />
            : pending.length === 0 ? <EmptyState icon="shieldCheck" title="All caught up" message="There are no organizations awaiting verification." /> : (
            <div className="panel-body flush">
              {pending.map((o) => (
                <div className="lrow" key={o.id}>
                  <div className="lrow-ico"><Icon name="building" size={19} stroke={1.9} /></div>
                  <div className="lrow-body">
                    <div className="lrow-title">{o.name}</div>
                    <div className="lrow-meta"><span>Reg. {o.registrationNumber}</span><span>{o.workArea}</span><span>{o.location}</span></div>
                    {o.mission && <div className="muted" style={{ fontSize: '.84rem', marginTop: 4 }}>{o.mission}</div>}
                  </div>
                  <div className="lrow-end" style={{ display: 'flex', gap: 8 }}>
                    <button className="btn btn-success btn-sm" disabled={busy === `o-${o.id}`} onClick={() => act(`o-${o.id}`, () => adminApi.verifyOrganization(o.id), 'Organization verified')}>Verify</button>
                    <button className="btn btn-danger btn-sm" disabled={busy === `o-${o.id}`} onClick={() => reject(o)}>Reject</button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {tab === 'causes' && (
        <div className="panel">
          <div className="panel-body">
            {causesError ? <ErrorState error={causesError} onRetry={reload} /> : (
              <>
                <form className="flex gap-8" style={{ marginBottom: 18 }} onSubmit={addCause}>
                  <input type="text" aria-label="New cause name" placeholder="New cause name" value={newCause} onChange={(e) => setNewCause(e.target.value)} style={{ flex: 1, padding: '11px 14px', border: '1px solid var(--border)', borderRadius: 'var(--radius-sm)', fontFamily: 'Inter' }} />
                  <button type="submit" className="btn btn-primary btn-sm" disabled={busy === 'cause-new'}><Icon name="plus" size={16} /> Add cause</button>
                </form>
                {causes.length === 0 && <EmptyState icon="tag" title="No causes yet" message="Add the first cause/category volunteers can filter by." />}
                <div className="flex gap-8" style={{ flexWrap: 'wrap' }}>
                  {causes.map((c) => (
                    <span key={c.id} className="cause-chip">
                      {c.name}
                      <button className="icon-act cmini" aria-label={`Edit ${c.name}`} disabled={busy === `c-${c.id}`} onClick={() => editCause(c)}><Icon name="edit" size={14} stroke={2.4} /></button>
                      <button className="icon-act no cmini" aria-label={`Delete ${c.name}`} disabled={busy === `c-${c.id}`} onClick={() => deleteCause(c)}><Icon name="x" size={14} stroke={2.4} /></button>
                    </span>
                  ))}
                </div>
              </>
            )}
          </div>
        </div>
      )}

      {tab === 'users' && (
        <div className="panel">
          {usersError ? <ErrorState error={usersError} onRetry={reload} /> : users.length === 0 ? <EmptyState icon="users" title="No users" message="No user accounts to show." /> : (
          <div className="tbl-wrap">
            <table className="tbl">
              <thead><tr><th>Email</th><th>Role</th><th>Joined</th><th>Status</th><th /></tr></thead>
              <tbody>
                {users.map((u) => (
                  <tr key={u.id}>
                    <td className="td-main">{u.email}</td>
                    <td className="muted">{RoleName[u.role]}</td>
                    <td>{new Date(u.createdAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })}</td>
                    <td><span className={`st ${u.isActive ? 'accepted' : 'noshow'}`}><span className="stdot" />{u.isActive ? 'Active' : 'Inactive'}</span></td>
                    <td>
                      <div className="row-actions">
                        {u.isActive
                          ? <button className="btn btn-danger btn-sm" disabled={busy === `u-${u.id}`} onClick={() => act(`u-${u.id}`, () => adminApi.deactivateUser(u.id), 'User deactivated')}>Deactivate</button>
                          : <button className="btn btn-success btn-sm" disabled={busy === `u-${u.id}`} onClick={() => act(`u-${u.id}`, () => adminApi.activateUser(u.id), 'User activated')}>Activate</button>}
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

      {tab === 'opps' && (
        <div className="panel">
          {oppsError ? <ErrorState error={oppsError} onRetry={reload} /> : opps.length === 0 ? <EmptyState icon="briefcase" title="No opportunities" message="No opportunities to moderate." /> : (
          <div className="tbl-wrap">
            <table className="tbl">
              <thead><tr><th>Opportunity</th><th>Organization</th><th>Cause</th><th>Status</th><th /></tr></thead>
              <tbody>
                {opps.map((o) => (
                  <tr key={o.id}>
                    <td className="td-main">{o.title}</td>
                    <td className="muted">{o.organizationName}</td>
                    <td className="muted">{o.causeName}</td>
                    <td><OpportunityStatusPill status={o.status} /></td>
                    <td>
                      <div className="row-actions">
                        <button className="btn btn-danger btn-sm" disabled={busy === `op-${o.id}`} onClick={() => act(`op-${o.id}`, () => adminApi.cancelOpportunity(o.id), 'Opportunity cancelled')}>Cancel</button>
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
    </main>
  );
}
