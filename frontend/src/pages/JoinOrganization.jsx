import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import Navbar from '../components/marketing/Navbar';
import Footer from '../components/marketing/Footer';
import Icon from '../components/Icon';
import Reveal from '../components/Reveal';
import { TextField, PasswordField, Field } from '../components/form/Field';
import { Spinner } from '../components/States';
import { useAuth } from '../auth/AuthContext';
import { useToast } from '../components/Toast';
import { ApiError } from '../api/client';
import logoWhite from '../assets/logos/logo-vertical-white.png';

const EMAIL = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const FIELDS = ['Education', 'Environment', 'Animals', 'Social Support', 'Health', 'Humanitarian Aid', 'Technology'];

const BENEFITS = [
  { icon: 'search', title: 'Reach the right volunteers', text: 'Publish opportunities with required skills, cause, location, format, duration, and capacity.' },
  { icon: 'checkCircle', title: 'Manage applications easily', text: 'Review applications, accept or reject volunteers, and track attendance.' },
  { icon: 'barChart', title: 'Verify and report impact', text: 'Approve completed hours, issue certificates, and measure total community contribution.' },
];

export default function JoinOrganization() {
  const { registerOrganization } = useAuth();
  const navigate = useNavigate();
  const toast = useToast();

  useEffect(() => { document.body.classList.remove('app'); window.scrollTo(0, 0); }, []);

  // NOTE: This form only collects fields the backend registration endpoint
  // actually persists (name, registration number, field of work, mission,
  // location, email, password). Extra intake fields are intentionally omitted
  // so we never ask for data that would be silently discarded.
  const [f, setF] = useState({
    name: '', reg: '', type: '', mission: '',
    email: '', country: '', city: '',
    password: '', confirm: '', authorized: false,
  });
  const [errors, setErrors] = useState({});
  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState('');

  const set = (k) => (e) => {
    const v = e.target.type === 'checkbox' ? e.target.checked : e.target.value;
    setF((s) => ({ ...s, [k]: v })); setErrors((x) => ({ ...x, [k]: undefined }));
  };

  const validate = () => {
    const e = {};
    if (!f.name.trim()) e.name = 'This field is required.';
    if (!f.reg.trim()) e.reg = 'This field is required.';
    if (!f.type) e.type = 'Please select a field of work.';
    if (!f.mission.trim()) e.mission = 'This field is required.';
    if (!f.email.trim()) e.email = 'This field is required.';
    else if (!EMAIL.test(f.email)) e.email = 'Please enter a valid email address.';
    if (!f.country.trim()) e.country = 'This field is required.';
    if (!f.city.trim()) e.city = 'This field is required.';
    if (!f.authorized) e.authorized = 'This confirmation is required.';
    if (!f.password) e.password = 'Please create a password.';
    else if (f.password.length < 8) e.password = 'Password must be at least 8 characters.';
    if (f.confirm !== f.password) e.confirm = 'Passwords do not match.';
    setErrors(e);
    if (Object.keys(e).length) {
      document.getElementById('register').scrollIntoView({ behavior: 'smooth' });
    }
    return Object.keys(e).length === 0;
  };

  const onSubmit = async (e) => {
    e.preventDefault();
    if (!validate()) return;
    setSubmitting(true);
    setFormError('');
    try {
      await registerOrganization({
        email: f.email.trim(),
        password: f.password,
        name: f.name.trim(),
        registrationNumber: f.reg.trim(),
        mission: f.mission.trim(),
        workArea: f.type,
        location: [f.city.trim(), f.country.trim()].filter(Boolean).join(', '),
      });
      toast.ok('Request submitted', 'An admin will review your organization shortly.');
      navigate('/org', { replace: true });
    } catch (err) {
      if (err instanceof ApiError) setFormError(err.message);
      else setFormError('Could not submit your request. Please try again.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <>
      <Navbar />

      <section className="org-hero">
        <div className="org-hero-grid" />
        <span className="org-hero-blob o1" /><span className="org-hero-blob o2" />
        <div className="container org-hero-inner">
          <div className="reveal in">
            <span className="org-hero-pill"><Icon name="shieldCheck" size={16} stroke={2} /> Organization profiles are reviewed before verification</span>
            <h1>Bring more volunteers to your mission.</h1>
            <p className="oh-sub">Register your organization, publish opportunities, manage applications, verify volunteer hours, and measure your social impact.</p>
            <p className="oh-supp">GiveBack helps nonprofit organizations reach motivated volunteers and coordinate community actions with confidence.</p>
            <div className="org-hero-cta">
              <a href="#register" className="btn btn-light">Register Your Organization</a>
              <a href="#how" className="btn btn-outline-light">Learn How It Works</a>
            </div>
          </div>
          <div className="org-hero-logo-wrap reveal in d1">
            <div className="org-hero-logo-card"><img src={logoWhite} alt="GiveBack" /></div>
          </div>
        </div>
      </section>

      <section className="section" id="how">
        <div className="container">
          <Reveal className="section-head">
            <span className="eyebrow">Why join</span>
            <h2 style={{ marginTop: 14 }}>Built for organizations that create impact</h2>
            <p>GiveBack gives organizations a structured way to publish volunteering opportunities, receive applications, confirm attendance, verify completed hours, and generate impact reports.</p>
          </Reveal>
          <div className="grid-3">
            {BENEFITS.map((b, i) => (
              <Reveal key={b.title} className="card" delay={i}>
                <div className="card-ico"><Icon name={b.icon} size={24} stroke={1.8} /></div>
                <h3>{b.title}</h3>
                <p>{b.text}</p>
              </Reveal>
            ))}
          </div>
        </div>
      </section>

      <section className="section org-form-section" id="register">
        <div className="container">
          <Reveal className="org-form-card">
            <div className="org-form-top">
              <h2>Register your organization</h2>
              <p>Submit your organization details. An administrator will review your profile before approval, and your profile becomes visible once verified.</p>
            </div>
            <div className="org-form-body">
              {formError && (
                <div className="alert" style={{ background: 'var(--red-bg)', border: '1px solid var(--red-bd)', color: 'var(--red)', marginBottom: 18 }}>
                  <Icon name="alert" size={20} /><span>{formError}</span>
                </div>
              )}
              <form onSubmit={onSubmit} noValidate>
                <div className="form-sec-title"><span className="fst-n">1</span> Organization information</div>
                <div className="form-sec-desc">Tell us about your organization and its work.</div>
                <div className="form-grid">
                  <TextField label="Organization name" required id="oname" placeholder="Green Future Initiative" value={f.name} onChange={set('name')} error={errors.name} />
                  <TextField label="Registration number" required id="oreg" placeholder="e.g. 20384756" value={f.reg} onChange={set('reg')} error={errors.reg} />
                  <Field className="full" label="Field of work" required htmlFor="otype" error={errors.type}>
                    <select id="otype" value={f.type} onChange={set('type')}>
                      <option value="">Select a field of work</option>
                      {FIELDS.map((x) => <option key={x}>{x}</option>)}
                    </select>
                  </Field>
                  <Field className="full" label="Mission statement" required htmlFor="omission" error={errors.mission}>
                    <textarea id="omission" placeholder="A short statement describing your organization's mission." value={f.mission} onChange={set('mission')} />
                  </Field>
                </div>

                <div className="form-sec">
                  <div className="form-sec-title"><span className="fst-n">2</span> Location</div>
                  <div className="form-sec-desc">Where your organization operates.</div>
                  <div className="form-grid">
                    <TextField label="Country" required id="ocountry" placeholder="Country" autoComplete="country-name" value={f.country} onChange={set('country')} error={errors.country} />
                    <TextField label="City" required id="ocity" placeholder="City" autoComplete="address-level2" value={f.city} onChange={set('city')} error={errors.city} />
                  </div>
                </div>

                <div className="form-sec">
                  <div className="form-sec-title"><span className="fst-n">3</span> Account setup</div>
                  <div className="form-sec-desc">Create the credentials for your organization account.</div>
                  <div className="form-grid">
                    <TextField className="full" label="Email" required type="email" id="oemail" placeholder="contact@organization.org" autoComplete="email" value={f.email} onChange={set('email')} error={errors.email} hint="Used as your account login." />
                    <PasswordField label="Password" required id="opwd" placeholder="At least 8 characters" autoComplete="new-password" value={f.password} onChange={set('password')} error={errors.password} />
                    <PasswordField label="Confirm password" required id="opwd2" placeholder="Re-enter password" autoComplete="new-password" value={f.confirm} onChange={set('confirm')} error={errors.confirm} />
                  </div>
                  <div className={`field full ${errors.authorized ? 'invalid' : ''}`} style={{ marginBottom: 0, marginTop: 6 }}>
                    <label className="check-row">
                      <span className="check-ctrl"><input type="checkbox" checked={f.authorized} onChange={set('authorized')} /><span className="check-box"><Icon name="check" size={14} stroke={3} /></span></span>
                      I confirm that I am authorized to register this organization. <span className="req">*</span>
                    </label>
                    <span className="error-msg" style={{ marginLeft: 33 }}><Icon name="alert" size={16} stroke={2} /><span className="em-text">{errors.authorized || 'This confirmation is required.'}</span></span>
                  </div>
                </div>

                <div className="fine-note">
                  <Icon name="info" size={18} stroke={2} />
                  An administrator reviews each organization before verification. Your profile becomes visible after verification.
                </div>

                <div className="form-actions">
                  <button type="submit" className="btn btn-primary" disabled={submitting}>
                    {submitting ? <><Spinner size={16} /> Submitting...</> : 'Submit Organization Request'}
                  </button>
                </div>
              </form>
              <div className="alt-link">Already registered <Link to="/login">Log in</Link></div>
            </div>
          </Reveal>
        </div>
      </section>

      <Footer />
    </>
  );
}
