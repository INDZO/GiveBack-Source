import { createContext, useCallback, useContext, useMemo, useRef, useState } from 'react';
import Icon from './Icon';

const ToastContext = createContext(null);

const ICON = { ok: 'check', err: 'x', info: 'info' };

export function ToastProvider({ children }) {
  const [toasts, setToasts] = useState([]);
  const idRef = useRef(0);

  const dismiss = useCallback((id) => {
    setToasts((list) => list.map((t) => (t.id === id ? { ...t, show: false } : t)));
    setTimeout(() => setToasts((list) => list.filter((t) => t.id !== id)), 400);
  }, []);

  const push = useCallback((type, title, sub) => {
    const id = ++idRef.current;
    setToasts((list) => [...list, { id, type, title, sub, show: false }]);
    requestAnimationFrame(() => requestAnimationFrame(() => {
      setToasts((list) => list.map((t) => (t.id === id ? { ...t, show: true } : t)));
    }));
    setTimeout(() => dismiss(id), 3600);
    return id;
  }, [dismiss]);

  const toast = useMemo(() => ({
    ok: (title, sub) => push('ok', title, sub),
    err: (title, sub) => push('err', title, sub),
    info: (title, sub) => push('info', title, sub),
  }), [push]);

  return (
    <ToastContext.Provider value={toast}>
      {children}
      <div className="toast-wrap" aria-live="polite" aria-atomic="false">
        {toasts.map((t) => (
          <div key={t.id} role="status" className={`toast ${t.type === 'err' ? 'err' : t.type === 'info' ? 'info' : ''} ${t.show ? 'show' : ''}`}>
            <span className="toast-ico"><Icon name={ICON[t.type] || 'check'} size={15} stroke={2.4} /></span>
            <div className="toast-txt"><b>{t.title}</b>{t.sub && <span>{t.sub}</span>}</div>
            <button type="button" className="toast-x" aria-label="Dismiss notification" onClick={() => dismiss(t.id)}><Icon name="x" size={14} stroke={2.4} /></button>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
}

export function useToast() {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error('useToast must be used within ToastProvider');
  return ctx;
}
