import { useEffect, useState } from 'react';
import { reservationsApi } from '../../api/reservations';
import { Reservation } from '../../types';
import { CalendarDays, X } from 'lucide-react';
import {
  PageHeader, PageSpinner, ReservationStatusBadge,
  EmptyState, Modal, ErrorAlert,
} from '../../components/ui';
import { format } from 'date-fns';

export default function GuestReservations() {
  const [reservations, setReservations] = useState<Reservation[]>([]);
  const [loading, setLoading] = useState(true);
  const [selected, setSelected] = useState<Reservation | null>(null);
  const [cancelling, setCancelling] = useState(false);
  const [cancelError, setCancelError] = useState('');
  const [cancelConfirm, setCancelConfirm] = useState<Reservation | null>(null);

  const load = () => {
    setLoading(true);
    reservationsApi.getMyReservations().then(setReservations).finally(() => setLoading(false));
  };

  useEffect(load, []);

  const handleCancel = async (r: Reservation) => {
    setCancelling(true);
    setCancelError('');
    try {
      await reservationsApi.cancel(r.id);
      setCancelConfirm(null);
      load();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } };
      setCancelError(e.response?.data?.message ?? 'Cancellation failed.');
    } finally {
      setCancelling(false);
    }
  };

  if (loading) return <PageSpinner />;

  const canCancel = (r: Reservation) => r.status === 'Confirmed';

  return (
    <div className="p-8">
      <PageHeader title="My Reservations" subtitle={`${reservations.length} reservation${reservations.length !== 1 ? 's' : ''} total`} />

      {reservations.length === 0 ? (
        <div className="card">
          <EmptyState message="You haven't made any reservations yet." icon={<CalendarDays size={48} />} />
        </div>
      ) : (
        <div className="space-y-3">
          {reservations.map((r) => (
            <div key={r.id} className="card hover:shadow-card-hover transition-all duration-200 cursor-pointer"
              onClick={() => setSelected(r)}>
              <div className="flex items-start justify-between">
                <div className="flex items-start gap-4">
                  <div className="w-11 h-11 rounded-xl bg-hotel-gold/10 flex items-center justify-center flex-shrink-0">
                    <CalendarDays size={20} className="text-hotel-gold" />
                  </div>
                  <div>
                    <p className="font-body font-semibold text-hotel-navy">
                      Room {r.roomNumber}
                      <span className="text-hotel-muted font-normal ml-1.5">({r.roomType})</span>
                    </p>
                    <p className="text-hotel-muted text-sm font-body mt-0.5">
                      {format(new Date(r.checkInDate), 'MMM d')} → {format(new Date(r.checkOutDate), 'MMM d, yyyy')}
                      <span className="mx-1.5 text-hotel-border">·</span>
                      {r.nights} night{r.nights !== 1 ? 's' : ''}
                      <span className="mx-1.5 text-hotel-border">·</span>
                      {r.numberOfGuests} guest{r.numberOfGuests !== 1 ? 's' : ''}
                    </p>
                    {r.specialRequests && (
                      <p className="text-hotel-muted text-xs font-body mt-1 italic">"{r.specialRequests}"</p>
                    )}
                  </div>
                </div>
                <div className="flex flex-col items-end gap-2">
                  <ReservationStatusBadge status={r.status} />
                  <p className="font-display text-hotel-navy text-lg">${r.totalPrice.toFixed(2)}</p>
                  {r.isPaid && <span className="badge bg-emerald-100 text-emerald-700 text-xs">Paid</span>}
                </div>
              </div>

              {canCancel(r) && (
                <div className="mt-3 pt-3 border-t border-hotel-border/50">
                  <button
                    className="btn-danger py-1.5 px-4 text-xs flex items-center gap-1.5"
                    onClick={(e) => { e.stopPropagation(); setCancelConfirm(r); }}
                  >
                    <X size={13} /> Cancel Reservation
                  </button>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Detail modal */}
      <Modal title="Reservation Details" open={!!selected} onClose={() => setSelected(null)}>
        {selected && (
          <div className="space-y-3">
            {[
              ['Reservation ID', selected.id.slice(0, 8) + '…'],
              ['Room', `${selected.roomNumber} (${selected.roomType})`],
              ['Check-in', format(new Date(selected.checkInDate), 'MMMM d, yyyy')],
              ['Check-out', format(new Date(selected.checkOutDate), 'MMMM d, yyyy')],
              ['Duration', `${selected.nights} night${selected.nights !== 1 ? 's' : ''}`],
              ['Guests', selected.numberOfGuests],
              ['Total Price', `$${selected.totalPrice.toFixed(2)}`],
              ['Payment', selected.isPaid ? 'Paid' : 'Pending'],
              ['Status', selected.status],
              ['Booked on', format(new Date(selected.createdAt), 'MMM d, yyyy')],
            ].map(([k, v]) => (
              <div key={String(k)} className="flex justify-between py-2 border-b border-hotel-border/50 text-sm font-body">
                <span className="text-hotel-muted">{k}</span>
                <span className="font-semibold text-hotel-navy">{v}</span>
              </div>
            ))}
            {selected.specialRequests && (
              <div className="pt-2">
                <p className="text-hotel-muted text-xs uppercase tracking-wide font-semibold mb-1">Special Requests</p>
                <p className="text-hotel-slate text-sm italic">"{selected.specialRequests}"</p>
              </div>
            )}
          </div>
        )}
      </Modal>

      {/* Cancel confirm modal */}
      <Modal title="Cancel Reservation" open={!!cancelConfirm} onClose={() => { setCancelConfirm(null); setCancelError(''); }}>
        {cancelError && <ErrorAlert message={cancelError} />}
        <p className="text-hotel-slate font-body text-sm mb-6">
          Are you sure you want to cancel your reservation for{' '}
          <strong>Room {cancelConfirm?.roomNumber}</strong>{' '}
          on{' '}
          <strong>{cancelConfirm && format(new Date(cancelConfirm.checkInDate), 'MMM d, yyyy')}</strong>?
          This action cannot be undone.
        </p>
        <div className="flex gap-3">
          <button className="btn-secondary flex-1" onClick={() => setCancelConfirm(null)}>Keep it</button>
          <button className="btn-danger flex-1" onClick={() => cancelConfirm && handleCancel(cancelConfirm)} disabled={cancelling}>
            {cancelling ? 'Cancelling…' : 'Yes, Cancel'}
          </button>
        </div>
      </Modal>
    </div>
  );
}
