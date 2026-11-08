import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import Icon from '../components/Icon';
import { TextField, PasswordField, Field } from '../components/form/Field';
import TagMultiSelect from '../components/form/TagMultiSelect';
import { Spinner } from '../components/States';
import { useAuth } from '../auth/AuthContext';
import { useAsync } from '../lib/useAsync';
import { lookupsApi } from '../api/services';
import { useToast } from '../components/Toast';
import { ApiError } from '../api/client';
import { DayOfWeek } from '../lib/enums';
import logo from '../assets/logos/logo-vertical.png';

const EMAIL = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

const DEFAULT_AVAILABILITY = [1, 2, 3, 4, 5, 6, 0].map((dayOfWeek) => ({
  dayOfWeek,
  enabled: false,
  startTime: '09:00',
  endTime: '17:00',
}));

function buildAvailability(availability) {
  const selected = availability
    .filter((a) => a.enabled)
    .map((a) => ({ dayOfWeek: a.dayOfWeek, startTime: `${a.startTime}:00`, endTime: `${a.endTime}:00` }));
  return selected.length ? selected : null;
}

export default function RegisterVolunteer() {
  const { registerVolunteer } = useAuth();
  const navigate = useNavigate();
  const toast = useToast();
  const { data: skills } = useAsync(() => lookupsApi.skills(), []);
  const { data: causes } = useAsync(() => lookupsApi.causes(), []);

  const [f, setF] = useState({
    firstName: '', lastName: '', email: '', password: '', confirm: '',
    bio: '', location: '', phone: '', availability: DEFAULT_AVAILABILITY, agree: false,
  });
  const [skillIds, setSkillIds] = useState([]);
  const [causeIds, setCauseIds] = useState([]);
  const [errors, setErrors] = useState({});
  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState('');

  const set = (k) => (e) => {
    const v = e.target.type === 'checkbox' ? e.target.checked : e.target.value;
    setF((s) => ({ ...s, [k]: v }));
    setErrors((x) => ({ ...x, [k]: undefined }));
  };

  const updateAvailability = (day, patch) => {
    setF((s) => ({
      ...s,
      availability: s.availability.map((a) => (a.dayOfWeek === day ? { ...a, ...patch } : a)),
    }));
    setErrors((x) => ({ ...x, availability: undefined }));
  };

  const validate = () => {
    const e = {};
    if (!f.firstName.trim()) e.firstName = 'Please enter your first name.';
    if (!f.lastName.trim()) e.lastName = 'Please enter your last name.';
    if (!f.email.trim()) e.email = 'Please enter your email address.';
    else if (!EMAIL.test(f.email)) e.email = 'Please enter a valid email address.';
    if (!f.password) e.password = 'Please create a password.';
    else if (f.password.length < 8) e.password = 'Password must be at least 8 characters.';
    if (f.confirm !== f.password) e.confirm = 'Passwords do not match.';
    if (!f.location.trim()) e.location = 'Please enter your location.';
    f.availability.forEach((a) => {
      if (!a.enabled) return;
      if (!a.startTime || !a.endTime) e.availability = 'Enter both start and end time for selected days.';
      else if (a.endTime <= a.startTime) e.availability = 'End time must be after start time for selected days.';
    });
    if (!f.agree) e.agree = 'This confirmation is required.';
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const onSubmit = async (e) => {
    e.preventDefault();
    if (!validate()) return;
    setSubmitting(true);
    setFormError('');
    try {
      const bioParts = [f.bio.trim(), f.location && `Based in ${f.location.trim()}.`, f.phone && `Phone: ${f.phone.trim()}.`].filter(Boolean);
      await registerVolunteer({
        email: f.email.trim(),
        password: f.password,
        firstName: f.firstName.trim(),
        lastName: f.lastName.trim(),
        bio: bioParts.join(' ') || null,
        skillIds,
        causeIds,
        availability: buildAvailability(f.availability),
      });
      toast.ok('Welcome to GiveBack!', 'Your volunteer account is ready.');
      navigate('/app', { replace: true });
    } catch (err) {
      if (err instanceof ApiError && err.fieldErrors) {
        setFormError(err.message);
      } else {
        setFormError(err.message || 'Could not create your account. Please try again.');
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <main className="auth">
      <section className="auth-brand brand-light">
        <Link to="/" className="auth-back"><Icon name="arrowLeft" size={18} stroke={2.2} />Back to home</Link>
        <span className="al-glow g1" />
        <div className="auth-brand-inner">
          <Link to="/" aria-label="GiveBack home"><img className="auth-logo" src={logo} alt="GiveBack" /></Link>
          <h2>Join GiveBack as a Volunteer</h2>
          <p className="bp-sub">Create your profile, discover meaningful opportunities, track your hours, and earn recognition for your contribution.</p>
          <ul className="brand-points">
            {['Get matched to opportunities that fit you', 'Log hours and have them verified', 'Earn certificates and badges'].map((t) => (
              <li key={t}><span className="bp-check"><Icon name="check" size={14} stroke={2.6} /></span>{t}</li>
            ))}
          </ul>
        </div>
      </section>

      <section className="auth-form-side">
        <div className="auth-form-wrap">
          <div className="form-card purple">
            <div className="form-head">
              <h1>Create volunteer account</h1>
              <p>It only takes a couple of minutes to get started.</p>
            </div>

            {formError && (
              <div className="alert" style={{ background: 'var(--red-bg)', border: '1px solid var(--red-bd)', color: 'var(--red)', marginBottom: 16 }}>
                <Icon name="alert" size={20} /><span>{formError}</span>
              </div>
            )}

            <form onSubmit={onSubmit} noValidate>
              <div className="form-sec-title"><span className="fst-n">1</span> Account information</div>
              <div className="form-sec-desc">Your login details and basic identity.</div>
              <div className="form-grid">
                <TextField label="First name" required id="fname" placeholder="Mara" autoComplete="given-name" value={f.firstName} onChange={set('firstName')} error={errors.firstName} />
                <TextField label="Last name" required id="lname" placeholder="Jovanovic" autoComplete="family-name" value={f.lastName} onChange={set('lastName')} error={errors.lastName} />
                <TextField className="full" label="Email address" required type="email" id="vemail" placeholder="you@example.com" autoComplete="email" value={f.email} onChange={set('email')} error={errors.email} />
                <PasswordField label="Password" required id="vpwd" placeholder="At least 8 characters" autoComplete="new-password" value={f.password} onChange={set('password')} error={errors.password} />
                <PasswordField label="Confirm password" required id="vpwd2" placeholder="Re-enter password" autoComplete="new-password" value={f.confirm} onChange={set('confirm')} error={errors.confirm} />
              </div>

              <div className="form-sec">
                <div className="form-sec-title"><span className="fst-n">2</span> Volunteer profile</div>
                <div className="form-sec-desc">Help organizations understand how you'd like to contribute.</div>
                <Field className="full" label="Short biography">
                  <textarea id="bio" placeholder="Tell organizations a little about yourself and why you volunteer." value={f.bio} onChange={set('bio')} />
                </Field>
                <Field className="full" label="Skills" hint="Select all that apply.">
                  <TagMultiSelect options={skills || []} value={skillIds} onChange={setSkillIds} />
                </Field>
                <Field className="full" label="Areas of interest" hint="Choose the causes you care about.">
                  <TagMultiSelect options={causes || []} value={causeIds} onChange={setCauseIds} />
                </Field>
                <Field className="full" label="Availability" error={errors.availability} hint="Choose any days and time ranges that work for you.">
                  <div className="stack" style={{ gap: 10 }}>
                    {f.availability.map((a) => (
                      <div className="lrow avail-row" key={a.dayOfWeek} style={{ padding: 12 }}>
                        <label className="check-row" style={{ minWidth: 130 }}>
                          <span className="check-ctrl">
                            <input type="checkbox" checked={a.enabled} onChange={(event) => updateAvailability(a.dayOfWeek, { enabled: event.target.checked })} />
                            <span className="check-box"><Icon name="check" size={14} stroke={3} /></span>
                          </span>
                          {DayOfWeek[a.dayOfWeek]}
                        </label>
                        <div className="flex gap-8" style={{ flexWrap: 'wrap' }}>
                          <input aria-label={`${DayOfWeek[a.dayOfWeek]} start time`} type="time" value={a.startTime} onChange={(event) => updateAvailability(a.dayOfWeek, { startTime: event.target.value })} disabled={!a.enabled} />
                          <input aria-label={`${DayOfWeek[a.dayOfWeek]} end time`} type="time" value={a.endTime} onChange={(event) => updateAvailability(a.dayOfWeek, { endTime: event.target.value })} disabled={!a.enabled} />
                        </div>
                      </div>
                    ))}
                  </div>
                </Field>
                <div className="form-grid">
                  <TextField label="Location" required id="vloc" placeholder="City, Country" autoComplete="address-level2" value={f.location} onChange={set('location')} error={errors.location} />
                  <TextField label={<>Phone number <span className="opt">(optional)</span></>} type="tel" id="vphone" placeholder="+381 ..." autoComplete="tel" value={f.phone} onChange={set('phone')} />
                </div>
              </div>

              <div className={`field full ${errors.agree ? 'invalid' : ''}`} style={{ marginTop: 6 }}>
                <label className="check-row">
                  <span className="check-ctrl"><input type="checkbox" checked={f.agree} onChange={set('agree')} /><span className="check-box"><Icon name="check" size={14} stroke={3} /></span></span>
                  I agree to use GiveBack responsibly and accept the platform rules. <span className="req">*</span>
                </label>
                <span className="error-msg" style={{ marginLeft: 33 }}><Icon name="alert" size={16} stroke={2} /><span className="em-text">{errors.agree || 'This confirmation is required.'}</span></span>
              </div>

              <div className="form-actions">
                <button type="submit" className="btn btn-primary" disabled={submitting}>
                  {submitting ? <><Spinner size={16} /> Creating...</> : 'Create Volunteer Account'}
                </button>
              </div>
            </form>
            <div className="alt-link">Already have an account <Link to="/login">Log in</Link></div>
          </div>
        </div>
      </section>
    </main>
  );
}
