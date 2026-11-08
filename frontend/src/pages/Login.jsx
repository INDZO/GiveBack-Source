import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import Icon from '../components/Icon';
import { TextField, PasswordField } from '../components/form/Field';
import { Spinner } from '../components/States';
import { useAuth } from '../auth/AuthContext';
import { dashboardPath } from '../lib/routes';
import { ApiError } from '../api/client';
import logoWhite from '../assets/logos/logo-vertical-white.png';

const EMAIL = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;


export default function Login() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [form, setForm] = useState({ email: '', password: '' });
  const [errors, setErrors] = useState({});
  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState('');

  const redirectTo = location.state?.from;
  const set = (k) => (e) => { setForm((f) => ({ ...f, [k]: e.target.value })); setErrors((x) => ({ ...x, [k]: undefined })); };

  const validate = () => {
    const e = {};
    if (!form.email.trim()) e.email = 'Please enter your email address.';
    else if (!EMAIL.test(form.email)) e.email = 'Please enter a valid email address.';
    if (!form.password) e.password = 'Please enter your password.';
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const doLogin = async (email, password) => {
    setSubmitting(true);
    setFormError('');
    try {
      const session = await login(email, password);
      navigate(redirectTo || dashboardPath(session.role), { replace: true });
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) setFormError('Incorrect email or password.');
      else if (err instanceof ApiError && err.isBusy) setFormError('The service is busy right now. Please try again shortly.');
      else setFormError(err.message || 'Could not sign in. Please try again.');
    } finally {
      setSubmitting(false);
    }
  };

  const onSubmit = (e) => {
    e.preventDefault();
    if (!validate()) return;
    doLogin(form.email.trim(), form.password);
  };

  return (
    <main className="auth">
      <section className="auth-brand brand-purple">
        <Link to="/" className="auth-back"><Icon name="arrowLeft" size={18} stroke={2.2} />Back to home</Link>
        <div className="ab-grid" />
        <span className="ab-blob b1" /><span className="ab-blob b2" />
        <div className="auth-brand-inner">
          <Link to="/" aria-label="GiveBack home"><img className="auth-logo" src={logoWhite} alt="GiveBack" /></Link>
          <h2>Sign in and continue making an impact.</h2>
          <p className="bp-sub">Access your opportunities, applications, verified hours, certificates, and organization tools.</p>
          <ul className="brand-points">
            {['Track your verified volunteer hours', 'Manage applications and certificates', "Coordinate your organization's actions"].map((t) => (
              <li key={t}><span className="bp-check"><Icon name="check" size={14} stroke={2.6} /></span>{t}</li>
            ))}
          </ul>
        </div>
      </section>

      <section className="auth-form-side">
        <div className="auth-form-wrap">
          <div className="form-card tight">
            <div className="form-head">
              <h1>Welcome back</h1>
              <p>Log in to your GiveBack account.</p>
            </div>

            {formError && (
              <div className="alert" style={{ background: 'var(--red-bg)', border: '1px solid var(--red-bd)', color: 'var(--red)', marginBottom: 18 }}>
                <Icon name="alert" size={20} /><span>{formError}</span>
              </div>
            )}

            <form onSubmit={onSubmit} noValidate>
              <TextField
                label="Email address" required type="email" id="email" name="email"
                placeholder="demo@example.invalid" autoComplete="email"
                value={form.email} onChange={set('email')} error={errors.email}
              />
              <PasswordField
                label="Password" required id="password" name="password"
                placeholder="Enter your password" autoComplete="current-password"
                value={form.password} onChange={set('password')} error={errors.password}
              />
              {/* "Remember me" and "Forgot password" were dead controls (no
                  session-vs-local storage switch and no password-reset endpoint
                  exist), so they are intentionally omitted rather than shown
                  as non-functional UI. */}
              <div className="form-actions" style={{ marginTop: 6 }}>
                <button type="submit" className="btn btn-primary" disabled={submitting}>
                  {submitting ? <><Spinner size={16} /> Signing in…</> : 'Log In'}
                </button>
              </div>
            </form>



            <div className="or-div">New to GiveBack</div>
            <div className="alt-link" style={{ marginTop: 0 }}>
              Don't have an account<br />
              <Link to="/register-volunteer">Join as a Volunteer</Link> &nbsp;·&nbsp; <Link to="/join-organization">Register an Organization</Link>
            </div>
            <div className="fine-note">
              <Icon name="shieldCheck" size={18} stroke={2} />
              By continuing, you agree to use GiveBack responsibly and in accordance with platform rules.
            </div>
          </div>
        </div>
      </section>

      <style>{`
        .demo-roles{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:10px}
        .btn-demo{flex-direction:column;gap:7px;padding:14px 8px;font-size:.86rem}
        .btn-demo svg{width:22px;height:22px}
        /* 3-up demo buttons can't fit the longest label on phones — stack them. */
        @media(max-width:430px){.demo-roles{grid-template-columns:1fr}.btn-demo{flex-direction:row;justify-content:center}}
      `}</style>
    </main>
  );
}
