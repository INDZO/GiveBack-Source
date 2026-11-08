import Icon from './Icon';

/* Loading / empty / error states reused across data-bound pages.
   They lean on app.css helpers (.empty, .q-spinner) where available. */

export function Spinner({ size = 28 }) {
  return <span className="gb-spinner" style={{ width: size, height: size }} aria-label="Loading" />;
}

export function Loading({ label = 'Loading…' }) {
  return (
    <div className="gb-state">
      <Spinner />
      <p>{label}</p>
    </div>
  );
}

export function EmptyState({ icon = 'list', title = 'Nothing here yet', message }) {
  return (
    <div className="empty">
      <div className="empty-ico"><Icon name={icon} size={26} /></div>
      <h3>{title}</h3>
      {message && <p>{message}</p>}
    </div>
  );
}

export function ErrorState({ error, onRetry }) {
  const status = error?.status;
  const busy = error?.isBusy || status === 503;
  const forbidden = status === 403;
  const notFound = status === 404;

  let title = 'Something went wrong';
  let message = error?.message || 'Could not load data.';
  if (busy) {
    title = 'Service is busy';
    message = `This area is receiving a lot of traffic. Please try again${error?.retryAfter ? ` in ${error.retryAfter}s` : ' shortly'}.`;
  } else if (forbidden) {
    title = "You don't have access";
    message = error?.message || 'You are not allowed to view this. It may require a different role or an invitation.';
  } else if (notFound) {
    title = 'Not found';
    message = error?.message || 'This item could not be found, or you do not have access to it.';
  }

  return (
    <div className="gb-state gb-state--error">
      <div className="empty-ico" style={{ color: forbidden ? 'var(--amber, #B45A14)' : 'var(--red, #B91C1C)' }}>
        <Icon name={forbidden ? 'shieldCheck' : 'alert'} size={26} />
      </div>
      <h3>{title}</h3>
      <p>{message}</p>
      {/* Retry makes no sense for a permission error. */}
      {onRetry && !forbidden && <button type="button" className="btn btn-secondary btn-sm" onClick={onRetry}>Try again</button>}
    </div>
  );
}

/* Convenience wrapper: renders loading/error/empty/children based on async state. */
export function AsyncBoundary({ loading, error, empty, onRetry, children, loadingLabel, emptyProps }) {
  if (loading) return <Loading label={loadingLabel} />;
  if (error) return <ErrorState error={error} onRetry={onRetry} />;
  if (empty) return <EmptyState {...emptyProps} />;
  return children;
}
