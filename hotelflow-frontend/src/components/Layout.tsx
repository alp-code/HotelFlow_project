import { NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import {
  LayoutDashboard, CalendarDays, BedDouble, ClipboardList,
  Users, LogOut, Hotel, Wrench, CheckSquare, ListTodo,
} from 'lucide-react';

const guestLinks = [
  { to: '/guest', label: 'Dashboard', icon: LayoutDashboard, end: true },
  { to: '/guest/rooms', label: 'Browse Rooms', icon: BedDouble },
  { to: '/guest/reservations', label: 'My Reservations', icon: CalendarDays },
];

const staffLinks = [
  { to: '/staff', label: 'Dashboard', icon: LayoutDashboard, end: true },
  { to: '/staff/reservations', label: 'All Reservations', icon: CalendarDays },
  { to: '/staff/checkouts', label: "Today's Checkouts", icon: CheckSquare },
  { to: '/staff/rooms', label: 'Rooms & Types', icon: BedDouble },
  { to: '/staff/users', label: 'Users', icon: Users },
];

const housekeepingLinks = [
  { to: '/housekeeping', label: 'Dashboard', icon: LayoutDashboard, end: true },
  { to: '/housekeeping/my-tasks', label: 'My Tasks', icon: ListTodo },
  { to: '/housekeeping/available', label: 'Available Tasks', icon: ClipboardList },
  { to: '/housekeeping/all', label: 'All Tasks', icon: Wrench },
];

export default function Layout({ children }: { children: React.ReactNode }) {
  const { role, email, logout } = useAuth();
  const navigate = useNavigate();

  const links =
    role === 'Staff' ? staffLinks : role === 'Housekeeping' ? housekeepingLinks : guestLinks;

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const roleLabel = role === 'Staff' ? 'Staff' : role === 'Housekeeping' ? 'Housekeeping' : 'Guest';
  const roleBadgeColor =
    role === 'Staff'
      ? 'bg-amber-100 text-amber-800'
      : role === 'Housekeeping'
      ? 'bg-blue-100 text-blue-800'
      : 'bg-green-100 text-green-800';

  return (
    <div className="flex h-screen overflow-hidden bg-hotel-cream">
      {/* Sidebar */}
      <aside className="w-64 bg-hotel-navy flex flex-col flex-shrink-0">
        {/* Logo */}
        <div className="px-6 py-6 border-b border-white/10">
          <div className="flex items-center gap-3">
            <div className="w-9 h-9 bg-hotel-gold rounded-lg flex items-center justify-center">
              <Hotel size={18} className="text-white" />
            </div>
            <div>
              <h1 className="font-display text-white text-lg leading-tight">HotelFlow</h1>
              <p className="text-white/40 text-xs font-body">Management System</p>
            </div>
          </div>
        </div>

        {/* Navigation */}
        <nav className="flex-1 px-3 py-5 space-y-1 overflow-y-auto">
          {links.map(({ to, label, icon: Icon, end }) => (
            <NavLink
              key={to}
              to={to}
              end={end}
              className={({ isActive }) =>
                `flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-body font-medium transition-all duration-150 ${
                  isActive
                    ? 'sidebar-active text-hotel-gold'
                    : 'text-white/60 hover:text-white hover:bg-white/5'
                }`
              }
            >
              <Icon size={16} />
              {label}
            </NavLink>
          ))}
        </nav>

        {/* User info */}
        <div className="px-4 py-4 border-t border-white/10">
          <div className="flex items-center gap-3 mb-3">
            <div className="w-8 h-8 rounded-full bg-hotel-gold/20 flex items-center justify-center">
              <span className="text-hotel-gold text-xs font-semibold font-body">
                {email?.[0]?.toUpperCase() ?? '?'}
              </span>
            </div>
            <div className="min-w-0">
              <p className="text-white text-xs font-body font-medium truncate">{email}</p>
              <span className={`text-xs px-1.5 py-0.5 rounded font-semibold ${roleBadgeColor}`}>
                {roleLabel}
              </span>
            </div>
          </div>
          <button
            onClick={handleLogout}
            className="w-full flex items-center gap-2 px-3 py-2 text-white/50 hover:text-red-400 hover:bg-red-400/10 rounded-lg transition-all text-sm font-body"
          >
            <LogOut size={15} />
            Sign out
          </button>
        </div>
      </aside>

      {/* Main content */}
      <main className="flex-1 overflow-y-auto">
        <div className="page-enter">{children}</div>
      </main>
    </div>
  );
}
