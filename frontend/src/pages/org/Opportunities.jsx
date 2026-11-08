import { useState } from 'react';
import Icon from '../../components/Icon';
import Modal from '../../components/Modal';
import { Loading, ErrorState, EmptyState, Spinner } from '../../components/States';
import { OpportunityStatusPill } from '../../components/StatusPill';
import TagMultiSelect from '../../components/form/TagMultiSelect';
import { useAsync } from '../../lib/useAsync';
import { useAuth } from '../../auth/AuthContext';
import { organizationsApi, opportunitiesApi, lookupsApi, volunteersApi } from '../../api/services';
import { useToast } from '../../components/Toast';
import {
  OpportunityFormat, OpportunityType, OpportunityVisibility,
  OpportunityTypeLabel, OpportunityFormatLabel,
} from '../../lib/enums';
import { oppCapabilities } from '../../lib/orgActions';
import { ApiError } from '../../api/client';

/* Format a stored instant as a value for <input type="datetime-local">, using
   LOCAL wall-clock components. Using toISOString() here would shift the value
   by the timezone offset (e.g. show 12:00 for an event the user entered at
   14:00 in GMT+2), so we build the string from local getters instead. */
const toLocalInput = (iso) => {
  if (!iso) return '';
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return '';
  const pad = (n) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
};

const blank = () => ({
  id: null, causeId: '', title: '', description: '', requiredVolunteersCount: 1,
  startAt: '', endAt: '', periodDescription: '', location: '',
  format: OpportunityFormat.OnSite, durationHours: 1, type: OpportunityType.OneTime,
  visibility: OpportunityVisibility.Public, isTeamBased: false, isUrgent: false, skillIds: [],
});

