import { useEffect, useRef, useState } from 'react';
import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom';
import Icon from '../Icon';
import { useAuth } from '../../auth/AuthContext';
import { notificationsApi } from '../../api/services';
import logo from '../../assets/logos/logo-horizontal.png';

const NAV = {
  volunteer: {
    role: 'Volunteer', accent: '', base: '/app',
    links: [
      { to: '/app', label: 'Dashboard', end: true },
      { to: '/app/opportunities', label: 'Opportunities' },
      { to: '/app/applications', label: 'Applications' },
      { to: '/app/hours', label: 'Hours & Certificates' },
      { to: '/app/teams', label: 'Teams' },
      { to: '/app/invitations', label: 'Invitations' },
      { to: '/app/report', label: 'Report' },
      { to: '/app/leaderboard', label: 'Leaderboard' },
    ],
  },
  organization: {
    role: 'Organization', accent: 'org', base: '/org',
    links: [
      { to: '/org', label: 'Dashboard', end: true },
      { to: '/org/opportunities', label: 'Opportunities' },
      { to: '/org/applicants', label: 'Applicants' },
      { to: '/org/reports', label: 'Reports' },
      { to: '/org/profile', label: 'Profile' },
    ],
  },
  admin: {
    role: 'Admin', accent: 'admin', base: '/admin',
    links: [
      { to: '/admin', label: 'Dashboard', end: true },
      { to: '/admin/verify', label: 'Verification & Moderation' },
    ],
  },
};

function displayName(variant, profile) {
  if (variant === 'organization') return profile?.name || 'Organization';
  if (variant === 'admin') return 'Administrator';
  if (profile) return `${profile.firstName} ${profile.lastName}`.trim();
  return 'Volunteer';
}

function initials(variant, profile) {
  if (variant === 'organization') return (profile?.name || 'OR').slice(0, 2).toUpperCase();
  if (variant === 'admin') return 'AD';
  if (profile) return `${profile.firstName?.[0] || ''}${profile.lastName?.[0] || ''}`.toUpperCase() || 'V';
  return 'V';
}

