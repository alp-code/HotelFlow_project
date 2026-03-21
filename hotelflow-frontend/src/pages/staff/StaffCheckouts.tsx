import { useEffect, useState } from 'react';
import { reservationsApi } from '../../api/reservations';
import { Reservation } from '../../types';
import { LogOut, DollarSign, Search } from 'lucide-react';
import { PageHeader, PageSpinner, ReservationStatusBadge, EmptyState, ErrorAlert } from '../../components/ui';
import { format } from 'date-fns';

export default function StaffCheckouts() {
  const [checkouts, setCheckouts] = useState<Reservation[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [actionError, setActionError] = useState('');
  const [actionId, setActionId] = useState<string | null>(null);

  const load = () => {
  setLoading(true);
  Promise.all([
    reservationsApi.getTodayCheckouts(),
    reservationsApi.getAll().catch(() => [] as Reservation[]),
  ]).then(([pending, all]) => {
    const todayCheckedOut = all.filter(
      (r) => r.status === 'CheckedOut' &&
      r.checkedOutAt &&
      new Date(r.checkedOutAt).toDateString() === new Date().toDateString()
    );
    const combined = [...pending, ...todayCheckedOut];
    const unique = combined.filter((r, i, self) => self.findIndex(x => x.id === r.id) === i);
    setCheckouts(unique);
    }).finally(() => setLoading(false));
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

  const filtered = checkouts.filter((r) => {
    const q = search.toLowerCase();
    return (r.guestName ?? '').toLowerCase().includes(q) || (r.guestEmail ?? '').toLowerCase().includes(q) || r.roomNumber.toLowerCase().includes(q);
  });

  if (loading) return <PageSpinner />;

  const total = checkouts.reduce((s, r) => s + r.totalPrice, 0);
  const unpaid = checkouts.filter((r) => !r.isPaid).length;

  return (
    <div className="p-8">
      <PageHeader
        title="Today's Checkouts"
        subtitle={`${checkouts.length} departure${checkouts.length !== 1 ? 's' : ''} — ${format(new Date(), 'EEEE, MMMM d')}`}
      />

      {actionError && <ErrorAlert message={actionError} />}

      {/* Summary strips */}
      <div className="flex gap-3 mb-6">
        <div className="bg-white border border-hotel-border rounded-xl px-4 py-3 flex items-center gap-3">
          <LogOut size={16} className="text-amber-500" />
          <div>
            <p className="text-xs text-hotel-muted font-body">Total departures</p>
            <p className="font-semibold text-hotel-navy font-body">{checkouts.length}</p>
          </div>
        </div>
        <div className="bg-white border border-hotel-border rounded-xl px-4 py-3 flex items-center gap-3">
          <DollarSign size={16} className="text-emerald-600" />
          <div>
            <p className="text-xs text-hotel-muted font-body">Revenue today</p>
            <p className="font-semibold text-hotel-navy font-body">${total.toFixed(2)}</p>
          </div>
        </div>
        {unpaid > 0 && (
          <div className="bg-red-50 border border-red-200 rounded-xl px-4 py-3 flex items-center gap-3">
            <DollarSign size={16} className="text-red-500" />
            <div>
              <p className="text-xs text-red-500 font-body">Unpaid</p>
              <p className="font-semibold text-red-700 font-body">{unpaid} reservation{unpaid !== 1 ? 's' : ''}</p>
            </div>
          </div>
        )}
      </div>

      {/* Search */}
      <div className="relative mb-5 max-w-sm">
        <Search size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-hotel-muted" />
        <input className="input-field pl-9" placeholder="Search guest or room…" value={search} onChange={(e) => setSearch(e.target.value)} />
      </div>

      <div className="card p-0 overflow-hidden">
        {filtered.length === 0 ? (
          <EmptyState message="No checkouts today." icon={<LogOut size={40} />} />
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-hotel-border">
                  {['Guest', 'Room', 'Check-in', 'Nights', 'Total', 'Paid', 'Status', 'Actions'].map((h) => (
                    <th key={h} className="table-header text-left">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {filtered.map((r) => (
                  <tr key={r.id} className="hover:bg-hotel-cream/40 transition-colors">
                    <td className="table-cell">
                      <p className="font-medium text-hotel-navy">{r.guestName}</p>
                      <p className="text-hotel-muted text-xs">{r.guestEmail}</p>
                    </td>
                    <td className="table-cell font-medium">{r.roomNumber}</td>
                    <td className="table-cell">{format(new Date(r.checkInDate), 'MMM d')}</td>
                    <td className="table-cell">{r.nights}</td>
                    <td className="table-cell font-semibold">${r.totalPrice.toFixed(2)}</td>
                    <td className="table-cell">
                      {r.isPaid
                        ? <span className="badge bg-emerald-100 text-emerald-700">Paid</span>
                        : <span className="badge bg-red-100 text-red-700">Unpaid</span>}
                    </td>
                    <td className="table-cell"><ReservationStatusBadge status={r.status} /></td>
                    <td className="table-cell">
                      <div className="flex items-center gap-1.5">
                        {r.status === 'CheckedIn' && (
                          <button
                            className="btn-secondary py-1.5 px-3 text-xs flex items-center gap-1"
                            onClick={() => act(() => reservationsApi.checkOut(r.id), r.id)}
                            disabled={actionId === r.id}
                          >
                            <LogOut size={12} />
                            {actionId === r.id ? '…' : 'Check Out'}
                          </button>
                        )}
                        {!r.isPaid && (
                          <button
                            className="p-1.5 rounded-lg bg-emerald-50 text-emerald-600 hover:bg-emerald-100 transition-colors"
                            title="Mark as Paid"
                            onClick={() => act(() => reservationsApi.markPaid(r.id), r.id)}
                            disabled={actionId === r.id}
                          >
                            <DollarSign size={14} />
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
