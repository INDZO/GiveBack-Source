import { useState } from 'react';
import Icon from '../Icon';

/* Renders the design's `.field` markup (auth.css) with label, control, and an
   accessible error message. `error` toggles the `.invalid` state. */
export function Field({ label, htmlFor, required, error, hint, children, className = '' }) {
  return (
    <div className={`field ${error ? 'invalid' : ''} ${className}`.trim()}>
      {label && (
        <label htmlFor={htmlFor}>
          {label} {required && <span className="req">*</span>}
        </label>
      )}
      {children}
      {hint && !error && <span className="hint">{hint}</span>}
      <span className="error-msg">
        <Icon name="alert" size={16} stroke={2} />
        <span className="em-text">{error || 'This field is required.'}</span>
      </span>
    </div>
  );
}

export function TextField({ label, required, error, hint, type = 'text', ...input }) {
  return (
    <Field label={label} htmlFor={input.id || input.name} required={required} error={error} hint={hint}>
      <input type={type} {...input} />
    </Field>
  );
}

export function PasswordField({ label = 'Password', required, error, ...input }) {
  const [show, setShow] = useState(false);
  return (
    <Field label={label} htmlFor={input.id || input.name} required={required} error={error}>
      <div className="pwd-wrap">
        <input type={show ? 'text' : 'password'} {...input} />
        <button type="button" className="pwd-toggle" aria-label={show ? 'Hide password' : 'Show password'} onClick={() => setShow((s) => !s)}>
          <Icon name={show ? 'eyeOff' : 'eye'} size={20} stroke={2} />
        </button>
      </div>
    </Field>
  );
}
