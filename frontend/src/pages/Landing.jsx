import { Link } from 'react-router-dom';
import Icon from '../components/Icon';
import Reveal from '../components/Reveal';
import CountUp from '../components/CountUp';
import { Spinner } from '../components/States';
import { useAsync } from '../lib/useAsync';
import { opportunitiesApi } from '../api/services';
import { causeTone, toneIcon, OpportunityFormat } from '../lib/enums';

/* Qualitative trust items — non-numeric so we never fabricate platform stats.
   The one genuine metric (open opportunities) is shown separately from the
   public, paginated /opportunities endpoint (totalCount). */
const TRUST = [
  { icon: 'shieldCheck', label: 'Verified organizations' },
  { icon: 'clock', label: 'Verified volunteer hours' },
  { icon: 'award', label: 'Certificates & badges' },
];

const STEPS = [
  { icon: 'search', title: 'Discover Opportunities', text: "Browse volunteer listings by location, category, and skills — find the cause that fits you." },
  { icon: 'send', title: 'Apply Easily', text: 'Join opportunities and connect with organizations in minutes — no friction, no paperwork.' },
  { icon: 'barChart', title: 'Track Your Impact', text: 'Log your hours, earn certificates, and showcase your contribution to the community.' },
];

const BENEFITS = [
  { icon: 'shieldCheck', title: 'Verified Organizations', text: "Every organization is reviewed by an admin before it can publish, so you always know who you're working with." },
  { icon: 'clock', title: 'Tracked Volunteer Hours', text: 'Your contribution is logged and verified by the organization — a record you can rely on and share.' },
  { icon: 'award', title: 'Certificates & Badges', text: 'Earn recognition for what you do — generate participation certificates and unlock milestone badges.' },
];

function fmtTime(o) {
  if (o.format === OpportunityFormat.Online) return 'Online session';
  try { return new Date(o.startAt).toLocaleString('en-US', { weekday: 'long', hour: '2-digit', minute: '2-digit' }); }
  catch { return 'Flexible'; }
}

function toCard(o) {
  const tone = causeTone(o.causeName);
  return {
    id: o.id, tone, icon: toneIcon[tone] || 'leaf', cat: o.causeName, title: o.title,
    desc: o.description, loc: o.format === OpportunityFormat.Online ? 'Online' : o.location,
    time: fmtTime(o), skill: o.skillNames?.[0] || o.causeName, isUrgent: o.isUrgent,
  };
}

function OppCard({ o, delay }) {
  return (
    <Reveal as="article" className="opp" delay={delay}>
      <div className={`opp-img ${o.tone}`}>
        <span className={`opp-cat ${o.tone}`}>{o.cat}</span>
        {o.isUrgent && <span className="opp-cat" style={{ right: 14, left: 'auto', background: 'var(--orange-bg,#FFF1E6)', color: 'var(--orange,#B45A14)' }}>Urgent</span>}
        <Icon name={o.icon} size={46} stroke={1.6} />
      </div>
      <div className="opp-body">
        <h3>{o.title}</h3>
        <p className="opp-desc">{o.desc}</p>
        <div className="opp-meta">
          <div><Icon name="mapPin" size={15} stroke={2} /><span className="mlabel">Location:</span> {o.loc}</div>
          <div><Icon name="clock" size={15} stroke={2} /><span className="mlabel">Time:</span> {o.time}</div>
        </div>
        <span className="opp-skill"><Icon name="users" size={13} stroke={2} />{o.skill}</span>
        <Link to={`/opportunities/${o.id}`} className="btn btn-primary">View Opportunity</Link>
      </div>
    </Reveal>
  );
}

