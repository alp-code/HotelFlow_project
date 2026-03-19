import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

interface Props {
  children: React.ReactNode;
  allowedRoles?: Array<'Staff' | 'Guest' | 'Housekeeping'>;
}

export default function ProtectedRoute({ children, allowedRoles }: Props) {
  const { isAuthenticated, role, loading } = useAuth();

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-hotel-cream">
        <div className="flex flex-col items-center gap-3">
          <div className="w-8 h-8 border-2 border-hotel-gold border-t-transparent rounded-full animate-spin" />
          <p className="text-hotel-muted text-sm font-body">Loading…</p>
        </div>
      </div>
    );
  }

  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (allowedRoles && role && !allowedRoles.includes(role)) {
    // Redirect to the correct dashboard
    if (role === 'Staff') return <Navigate to="/staff" replace />;
    if (role === 'Guest') return <Navigate to="/guest" replace />;
    if (role === 'Housekeeping') return <Navigate to="/housekeeping" replace />;
  }

  return <>{children}</>;
}
