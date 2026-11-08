/* Central API client for the GiveBack backend (ASP.NET Core, /api/v1).
   - Attaches the JWT access token.
   - Transparently refreshes the token once on 401 (refresh-token rotation).
   - Surfaces 503 + Retry-After from the ConcurrencyLimiterMiddleware as a
     typed error so the UI can show a friendly "try again shortly" message.
   - Parses RFC7807 ProblemDetails error bodies. */

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5066/api/v1';

const TOKEN_KEY = 'gb_access_token';
const REFRESH_KEY = 'gb_refresh_token';
const SESSION_KEY = 'gb_session';

export const tokenStore = {
  get access() { return localStorage.getItem(TOKEN_KEY); },
  get refresh() { return localStorage.getItem(REFRESH_KEY); },
  get session() {
    try { return JSON.parse(localStorage.getItem(SESSION_KEY)) || null; }
    catch { return null; }
  },
  set(auth) {
    if (auth.accessToken) localStorage.setItem(TOKEN_KEY, auth.accessToken);
    if (auth.refreshToken) localStorage.setItem(REFRESH_KEY, auth.refreshToken);
    localStorage.setItem(SESSION_KEY, JSON.stringify({ userId: auth.userId, role: auth.role }));
  },
  clear() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_KEY);
    localStorage.removeItem(SESSION_KEY);
  },
};

/* Broadcast a forced sign-out so AuthContext can drop the session, protected
   routes can redirect, and the UI can surface a "session expired" message.
   Used when a refresh attempt fails or a request is unauthenticated with no
   way to recover. Guarded so it only fires when there was an active session. */
export function notifySessionExpired() {
  if (typeof window === 'undefined') return;
  window.dispatchEvent(new CustomEvent('gb:session-expired'));
}

export class ApiError extends Error {
  constructor(message, { status, problem, retryAfter } = {}) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.problem = problem;
    this.retryAfter = retryAfter;
  }
  /** Field-level validation errors: { field: [messages] } */
  get fieldErrors() { return this.problem?.errors || null; }
  get isBusy() { return this.status === 503; }
}

let refreshing = null;

async function doRefresh() {
  const refreshToken = tokenStore.refresh;
  if (!refreshToken) {
    tokenStore.clear();
    notifySessionExpired();
    throw new ApiError('Session expired', { status: 401 });
  }
  let res;
  try {
    res = await fetch(`${BASE_URL}/auth/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken }),
    });
  } catch (networkErr) {
    // Network failure during refresh — do NOT clear the session (could be a
    // transient outage); surface the error so the caller can retry.
    throw new ApiError('Network error while refreshing session', { status: 0, problem: { detail: networkErr?.message } });
  }
  if (!res.ok) {
    // Refresh token is invalid/expired/rotated: truly log the user out.
    tokenStore.clear();
    notifySessionExpired();
    throw new ApiError('Session expired', { status: 401 });
  }
  const auth = await res.json();
  tokenStore.set(auth);
  return auth.accessToken;
}

async function parseError(res) {
  let problem = null;
  try { problem = await res.json(); } catch { /* non-json body */ }
  const retryAfter = Number(res.headers.get('Retry-After')) || undefined;
  const message =
    problem?.detail || problem?.title ||
    (res.status === 503 ? 'Service is busy' : `Request failed (${res.status})`);
  return new ApiError(message, { status: res.status, problem, retryAfter });
}

async function request(method, path, { body, auth = true, raw = false, query } = {}) {
  let url = `${BASE_URL}${path}`;
  if (query) {
    const qs = new URLSearchParams();
    Object.entries(query).forEach(([k, v]) => {
      if (v === undefined || v === null || v === '') return;
      if (Array.isArray(v)) v.forEach((item) => qs.append(k, item));
      else qs.append(k, v);
    });
    const s = qs.toString();
    if (s) url += `${url.includes('?') ? '&' : '?'}${s}`;
  }

  const send = async () => {
    const headers = {};
    if (body !== undefined) headers['Content-Type'] = 'application/json';
    if (auth && tokenStore.access) headers.Authorization = `Bearer ${tokenStore.access}`;
    return fetch(url, {
      method,
      headers,
      body: body !== undefined ? JSON.stringify(body) : undefined,
    });
  };

  let res = await send();

  // Transparent single refresh on 401.
  if (res.status === 401 && auth && tokenStore.refresh) {
    try {
      refreshing = refreshing || doRefresh();
      await refreshing;
    } finally {
      refreshing = null;
    }
    res = await send();
  } else if (res.status === 401 && auth && tokenStore.access) {
    // We sent a token, it was rejected, and there is no refresh token to
    // recover with: the session is dead. Force a real logout.
    tokenStore.clear();
    notifySessionExpired();
  }

  if (!res.ok) throw await parseError(res);

  if (raw) return res;
  if (res.status === 204) return null;
  const text = await res.text();
  return text ? JSON.parse(text) : null;
}

export const api = {
  get: (path, opts) => request('GET', path, opts),
  post: (path, body, opts) => request('POST', path, { ...opts, body }),
  put: (path, body, opts) => request('PUT', path, { ...opts, body }),
  patch: (path, body, opts) => request('PATCH', path, { ...opts, body }),
  del: (path, opts) => request('DELETE', path, opts),
  raw: (path, opts) => request('GET', path, { ...opts, raw: true }),
  baseUrl: BASE_URL,
};
