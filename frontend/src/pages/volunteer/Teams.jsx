import { useState } from 'react';
import Icon from '../../components/Icon';
import Modal from '../../components/Modal';
import { Loading, ErrorState, EmptyState, Spinner } from '../../components/States';
import { useAsync } from '../../lib/useAsync';
import { teamsApi } from '../../api/services';
import { useAuth } from '../../auth/AuthContext';
import { useToast } from '../../components/Toast';
import { TeamMemberStatus, TeamMemberStatusLabel } from '../../lib/enums';

export default function Teams() {
  const toast = useToast();
  const { profile } = useAuth();
  const myVolunteerId = profile?.id;
  const { data, loading, error, reload } = useAsync(() => teamsApi.mine(), []);
  const [modal, setModal] = useState(null);
  const [teamName, setTeamName] = useState('');
  const [inviteId, setInviteId] = useState('');
  const [busy, setBusy] = useState(null);

  if (loading) return <main className="page"><Loading label="Loading teams..." /></main>;
  if (error) return <main className="page"><ErrorState error={error} onRetry={reload} /></main>;

  const teams = data || [];

  const createTeam = async (event) => {
    event.preventDefault();
    if (!teamName.trim()) return;
    setBusy('new');
    try {
      await teamsApi.create(teamName.trim());
      setTeamName('');
      toast.ok('Team created', 'You are the captain. Invite members by their volunteer ID.');
      reload();
    } catch (err) {
      toast.err('Could not create team', err?.message);
    } finally {
      setBusy(null);
    }
  };

  const addMember = async (event) => {
    event.preventDefault();
    const id = Number(inviteId);
    if (!id) { toast.err('Enter a volunteer ID', 'A numeric volunteer ID is required.'); return; }
    setBusy('add');
    try {
      await teamsApi.addMember(modal.id, id);
      toast.ok('Invitation sent', 'The volunteer can confirm or decline participation.');
      setInviteId('');
      setModal(null);
      reload();
    } catch (err) {
      toast.err('Could not add member', err?.message);
    } finally {
      setBusy(null);
    }
  };

  const memberAction = async (team, member, action) => {
    setBusy(`${action}-${member.id}`);
    try {
      if (action === 'confirm') await teamsApi.confirm(team.id, member.id);
      else await teamsApi.decline(team.id, member.id);
      toast.ok(action === 'confirm' ? 'Participation confirmed' : 'Invitation declined');
      reload();
    } catch (err) {
      toast.err('Could not update membership', err?.message);
    } finally {
      setBusy(null);
    }
  };

  return (
    <main className="page">
      <div className="page-head">
        <div>
          <div className="ph-eyebrow">Team volunteering</div>
          <h1>My Teams</h1>
          <p className="ph-sub">Create teams as a captain, invite volunteers, and confirm or decline invitations you receive.</p>
        </div>
      </div>

      <form className="filters" onSubmit={createTeam}>
        <label className="f-search">
          <Icon name="users" size={18} />
          <input value={teamName} onChange={(e) => setTeamName(e.target.value)} placeholder="Team name" />
        </label>
        <button className="btn btn-primary btn-sm" disabled={busy === 'new'}>{busy === 'new' ? <Spinner size={15} /> : <><Icon name="plus" size={16} /> Create team</>}</button>
      </form>

      {teams.length === 0 ? <EmptyState icon="users" title="No teams yet" message="Create a team before applying to team-based opportunities." /> : (
        <div className="col-2 even">
          {teams.map((team) => {
            const isCaptain = team.captainVolunteerId === myVolunteerId;
            const members = team.members || [];
            return (
              <section className="panel" key={team.id}>
                <div className="panel-head">
                  <h3>
                    <span className="ph-ico"><Icon name="users" size={16} /></span>{team.name}
                    <span className={`st ${isCaptain ? 'verified' : 'submitted'}`} style={{ marginLeft: 8, padding: '3px 9px' }}>{isCaptain ? 'Captain' : 'Member'}</span>
                  </h3>
                  {isCaptain && <button className="btn btn-secondary btn-sm" onClick={() => { setInviteId(''); setModal(team); }}><Icon name="plus" size={15} /> Add member</button>}
                </div>
                <div className="panel-body flush">
                  {members.length === 0 && <EmptyState icon="users" title="No members invited" message={isCaptain ? 'Invite volunteers by their volunteer ID.' : 'The captain has not invited members yet.'} />}
                  {members.map((m) => {
                    const isMe = m.volunteerId === myVolunteerId;
                    const canRespond = isMe && m.status === TeamMemberStatus.Pending;
                    return (
                      <div className="lrow" key={m.id}>
                        <span className="mini-ava">{(m.volunteerName || '').slice(0, 2).toUpperCase()}</span>
                        <div className="lrow-body">
                          <div className="lrow-title">{m.volunteerName}{isMe && <span className="muted" style={{ fontWeight: 400 }}> (you)</span>}</div>
                          <div className="lrow-meta"><span>{TeamMemberStatusLabel[m.status]}</span></div>
                        </div>
                        {canRespond && (
                          <div className="lrow-end">
                            <button className="icon-act ok" aria-label="Confirm participation" disabled={busy === `confirm-${m.id}`} onClick={() => memberAction(team, m, 'confirm')}><Icon name="check" size={16} /></button>
                            <button className="icon-act no" aria-label="Decline invitation" disabled={busy === `decline-${m.id}`} onClick={() => memberAction(team, m, 'decline')}><Icon name="x" size={16} /></button>
                          </div>
                        )}
                      </div>
                    );
                  })}
                </div>
              </section>
            );
          })}
        </div>
      )}

      <Modal open={!!modal} onClose={() => setModal(null)} title={`Add member${modal ? ` to ${modal.name}` : ''}`}>
        {/* The volunteer directory search endpoint is restricted to Organization/Admin
            roles, so volunteers cannot search by name here. Members are invited by
            their volunteer ID instead (which the backend AddTeamMember accepts). */}
        <div className="alert" style={{ marginBottom: 14, padding: '11px 13px' }}>
          <Icon name="info" size={18} />
          <span style={{ fontSize: '.84rem' }}>Volunteer name search isn't available to volunteers. Invite a member by their volunteer ID — they can find it on their profile and share it with you.</span>
        </div>
        <form className="filters" onSubmit={addMember}>
          <label className="f-search">
            <Icon name="users" size={18} />
            <input type="number" min="1" value={inviteId} onChange={(e) => setInviteId(e.target.value)} placeholder="Volunteer ID" />
          </label>
          <button className="btn btn-primary btn-sm" disabled={busy === 'add'}>{busy === 'add' ? <Spinner size={15} /> : 'Send invite'}</button>
        </form>
      </Modal>
    </main>
  );
}