export default function Landing() {
  const { data, loading, error, reload } = useAsync(() => opportunitiesApi.list({ page: 1, pageSize: 6 }), []);
  const { data: urgentData } = useAsync(() => opportunitiesApi.urgent(), []);

  const items = Array.isArray(data) ? data : data?.items || [];
  const totalOpen = typeof data?.totalCount === 'number' ? data.totalCount : items.length;
  const featured = items.slice(0, 3).map(toCard);
  const urgent = (Array.isArray(urgentData) ? urgentData : []).slice(0, 3).map(toCard);

  return (
    <>
      {/* ===== Hero ===== */}
      <section className="hero">
        <div className="hero-bg">
          <span className="blob b1" /><span className="blob b2" />
          <div className="hero-grid" />
        </div>
        <div className="container hero-inner">
          <div className="hero-copy reveal in">
            <span className="pill"><span className="dot">New</span> Verified organizations, real impact</span>
            <h1>Give a little time. <span className="accent">Change a lot of lives.</span></h1>
            <p className="hero-sub">GiveBack connects volunteers with organizations that create meaningful impact in their communities.</p>
            <div className="hero-cta">
              <Link to="/opportunities" className="btn btn-primary">Explore Opportunities <Icon name="arrowRight" /></Link>
              <Link to="/join-organization" className="btn btn-secondary">Register Organization</Link>
            </div>
            <div className="hero-trust">
              <div className="avatars"><span>GB</span><span>VG</span><span>Ok</span><span>+</span></div>
              <span>Trusted by volunteers across <strong style={{ color: 'var(--ink)' }}>admin-verified organizations</strong>.</span>
            </div>
          </div>

          <div className="mock reveal in d1" aria-hidden="true">
            <div className="mock-card">
              <div className="mock-top">
                <div className="mt-left">
                  <div className="mock-ava">M</div>
                  <div className="mt-name">Volunteer preview<small>Sample dashboard</small></div>
                </div>
                <div className="mock-search"><Icon name="search" size={15} /> Search</div>
              </div>
              <div className="impact-panel">
                <div className="ip-label">Your volunteer hours, tracked &amp; verified</div>
                <div className="ip-big">47<small>hrs</small></div>
                <div className="bars">
                  <span style={{ height: '40%' }} /><span style={{ height: '62%' }} /><span style={{ height: '48%' }} />
                  <span style={{ height: '78%' }} /><span style={{ height: '58%' }} /><span className="hot" style={{ height: '96%' }} /><span style={{ height: '70%' }} />
                </div>
                <div className="ip-foot">
                  <span className="ip-chip"><Icon name="check" size={12} stroke={2.4} /> Verified hours</span>
                  <span className="ip-chip"><Icon name="trendingUp" size={12} stroke={2.4} /> Milestone badges</span>
                </div>
              </div>
              <div className="mock-list-h"><span>Recommended for you</span><Link to="/opportunities">See all</Link></div>
              <div className="mrow">
                <div className="mrow-ico cat-env"><Icon name="leaf" size={19} stroke={2} /></div>
                <div className="mrow-body">
                  <div className="mrow-title">Matched by your skills <span className="vbadge"><Icon name="shieldCheck" size={14} stroke={2.2} /></span></div>
                  <div className="mrow-meta"><span><Icon name="mapPin" size={11} stroke={2} />Near you</span><span>Flexible</span></div>
                </div>
                <span className="mrow-cat cat-env">Environment</span>
              </div>
              <div className="mrow">
                <div className="mrow-ico cat-edu"><Icon name="code" size={19} stroke={2} /></div>
                <div className="mrow-body">
                  <div className="mrow-title">Matched by your interests <span className="vbadge"><Icon name="shieldCheck" size={14} stroke={2.2} /></span></div>
                  <div className="mrow-meta"><span><Icon name="mapPin" size={11} stroke={2} />Online</span><span>Evenings</span></div>
                </div>
                <span className="mrow-cat cat-edu">Education</span>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* ===== Trust strip (one real metric + qualitative value props) ===== */}
      <section className="stats">
        <div className="container">
          <div className="stats-grid">
            <Reveal className="stat">
              <div className="num">
                {loading ? <Spinner size={26} /> : <CountUp to={totalOpen} suffix={totalOpen > 0 ? '+' : ''} />}
              </div>
              <div className="lbl">Open opportunities</div>
            </Reveal>
            {TRUST.map((t, i) => (
              <Reveal key={t.label} className="stat" delay={i + 1}>
                <div className="num" style={{ display: 'flex', justifyContent: 'center' }}>
                  <span style={{ width: 56, height: 56, borderRadius: 16, background: 'var(--purple-50)', color: 'var(--purple)', display: 'grid', placeItems: 'center' }}>
                    <Icon name={t.icon} size={26} stroke={1.8} />
                  </span>
                </div>
                <div className="lbl">{t.label}</div>
              </Reveal>
            ))}
          </div>
        </div>
      </section>

      {/* ===== How it works ===== */}
      <section className="section">
        <div className="container">
          <Reveal className="section-head">
            <span className="eyebrow">Simple by design</span>
            <h2 style={{ marginTop: 14 }}>How GiveBack Works</h2>
            <p>From discovery to real, measured impact — three steps and you're contributing.</p>
          </Reveal>
          <div className="steps">
            {STEPS.map((s, i) => (
              <Reveal key={s.title} className="step" delay={i}>
                <span className="step-num" />
                <div className="step-ico"><Icon name={s.icon} size={25} stroke={1.8} /></div>
                <h3>{s.title}</h3>
                <p>{s.text}</p>
              </Reveal>
            ))}
          </div>
        </div>
      </section>

      {/* ===== Urgent opportunities (only when the backend returns any) ===== */}
      {urgent.length > 0 && (
        <section className="section" style={{ background: 'var(--bg-soft)', borderTop: '1px solid var(--border)', borderBottom: '1px solid var(--border)' }}>
          <div className="container">
            <Reveal className="section-head">
              <span className="eyebrow" style={{ color: 'var(--orange,#B45A14)' }}><Icon name="alert" size={15} stroke={2.2} /> Urgent right now</span>
              <h2 style={{ marginTop: 14 }}>Help needed urgently</h2>
              <p>Time-sensitive actions from verified organizations — your help makes an immediate difference.</p>
            </Reveal>
            <div className="opps">
              {urgent.map((o, i) => <OppCard key={o.id} o={o} delay={i} />)}
            </div>
          </div>
        </section>
      )}

      {/* ===== Featured opportunities (real public data, honest empty state) ===== */}
      <section className="section">
        <div className="container">
          <Reveal className="section-head">
            <span className="eyebrow">Live now</span>
            <h2 style={{ marginTop: 14 }}>Featured Opportunities</h2>
            <p>Real openings from verified organizations, ready for you to join.</p>
          </Reveal>
          {loading ? (
            <div className="gb-state"><Spinner /><p>Loading opportunities…</p></div>
          ) : error ? (
            <div className="empty">
              <div className="empty-ico"><Icon name="alert" size={26} /></div>
              <h3>Couldn't load opportunities</h3>
              <p>We couldn't reach the server just now. Please try again.</p>
              <button type="button" className="btn btn-secondary" onClick={reload}>Try again</button>
            </div>
          ) : featured.length > 0 ? (
            <>
              <div className="opps">
                {featured.map((o, i) => <OppCard key={o.id} o={o} delay={i} />)}
              </div>
              <div style={{ textAlign: 'center', marginTop: 40 }}>
                <Link to="/opportunities" className="btn btn-secondary">Browse all opportunities <Icon name="arrowRight" size={16} /></Link>
              </div>
            </>
          ) : (
            <div className="empty">
              <div className="empty-ico"><Icon name="search" size={26} /></div>
              <h3>No open opportunities yet</h3>
              <p>New opportunities from verified organizations will appear here. Check back soon or register your organization to publish one.</p>
              <Link to="/join-organization" className="btn btn-primary">Register Organization</Link>
            </div>
          )}
        </div>
      </section>

      {/* ===== Why GiveBack ===== */}
      <section className="section" style={{ background: 'var(--bg-soft)', borderTop: '1px solid var(--border)', borderBottom: '1px solid var(--border)' }}>
        <div className="container">
          <Reveal className="section-head">
            <span className="eyebrow">Why GiveBack</span>
            <h2 style={{ marginTop: 14 }}>Built for trust and real impact</h2>
          </Reveal>
          <Reveal as="p" className="why-lead">GiveBack helps volunteers find opportunities that match their interests and skills, while helping organizations reach the right people faster. The platform tracks volunteer hours, verifies participation, and highlights real community impact.</Reveal>
          <div className="benefits">
            {BENEFITS.map((b, i) => (
              <Reveal key={b.title} className="benefit" delay={i}>
                <div className="benefit-ico"><Icon name={b.icon} size={24} stroke={1.8} /></div>
                <h3>{b.title}</h3>
                <p>{b.text}</p>
              </Reveal>
            ))}
          </div>
        </div>
      </section>

      {/* ===== Dual audience ===== */}
      <section className="section" style={{ paddingTop: 88, paddingBottom: 88 }}>
        <div className="container">
          <Reveal className="section-head">
            <span className="eyebrow">One platform, two sides</span>
            <h2 style={{ marginTop: 14 }}>Made for volunteers and organizations</h2>
          </Reveal>
          <div className="dual">
            <Reveal className="dual-card vol">
              <span className="dual-tag">For Volunteers</span>
              <h3>Do good, build your story</h3>
              <ul className="dual-list">
                {['Find meaningful opportunities', 'Build experience', 'Track hours and achievements', 'Support causes you care about'].map((t) => (
                  <li key={t}><span className="dual-check"><Icon name="check" size={14} stroke={2.6} /></span>{t}</li>
                ))}
              </ul>
              <Link to="/register-volunteer" className="btn btn-primary">Join as a Volunteer</Link>
            </Reveal>
            <Reveal className="dual-card org" delay={1}>
              <span className="dual-tag">For Organizations</span>
              <h3>Reach the right people, faster</h3>
              <ul className="dual-list">
                {['Reach motivated volunteers', 'Manage applications efficiently', 'Verify participation', 'Measure social impact'].map((t) => (
                  <li key={t}><span className="dual-check"><Icon name="check" size={14} stroke={2.6} /></span>{t}</li>
                ))}
              </ul>
              <Link to="/join-organization" className="btn btn-light">Register Organization</Link>
            </Reveal>
          </div>
        </div>
      </section>

      {/* ===== Final CTA ===== */}
      <section className="final">
        <div className="container">
          <Reveal className="final-panel">
            <div className="final-inner">
              <h2>Ready to make a difference</h2>
              <p>Join GiveBack as a volunteer or register your organization and start creating impact today.</p>
              <div className="final-cta">
                <Link to="/register-volunteer" className="btn btn-light">Join as a Volunteer</Link>
                <Link to="/join-organization" className="btn btn-outline-light">Register Organization</Link>
              </div>
            </div>
          </Reveal>
        </div>
      </section>
    </>
  );
}
