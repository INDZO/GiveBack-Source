import { useMemo, useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import Icon from '../components/Icon';
import { AsyncBoundary } from '../components/States';
import { useAsync } from '../lib/useAsync';
import { opportunitiesApi, lookupsApi } from '../api/services';
import { useAuth } from '../auth/AuthContext';
import {
  causeTone, toneIcon, OpportunityFormat, OpportunityType, OpportunityTypeLabel, UserRole,
} from '../lib/enums';
import {
  DEFAULT_OPPORTUNITIES_PAGE_SIZE,
  OPPORTUNITY_SORT_OPTIONS,
  normalizePagedOpportunities,
} from '../lib/opportunities';

function fmtDateTime(o) {
  if (o.format === OpportunityFormat.Online && !o.startAt) return 'Flexible · Remote';
  try {
    return new Date(o.startAt).toLocaleString('en-US', { weekday: 'short', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
  } catch { return 'Flexible'; }
}

function OpportunityCard({ o, basePath }) {
  const tone = causeTone(o.causeName);
  const filled = o.requiredVolunteersCount ? Math.min(100, Math.round((o.acceptedVolunteersCount / o.requiredVolunteersCount) * 100)) : 0;
  const nearlyFull = filled >= 90;
  return (
    <article className="ocard">
      <div className={`ocard-top ${tone}`}>
        <span className={`ocard-cat cat ${tone}`}>{o.causeName}</span>
        {o.isUrgent && <span className="st full" style={{ position: 'absolute', top: 14, right: 14 }}>Urgent</span>}
        <Icon name={toneIcon[tone] || 'leaf'} size={46} stroke={1.5} />
      </div>
      <div className="ocard-body">
        <div className="ocard-org">{o.organizationName} <span className="vchk"><Icon name="shieldCheck" size={14} stroke={2.4} /></span></div>
        <h3>{o.title}</h3>
        <div className="ocard-meta">
          <div><Icon name="mapPin" size={15} stroke={2} /><span className="ml">{o.format === OpportunityFormat.Online ? 'Online' : o.location}</span></div>
          <div><Icon name="clock" size={15} stroke={2} />{fmtDateTime(o)}</div>
        </div>
        <div className="ocard-cap">
          <div className="cap-row">
            <span>Spots filled</span>
            {nearlyFull
              ? <span className="st full" style={{ padding: '2px 9px' }}>Nearly full</span>
              : <span><b>{o.acceptedVolunteersCount}</b>/{o.requiredVolunteersCount}</span>}
          </div>
          <div className="progress"><span style={{ width: `${filled}%`, ...(nearlyFull ? { background: 'linear-gradient(90deg,var(--orange),#e0913f)' } : {}) }} /></div>
        </div>
        <div className="ocard-foot"><Link to={`${basePath}/${o.id}`} className="btn btn-primary btn-sm">View &amp; apply</Link></div>
      </div>
    </article>
  );
}

export default function Opportunities({ shell = false }) {
  const { role } = useAuth();
  const navigate = useNavigate();
  const [params, setParams] = useSearchParams();
  const basePath = shell ? '/app/opportunities' : '/opportunities';

  const [search, setSearch] = useState(params.get('search') || '');
  const [causeId, setCauseId] = useState(params.get('causeId') || '');
  const [format, setFormat] = useState(params.get('format') || '');
  const [location, setLocation] = useState(params.get('location') || '');
  const [dateFrom, setDateFrom] = useState(params.get('dateFrom') || params.get('date') || '');
  const [dateTo, setDateTo] = useState(params.get('dateTo') || params.get('date') || '');
  const [skillId, setSkillId] = useState(params.get('skillId') || '');
  const [minDurationHours, setMinDurationHours] = useState(params.get('minDurationHours') || '');
  const [maxDurationHours, setMaxDurationHours] = useState(params.get('maxDurationHours') || '');
  const [type, setType] = useState(params.get('type') || '');
  const [urgentOnly, setUrgentOnly] = useState(params.get('urgent') === '1');
  const [matchedOnly, setMatchedOnly] = useState(params.get('matched') === '1');
  const [sortBy, setSortBy] = useState(params.get('sortBy') || '');
  const [appliedSearch, setAppliedSearch] = useState(params.get('search') || '');
  const [page, setPage] = useState(Math.max(1, Number(params.get('page')) || 1));

  const { data: causes, error: causesError } = useAsync(() => lookupsApi.causes(), []);
  const { data: skills, error: skillsError } = useAsync(() => lookupsApi.skills(), []);
  const lookupsFailed = causesError || skillsError;

  const query = useMemo(() => ({
    search: appliedSearch || undefined,
    causeId: causeId || undefined,
    location: location || undefined,
    dateFrom: dateFrom || undefined,
    dateTo: dateTo || undefined,
    skillIds: skillId ? [skillId] : undefined,
    format: format || undefined,
    minDurationHours: minDurationHours || undefined,
    maxDurationHours: maxDurationHours || undefined,
    type: type || undefined,
    isUrgent: urgentOnly || undefined,
    sortBy: sortBy || undefined,
    page,
    pageSize: DEFAULT_OPPORTUNITIES_PAGE_SIZE,
  }), [appliedSearch, causeId, location, dateFrom, dateTo, skillId, format, minDurationHours, maxDurationHours, type, urgentOnly, sortBy, page]);

  const { data, loading, error, reload } = useAsync(
    () => (matchedOnly && role === UserRole.Volunteer ? opportunitiesApi.matchingMe() : opportunitiesApi.list(query)),
    [query, matchedOnly, role],
  );
  const paged = normalizePagedOpportunities(data);
  const items = paged.items;

  const { data: urgentData } = useAsync(() => opportunitiesApi.urgent(), []);
  const urgent = (Array.isArray(urgentData) ? urgentData : []).slice(0, 3);
  /* Only highlight the urgent band on the default view (no urgent/matched filter active). */
  const showUrgentBand = urgent.length > 0 && !urgentOnly && !matchedOnly;

  const submitSearch = (e) => {
    e.preventDefault();
    setPage(1);
    setAppliedSearch(search.trim());
    const next = {};
    if (search.trim()) next.search = search.trim();
    if (causeId) next.causeId = causeId;
    if (format) next.format = format;
    if (location.trim()) next.location = location.trim();
    if (dateFrom) next.dateFrom = dateFrom;
    if (dateTo) next.dateTo = dateTo;
    if (skillId) next.skillId = skillId;
    if (minDurationHours) next.minDurationHours = minDurationHours;
    if (maxDurationHours) next.maxDurationHours = maxDurationHours;
    if (type) next.type = type;
    if (urgentOnly) next.urgent = '1';
    if (matchedOnly) next.matched = '1';
    if (sortBy) next.sortBy = sortBy;
    setParams(next);
  };

  const clearFilters = () => {
    setSearch('');
    setAppliedSearch('');
    setCauseId('');
    setFormat('');
    setLocation('');
    setDateFrom('');
    setDateTo('');
    setSkillId('');
    setMinDurationHours('');
    setMaxDurationHours('');
    setType('');
    setUrgentOnly(false);
    setMatchedOnly(false);
    setSortBy('');
    setPage(1);
    setParams({});
  };

  const goToPage = (nextPage) => {
    const normalized = Math.max(1, Math.min(nextPage, paged.totalPages || 1));
    setPage(normalized);
    setParams((current) => {
      const next = Object.fromEntries(current);
      if (normalized > 1) next.page = String(normalized);
      else delete next.page;
      return next;
    });
  };

  const content = (
    <main className="page">
      <div className="page-head">
        <div>
          <div className="ph-eyebrow">Live now</div>
          <h1>Find your next opportunity</h1>
          <p className="ph-sub">Browse verified openings near you and online.{!loading && <> <b style={{ color: 'var(--ink)' }}>{matchedOnly ? items.length : paged.totalCount} opportunities</b> match your filters.</>}</p>
        </div>
        {role === UserRole.Volunteer && (
          <div className="ph-actions">
            <button className="btn btn-secondary btn-sm" onClick={() => { setMatchedOnly(true); navigate('/app/opportunities?matched=1'); }}>
              <Icon name="target" size={16} /> Matched for you
            </button>
          </div>
        )}
      </div>

      {showUrgentBand && (
        <section className="panel" style={{ marginBottom: 24, borderColor: 'var(--orange-bd,#FFD9B8)' }}>
          <div className="panel-head" style={{ background: 'var(--orange-bg,#FFF1E6)' }}>
            <h3><span className="ph-ico" style={{ background: 'var(--orange-bg,#FFF1E6)', color: 'var(--orange,#B45A14)' }}><Icon name="alert" size={16} /></span>Urgent — help needed now</h3>
            <button type="button" className="link" onClick={() => { setUrgentOnly(true); setPage(1); setParams((p) => { const n = Object.fromEntries(p); n.urgent = '1'; delete n.page; return n; }); }}>See all urgent <Icon name="arrowRight" size={15} /></button>
          </div>
          <div className="panel-body">
            <div className="opp-grid">
              {urgent.map((o) => <OpportunityCard key={o.id} o={o} basePath={basePath} />)}
            </div>
          </div>
        </section>
      )}

      {lookupsFailed && (
        <div className="alert" style={{ background: 'var(--amber-bg,#FFF7E6)', border: '1px solid var(--amber-bd,#FFE2A8)', color: 'var(--amber,#B45A14)', marginBottom: 16, display: 'flex', gap: 10, alignItems: 'center', padding: '12px 14px', borderRadius: 10 }}>
          <Icon name="alert" size={18} />
          <span>Some filter options (causes/skills) could not be loaded, so a few filters may be unavailable. Search and browsing still work.</span>
        </div>
      )}

      <form className="filters" onSubmit={submitSearch}>
        <label className="f-search">
          <Icon name="search" size={18} stroke={2} />
          <input type="text" placeholder="Search by title, organization, or skill…" value={search} onChange={(e) => setSearch(e.target.value)} />
        </label>
        <div className="f-divider" />
        <input aria-label="Location" type="text" placeholder="Location" value={location} onChange={(e) => setLocation(e.target.value)} />
        <label className="filter-field">
          <span>From</span>
          <input aria-label="From date" type="date" value={dateFrom} max={dateTo || undefined} onChange={(e) => setDateFrom(e.target.value)} />
        </label>
        <label className="filter-field">
          <span>To</span>
          <input aria-label="To date" type="date" value={dateTo} min={dateFrom || undefined} onChange={(e) => setDateTo(e.target.value)} />
        </label>
        <select aria-label="Format" value={format} onChange={(e) => setFormat(e.target.value)}>
          <option value="">Any format</option>
          <option value={OpportunityFormat.OnSite}>In person</option>
          <option value={OpportunityFormat.Online}>Online</option>
        </select>
        <select aria-label="Opportunity type" value={type} onChange={(e) => setType(e.target.value)}>
          <option value="">Any type</option>
          <option value={OpportunityType.OneTime}>{OpportunityTypeLabel[OpportunityType.OneTime]}</option>
          <option value={OpportunityType.Periodic}>{OpportunityTypeLabel[OpportunityType.Periodic]}</option>
          <option value={OpportunityType.LongTerm}>{OpportunityTypeLabel[OpportunityType.LongTerm]}</option>
        </select>
        <select aria-label="Required skill" value={skillId} onChange={(e) => setSkillId(e.target.value)}>
          <option value="">Any skill</option>
          {(skills || []).map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
        </select>
        <select aria-label="Minimum duration" value={minDurationHours} onChange={(e) => setMinDurationHours(e.target.value)}>
          <option value="">Any minimum</option>
          <option value="1">At least 1h</option>
          <option value="2">At least 2h</option>
          <option value="4">At least 4h</option>
        </select>
        <select aria-label="Duration" value={maxDurationHours} onChange={(e) => setMaxDurationHours(e.target.value)}>
          <option value="">Any duration</option>
          <option value="2">Up to 2h</option>
          <option value="4">Up to 4h</option>
          <option value="8">Up to 8h</option>
        </select>
        <select aria-label="Sort" value={sortBy} onChange={(e) => setSortBy(e.target.value)}>
          {OPPORTUNITY_SORT_OPTIONS.map((o) => <option key={o.value || 'default'} value={o.value}>{o.label}</option>)}
        </select>
        <label className="check-row" style={{ padding: '0 4px' }}>
          <span className="check-ctrl"><input type="checkbox" checked={urgentOnly} onChange={(e) => setUrgentOnly(e.target.checked)} /><span className="check-box"><Icon name="check" size={14} stroke={3} /></span></span>
          Urgent
        </label>
        {role === UserRole.Volunteer && (
          <label className="check-row" style={{ padding: '0 4px' }}>
            <span className="check-ctrl"><input type="checkbox" checked={matchedOnly} onChange={(e) => setMatchedOnly(e.target.checked)} /><span className="check-box"><Icon name="check" size={14} stroke={3} /></span></span>
            Matched
          </label>
        )}
        <button type="submit" className="btn btn-primary btn-sm">Search</button>
        <button type="button" className="btn btn-secondary btn-sm" onClick={clearFilters}>Clear</button>
      </form>

      <div className="chips" style={{ marginBottom: 24 }}>
        <button type="button" className={`chip${causeId === '' ? ' on' : ''}`} onClick={() => setCauseId('')}>All causes</button>
        {(causes || []).map((c) => (
          <button type="button" key={c.id} className={`chip${String(causeId) === String(c.id) ? ' on' : ''}`} onClick={() => setCauseId(c.id)}>
            <span className="cdot" style={{ background: `var(--${({ env: 'green', ani: 'orange', edu: 'purple', health: 'red', tech: 'blue' })[causeTone(c.name)]})` }} />
            {c.name}
          </button>
        ))}
      </div>

      <AsyncBoundary
        loading={loading} error={error} onRetry={reload}
        empty={!loading && !error && items.length === 0}
        emptyProps={{ icon: 'search', title: 'No opportunities found', message: 'Try adjusting your filters or search terms.' }}
        loadingLabel="Loading opportunities…"
      >
        <div className="opp-grid">
          {items.map((o) => <OpportunityCard key={o.id} o={o} basePath={basePath} />)}
        </div>
        {!matchedOnly && paged.totalPages > 1 && (
          <div className="panel-foot" style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 12, marginTop: 24 }}>
            <button type="button" className="btn btn-secondary btn-sm" disabled={paged.page <= 1} onClick={() => goToPage(paged.page - 1)}>
              <Icon name="arrowLeft" size={15} /> Previous
            </button>
            <span className="muted" style={{ fontSize: '.9rem' }}>
              Page {paged.page} of {paged.totalPages} · Showing {items.length} of {paged.totalCount}
            </span>
            <button type="button" className="btn btn-secondary btn-sm" disabled={paged.page >= paged.totalPages} onClick={() => goToPage(paged.page + 1)}>
              Next <Icon name="arrowRight" size={15} />
            </button>
          </div>
        )}
      </AsyncBoundary>
    </main>
  );

  if (shell) return content;
  return <div className="opp-public">{content}</div>;
}