export default function OrgOpportunities() {
  const toast = useToast();
  const { profile } = useAuth();
  const verified = !!profile?.isVerified;
  const { data, loading, error, reload } = useAsync(() => organizationsApi.myOpportunities(), []);
  const { data: causes } = useAsync(() => lookupsApi.causes(), []);
  const { data: skills } = useAsync(() => lookupsApi.skills(), []);

  const [modal, setModal] = useState(false);
  const [form, setForm] = useState(blank());
  const [errors, setErrors] = useState({});
  const [saving, setSaving] = useState(false);
  const [busyId, setBusyId] = useState(null);
  const [inviteFor, setInviteFor] = useState(null);
  const [inviteSearch, setInviteSearch] = useState('');
  const [inviteResults, setInviteResults] = useState([]);
  const [inviteBusy, setInviteBusy] = useState(null);

  const opps = data || [];
  const set = (k) => (e) => {
    const v = e.target.type === 'checkbox' ? e.target.checked : e.target.value;
    setForm((f) => ({ ...f, [k]: v })); setErrors((x) => ({ ...x, [k]: undefined }));
  };

  const openCreate = () => {
    if (!verified) { toast.err('Verification required', 'Your organization must be verified before publishing opportunities.'); return; }
    setForm(blank()); setErrors({}); setModal(true);
  };
  const openEdit = (o) => {
    setForm({
      id: o.id, causeId: o.causeId, title: o.title, description: o.description,
      requiredVolunteersCount: o.requiredVolunteersCount, startAt: toLocalInput(o.startAt),
      endAt: o.endAt ? toLocalInput(o.endAt) : '', periodDescription: o.periodDescription || '',
      location: o.location, format: o.format, durationHours: o.durationHours, type: o.type,
      visibility: o.visibility, isTeamBased: o.isTeamBased, isUrgent: o.isUrgent,
      skillIds: o.skillIds || o.skills?.map((s) => s.id) || [],
    });
    setErrors({}); setModal(true);
  };

  const validate = () => {
    const e = {};
    if (!form.title.trim()) e.title = 'Title is required.';
    if (!form.causeId) e.causeId = 'Select a cause.';
    if (!form.description.trim()) e.description = 'Description is required.';
    if (!form.requiredVolunteersCount || Number(form.requiredVolunteersCount) < 1) e.requiredVolunteersCount = 'At least 1 volunteer.';
    if (!form.startAt) e.startAt = 'Start date is required.';
    // Backend requires EndAt for Periodic and Long-term opportunities, and
    // EndAt must be after StartAt whenever it is provided. Mirror those rules
    // so we never POST a payload the API will reject.
    const needsEnd = [OpportunityType.Periodic, OpportunityType.LongTerm].includes(Number(form.type));
    if (needsEnd && !form.endAt) {
      e.endAt = 'End date is required for recurring and long-term opportunities.';
    } else if (form.endAt && form.startAt && new Date(form.endAt) <= new Date(form.startAt)) {
      e.endAt = 'End date must be after the start date.';
    }
    if (Number(form.format) === OpportunityFormat.OnSite && !form.location.trim()) e.location = 'Location is required for on-site.';
    if (!form.durationHours || Number(form.durationHours) <= 0) e.durationHours = 'Enter the duration.';
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const save = async (e) => {
    e.preventDefault();
    if (!validate()) return;
    setSaving(true);
    const dto = {
      causeId: Number(form.causeId),
      title: form.title.trim(),
      description: form.description.trim(),
      requiredVolunteersCount: Number(form.requiredVolunteersCount),
      startAt: new Date(form.startAt).toISOString(),
      endAt: form.endAt ? new Date(form.endAt).toISOString() : null,
      periodDescription: form.periodDescription.trim() || null,
      location: Number(form.format) === OpportunityFormat.Online ? (form.location.trim() || 'Online') : form.location.trim(),
      format: Number(form.format),
      durationHours: Number(form.durationHours),
      type: Number(form.type),
      visibility: Number(form.visibility),
      isTeamBased: form.isTeamBased,
      isUrgent: form.isUrgent,
      skillIds: form.skillIds,
    };
    try {
      if (form.id) { await opportunitiesApi.update(form.id, dto); toast.ok('Opportunity updated'); }
      else { await opportunitiesApi.create(dto); toast.ok('Opportunity published'); }
      setModal(false);
      reload();
    } catch (err) {
      if (err instanceof ApiError && err.fieldErrors) toast.err('Check the form', err.message);
      else toast.err('Could not save', err.message);
    } finally {
      setSaving(false);
    }
  };

  const action = async (o, fn, label) => {
    setBusyId(o.id);
    try { await fn(o.id); toast.ok(label); reload(); }
    catch (err) { toast.err('Action failed', err.message); }
    finally { setBusyId(null); }
  };

  const remove = (o) => { if (window.confirm(`Delete "${o.title}" This cannot be undone.`)) action(o, opportunitiesApi.remove, 'Opportunity deleted'); };

  const searchVolunteers = async (event) => {
    event.preventDefault();
    if (!inviteSearch.trim()) return;
    setInviteBusy('search');
    try {
      setInviteResults(await volunteersApi.search({ name: inviteSearch.trim() }));
    } catch (err) {
      toast.err('Could not search volunteers', err?.message);
    } finally {
      setInviteBusy(null);
    }
  };

  const inviteVolunteer = async (volunteer) => {
    setInviteBusy(`v-${volunteer.id}`);
    try {
      await opportunitiesApi.invite(inviteFor.id, volunteer.id);
      toast.ok('Invitation sent', `${volunteer.firstName} ${volunteer.lastName} can accept or decline it.`);
      setInviteFor(null);
      setInviteSearch('');
      setInviteResults([]);
    } catch (err) {
      toast.err('Could not invite volunteer', err?.message);
    } finally {
      setInviteBusy(null);
    }
  };

  return (
    <main className="page">
      <div className="page-head">
        <div>
          <div className="ph-eyebrow">Manage</div>
          <h1>Opportunities</h1>
          <p className="ph-sub">Create and manage the opportunities your organization publishes.</p>
        </div>
        <div className="ph-actions">
          <button className="btn btn-primary btn-sm" onClick={openCreate} disabled={!verified} title={verified ? undefined : 'Available after your organization is verified'}>
            <Icon name="plus" size={16} /> New opportunity
          </button>
        </div>
      </div>

      {!verified && (
        <div className="alert" style={{ background: 'var(--amber-bg,#FFF7E6)', border: '1px solid var(--amber-bd,#FFE2A8)', color: 'var(--amber,#B45A14)', marginBottom: 20, display: 'flex', gap: 10, alignItems: 'center', padding: '14px 16px', borderRadius: 12 }}>
          <Icon name="clock" size={20} />
          <span><b>Verification pending.</b> An administrator is reviewing your organization. Publishing opportunities and inviting volunteers unlock once you are verified.</span>
        </div>
      )}

      {loading && <Loading label="Loading opportunities…" />}
      {error && <ErrorState error={error} onRetry={reload} />}
      {!loading && !error && opps.length === 0 && <EmptyState icon="briefcase" title="No opportunities yet" message="Publish your first opportunity to start recruiting volunteers." />}

      {!loading && !error && opps.length > 0 && (
        <div className="panel">
          <div className="tbl-wrap">
            <table className="tbl">
              <thead><tr><th>Opportunity</th><th>Cause</th><th>Type</th><th>Spots</th><th>Status</th><th /></tr></thead>
              <tbody>
                {opps.map((o) => (
                  <tr key={o.id}>
                    <td>
                      <div className="td-main">{o.title} {o.isUrgent && <span className="st full" style={{ marginLeft: 6 }}>Urgent</span>}</div>
                      <div className="td-sub">{OpportunityFormatLabel[o.format]} · {o.durationHours}h{o.isTeamBased ? ' · Team' : ''}</div>
                    </td>
                    <td className="muted">{o.causeName}</td>
                    <td className="muted">{OpportunityTypeLabel[o.type]}</td>
                    <td className="tnum">{o.acceptedVolunteersCount}/{o.requiredVolunteersCount}</td>
                    <td><OpportunityStatusPill status={o.status} /></td>
                    <td>
                      {(() => {
                        const cap = oppCapabilities(o, { verified });
                        if (cap.none) return <span className="muted" style={{ fontSize: '.82rem' }}>No actions</span>;
                        return (
                          <div className="row-actions">
                            {cap.canEdit && <button className="icon-act" aria-label="Edit" onClick={() => openEdit(o)}><Icon name="edit" size={16} stroke={2} /></button>}
                            {cap.canInvite && <button className="icon-act" aria-label="Invite volunteer" onClick={() => setInviteFor(o)}><Icon name="send" size={16} stroke={2} /></button>}
                            {cap.canComplete && <button className="icon-act" aria-label="Complete" disabled={busyId === o.id} onClick={() => action(o, opportunitiesApi.complete, 'Opportunity completed')}><Icon name="check" size={16} stroke={2} /></button>}
                            {cap.canCancel && <button className="icon-act no" aria-label="Cancel" disabled={busyId === o.id} onClick={() => action(o, opportunitiesApi.cancel, 'Opportunity cancelled')}><Icon name="x" size={16} stroke={2} /></button>}
                            {cap.canDelete && <button className="icon-act no" aria-label="Delete" disabled={busyId === o.id} onClick={() => remove(o)}><Icon name="trash" size={16} stroke={2} /></button>}
                          </div>
                        );
                      })()}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      <Modal
        open={modal} onClose={() => setModal(false)} width={680}
        title={form.id ? 'Edit opportunity' : 'New opportunity'}
        footer={(
          <>
            <button className="btn btn-secondary btn-sm" onClick={() => setModal(false)}>Cancel</button>
            <button className="btn btn-primary btn-sm" onClick={save} disabled={saving}>{saving ? <><Spinner size={15} /> Saving…</> : (form.id ? 'Save changes' : 'Publish')}</button>
          </>
        )}
      >
        <form onSubmit={save}>
          {/* Section 1 — Basics */}
          <div className="msec-h">Basics</div>
          <div className={`field ${errors.title ? 'invalid' : ''}`}>
            <label htmlFor="opp-title">Title <span className="req">*</span></label>
            <input id="opp-title" type="text" placeholder="Community Park Cleanup" value={form.title} onChange={set('title')}
              aria-invalid={!!errors.title} aria-describedby={errors.title ? 'opp-title-err' : undefined} />
            <span className="error-msg" id="opp-title-err"><Icon name="alert" size={16} stroke={2} /><span className="em-text">{errors.title}</span></span>
          </div>
          <div className="form-grid">
            <div className={`field ${errors.causeId ? 'invalid' : ''}`}>
              <label htmlFor="opp-cause">Cause <span className="req">*</span></label>
              <select id="opp-cause" value={form.causeId} onChange={set('causeId')} aria-invalid={!!errors.causeId} aria-describedby={errors.causeId ? 'opp-cause-err' : undefined}>
                <option value="">Select a cause</option>
                {(causes || []).map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
              <span className="error-msg" id="opp-cause-err"><Icon name="alert" size={16} stroke={2} /><span className="em-text">{errors.causeId}</span></span>
            </div>
            <div className="field">
              <label htmlFor="opp-type">Type</label>
              <select id="opp-type" value={form.type} onChange={set('type')}>
                <option value={OpportunityType.OneTime}>One-time</option>
                <option value={OpportunityType.Periodic}>Recurring</option>
                <option value={OpportunityType.LongTerm}>Long-term</option>
              </select>
            </div>
          </div>
          <div className={`field ${errors.description ? 'invalid' : ''}`}>
            <label htmlFor="opp-desc">Description <span className="req">*</span></label>
            <textarea id="opp-desc" placeholder="Describe what volunteers will do." value={form.description} onChange={set('description')}
              aria-invalid={!!errors.description} aria-describedby={errors.description ? 'opp-desc-err' : undefined} />
            <span className="error-msg" id="opp-desc-err"><Icon name="alert" size={16} stroke={2} /><span className="em-text">{errors.description}</span></span>
          </div>

          {/* Section 2 — Schedule & location */}
          <div className="msec-h">Schedule &amp; location</div>
          <div className="form-grid">
            <div className="field">
              <label htmlFor="opp-format">Format</label>
              <select id="opp-format" value={form.format} onChange={set('format')}>
                <option value={OpportunityFormat.OnSite}>On-site</option>
                <option value={OpportunityFormat.Online}>Online</option>
              </select>
            </div>
            <div className={`field ${errors.location ? 'invalid' : ''}`}>
              <label htmlFor="opp-loc">Location {Number(form.format) === OpportunityFormat.OnSite && <span className="req">*</span>}</label>
              <input id="opp-loc" type="text" placeholder={Number(form.format) === OpportunityFormat.Online ? 'Online' : 'City / venue'} value={form.location} onChange={set('location')}
                aria-invalid={!!errors.location} aria-describedby={errors.location ? 'opp-loc-err' : undefined} />
              <span className="error-msg" id="opp-loc-err"><Icon name="alert" size={16} stroke={2} /><span className="em-text">{errors.location}</span></span>
            </div>
            <div className={`field ${errors.startAt ? 'invalid' : ''}`}>
              <label htmlFor="opp-start">Starts <span className="req">*</span></label>
              <input id="opp-start" type="datetime-local" value={form.startAt} onChange={set('startAt')}
                aria-invalid={!!errors.startAt} aria-describedby={errors.startAt ? 'opp-start-err' : undefined} />
              <span className="error-msg" id="opp-start-err"><Icon name="alert" size={16} stroke={2} /><span className="em-text">{errors.startAt}</span></span>
            </div>
            <div className={`field ${errors.endAt ? 'invalid' : ''}`}>
              <label htmlFor="opp-end">Ends {[OpportunityType.Periodic, OpportunityType.LongTerm].includes(Number(form.type))
                ? <span className="req">*</span>
                : <span className="opt">(optional)</span>}</label>
              <input id="opp-end" type="datetime-local" value={form.endAt} onChange={set('endAt')}
                aria-invalid={!!errors.endAt} aria-describedby={errors.endAt ? 'opp-end-err' : undefined} />
              <span className="error-msg" id="opp-end-err"><Icon name="alert" size={16} stroke={2} /><span className="em-text">{errors.endAt}</span></span>
            </div>
            <div className="field full">
              <label htmlFor="opp-period">Recurring period <span className="opt">(optional)</span></label>
              <input id="opp-period" type="text" placeholder="e.g. Every Saturday" value={form.periodDescription} onChange={set('periodDescription')} />
            </div>
          </div>

          {/* Section 3 — Capacity & skills */}
          <div className="msec-h">Capacity &amp; skills</div>
          <div className="form-grid">
            <div className={`field ${errors.requiredVolunteersCount ? 'invalid' : ''}`}>
              <label htmlFor="opp-vol">Volunteers needed <span className="req">*</span></label>
              <input id="opp-vol" type="number" min="1" value={form.requiredVolunteersCount} onChange={set('requiredVolunteersCount')}
                aria-invalid={!!errors.requiredVolunteersCount} aria-describedby={errors.requiredVolunteersCount ? 'opp-vol-err' : undefined} />
              <span className="error-msg" id="opp-vol-err"><Icon name="alert" size={16} stroke={2} /><span className="em-text">{errors.requiredVolunteersCount}</span></span>
            </div>
            <div className={`field ${errors.durationHours ? 'invalid' : ''}`}>
              <label htmlFor="opp-dur">Duration (hours) <span className="req">*</span></label>
              <input id="opp-dur" type="number" min="0.5" step="0.5" value={form.durationHours} onChange={set('durationHours')}
                aria-invalid={!!errors.durationHours} aria-describedby={errors.durationHours ? 'opp-dur-err' : undefined} />
              <span className="error-msg" id="opp-dur-err"><Icon name="alert" size={16} stroke={2} /><span className="em-text">{errors.durationHours}</span></span>
            </div>
          </div>
          <div className="field">
            <label>Required skills</label>
            <TagMultiSelect options={skills || []} value={form.skillIds} onChange={(v) => setForm((f) => ({ ...f, skillIds: v }))} />
          </div>

          {/* Section 4 — Visibility & options */}
          <div className="msec-h">Visibility &amp; options</div>
          <div className="form-grid">
            <div className="field">
              <label htmlFor="opp-vis">Visibility</label>
              <select id="opp-vis" value={form.visibility} onChange={set('visibility')}>
                <option value={OpportunityVisibility.Public}>Public</option>
                <option value={OpportunityVisibility.Private}>Private (invite only)</option>
              </select>
            </div>
          </div>
          <div className="flex gap-16" style={{ flexWrap: 'wrap', marginTop: 4 }}>
            <label className="check-row"><span className="check-ctrl"><input type="checkbox" checked={form.isTeamBased} onChange={set('isTeamBased')} /><span className="check-box"><Icon name="check" size={14} stroke={3} /></span></span>Team-based</label>
            <label className="check-row"><span className="check-ctrl"><input type="checkbox" checked={form.isUrgent} onChange={set('isUrgent')} /><span className="check-box"><Icon name="check" size={14} stroke={3} /></span></span>Mark as urgent</label>
          </div>
        </form>
      </Modal>

      <Modal open={!!inviteFor} onClose={() => setInviteFor(null)} title={`Invite volunteer${inviteFor ? ` to ${inviteFor.title}` : ''}`}>
        <form className="filters" onSubmit={searchVolunteers} style={{ marginBottom: 16 }}>
          <label className="f-search">
            <Icon name="search" size={18} />
            <input value={inviteSearch} onChange={(e) => setInviteSearch(e.target.value)} placeholder="Search by volunteer name" />
          </label>
          <button className="btn btn-primary btn-sm" disabled={inviteBusy === 'search'}>{inviteBusy === 'search' ? <Spinner size={15} /> : 'Search'}</button>
        </form>
        {inviteResults.length === 0 && <p className="muted" style={{ fontSize: '.9rem' }}>Search volunteers, then send an invitation for this opportunity.</p>}
        {inviteResults.map((v) => (
          <div className="lrow" key={v.id}>
            <span className="mini-ava">{`${v.firstName?.[0] || ''}${v.lastName?.[0] || ''}`.toUpperCase()}</span>
            <div className="lrow-body">
              <div className="lrow-title">{v.firstName} {v.lastName}</div>
              <div className="lrow-meta"><span>{v.totalHours || 0} verified hours</span></div>
            </div>
            <button className="btn btn-secondary btn-sm" disabled={inviteBusy === `v-${v.id}`} onClick={() => inviteVolunteer(v)}>Invite</button>
          </div>
        ))}
      </Modal>
    </main>
  );
}
