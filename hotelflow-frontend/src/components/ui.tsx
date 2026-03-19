import { X } from 'lucide-react';

// ─── Status Badges ────────────────────────────────────────────────────────────
const roomStatusColors: Record<string, string> = {
  Available: 'bg-emerald-100 text-emerald-800',
  Occupied: 'bg-blue-100 text-blue-800',
  NeedsCleaning: 'bg-amber-100 text-amber-800',
  OutOfService: 'bg-red-100 text-red-700',
};

const reservationStatusColors: Record<string, string> = {
  Confirmed: 'bg-sky-100 text-sky-800',
  CheckedIn: 'bg-emerald-100 text-emerald-800',
  CheckedOut: 'bg-gray-100 text-gray-600',
  Cancelled: 'bg-red-100 text-red-700',
  NoShow: 'bg-orange-100 text-orange-700',
};

const taskStatusColors: Record<string, string> = {
  Pending: 'bg-amber-100 text-amber-800',
  InProgress: 'bg-blue-100 text-blue-800',
  Completed: 'bg-emerald-100 text-emerald-800',
  Cancelled: 'bg-gray-100 text-gray-600',
  Failed: 'bg-red-100 text-red-700',
};

const taskTypeColors: Record<string, string> = {
  Cleaning: 'bg-cyan-100 text-cyan-800',
  Maintenance: 'bg-orange-100 text-orange-800',
  Inspection: 'bg-violet-100 text-violet-800',
  Restocking: 'bg-teal-100 text-teal-800',
  Setup: 'bg-pink-100 text-pink-800',
};

export function RoomStatusBadge({ status }: { status: string }) {
  return (
    <span className={`badge ${roomStatusColors[status] ?? 'bg-gray-100 text-gray-600'}`}>
      {status === 'NeedsCleaning' ? 'Needs Cleaning' : status}
    </span>
  );
}

export function ReservationStatusBadge({ status }: { status: string }) {
  return (
    <span className={`badge ${reservationStatusColors[status] ?? 'bg-gray-100 text-gray-600'}`}>
      {status}
    </span>
  );
}

export function TaskStatusBadge({ status }: { status: string }) {
  return (
    <span className={`badge ${taskStatusColors[status] ?? 'bg-gray-100 text-gray-600'}`}>
      {status}
    </span>
  );
}

export function TaskTypeBadge({ type }: { type: string }) {
  return (
    <span className={`badge ${taskTypeColors[type] ?? 'bg-gray-100 text-gray-600'}`}>{type}</span>
  );
}

// ─── Spinner ─────────────────────────────────────────────────────────────────
export function Spinner({ size = 'md' }: { size?: 'sm' | 'md' | 'lg' }) {
  const s = size === 'sm' ? 'w-4 h-4' : size === 'lg' ? 'w-10 h-10' : 'w-6 h-6';
  return (
    <div
      className={`${s} border-2 border-hotel-gold border-t-transparent rounded-full animate-spin`}
    />
  );
}

export function PageSpinner() {
  return (
    <div className="flex items-center justify-center h-64">
      <Spinner size="lg" />
    </div>
  );
}

// ─── Stat Card ────────────────────────────────────────────────────────────────
interface StatCardProps {
  label: string;
  value: string | number;
  icon: React.ReactNode;
  accent?: string;
  sub?: string;
}
export function StatCard({ label, value, icon, accent = 'bg-hotel-gold/10', sub }: StatCardProps) {
  return (
    <div className="card flex items-start gap-4">
      <div className={`${accent} p-3 rounded-xl`}>{icon}</div>
      <div>
        <p className="text-hotel-muted text-xs font-body font-medium uppercase tracking-wide">{label}</p>
        <p className="text-hotel-navy text-2xl font-display font-semibold mt-0.5">{value}</p>
        {sub && <p className="text-hotel-muted text-xs font-body mt-0.5">{sub}</p>}
      </div>
    </div>
  );
}

// ─── Modal ────────────────────────────────────────────────────────────────────
interface ModalProps {
  title: string;
  open: boolean;
  onClose: () => void;
  children: React.ReactNode;
  width?: string;
}
export function Modal({ title, open, onClose, children, width = 'max-w-lg' }: ModalProps) {
  if (!open) return null;
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-black/40 backdrop-blur-sm" onClick={onClose} />
      <div className={`relative bg-white rounded-2xl shadow-2xl w-full ${width} max-h-[90vh] overflow-y-auto`}>
        <div className="flex items-center justify-between px-6 py-4 border-b border-hotel-border">
          <h2 className="font-display text-hotel-navy text-lg">{title}</h2>
          <button onClick={onClose} className="p-1.5 rounded-lg hover:bg-hotel-cream text-hotel-muted hover:text-hotel-navy transition-colors">
            <X size={18} />
          </button>
        </div>
        <div className="p-6">{children}</div>
      </div>
    </div>
  );
}

// ─── Page Header ─────────────────────────────────────────────────────────────
export function PageHeader({ title, subtitle, action }: { title: string; subtitle?: string; action?: React.ReactNode }) {
  return (
    <div className="flex items-start justify-between mb-7">
      <div>
        <h1 className="font-display text-hotel-navy text-2xl font-semibold">{title}</h1>
        {subtitle && <p className="text-hotel-muted font-body text-sm mt-1">{subtitle}</p>}
      </div>
      {action && <div>{action}</div>}
    </div>
  );
}

// ─── Empty State ─────────────────────────────────────────────────────────────
export function EmptyState({ message, icon }: { message: string; icon?: React.ReactNode }) {
  return (
    <div className="flex flex-col items-center justify-center py-16 text-center">
      {icon && <div className="text-hotel-muted mb-3 opacity-40">{icon}</div>}
      <p className="text-hotel-muted font-body text-sm">{message}</p>
    </div>
  );
}

// ─── Error Alert ─────────────────────────────────────────────────────────────
export function ErrorAlert({ message }: { message: string }) {
  return (
    <div className="bg-red-50 border border-red-200 text-red-700 rounded-xl px-4 py-3 text-sm font-body mb-4">
      {message}
    </div>
  );
}

// ─── Form Field ──────────────────────────────────────────────────────────────
export function FormField({
  label,
  children,
  error,
}: {
  label: string;
  children: React.ReactNode;
  error?: string;
}) {
  return (
    <div className="space-y-1.5">
      <label className="block text-xs font-semibold font-body text-hotel-slate uppercase tracking-wide">
        {label}
      </label>
      {children}
      {error && <p className="text-red-600 text-xs font-body">{error}</p>}
    </div>
  );
}