export default function AppShell({ variant = 'volunteer' }) {
  const cfg = NAV[variant];
  const { profile, logout, session } = useAuth();
  const navigate = useNavigate();
  const [openDD, setOpenDD] = useState(null); // 'notif' | 'avatar' | null
  const [drawer, setDrawer] = useState(false);
  const [notifs, setNotifs] = useState([]);
  const headerRef = useRef(null);

  useEffect(() => {
    document.body.classList.add('app');
    return () => document.body.classList.remove('app');
  }, []);

  useEffect(() => {
    if (variant === 'admin') {
      setNotifs([]);
      return undefined;
    }
    let active = true;
    notificationsApi.list().then((data) => { if (active) setNotifs(data || []); }).catch(() => {});
    return () => { active = false; };
  }, [session?.userId, variant]);

  useEffect(() => {
    const onDoc = (e) => { if (headerRef.current && !headerRef.current.contains(e.target)) setOpenDD(null); };
    document.addEventListener('click', onDoc);
    return () => document.removeEventListener('click', onDoc);
  }, []);

  const unread = notifs.filter((n) => !n.isRead).length;
  const name = displayName(variant, profile);
  const subtitle = variant === 'volunteer' ? 'Volunteer' : variant === 'organization' ? 'Organization' : 'Administrator';

  const doLogout = async () => { await logout(); navigate('/login'); };

  const markAllRead = async () => {
    try { await notificationsApi.markAllRead(); setNotifs((l) => l.map((n) => ({ ...n, isRead: true }))); } catch { /* ignore */ }
  };

  return (
    <>
      <header className={`topnav topnav-${variant}`} ref={headerRef}>
        <div className="topnav-inner">
          <Link to="/" className="tn-logo" aria-label="GiveBack home">
            <img src={logo} alt="GiveBack" />
            <span className={`tn-role ${cfg.accent}`}>{cfg.role}</span>
          </Link>
          <nav className="tn-links">
            {cfg.links.map((l) => (
              <NavLink key={l.to} to={l.to} end={l.end} className={({ isActive }) => (isActive ? 'active' : '')}>
                {l.label}
              </NavLink>
            ))}
          </nav>
          <div className="tn-right">
            {variant !== 'admin' && (
              <div className={`dd${openDD === 'notif' ? ' open' : ''}`}>
                <button
                  className="tn-icon" aria-label="Notifications"
                  onClick={(e) => { e.stopPropagation(); setOpenDD(openDD === 'notif' ? null : 'notif'); }}
                >
                  <Icon name="bell" size={19} />
                  {unread > 0 && <span className="dot" />}
                </button>
                <div className="dd-menu dd-notif">
                  <div className="dd-head" style={{ justifyContent: 'space-between' }}>
                    <span className="ddh-name">Notifications</span>
                    {unread > 0 && <button type="button" style={{ width: 'auto', padding: '4px 8px', fontSize: '.78rem' }} onClick={markAllRead}>Mark all read</button>}
                  </div>
                  {notifs.length === 0 && <div className="dd-label" style={{ padding: '14px 12px' }}>You're all caught up.</div>}
                  {notifs.slice(0, 6).map((n) => (
                    <button
                      key={n.id} type="button"
                      style={{ alignItems: 'flex-start', opacity: n.isRead ? 0.62 : 1 }}
                      onClick={async () => {
                        if (!n.isRead) { try { await notificationsApi.markRead(n.id); setNotifs((l) => l.map((x) => (x.id === n.id ? { ...x, isRead: true } : x))); } catch { /* ignore */ } }
                      }}
                    >
                      <Icon name="bell" size={16} />
                      <span style={{ fontWeight: 500, lineHeight: 1.35 }}>
                        <b style={{ display: 'block' }}>{n.title}</b>
                        <span style={{ color: 'var(--muted)', fontWeight: 400, fontSize: '.82rem' }}>{n.message}</span>
                      </span>
                    </button>
                  ))}
                </div>
              </div>
            )}

            {/* Avatar */}
            <div className={`dd${openDD === 'avatar' ? ' open' : ''}`}>
              <button className="tn-avatar" onClick={(e) => { e.stopPropagation(); setOpenDD(openDD === 'avatar' ? null : 'avatar'); }}>
                <span className={`tn-ava ${cfg.accent}`}>{initials(variant, profile)}</span>
                <span className="tn-aname">{name}</span>
                <Icon name="chevronDown" size={15} className="chev" />
              </button>
              <div className="dd-menu">
                <div className="dd-head">
                  <span className={`tn-ava ${cfg.accent}`}>{initials(variant, profile)}</span>
                  <div>
                    <div className="ddh-name">{name}</div>
                    <div className="ddh-sub">{subtitle}</div>
                  </div>
                </div>
                <Link to={cfg.base}><Icon name="home" size={17} />Dashboard</Link>
                {variant !== 'admin' && <Link to={`${cfg.base}/profile`}><Icon name="settings" size={17} />Profile settings</Link>}
                {variant === 'volunteer' && <Link to="/app/hours"><Icon name="award" size={17} />Certificates & badges</Link>}
                {variant === 'organization' && <Link to="/org/reports"><Icon name="barChart" size={17} />Impact reports</Link>}
                {variant !== 'admin' && <Link to={`${cfg.base}/notifications`}><Icon name="bell" size={17} />Notifications</Link>}
                <div className="dd-sep" />
                <button type="button" onClick={doLogout}><Icon name="logOut" size={17} />Log out</button>
              </div>
            </div>

            <button className="tn-burger" aria-label="Menu" onClick={(e) => { e.stopPropagation(); setDrawer((d) => !d); }}>
              <Icon name="menu" size={21} />
            </button>
          </div>
        </div>
        <div className={`app-drawer${drawer ? ' open' : ''}`}>
          {cfg.links.map((l) => (
            <NavLink key={l.to} to={l.to} end={l.end} onClick={() => setDrawer(false)} className={({ isActive }) => (isActive ? 'active' : '')}>
              {l.label}
            </NavLink>
          ))}
        </div>
      </header>
      <Outlet />
    </>
  );
}
