import { useState } from 'react';
import Icon from '../../components/Icon';
import Modal from '../../components/Modal';
import { Loading, ErrorState, EmptyState, Spinner } from '../../components/States';
import { OpportunityStatusPill } from '../../components/StatusPill';
import { useAsync } from '../../lib/useAsync';
import { organizationsApi, impactApi } from '../../api/services';
import { useToast } from '../../components/Toast';
import { OpportunityStatus } from '../../lib/enums';
import { ApiError } from '../../api/client';

export default function OrgReports() {
  const toast = useToast();
  const { data, loading, error, reload } = useAsync(async () => {
    const [impact, opps] = await Promise.allSettled([organizationsApi.impact(), organizationsApi.myOpportunities()]);
    // Both feeds failing => the page itself failed; one failing => partial error.
    if (impact.status === 'rejected' && opps.status === 'rejected') throw impact.reason;
    const opportunities = opps.status === 'fulfilled' ? opps.value : [];
    const reported = opportunities.filter((o) => o.hasImpactReport);
    const reports = await Promise.allSettled(reported.map((o) => impactApi.opportunityReport(o.id)));
    const reportByOpportunityId = reports.reduce((acc, result) => {
      if (result.status === 'fulfilled' && result.value?.opportunityId != null) {
        acc[result.value.opportunityId] = result.value;
      }
      return acc;
    }, {});
    return {
      impact: impact.status === 'fulfilled' ? impact.value : null,
      impactError: impact.status === 'rejected' ? impact.reason : null,
      opps: opportunities,
      oppsError: opps.status === 'rejected' ? opps.reason : null,
      reportByOpportunityId,
    };
  }, []);

  const [modal, setModal] = useState(null); // opportunity being reported
  const [form, setForm] = useState({ beneficiaries: '', summary: '' });
  const [saving, setSaving] = useState(false);
  const [reportErr, setReportErr] = useState('');

  if (loading) return <main className="page"><Loading label="Loading reports..." /></main>;
  if (error) return <main className="page"><ErrorState error={error} onRetry={reload} /></main>;

  const { impact, impactError, opps, oppsError, reportByOpportunityId } = data;
  const completed = opps.filter((o) => o.status === OpportunityStatus.Completed);

  const KPIS = [
    { icon: 'clock', val: impact?.totalHours ?? 0, lbl: 'Total volunteer hours' },
    { icon: 'users', tone: 'green', val: impact?.volunteersCount ?? 0, lbl: 'Volunteers engaged' },
    { icon: 'heartHand', tone: 'orange', val: impact?.beneficiariesHelpedCount ?? 0, lbl: 'Beneficiaries helped' },
    { icon: 'fileText', tone: 'blue', val: impact?.reportsCount ?? 0, lbl: 'Reports published' },
  ];
  const impactBars = [
    { label: 'Hours', value: impact?.totalHours ?? 0 },
    { label: 'Volunteers', value: impact?.volunteersCount ?? 0 },
    { label: 'Helped', value: impact?.beneficiariesHelpedCount ?? 0 },
    { label: 'Reports', value: impact?.reportsCount ?? 0 },
  ];
  const maxImpact = Math.max(1, ...impactBars.map((b) => Number(b.value) || 0));

  const openReport = (o) => { setForm({ beneficiaries: '', summary: '' }); setReportErr(''); setModal(o); };

  const saveReport = async (e) => {
    e.preventDefault();
    if (!form.summary.trim()) { setReportErr('Add a short summary of the impact.'); return; }
    setSaving(true);
    setReportErr('');
    try {
      await impactApi.createReport(modal.id, {
        beneficiariesHelpedCount: Number(form.beneficiaries) || 0,
        summary: form.summary.trim(),
      });
      toast.ok('Impact report published', `Report saved for ${modal.title}.`);
      setModal(null);
      reload();
    } catch (err) {
      // The backend rejects a duplicate report, an opportunity with no verified
      // hours, or an invalid status (409/400). We can't always predict these from
      // the DTO, so we surface the exact reason inline rather than hide it.
      if (err instanceof ApiError && (err.status === 409 || err.status === 400)) {
        setReportErr(err.message || 'A report already exists or there are no verified hours yet.');
      } else {
        setReportErr(err?.message || 'Could not save report. Please try again.');
      }
    } finally {
      setSaving(false);
    }
  };

  return (
    <main className="page">
      <div className="page-head">
        <div>
          <div className="ph-eyebrow">Impact</div>
          <h1>Impact Reports</h1>
          <p className="ph-sub">Measure your organization's cumulative contribution and publish per-opportunity reports.</p>
        </div>
      </div>

      {impactError ? (
        <div className="panel" style={{ marginBottom: 24 }}><ErrorState error={impactError} onRetry={reload} /></div>
      ) : (
        <>
          <div className="kpi-grid">
            {KPIS.map((k) => (
              <div className="kpi" key={k.lbl}>
                <div className="kpi-top"><div className={`kpi-ico ${k.tone || ''}`}><Icon name={k.icon} size={21} stroke={1.9} /></div></div>
                <div className="kpi-num">{k.val}</div>
                <div className="kpi-lbl">{k.lbl}</div>
              </div>
            ))}
          </div>

          <section className="panel" style={{ marginBottom: 24 }}>
            <div className="panel-head"><h3><span className="ph-ico"><Icon name="barChart" size={16} stroke={2} /></span>Organization impact breakdown</h3></div>
            <div className="panel-body">
              <div className="barchart impact-chart" style={{ height: 240 }}>
                {impactBars.map((b) => {
                  const value = Number(b.value) || 0;
                  return (
                    <div className="bc" key={b.label}>
                      <div className={`bc-bar${value > 0 ? ' hot' : ''}`} style={{ height: Math.max(10, Math.round((value / maxImpact) * 190)) }}>
                        {value > 0 && <span className="bc-val">{value}</span>}
                      </div>
                      <div className="bc-lbl">{b.label}</div>
                    </div>
                  );
                })}
              </div>
            </div>
          </section>
        </>
      )}

      <section className="panel">
        <div className="panel-head"><h3><span className="ph-ico"><Icon name="barChart" size={16} stroke={2} /></span>Completed opportunities</h3></div>
        <div className="panel-body flush">
          {oppsError && <ErrorState error={oppsError} onRetry={reload} />}
          {!oppsError && completed.length === 0 && <EmptyState icon="barChart" title="No completed opportunities" message="Once an opportunity is completed you can publish its impact report." />}
          {!oppsError && completed.map((o) => {
            const report = reportByOpportunityId?.[o.id];
            return (
              <div className="lrow" key={o.id}>
                <div className="lrow-body">
                  <div className="lrow-title">{o.title} <OpportunityStatusPill status={o.status} /></div>
                  <div className="lrow-meta">
                    <span>{report?.volunteersCount ?? o.acceptedVolunteersCount} volunteer{(report?.volunteersCount ?? o.acceptedVolunteersCount) === 1 ? '' : 's'}</span>
                    {report ? <span>{report.totalHours} verified hours</span> : <span>{o.durationHours}h planned</span>}
                    <span>{o.causeName}</span>
                    {report ? <span>{report.beneficiariesHelpedCount} helped</span> : o.verifiedHoursCount != null && <span>{o.verifiedHoursCount} verified log{o.verifiedHoursCount === 1 ? '' : 's'}</span>}
                  </div>
                  {report?.summary && <div className="muted" style={{ fontSize: '.86rem', marginTop: 6, lineHeight: 1.5 }}>{report.summary}</div>}
                </div>
                <div className="lrow-end">
                  {/* Backend capability flags drive this: a report is created only
                      once, and only when the opportunity has verified hours. */}
                  {o.hasImpactReport ? (
                    <span className="st verified"><Icon name="check" size={13} stroke={2.4} /> Reported</span>
                  ) : o.canCreateImpactReport ? (
                    <button className="btn btn-primary btn-sm" onClick={() => openReport(o)}><Icon name="fileText" size={15} /> Create report</button>
                  ) : (
                    <button className="btn btn-secondary btn-sm" disabled title="Requires at least one verified hour log">No verified hours yet</button>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      </section>

      <Modal
        open={!!modal} onClose={() => setModal(null)} title={`Impact report - ${modal?.title || ''}`}
        footer={(
          <>
            <button className="btn btn-secondary btn-sm" onClick={() => setModal(null)}>Cancel</button>
            <button className="btn btn-primary btn-sm" onClick={saveReport} disabled={saving}>{saving ? <><Spinner size={15} /> Saving…</> : 'Publish report'}</button>
          </>
        )}
      >
        <form onSubmit={saveReport}>
          {reportErr && (
            <div className="alert" style={{ background: 'var(--red-bg)', border: '1px solid var(--red-bd)', color: 'var(--red)', marginBottom: 14, display: 'flex', gap: 8, alignItems: 'center', padding: '12px 14px', borderRadius: 10 }}>
              <Icon name="alert" size={18} /><span>{reportErr}</span>
            </div>
          )}
          <div className="field">
            <label htmlFor="rep-ben">Beneficiaries helped</label>
            <input id="rep-ben" type="number" min="0" placeholder="e.g. 120" value={form.beneficiaries} onChange={(e) => setForm((f) => ({ ...f, beneficiaries: e.target.value }))} />
          </div>
          <div className="field">
            <label htmlFor="rep-sum">Summary <span className="req">*</span></label>
            <textarea id="rep-sum" placeholder="Describe the outcome and impact of this opportunity." value={form.summary} onChange={(e) => setForm((f) => ({ ...f, summary: e.target.value }))} />
          </div>
        </form>
      </Modal>
    </main>
  );
}
