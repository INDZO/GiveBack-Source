import { useState } from 'react';
import { Link, NavLink } from 'react-router-dom';
import Icon from '../Icon';
import { useAuth } from '../../auth/AuthContext';
import { dashboardPath } from '../../lib/routes';
import logo from '../../assets/logos/logo-horizontal.png';

export default function Navbar() {
  const [open, setOpen] = useState(false);
  const { isAuthenticated, role } = useAuth();
  const close = () => setOpen(false);

  const links = [
    { to: '/', label: 'Home', end: true },
    { to: '/opportunities', label: 'Opportunities' },
    { to: '/about', label: 'About' },
    { to: '/join-organization', label: 'Join as Organization' },
  ];

  const cta = isAuthenticated
    ? { to: dashboardPath(role), label: 'Go to Dashboard' }
    : { to: '/login', label: 'Login' };

  return (
    <header className="nav">
      <div className="container nav-inner">
        <Link to="/" className="nav-logo" aria-label="GiveBack home"><img src={logo} alt="GiveBack" /></Link>
        <nav className="nav-links">
          {links.map((l) => (
            <NavLink key={l.to} to={l.to} end={l.end} className={({ isActive }) => `link${isActive ? ' active' : ''}`}>
              {l.label}
            </NavLink>
          ))}
        </nav>
        <div className="nav-right">
          <Link to={cta.to} className="btn btn-primary">{cta.label}</Link>
          <button className="nav-toggle" aria-label="Open menu" onClick={() => setOpen((o) => !o)}>
            <Icon name="menu" size={22} />
          </button>
        </div>
      </div>
      <div className={`mobile-menu${open ? ' open' : ''}`}>
        {links.map((l) => (
          <NavLink key={l.to} to={l.to} end={l.end} onClick={close}>{l.label}</NavLink>
        ))}
        <Link to={cta.to} className="btn btn-primary" onClick={close}>{cta.label}</Link>
      </div>
    </header>
  );
}
