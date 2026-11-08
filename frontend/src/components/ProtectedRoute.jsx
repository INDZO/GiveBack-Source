import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { dashboardPath } from '../lib/routes';

/* Guards app routes. Unauthenticated users go to /login (preserving intent);
   authenticated users with the wrong role are bounced to their own dashboard. */
export default function ProtectedRoute({ role, children }) {
  const { isAuthenticated, role: currentRole } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  }
  if (role && currentRole !== role) {
    return <Navigate to={dashboardPath(currentRole)} replace />;
  }
  return children;
}
