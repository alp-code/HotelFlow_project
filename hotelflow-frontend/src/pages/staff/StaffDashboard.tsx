import { useEffect, useState } from 'react';
import { reservationsApi } from '../../api/reservations';
import { Reservation } from '../../types';
import { CalendarDays, Users, LogIn, LogOut, Clock, AlertCircle } from 'lucide-react';
import { StatCard, PageSpinner, ReservationStatusBadge, PageHeader, ErrorAlert } from '../../components/ui';
import { format } from 'date-fns';
import { Link } from 'react-router-dom';


export default function StaffDashboard() {
  const [all, setAll] = useState<Reservation[]>([]);
  const [checkouts, setCheckouts] = useState<Reservation[]>([]);
  const [loading, setLoading] = useState(true);
  const [actionError, setActionError] = useState('');
  const [actionLoading, setActionLoading] = useState<string | null>(null);

  const today = format(new Date(), 'yyyy-MM-dd');

  const load = async () => {
    setLoading(true);
    const [a, c, allRes] = await Promise.all([
      reservationsApi.getAll(today).catch(() => [] as Reservation[]),
      reservationsApi.getTodayCheckouts().catch(() => [] as Reservation[]),
      reservationsApi.getAll().catch(() => [] as Reservation[]),
    ]);
    setAll(a);
    const checkedOutToday = allRes.filter(
      (r) => r.status === 'CheckedOut' &&
      r.checkedOutAt &&
      new Date(r.checkedOutAt).toDateString() === new Date().toDateString()
    );
    const combined = [...c, ...checkedOutToday].filter(
      (r, i, self) => self.findIndex(x => x.id === r.id) === i
    );
    setCheckouts(combined);
    setLoading(false);
  };

  useEffect(() => { load(); }, []);

  const doCheckIn = async (id: string) => {
    setActionLoading(id + '_in');
    setActionError('');
    try {
      await reservationsApi.checkIn(id);
      await load();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } };
      setActionError(e.response?.data?.message ?? 'Check-in failed.');
    } finally {
      setActionLoading(null);
    }
  };

  const doCheckOut = async (id: string) => {
    setActionLoading(id + '_out');
    setActionError('');
    try {
      await reservationsApi.checkOut(id);
      await load();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } };
      setActionError(e.response?.data?.message ?? 'Check-out failed.');
    } finally {
      setActionLoading(null);
    }
  };

  if (loading) return <PageSpinner />;

  const confirmed = all.filter((r) => r.status === 'Confirmed' && new Date(r.checkInDate).toDateString() === new Date().toDateString());
  const checkedIn = all.filter((r) => r.status === 'CheckedIn');

  return (
    <div className="p-8">
      <PageHeader
        title="Staff Dashboard"
        subtitle={`Today — ${format(new Date(), 'EEEE, MMMM d, yyyy')}`}
      />

      {actionError && <ErrorAlert message={actionError} />}

      {/* Stats */}
      <div className="grid grid-cols-2 xl:grid-cols-4 gap-4 mb-8">
        <StatCard label="Arrivals Today" value={confirmed.length}
          icon={<LogIn size={20} className="text-sky-600" />} accent="bg-sky-50" />
        <StatCard label="Currently Checked In" value={checkedIn.length}
          icon={<Users size={20} className="text-emerald-600" />} accent="bg-emerald-50" />
        <StatCard label="Departures Today" value={checkouts.length}
          icon={<LogOut size={20} className="text-amber-600" />} accent="bg-amber-50" />
        <StatCard label="No Shows" value={all.filter((r) => r.status === 'NoShow').length}
          icon={<AlertCircle size={20} className="text-red-500" />} accent="bg-red-50" />
      </div>

      {/* Today's arrivals */}
      <div className="card mb-6">
        <div className="flex items-center justify-between mb-4">
          <h2 className="font-display text-hotel-navy text-lg">Today's Arrivals</h2>
          <Link to="/staff/reservations" className="text-hotel-gold text-sm font-body font-medium hover:text-hotel-gold-light">
            Manage all →
          </Link>
        </div>
        {confirmed.length === 0 ? (
          <p className="text-hotel-muted text-sm font-body text-center py-6">No arrivals scheduled for today.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr>
                  {['Guest', 'Room', 'Check-out', 'Guests', 'Status', 'Actions'].map((h) => (
                    <th key={h} className="table-header text-left">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {confirmed.map((r) => (
                  <tr key={r.id} className="hover:bg-hotel-cream/40 transition-colors">
                    <td className="table-cell">
                      <p className="font-medium text-hotel-navy">{r.guestName}</p>
                      <p className="text-hotel-muted text-xs">{r.guestEmail}</p>
                    </td>
                    <td className="table-cell font-medium text-hotel-navy">{r.roomNumber}</td>
                    <td className="table-cell">{format(new Date(r.checkOutDate), 'MMM d')}</td>
                    <td className="table-cell">{r.numberOfGuests}</td>
                    <td className="table-cell"><ReservationStatusBadge status={r.status} /></td>
                    <td className="table-cell">
                      <button
                        className="btn-primary py-1.5 px-3 text-xs flex items-center gap-1"
                        onClick={() => doCheckIn(r.id)}
                        disabled={actionLoading === r.id + '_in'}
                      >
                        <LogIn size={12} />
                        {actionLoading === r.id + '_in' ? '…' : 'Check In'}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Today's departures */}
      <div className="card">
        <div className="flex items-center justify-between mb-4">
          <h2 className="font-display text-hotel-navy text-lg">Today's Departures</h2>
          <Link to="/staff/checkouts" className="text-hotel-gold text-sm font-body font-medium hover:text-hotel-gold-light">
            View all →
          </Link>
        </div>
        {checkouts.length === 0 ? (
          <p className="text-hotel-muted text-sm font-body text-center py-6">No departures scheduled for today.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr>
                  {['Guest', 'Room', 'Nights', 'Total', 'Paid', 'Actions'].map((h) => (
                    <th key={h} className="table-header text-left">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {checkouts.map((r) => (
                  <tr key={r.id} className="hover:bg-hotel-cream/40 transition-colors">
                    <td className="table-cell">
                      <p className="font-medium text-hotel-navy">{r.guestName}</p>
                      <p className="text-hotel-muted text-xs">{r.guestEmail}</p>
                    </td>
                    <td className="table-cell font-medium">{r.roomNumber}</td>
                    <td className="table-cell">{r.nights}</td>
                    <td className="table-cell font-semibold">${r.totalPrice.toFixed(2)}</td>
                    <td className="table-cell">
                      {r.isPaid
                        ? <span className="badge bg-emerald-100 text-emerald-700">Paid</span>
                        : <span className="badge bg-red-100 text-red-700">Unpaid</span>}
                    </td>
                    <td className="table-cell">
                      {r.status === 'CheckedIn' && (
                        <button
                          className="btn-secondary py-1.5 px-3 text-xs flex items-center gap-1"
                          onClick={() => doCheckOut(r.id)}
                          disabled={actionLoading === r.id + '_out'}
                        >
                          <LogOut size={12} />
                          {actionLoading === r.id + '_out' ? '…' : 'Check Out'}
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

            {/* Currently Checked In */}
      <div className="card mt-6">
        <div className="flex items-center justify-between mb-4">
          <h2 className="font-display text-hotel-navy text-lg">Currently in the Hotel</h2>
          <span className="badge bg-emerald-100 text-emerald-700">{checkedIn.length} guest{checkedIn.length !== 1 ? 's' : ''}</span>
        </div>
        {checkedIn.length === 0 ? (
          <p className="text-hotel-muted text-sm font-body text-center py-6">No guests currently checked in.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr>
                  {['Guest', 'Room', 'Check-in', 'Check-out', 'Nights Left', 'Actions'].map((h) => (
                    <th key={h} className="table-header text-left">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {checkedIn.map((r) => {
                  const nightsLeft = Math.ceil((new Date(r.checkOutDate).getTime() - new Date().getTime()) / 86400000);
                  return (
                    <tr key={r.id} className="hover:bg-hotel-cream/40 transition-colors">
                      <td className="table-cell">
                        <p className="font-medium text-hotel-navy">{r.guestName}</p>
                        <p className="text-hotel-muted text-xs">{r.guestEmail}</p>
                      </td>
                      <td className="table-cell font-medium text-hotel-navy">{r.roomNumber}</td>
                      <td className="table-cell">{format(new Date(r.checkInDate), 'MMM d')}</td>
                      <td className="table-cell">{format(new Date(r.checkOutDate), 'MMM d')}</td>
                      <td className="table-cell">
                        <span className={`badge ${nightsLeft <= 1 ? 'bg-amber-100 text-amber-700' : 'bg-emerald-100 text-emerald-700'}`}>
                          {nightsLeft <= 0 ? 'Due today' : `${nightsLeft} night${nightsLeft !== 1 ? 's' : ''}`}
                        </span>
                      </td>
                      <td className="table-cell">
                        <button
                          className="btn-secondary py-1.5 px-3 text-xs flex items-center gap-1"
                          onClick={() => doCheckOut(r.id)}
                          disabled={actionLoading === r.id + '_out'}
                        >
                          <LogOut size={12} />
                          {actionLoading === r.id + '_out' ? '…' : 'Check Out'}
                        </button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
