import { useEffect } from 'react';
import { Routes, Route, Navigate, useNavigate } from 'react-router-dom';

import { useToast } from './components/Toast';
import MarketingLayout from './components/marketing/MarketingLayout';
import AppShell from './components/app/AppShell';
import ProtectedRoute from './components/ProtectedRoute';
import { UserRole } from './lib/enums';

import Landing from './pages/Landing';
import About from './pages/About';
import Login from './pages/Login';
import RegisterVolunteer from './pages/RegisterVolunteer';
import JoinOrganization from './pages/JoinOrganization';
import Opportunities from './pages/Opportunities';
import OpportunityDetail from './pages/OpportunityDetail';
import Profile from './pages/Profile';
import Notifications from './pages/Notifications';

import VolunteerDashboard from './pages/volunteer/Dashboard';
import MyApplications from './pages/volunteer/MyApplications';
import MyHours from './pages/volunteer/MyHours';
import Leaderboard from './pages/volunteer/Leaderboard';
import Teams from './pages/volunteer/Teams';
import Invitations from './pages/volunteer/Invitations';
import VolunteerReport from './pages/volunteer/Report';

import OrgDashboard from './pages/org/Dashboard';
import OrgOpportunities from './pages/org/Opportunities';
import OrgApplicants from './pages/org/Applicants';
import OrgReports from './pages/org/Reports';

import AdminDashboard from './pages/admin/Dashboard';
import AdminVerify from './pages/admin/Verify';

/* Surfaces a session-expired toast and bounces to /login when the API client
   reports that a token refresh failed. Lives inside ToastProvider + Router. */
function SessionExpiryWatcher() {
  const toast = useToast();
  const navigate = useNavigate();
  useEffect(() => {
    const onExpired = () => {
      toast.info('Session expired', 'Please log in again to continue.');
      navigate('/login', { replace: true, state: { expired: true } });
    };
    window.addEventListener('gb:session-expired', onExpired);
    return () => window.removeEventListener('gb:session-expired', onExpired);
  }, [toast, navigate]);
  return null;
}

export default function App() {
  return (
    <>
    <SessionExpiryWatcher />
    <Routes>
      {/* ---- Public / marketing ---- */}
      <Route element={<MarketingLayout />}>
        <Route index element={<Landing />} />
        <Route path="/about" element={<About />} />
        <Route path="/opportunities" element={<Opportunities />} />
        <Route path="/opportunities/:id" element={<OpportunityDetail />} />
      </Route>

      {/* ---- Auth (self-contained layouts) ---- */}
      <Route path="/login" element={<Login />} />
      <Route path="/register-volunteer" element={<RegisterVolunteer />} />
      <Route path="/join-organization" element={<JoinOrganization />} />

      {/* ---- Volunteer app ---- */}
      <Route
        path="/app"
        element={<ProtectedRoute role={UserRole.Volunteer}><AppShell variant="volunteer" /></ProtectedRoute>}
      >
        <Route index element={<VolunteerDashboard />} />
        <Route path="opportunities" element={<Opportunities shell />} />
        <Route path="opportunities/:id" element={<OpportunityDetail shell />} />
        <Route path="applications" element={<MyApplications />} />
        <Route path="hours" element={<MyHours />} />
        <Route path="teams" element={<Teams />} />
        <Route path="invitations" element={<Invitations />} />
        <Route path="report" element={<VolunteerReport />} />
        <Route path="leaderboard" element={<Leaderboard />} />
        <Route path="notifications" element={<Notifications />} />
        <Route path="profile" element={<Profile />} />
      </Route>

      {/* ---- Organization app ---- */}
      <Route
        path="/org"
        element={<ProtectedRoute role={UserRole.Organization}><AppShell variant="organization" /></ProtectedRoute>}
      >
        <Route index element={<OrgDashboard />} />
        <Route path="opportunities" element={<OrgOpportunities />} />
        <Route path="applicants" element={<OrgApplicants />} />
        <Route path="reports" element={<OrgReports />} />
        <Route path="notifications" element={<Notifications />} />
        <Route path="profile" element={<Profile />} />
      </Route>

      {/* ---- Admin app ---- */}
      <Route
        path="/admin"
        element={<ProtectedRoute role={UserRole.Admin}><AppShell variant="admin" /></ProtectedRoute>}
      >
        <Route index element={<AdminDashboard />} />
        <Route path="verify" element={<AdminVerify />} />
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
    </>
  );
}
