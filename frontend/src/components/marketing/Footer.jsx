import { Link } from 'react-router-dom';
import logo from '../../assets/logos/logo-vertical-clean.png';

export default function Footer() {
  return (
    <footer className="footer">
      <div className="container">
        <div className="footer-inner">
          <div>
            <div className="footer-logo"><img src={logo} alt="GiveBack" /></div>
            <p className="footer-mission">A platform that connects people, organizations, and good ideas.</p>
          </div>
          <div className="footer-links">
            <span className="fl-title">Explore</span>
            <Link to="/">Home</Link>
            <Link to="/opportunities">Opportunities</Link>
            <Link to="/about">About</Link>
            <Link to="/login">Login</Link>
          </div>
        </div>
        {/* Social links removed: they pointed at href="#" (dead links). This is a
            student project with no live social presence, so no fake links are shown. */}
        <div className="footer-bottom">
          <span>© 2026 GiveBack. All rights reserved.</span>
          <span className="footer-note">A student project for connecting volunteers and nonprofit organizations.</span>
        </div>
      </div>
    </footer>
  );
}
