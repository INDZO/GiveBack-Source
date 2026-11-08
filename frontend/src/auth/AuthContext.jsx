import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { tokenStore } from '../api/client';
import { authApi, volunteersApi, organizationsApi } from '../api/services';
import { UserRole } from '../lib/enums';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [session, setSession] = useState(() => tokenStore.session);
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(false);

  const role = session?.role ?? null;

  /* Loads the role-specific profile (volunteer/organization) for nav display. */
  const loadProfile = useCallback(async (activeRole) => {
    try {
      if (activeRole === UserRole.Volunteer) setProfile(await volunteersApi.me());
      else if (activeRole === UserRole.Organization) setProfile(await organizationsApi.me());
      else setProfile(null);
    } catch {
      setProfile(null);
    }
  }, []);

  useEffect(() => {
    if (session?.role) loadProfile(session.role);
  }, [session, loadProfile]);

  /* Forced sign-out triggered by the API client when a token refresh fails.
     Clearing the session here makes ProtectedRoute redirect to /login and
     prevents protected pages from rendering stale profile data. */
  useEffect(() => {
    const onExpired = () => {
      tokenStore.clear();
      setSession(null);
      setProfile(null);
    };
    window.addEventListener('gb:session-expired', onExpired);
    return () => window.removeEventListener('gb:session-expired', onExpired);
  }, []);

  const applyAuth = useCallback(async (auth) => {
    tokenStore.set(auth);
    const next = { userId: auth.userId, role: auth.role };
    setSession(next);
    await loadProfile(auth.role);
    return next;
  }, [loadProfile]);

  const login = useCallback(async (email, password) => {
    setLoading(true);
    try {
      const auth = await authApi.login(email, password);
      return await applyAuth(auth);
    } finally {
      setLoading(false);
    }
  }, [applyAuth]);

  const registerVolunteer = useCallback(async (payload) => {
    const auth = await authApi.registerVolunteer(payload);
    return applyAuth(auth);
  }, [applyAuth]);

  const registerOrganization = useCallback(async (payload) => {
    const auth = await authApi.registerOrganization(payload);
    return applyAuth(auth);
  }, [applyAuth]);

  const logout = useCallback(async () => {
    try { if (tokenStore.refresh) await authApi.logout(tokenStore.refresh); } catch { /* ignore */ }
    tokenStore.clear();
    setSession(null);
    setProfile(null);
  }, []);

  const refreshProfile = useCallback(() => {
    if (session?.role) return loadProfile(session.role);
    return Promise.resolve();
  }, [session, loadProfile]);

  const value = useMemo(() => ({
    session, role, profile, loading,
    isAuthenticated: !!session,
    login, logout, registerVolunteer, registerOrganization, refreshProfile, setProfile,
  }), [session, role, profile, loading, login, logout, registerVolunteer, registerOrganization, refreshProfile]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
