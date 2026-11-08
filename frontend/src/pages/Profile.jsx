import { useEffect, useState } from 'react';
import Icon from '../components/Icon';
import { Loading, ErrorState, Spinner } from '../components/States';
import TagMultiSelect from '../components/form/TagMultiSelect';
import { useAsync } from '../lib/useAsync';
import { useAuth } from '../auth/AuthContext';
import { lookupsApi, organizationsApi, volunteersApi } from '../api/services';
import { useToast } from '../components/Toast';
import { DayOfWeek, UserRole } from '../lib/enums';

/* All seven days (Mon→Sun), matching backend System.DayOfWeek values
   (Sun=0, Mon=1 … Sat=6). Weekends must be editable and preserved on save. */
const DEFAULT_DAYS = [1, 2, 3, 4, 5, 6, 0].map((dayOfWeek) => ({
  dayOfWeek,
  enabled: false,
  startTime: '09:00',
  endTime: '17:00',
}));

function timeValue(value) {
  if (!value) return '09:00';
  return String(value).slice(0, 5);
}

function availabilityFrom(profile) {
  const existing = new Map((profile?.availability || []).map((a) => [a.dayOfWeek, a]));
  return DEFAULT_DAYS.map((d) => {
    const saved = existing.get(d.dayOfWeek);
    return saved
      ? { dayOfWeek: d.dayOfWeek, enabled: true, startTime: timeValue(saved.startTime), endTime: timeValue(saved.endTime) }
      : d;
  });
}

