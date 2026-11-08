import { Link } from 'react-router-dom';
import Icon from '../components/Icon';
import { Loading, ErrorState, EmptyState } from '../components/States';
import { useAsync } from '../lib/useAsync';
import { notificationsApi } from '../api/services';
import { useToast } from '../components/Toast';
import { useAuth } from '../auth/AuthContext';
import { notificationAction } from '../lib/notifications';

export default function Notifications() {
  const toast = useToast();
  const { role } = useAuth();
  const { data, loading, error, reload, setData } = useAsync(() => notificationsApi.list(), []);
  const notifications = data || [];

  if (loading) return <main className="page narrow"><Loading label="Loading notifications..." /></main>;
  if (error) return <main className="page narrow"><ErrorState error={error} onRetry={reload} /></main>;

  const markRead = async (notification) => {
    if (notification.isRead) return;
    try {
      await notificationsApi.markRead(notification.id);
      setData((list) => (list || []).map((n) => (n.id === notification.id ? { ...n, isRead: true } : n)));
    } catch (err) {
      toast.err('Could not update notification', err?.message);
    }
  };

  const markAll = async () => {
    try {
      await notificationsApi.markAllRead();
      setData((list) => (list || []).map((n) => ({ ...n, isRead: true })));
      toast.ok('Notifications marked as read');
    } catch (err) {
      toast.err('Could not mark all read', err?.message);
    }
  };

  const unread = notifications.filter((n) => !n.isRead).length;

  return (
    <main className="page narrow">
      <div className="page-head">
        <div>
          <div className="ph-eyebrow">Updates</div>
          <h1>Notifications</h1>
          <p className="ph-sub">Matching opportunities, reminders, milestones, application updates, and project endings.</p>
        </div>
        {unread > 0 && <button className="btn btn-secondary btn-sm" onClick={markAll}>Mark all read</button>}
      </div>

      <section className="panel">
        {notifications.length === 0 ? <EmptyState icon="bell" title="No notifications" message="Important updates will appear here." /> : (
          <div className="panel-body flush">
            {notifications.map((n) => {
              const action = notificationAction(n, role);
              return (
                <div
                  key={n.id}
                  className="lrow"
                  style={{ borderLeft: n.isRead ? 'none' : '4px solid var(--purple)' }}
                >
                  <div className={`lrow-ico ${n.isRead ? 'edu' : 'tech'}`}><Icon name="bell" size={19} /></div>
                  <div className="lrow-body">
                    <div className="lrow-title">{n.title} {!n.isRead && <span className="st submitted">New</span>}</div>
                    <div className="muted" style={{ fontSize: '.9rem', marginTop: 4 }}>{n.message}</div>
                    <div className="lrow-meta"><span>{new Date(n.createdAt).toLocaleString('en-US', { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' })}</span></div>
                  </div>
                  <div className="lrow-end" style={{ display: 'flex', gap: 8, flexWrap: 'wrap', alignItems: 'center' }}>
                    {action && (
                      <Link to={action.to} className="btn btn-secondary btn-sm" onClick={() => markRead(n)}>
                        {action.label} <Icon name="arrowRight" size={15} />
                      </Link>
                    )}
                    {!n.isRead && (
                      <button type="button" className="link" style={{ fontSize: '.82rem' }} onClick={() => markRead(n)}>Mark read</button>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </section>
    </main>
  );
}
