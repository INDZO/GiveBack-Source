import { useEffect, useId, useRef } from 'react';
import Icon from './Icon';

const FOCUSABLE = 'a[href],button:not([disabled]),textarea:not([disabled]),input:not([disabled]),select:not([disabled]),[tabindex]:not([tabindex="-1"])';
const FIELD_FOCUSABLE = 'input:not([disabled]):not([type="hidden"]),textarea:not([disabled]),select:not([disabled])';

/* Accessible modal matching app.css `.modal-back`/`.modal`.
   - role="dialog" + aria-modal + aria-labelledby
   - Escape + backdrop click to close
   - focus trap inside the dialog, returns focus to the opener on close
   - sticky header/footer so long forms keep Cancel/Submit reachable */
export default function Modal({ open, onClose, title, children, footer, width = 520 }) {
  const dialogRef = useRef(null);
  const openerRef = useRef(null);
  const onCloseRef = useRef(onClose);
  const titleId = useId();

  useEffect(() => {
    onCloseRef.current = onClose;
  }, [onClose]);

  useEffect(() => {
    if (!open) return undefined;
    openerRef.current = document.activeElement;
    const onKey = (e) => {
      if (e.key === 'Escape') { onCloseRef.current(); return; }
      if (e.key === 'Tab') {
        const nodes = dialogRef.current?.querySelectorAll(FOCUSABLE);
        if (!nodes || nodes.length === 0) return;
        const list = Array.from(nodes).filter((n) => n.offsetParent !== null || n === document.activeElement);
        if (list.length === 0) return;
        const first = list[0];
        const last = list[list.length - 1];
        if (e.shiftKey && document.activeElement === first) { e.preventDefault(); last.focus(); }
        else if (!e.shiftKey && document.activeElement === last) { e.preventDefault(); first.focus(); }
      }
    };
    document.addEventListener('keydown', onKey);
    document.body.style.overflow = 'hidden';
    // Move focus into the dialog without treating the close button as the
    // preferred field for form modals.
    const t = setTimeout(() => {
      if (!dialogRef.current) return;
      if (dialogRef.current.contains(document.activeElement)) return;
      const target = dialogRef.current.querySelector('[autofocus], [data-autofocus]')
        || dialogRef.current.querySelector(FIELD_FOCUSABLE)
        || dialogRef.current.querySelector(FOCUSABLE)
        || dialogRef.current;
      target.focus();
    }, 0);
    return () => {
      document.removeEventListener('keydown', onKey);
      document.body.style.overflow = '';
      clearTimeout(t);
      // Return focus to whatever opened the modal.
      if (openerRef.current && typeof openerRef.current.focus === 'function') openerRef.current.focus();
    };
  }, [open]);

  if (!open) return null;
  return (
    <div className="modal-back show" onMouseDown={(e) => { if (e.target === e.currentTarget) onClose(); }}>
      <div
        className="modal" style={{ maxWidth: width }}
        role="dialog" aria-modal="true" aria-labelledby={titleId}
        ref={dialogRef} tabIndex={-1}
      >
        <div className="modal-head">
          <h3 id={titleId}>{title}</h3>
          <button className="modal-x" aria-label="Close" onClick={onClose}><Icon name="x" size={18} stroke={2} /></button>
        </div>
        <div className="modal-body">{children}</div>
        {footer && <div className="modal-foot">{footer}</div>}
      </div>
    </div>
  );
}
