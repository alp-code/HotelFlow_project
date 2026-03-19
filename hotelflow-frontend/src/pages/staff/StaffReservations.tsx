import { useEffect, useState } from 'react';
import { reservationsApi } from '../../api/reservations';
import { Reservation } from '../../types';
import { Search, LogIn, LogOut, DollarSign, XCircle } from 'lucide-react';
import {
  PageHeader, PageSpinner, ReservationStatusBadge, EmptyState, ErrorAlert,
} from '../../components/ui';
import { format } from 'date-fns';

export default function StaffReservations() {
  const [reservations, setReservations] = useState<Reservation[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [actionError, setActionError] = useState('');
  const [actionId, setActionId] = useState<string | null>(null);

  const load = () => {
    setLoading(true);
    reservationsApi.getAll().then(setReservations).finally(() => setLoading(false));
  };

  useEffect(load, []);

  const act = async (fn: () => Promise<unknown>, id: string) => {
    setActionId(id);
    setActionError('');
    try { await fn(); load(); }
    catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } };
      setActionError(e.response?.data?.message ?? 'Action failed.');
    } finally { setActionId(null); }
  };

  const filtered = reservations.filter((r) => {
    const q = search.toLowerCase();
    return (
      r.guestName.toLowerCase().includes(q) ||
      r.guestEmail.toLowerCase().includes(q) ||
      r.roomNumber.toLowerCase().includes(q)
    );
  });

  if (loading) return <PageSpinner />;

  return (
    <div className="p-8">
      <PageHeader title="All Reservations" subtitle={`${reservations.length} total`} />

      {actionError && <ErrorAlert message={actionError} />}

      {/* Search */}
      <div className="relative mb-5 max-w-sm">
        <Search size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-hotel-muted" />
        <input
          className="input-field pl-9"
          placeholder="Search by guest, email, or room…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>

      <div className="card p-0 overflow-hidden">
        {filtered.length === 0 ? (
          <EmptyState message="No reservations found." icon={<Search size={40} />} />
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-hotel-border">
                  {['Guest', 'Room', 'Check-in', 'Check-out', 'Guests', 'Total', 'Paid', 'Status', 'Actions'].map((h) => (
                    <th key={h} className="table-header text-left">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {filtered.map((r) => (
                  <tr key={r.id} className="hover:bg-hotel-cream/40 transition-colors">
                    <td className="table-cell">
                      <p className="font-medium text-hotel-navy text-sm">{r.guestName}</p>
                      <p className="text-hotel-muted text-xs">{r.guestEmail}</p>
                    </td>
                    <td className="table-cell font-medium text-hotel-navy">{r.roomNumber}</td>
                    <td className="table-cell text-sm">{format(new Date(r.checkInDate), 'MMM d, yy')}</td>
                    <td className="table-cell text-sm">{format(new Date(r.checkOutDate), 'MMM d, yy')}</td>
                    <td className="table-cell">{r.numberOfGuests}</td>
                    <td className="table-cell font-semibold">${r.totalPrice.toFixed(2)}</td>
                    <td className="table-cell">
                      {r.isPaid
                        ? <span className="badge bg-emerald-100 text-emerald-700">Yes</span>
                        : <span className="badge bg-red-100 text-red-700">No</span>}
                    </td>
                    <td className="table-cell"><ReservationStatusBadge status={r.status} /></td>
                    <td className="table-cell">
                      <div className="flex items-center gap-1.5">
                        {r.status === 'Confirmed' && (
                          <button title="Check In"
                            className="p-1.5 rounded-lg bg-sky-50 text-sky-600 hover:bg-sky-100 transition-colors"
                            onClick={() => act(() => reservationsApi.checkIn(r.id), r.id)}
                            disabled={actionId === r.id}>
                            <LogIn size={14} />
                          </button>
                        )}
                        {r.status === 'CheckedIn' && (
                          <button title="Check Out"
                            className="p-1.5 rounded-lg bg-amber-50 text-amber-600 hover:bg-amber-100 transition-colors"
                            onClick={() => act(() => reservationsApi.checkOut(r.id), r.id)}
                            disabled={actionId === r.id}>
                            <LogOut size={14} />
                          </button>
                        )}
                        {!r.isPaid && (r.status === 'CheckedIn' || r.status === 'CheckedOut') && (
                          <button title="Mark Paid"
                            className="p-1.5 rounded-lg bg-emerald-50 text-emerald-600 hover:bg-emerald-100 transition-colors"
                            onClick={() => act(() => reservationsApi.markPaid(r.id), r.id)}
                            disabled={actionId === r.id}>
                            <DollarSign size={14} />
                          </button>
                        )}
                        {r.status === 'Confirmed' && (
                          <button title="No Show"
                            className="p-1.5 rounded-lg bg-red-50 text-red-500 hover:bg-red-100 transition-colors"
                            onClick={() => act(() => reservationsApi.markNoShow(r.id), r.id)}
                            disabled={actionId === r.id}>
                            <XCircle size={14} />
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
