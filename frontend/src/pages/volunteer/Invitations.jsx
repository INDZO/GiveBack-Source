import { useState } from 'react';
import { Link } from 'react-router-dom';
import Icon from '../../components/Icon';
import { Loading, ErrorState, EmptyState, Spinner } from '../../components/States';
import { useAsync } from '../../lib/useAsync';
import { opportunitiesApi } from '../../api/services';
import { useToast } from '../../components/Toast';
import { ApiError } from '../../api/client';
import { InvitationStatus, InvitationStatusLabel } from '../../lib/enums';

const STATUS_TONE = { 1: 'submitted', 2: 'accepted', 3: 'rejected' };

function InvitationStatusPill({ status }) {
  return (
    <span className={`st ${STATUS_TONE[status] || 'submitted'}`}>
      <span className="stdot" />{InvitationStatusLabel[status] || 'Pending'}
    </span>
  );
}

export default function Invitations() {
  const toast = useToast();
  const { data, loading, error, reload, setData } = useAsync(() => opportunitiesApi.myInvitations(), []);
  const invitations = data || [];
  const [busyId, setBusyId] = useState(null);

  if (loading) return <main className="page narrow"><Loading label="Loading your invitations..." /></main>;
  if (error) return <main className="page narrow"><ErrorState error={error} onRetry={reload} /></main>;

  const act = async (inv, kind) => {
    setBusyId(inv.id);
    try {
      if (kind === 'accept') await opportunitiesApi.acceptInvitation(inv.id);
      else await opportunitiesApi.declineInvitation(inv.id);
      const newStatus = kind === 'accept' ? InvitationStatus.Accepted : InvitationStatus.Declined;
      setData((list) => (list || []).map((i) => (i.id === inv.id ? { ...i, status: newStatus } : i)));
      if (kind === 'accept') toast.ok('Invitation accepted', 'You can now open and apply to this opportunity.');
      else toast.ok('Invitation declined');
    } catch (err) {
      const msg = err instanceof ApiError ? err.message : 'Please try again.';
      toast.err(kind === 'accept' ? 'Could not accept invitation' : 'Could not decline invitation', msg);
      reload();
    } finally {
      setBusyId(null);
    }
  };

  const pending = invitations.filter((i) => i.status === InvitationStatus.Pending).length;

  return (
    <main className="page narrow">
      <div className="page-head">
        <div>
          <div className="ph-eyebrow">Private opportunities</div>
          <h1>Invitations</h1>
          <p className="ph-sub">Organizations can invite you directly to private opportunities. {pending > 0 ? `You have ${pending} pending.` : 'Accepted invitations let you open and apply.'}</p>
        </div>
      </div>

      <section className="panel">
        {invitations.length === 0 ? (
          <EmptyState icon="mail" title="No invitations yet" message="When an organization invites you to a private opportunity, it will appear here." />
        ) : (
          <div className="panel-body flush">
            {invitations.map((inv) => {
              const pendingInv = inv.status === InvitationStatus.Pending;
              return (
                <div className="lrow" key={inv.id}>
                  <div className="lrow-ico edu"><Icon name="mail" size={19} stroke={1.9} /></div>
                  <div className="lrow-body">
                    <div className="lrow-title">
                      <Link to={`/app/opportunities/${inv.opportunityId}`}>{inv.opportunityTitle}</Link>{' '}
                      <InvitationStatusPill status={inv.status} />
                    </div>
                    <div className="muted" style={{ fontSize: '.9rem', marginTop: 2 }}>{inv.organizationName}</div>
                    <div className="lrow-meta">
                      {inv.startAt && <span><Icon name="clock" size={14} stroke={2} /> {new Date(inv.startAt).toLocaleString('en-US', { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' })}</span>}
                      {inv.invitedAt && <span>Invited {new Date(inv.invitedAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}</span>}
                    </div>
                  </div>
                  <div className="lrow-end" style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
                    {pendingInv ? (
                      <>
                        <button className="btn btn-primary btn-sm" disabled={busyId === inv.id} onClick={() => act(inv, 'accept')}>
                          {busyId === inv.id ? <Spinner size={15} /> : <><Icon name="check" size={15} /> Accept</>}
                        </button>
                        <button className="btn btn-secondary btn-sm" disabled={busyId === inv.id} onClick={() => act(inv, 'decline')}>Decline</button>
                      </>
                    ) : (
                      <Link to={`/app/opportunities/${inv.opportunityId}`} className="btn btn-secondary btn-sm">View opportunity</Link>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </section>
    </main>
  );
}
