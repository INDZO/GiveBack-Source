import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';

/* Design stylesheets - order matches the original handoff cascade:
   about.css (shared base) -> landing -> auth -> app -> local extras. */
import './styles/about.css';
import './styles/landing.css';
import './styles/auth.css';
import './styles/app.css';
import './styles/extra.css';

import App from './App.jsx';
import { AuthProvider } from './auth/AuthContext';
import { ToastProvider } from './components/Toast';

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <BrowserRouter>
      <AuthProvider>
        <ToastProvider>
          <App />
        </ToastProvider>
      </AuthProvider>
    </BrowserRouter>
  </StrictMode>,
);
