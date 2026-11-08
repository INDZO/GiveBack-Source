import { Link } from 'react-router-dom';
import Icon from '../components/Icon';
import Reveal from '../components/Reveal';
import logo from '../assets/logos/logo-vertical-clean.png';

const FEATURES = [
  { n: '01', icon: 'list', title: 'Opportunity Management', text: 'Organizations can create public or private opportunities, define required skills, location, format, duration, capacity, and opportunity type.' },
  { n: '02', icon: 'search', title: 'Smart Discovery', text: 'Volunteers can filter opportunities by cause, location, date, skills, online or in-person format, and duration.' },
  { n: '03', icon: 'checkCircle', title: 'Application Workflow', text: 'Volunteers apply with a short message, while organizations manage statuses such as submitted, accepted, rejected, attended, and no-show.' },
  { n: '04', icon: 'users', title: 'Team Volunteering', text: 'Team captains can apply on behalf of a group, while members confirm their participation.' },
  { n: '05', icon: 'clock', title: 'Verified Volunteer Hours', text: 'Volunteers submit completed hours with a short description, and organizations approve or adjust them.' },
  { n: '06', icon: 'award', title: 'Certificates and Badges', text: 'The system can generate participation certificates and award badges for milestones such as first hour, 100 hours, multiple causes, or crisis response.' },
  { n: '07', icon: 'barChart', title: 'Impact Reports', text: 'The platform tracks impact by opportunity, organization, and volunteer, including hours, number of participants, completed actions, and people helped.' },
  { n: '08', icon: 'shieldCheck', title: 'Admin Moderation', text: 'Admins verify organizations, manage categories and causes, moderate content, and monitor platform statistics.' },
];

const DB = [
  { icon: 'layers', title: 'Relational Integrity', text: 'Relationships between volunteers, organizations, opportunities, applications, hours, and certificates are modeled with clear constraints and consistent data rules.' },
  { icon: 'barChart', title: 'Reports and Aggregations', text: 'SQL-based aggregation is used for certificates, volunteer profiles, organization impact reports, and platform statistics.' },
  { icon: 'shieldCheck', title: 'Capacity and Consistency', text: 'The system must prevent opportunity capacity from being exceeded when many volunteers apply at the same time.' },
];

/* What the platform measures — described qualitatively so no fabricated
   platform totals are ever presented as real data. */
const IMPACT = [
  { icon: 'clock', lbl: 'Volunteer Hours', d: 'Every hour is logged by the volunteer and verified by the organization.' },
  { icon: 'checkCircle', lbl: 'Completed Actions', d: 'Opportunities are tracked end to end, from open to completed.' },
  { icon: 'shieldCheck', lbl: 'Verified Participation', d: 'Attendance and hours are confirmed by the organizing organization.' },
  { icon: 'barChart', lbl: 'Impact Reports', d: 'Per-opportunity, per-organization, and per-volunteer summaries are generated from real data.' },
];

const FLOW = [
  { n: 'STEP 1', t: 'Request arrives', d: 'A volunteer applies to a popular opportunity.' },
  { n: 'STEP 2', t: 'Limiter checks slots', d: 'Within the parallel limit Process now. Otherwise queue it.' },
  { n: 'STEP 3', t: 'Queue with timeout', d: 'Waits for a free slot up to a configurable timeout.' },
  { n: 'STEP 4', t: 'Graceful fallback', d: "If it can't be served in time, return a clear retry response." },
];