export default function Profile() {
  const { role, refreshProfile } = useAuth();
  const toast = useToast();
  const isVolunteer = role === UserRole.Volunteer;

  const { data, loading, error, reload } = useAsync(async () => {
    const [profile, causes, skills] = await Promise.allSettled([
      isVolunteer ? volunteersApi.me() : organizationsApi.me(),
      isVolunteer ? lookupsApi.causes() : Promise.resolve([]),
      isVolunteer ? lookupsApi.skills() : Promise.resolve([]),
    ]);
    return {
      profile: profile.status === 'fulfilled' ? profile.value : null,
      causes: causes.status === 'fulfilled' ? causes.value : [],
      skills: skills.status === 'fulfilled' ? skills.value : [],
    };
  }, [isVolunteer]);

  const [form, setForm] = useState(null);
  const [errors, setErrors] = useState({});
  const [saving, setSaving] = useState(false);
  const [copiedId, setCopiedId] = useState(false);

  const copyVolunteerId = async (id) => {
    try {
      await navigator.clipboard.writeText(String(id));
      setCopiedId(true);
      setTimeout(() => setCopiedId(false), 1600);
    } catch { /* clipboard unavailable; the ID is still visible to copy manually */ }
  };

  useEffect(() => {
    if (!data?.profile) return;
    const p = data.profile;
    setForm(isVolunteer ? {
      firstName: p.firstName || '',
      lastName: p.lastName || '',
      bio: p.bio || '',
      skillIds: p.skillIds || [],
      causeIds: p.causeIds || [],
      availability: availabilityFrom(p),
    } : {
      name: p.name || '',
      registrationNumber: p.registrationNumber || '',
      mission: p.mission || '',
      workArea: p.workArea || '',
      location: p.location || '',
    });
  }, [data, isVolunteer]);

  if (loading || !form) return <main className="page"><Loading label="Loading profile..." /></main>;
  if (error) return <main className="page"><ErrorState error={error} onRetry={reload} /></main>;

  const set = (key) => (event) => {
    setForm((f) => ({ ...f, [key]: event.target.value }));
    setErrors((e) => ({ ...e, [key]: undefined }));
  };

  const validate = () => {
    const next = {};
    if (isVolunteer) {
      if (!form.firstName.trim()) next.firstName = 'First name is required.';
      if (!form.lastName.trim()) next.lastName = 'Last name is required.';
    } else {
      if (!form.name.trim()) next.name = 'Organization name is required.';
      if (!form.registrationNumber.trim()) next.registrationNumber = 'Registration number is required.';
      if (!form.mission.trim()) next.mission = 'Mission is required.';
      if (!form.workArea.trim()) next.workArea = 'Field of work is required.';
      if (!form.location.trim()) next.location = 'Location is required.';
    }
    setErrors(next);
    return Object.keys(next).length === 0;
  };

  const save = async (event) => {
    event.preventDefault();
    if (!validate()) return;
    setSaving(true);
    try {
      if (isVolunteer) {
        await volunteersApi.updateMe({
          firstName: form.firstName.trim(),
          lastName: form.lastName.trim(),
          bio: form.bio.trim() || null,
        });
        await volunteersApi.updateSkills(form.skillIds);
        await volunteersApi.updateInterests(form.causeIds);
        await volunteersApi.updateAvailability(
          form.availability
            .filter((a) => a.enabled)
            .map((a) => ({ dayOfWeek: a.dayOfWeek, startTime: `${a.startTime}:00`, endTime: `${a.endTime}:00` })),
        );
      } else {
        await organizationsApi.updateMe({
          name: form.name.trim(),
          registrationNumber: form.registrationNumber.trim(),
          mission: form.mission.trim(),
          workArea: form.workArea.trim(),
          location: form.location.trim(),
        });
      }
      await refreshProfile();
      toast.ok('Profile updated');
      reload();
    } catch (err) {
      toast.err('Could not save profile', err?.message);
    } finally {
      setSaving(false);
    }
  };

  const updateAvailability = (day, patch) => {
    setForm((f) => ({
      ...f,
      availability: f.availability.map((a) => (a.dayOfWeek === day ? { ...a, ...patch } : a)),
    }));
  };

  const profile = data.profile || {};

  return (
    <main className="page narrow">
      <div className="page-head">
        <div>
          <div className="ph-eyebrow">{isVolunteer ? 'Volunteer profile' : 'Organization profile'}</div>
          <h1>Profile settings</h1>
          <p className="ph-sub">
            Keep your public information accurate so {isVolunteer ? 'organizations can match you with the right actions.' : 'volunteers and admins can understand your mission.'}
          </p>
        </div>
      </div>

      {isVolunteer && data?.profile?.id != null && (
        <div className="panel" style={{ marginBottom: 24 }}>
          <div className="panel-body" style={{ display: 'flex', flexWrap: 'wrap', alignItems: 'center', gap: 12, justifyContent: 'space-between' }}>
            <div>
              <div className="section-label" style={{ marginBottom: 6 }}>Your Volunteer ID</div>
              <div className="tnum" style={{ fontSize: '1.5rem', color: 'var(--purple)' }} aria-label={`Your volunteer ID is ${data.profile.id}`}>#{data.profile.id}</div>
              <p className="muted" style={{ fontSize: '.85rem', marginTop: 4, maxWidth: 420 }}>Share this ID with a team captain so they can invite you to a team.</p>
            </div>
            <button type="button" className="btn btn-secondary btn-sm" onClick={() => copyVolunteerId(data.profile.id)}>
              <Icon name={copiedId ? 'check' : 'fileText'} size={16} /> {copiedId ? 'Copied' : 'Copy ID'}
            </button>
          </div>
        </div>
      )}

      {!isVolunteer && (
        <div className={`alert ${profile.isVerified ? 'green' : 'amber'}`} style={{ marginBottom: 24 }}>
          <Icon name={profile.isVerified ? 'shieldCheck' : 'alert'} size={20} />
          <span>
            <span className="a-title">{profile.isVerified ? 'Verified organization' : 'Verification pending'}</span>
            {profile.isVerified
              ? `Verified${profile.verifiedAt ? ` on ${new Date(profile.verifiedAt).toLocaleDateString('en-US')}` : ''}.`
              : (profile.verificationRejectionReason || 'An admin must verify registration details before the organization is fully trusted.')}
          </span>
        </div>
      )}

      <form className="panel" onSubmit={save}>
        <div className="panel-body">
          {isVolunteer ? (
            <>
              <div className="form-grid">
                <div className={`field ${errors.firstName ? 'invalid' : ''}`}>
                  <label htmlFor="firstName">First name <span className="req">*</span></label>
                  <input id="firstName" value={form.firstName} onChange={set('firstName')} />
                  <span className="error-msg"><Icon name="alert" size={16} /><span className="em-text">{errors.firstName}</span></span>
                </div>
                <div className={`field ${errors.lastName ? 'invalid' : ''}`}>
                  <label htmlFor="lastName">Last name <span className="req">*</span></label>
                  <input id="lastName" value={form.lastName} onChange={set('lastName')} />
                  <span className="error-msg"><Icon name="alert" size={16} /><span className="em-text">{errors.lastName}</span></span>
                </div>
              </div>
              <div className="field">
                <label htmlFor="bio">Biography</label>
                <textarea id="bio" value={form.bio} onChange={set('bio')} placeholder="Share your volunteering background, interests, or constraints." />
              </div>
              <div className="field">
                <label>Skills</label>
                <TagMultiSelect options={data.skills} value={form.skillIds} onChange={(skillIds) => setForm((f) => ({ ...f, skillIds }))} />
              </div>
              <div className="field">
                <label>Areas of interest</label>
                <TagMultiSelect options={data.causes} value={form.causeIds} onChange={(causeIds) => setForm((f) => ({ ...f, causeIds }))} />
              </div>
              <div className="field">
                <label>Availability</label>
                <div className="stack" style={{ gap: 10 }}>
                  {form.availability.map((a) => (
                    <div className="lrow avail-row" key={a.dayOfWeek} style={{ padding: 12 }}>
                      <label className="check-row" style={{ minWidth: 130 }}>
                        <span className="check-ctrl">
                          <input type="checkbox" checked={a.enabled} onChange={(e) => updateAvailability(a.dayOfWeek, { enabled: e.target.checked })} />
                          <span className="check-box"><Icon name="check" size={14} stroke={3} /></span>
                        </span>
                        {DayOfWeek[a.dayOfWeek]}
                      </label>
                      <div className="flex gap-8" style={{ flexWrap: 'wrap' }}>
                        <input aria-label={`${DayOfWeek[a.dayOfWeek]} start time`} type="time" value={a.startTime} onChange={(e) => updateAvailability(a.dayOfWeek, { startTime: e.target.value })} disabled={!a.enabled} />
                        <input aria-label={`${DayOfWeek[a.dayOfWeek]} end time`} type="time" value={a.endTime} onChange={(e) => updateAvailability(a.dayOfWeek, { endTime: e.target.value })} disabled={!a.enabled} />
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </>
          ) : (
            <>
              <div className="form-grid">
                <div className={`field ${errors.name ? 'invalid' : ''}`}>
                  <label htmlFor="orgName">Organization name <span className="req">*</span></label>
                  <input id="orgName" value={form.name} onChange={set('name')} />
                  <span className="error-msg"><Icon name="alert" size={16} /><span className="em-text">{errors.name}</span></span>
                </div>
                <div className={`field ${errors.registrationNumber ? 'invalid' : ''}`}>
                  <label htmlFor="registrationNumber">Registration number <span className="req">*</span></label>
                  <input id="registrationNumber" value={form.registrationNumber} onChange={set('registrationNumber')} />
                  <span className="error-msg"><Icon name="alert" size={16} /><span className="em-text">{errors.registrationNumber}</span></span>
                </div>
              </div>
              <div className={`field ${errors.workArea ? 'invalid' : ''}`}>
                <label htmlFor="workArea">Field of work <span className="req">*</span></label>
                <input id="workArea" value={form.workArea} onChange={set('workArea')} />
                <span className="error-msg"><Icon name="alert" size={16} /><span className="em-text">{errors.workArea}</span></span>
              </div>
              <div className={`field ${errors.location ? 'invalid' : ''}`}>
                <label htmlFor="location">Location <span className="req">*</span></label>
                <input id="location" value={form.location} onChange={set('location')} />
                <span className="error-msg"><Icon name="alert" size={16} /><span className="em-text">{errors.location}</span></span>
              </div>
              <div className={`field ${errors.mission ? 'invalid' : ''}`}>
                <label htmlFor="mission">Mission <span className="req">*</span></label>
                <textarea id="mission" value={form.mission} onChange={set('mission')} />
                <span className="error-msg"><Icon name="alert" size={16} /><span className="em-text">{errors.mission}</span></span>
              </div>
            </>
          )}
        </div>
        <div className="modal-foot">
          <button type="submit" className="btn btn-primary btn-sm" disabled={saving}>
            {saving ? <><Spinner size={15} /> Saving...</> : 'Save changes'}
          </button>
        </div>
      </form>
    </main>
  );
}
