import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import Layout from './components/Layout';

// Auth
import LoginPage from './pages/auth/LoginPage';
import RegisterPage from './pages/auth/RegisterPage';

// Guest
import GuestDashboard from './pages/guest/GuestDashboard';
import GuestRooms from './pages/guest/GuestRooms';
import GuestReservations from './pages/guest/GuestReservations';

// Staff
import StaffDashboard from './pages/staff/StaffDashboard';
import StaffReservations from './pages/staff/StaffReservations';
import StaffCheckouts from './pages/staff/StaffCheckouts';
import StaffRooms from './pages/staff/StaffRooms';
import StaffUsers from './pages/staff/StaffUsers';

// Housekeeping
import HousekeepingDashboard from './pages/housekeeping/HousekeepingDashboard';
import HousekeepingMyTasks from './pages/housekeeping/HousekeepingMyTasks';
import HousekeepingAvailable from './pages/housekeeping/HousekeepingAvailable';
import HousekeepingAllTasks from './pages/housekeeping/HousekeepingAllTasks';

// Profile Page
import ProfilePage from './pages/profile/ProfilePage';


function RootRedirect() {
  const { isAuthenticated, role, loading } = useAuth();
  if (loading) return null;
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (role === 'Staff') return <Navigate to="/staff" replace />;
  if (role === 'Housekeeping') return <Navigate to="/housekeeping" replace />;
  return <Navigate to="/guest" replace />;
}

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          {/* Public */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/" element={<RootRedirect />} />

          {/* Guest */}
          <Route path="/guest" element={
            <ProtectedRoute allowedRoles={['Guest']}>
              <Layout><GuestDashboard /></Layout>
            </ProtectedRoute>
          } />
          <Route path="/guest/rooms" element={
            <ProtectedRoute allowedRoles={['Guest']}>
              <Layout><GuestRooms /></Layout>
            </ProtectedRoute>
          } />
          <Route path="/guest/reservations" element={
            <ProtectedRoute allowedRoles={['Guest']}>
              <Layout><GuestReservations /></Layout>
            </ProtectedRoute>
          } />
          <Route path="/guest/profile" element={
            <ProtectedRoute allowedRoles={['Guest']}>
              <Layout><ProfilePage /></Layout>
            </ProtectedRoute>
          } />


          {/* Staff */}
          <Route path="/staff" element={
            <ProtectedRoute allowedRoles={['Staff']}>
              <Layout><StaffDashboard /></Layout>
            </ProtectedRoute>
          } />
          <Route path="/staff/reservations" element={
            <ProtectedRoute allowedRoles={['Staff']}>
              <Layout><StaffReservations /></Layout>
            </ProtectedRoute>
          } />
          <Route path="/staff/checkouts" element={
            <ProtectedRoute allowedRoles={['Staff']}>
              <Layout><StaffCheckouts /></Layout>
            </ProtectedRoute>
          } />
          <Route path="/staff/rooms" element={
            <ProtectedRoute allowedRoles={['Staff']}>
              <Layout><StaffRooms /></Layout>
            </ProtectedRoute>
          } />
          <Route path="/staff/users" element={
            <ProtectedRoute allowedRoles={['Staff']}>
              <Layout><StaffUsers /></Layout>
            </ProtectedRoute>
          } />
          <Route path="/staff/profile" element={
            <ProtectedRoute allowedRoles={['Staff']}>
              <Layout><ProfilePage /></Layout>
            </ProtectedRoute>
          } />

          {/* Housekeeping */}
          <Route path="/housekeeping" element={
            <ProtectedRoute allowedRoles={['Housekeeping', 'Staff']}>
              <Layout><HousekeepingDashboard /></Layout>
            </ProtectedRoute>
          } />
          <Route path="/housekeeping/my-tasks" element={
            <ProtectedRoute allowedRoles={['Housekeeping']}>
              <Layout><HousekeepingMyTasks /></Layout>
            </ProtectedRoute>
          } />
          <Route path="/housekeeping/available" element={
            <ProtectedRoute allowedRoles={['Housekeeping', 'Staff']}>
              <Layout><HousekeepingAvailable /></Layout>
            </ProtectedRoute>
          } />
          <Route path="/housekeeping/all" element={
            <ProtectedRoute allowedRoles={['Housekeeping', 'Staff']}>
              <Layout><HousekeepingAllTasks /></Layout>
            </ProtectedRoute>
          } />
          <Route path="/housekeeping/profile" element={
            <ProtectedRoute allowedRoles={['Housekeeping']}>
              <Layout><ProfilePage /></Layout>
            </ProtectedRoute>
          } />

          {/* Fallback */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