export default function About() {
  return (
    <>
      <section className="ahero">
        <div className="ahero-grid" />
        <span className="ahero-blob x1" /><span className="ahero-blob x2" />
        <div className="container">
          <div className="ahero-inner reveal in">
            <img className="ahero-logo" src={logo} alt="GiveBack" />
            <span className="ahero-pill">Advanced Databases · Student Project</span>
            <h1>About GiveBack</h1>
            <p className="sub">A student project built for the Advanced Databases course.</p>
            <p className="lead">GiveBack is a volunteering platform designed to connect volunteers with nonprofit organizations, support verified participation, and measure real community impact.</p>
            <div className="ahero-cta">
              <Link to="/opportunities" className="btn btn-light">Explore Opportunities</Link>
              <Link to="/" className="btn btn-outline-light">Back to Home</Link>
            </div>
          </div>
        </div>
      </section>

      <section className="section">
        <div className="container">
          <Reveal className="section-head">
            <span className="eyebrow">The context</span>
            <h2 style={{ marginTop: 14 }}>Built as a Student Project</h2>
            <p>GiveBack was created as a student project for the Advanced Databases course. The goal of the project is to design and implement a realistic web platform with a strong relational database model, clear business logic, data integrity, reporting, and support for high-concurrency scenarios.</p>
          </Reveal>
          <Reveal className="note" delay={1} style={{ marginLeft: 'auto', marginRight: 'auto' }}>
            <div className="note-ico"><Icon name="info" size={20} stroke={2} /></div>
            <p>This project focuses not only on the user interface, but also on <strong>database structure, application workflow, verification processes, reporting, and reliable handling of simultaneous user actions.</strong></p>
          </Reveal>
        </div>
      </section>

      <section className="section" style={{ background: 'var(--bg-soft)', borderTop: '1px solid var(--border)', borderBottom: '1px solid var(--border)' }}>
        <div className="container">
          <div className="split">
            <Reveal className="split-copy">
              <span className="eyebrow">The idea</span>
              <h2>The Idea Behind GiveBack</h2>
              <p>GiveBack is a platform that connects volunteers with nonprofit organizations. Organizations can publish volunteering opportunities such as park cleanups, animal shelter support, children's education, translation, technical help, or programming for social good. Volunteers can apply for opportunities, log completed hours, receive verification from organizations, and generate certificates of participation.</p>
              <p>The platform also supports team volunteering, volunteer recognition, badges, and social impact reports that show how much time, effort, and value has been contributed to the community.</p>
            </Reveal>
            <Reveal className="idea-mock" delay={1}>
              <div style={{ fontSize: '.72rem', fontWeight: 600, letterSpacing: '.05em', textTransform: 'uppercase', color: 'var(--muted)', marginBottom: 12 }}>Sample illustration</div>
              <div className="im-head">
                <div className="imh-l">
                  <div className="im-ava">M</div>
                  <div className="im-name">Mara Jovanović<small>Volunteer · Novi Pazar</small></div>
                </div>
                <span className="im-badge"><Icon name="check" size={13} stroke={2.4} />Verified</span>
              </div>
              <div className="im-cert">
                <div className="ic-seal"><Icon name="award" size={20} stroke={2} /></div>
                <div className="ic-label">Certificate of Participation</div>
                <div className="ic-title">120 Volunteer Hours</div>
                <div className="ic-sub">Across 5 verified opportunities</div>
              </div>
              <div className="im-stats">
                <div className="im-stat"><div className="s-num">120</div><div className="s-lbl">Verified hours</div></div>
                <div className="im-stat"><div className="s-num">14</div><div className="s-lbl">Completed actions</div></div>
              </div>
              <div className="im-badges">
                <span className="im-chip"><Icon name="award" size={16} stroke={2} />100h Badge</span>
                <span className="im-chip"><Icon name="users" size={16} stroke={2} />Team Player</span>
              </div>
            </Reveal>
          </div>
        </div>
      </section>

      <section className="section">
        <div className="container">
          <Reveal className="section-head">
            <span className="eyebrow">Who it's for</span>
            <h2 style={{ marginTop: 14 }}>One platform, two sides</h2>
            <p>GiveBack is designed around the needs of both the people who give their time and the organizations that put it to work.</p>
          </Reveal>
          <div className="dual">
            <Reveal className="dual-card vol">
              <span className="dual-tag">For Volunteers</span>
              <h3>Do good, build your story</h3>
              <p className="dual-desc">Volunteers can create a profile, define their skills, interests, and availability, discover opportunities that match them, apply to activities, track completed hours, earn badges, and generate certificates.</p>
              <ul className="dual-list">
                {['Personal volunteer profile', 'Skills and interests', 'Opportunity matching', 'Hour tracking', 'Certificates and badges'].map((t) => (
                  <li key={t}><span className="dual-check"><Icon name="check" size={14} stroke={2.6} /></span>{t}</li>
                ))}
              </ul>
            </Reveal>
            <Reveal className="dual-card org" delay={1}>
              <span className="dual-tag">For Organizations</span>
              <h3>Reach the right people, faster</h3>
              <p className="dual-desc">Organizations can create verified profiles, publish opportunities, manage applications, confirm attendance, verify volunteer hours, and generate impact reports.</p>
              <ul className="dual-list">
                {['Verified organization profile', 'Opportunity publishing', 'Application management', 'Hour verification', 'Impact reporting'].map((t) => (
                  <li key={t}><span className="dual-check"><Icon name="check" size={14} stroke={2.6} /></span>{t}</li>
                ))}
              </ul>
            </Reveal>
          </div>
        </div>
      </section>

      <section className="section" style={{ background: 'var(--bg-soft)', borderTop: '1px solid var(--border)', borderBottom: '1px solid var(--border)' }}>
        <div className="container">
          <Reveal className="section-head">
            <span className="eyebrow">Capabilities</span>
            <h2 style={{ marginTop: 14 }}>Key Platform Features</h2>
            <p>Everything from publishing an opportunity to verifying hours and awarding recognition — modeled as one connected workflow.</p>
          </Reveal>
          <div className="feat-grid">
            {FEATURES.map((c, i) => (
              <Reveal key={c.n} className="card feat" delay={i % 3}>
                <span className="feat-num">{c.n}</span>
                <div className="card-ico"><Icon name={c.icon} size={24} stroke={1.8} /></div>
                <h3>{c.title}</h3>
                <p>{c.text}</p>
              </Reveal>
            ))}
          </div>
        </div>
      </section>

      <section className="section">
        <div className="container">
          <Reveal className="section-head">
            <span className="eyebrow">Under the hood</span>
            <h2 style={{ marginTop: 14 }}>Database-Centered Design</h2>
            <p>The project is designed around a relational database such as PostgreSQL or SQL Server. This is important because the core domain contains strongly connected entities: opportunities, applications, volunteer hours, verification records, organizations, volunteers, certificates, badges, and reports.</p>
          </Reveal>
          <div className="grid-3">
            {DB.map((c, i) => (
              <Reveal key={c.title} className="card" delay={i}>
                <div className="card-ico"><Icon name={c.icon} size={24} stroke={1.8} /></div>
                <h3>{c.title}</h3>
                <p>{c.text}</p>
              </Reveal>
            ))}
          </div>
        </div>
      </section>

      <section className="section concurrency">
        <div className="container">
          <Reveal className="section-head">
            <span className="eyebrow">Technical challenge</span>
            <h2 style={{ marginTop: 14 }}>Handling Popular Opportunities</h2>
            <p>One of the important technical challenges in GiveBack is concurrency. Popular volunteering opportunities can receive many applications at the same time. The system must accept applications reliably without exceeding the available capacity and without creating a poor user experience.</p>
          </Reveal>
          <Reveal className="code-card" delay={1}>
            <div className="code-head">
              <div className="code-ico"><Icon name="code" size={22} stroke={1.9} /></div>
              <div>
                <h3>Concurrency Limiter Middleware</h3>
                <p>Controls parallel requests — globally and per endpoint</p>
              </div>
            </div>
            <div className="code-body">
              <p>The project includes a custom concurrency limiter middleware that controls the number of parallel requests globally and per endpoint. For example, the endpoint for applying to an opportunity can be limited to a defined number of parallel operations. Requests above the limit can wait in a queue with a configurable timeout, and if they cannot be processed in time, the system returns a <code>503</code> response with a <code>Retry-After</code> header.</p>
              <div className="code-flow">
                {FLOW.map((s, i) => (
                  <div key={s.n} style={{ display: 'contents' }}>
                    <div className="flow-step">
                      <div className="fs-n">{s.n}</div>
                      <div className="fs-t">{s.t}</div>
                      <div className="fs-d">{s.d}</div>
                    </div>
                    {i < FLOW.length - 1 && <div className="flow-arrow"><Icon name="arrowRight" size={20} stroke={2.4} /></div>}
                  </div>
                ))}
              </div>
              <div className="resp-badge">HTTP 503 · Retry-After</div>
            </div>
          </Reveal>
        </div>
      </section>

      <section className="section">
        <div className="container">
          <Reveal className="section-head">
            <span className="eyebrow">The outcome</span>
            <h2 style={{ marginTop: 14 }}>Measuring Social Impact</h2>
            <p>GiveBack is not only about applying to volunteering opportunities. It is also about measuring contribution. The platform tracks completed actions, total volunteer hours, verified participation, helped communities, and individual volunteer achievements.</p>
          </Reveal>
          <div className="impact-grid">
            {IMPACT.map((c, i) => (
              <Reveal key={c.lbl} className="impact-card" delay={i}>
                <div className="impact-ico"><Icon name={c.icon} size={24} stroke={1.8} /></div>
                <div className="ic-lbl">{c.lbl}</div>
                <div className="ic-d">{c.d}</div>
              </Reveal>
            ))}
          </div>
        </div>
      </section>

      <section className="final">
        <div className="container">
          <Reveal className="final-panel">
            <div className="final-inner">
              <h2>GiveBack turns good intentions into measurable impact.</h2>
              <p>Explore how volunteers, organizations, and technology can work together to support meaningful community action.</p>
              <div className="final-cta">
                <Link to="/opportunities" className="btn btn-light">Explore Opportunities</Link>
                <Link to="/" className="btn btn-outline-light">Back to Home</Link>
              </div>
            </div>
          </Reveal>
        </div>
      </section>
    </>
  );
}
