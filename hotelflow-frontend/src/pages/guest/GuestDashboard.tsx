import { useEffect, useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import { reservationsApi } from '../../api/reservations';
import { Reservation } from '../../types';
import { CalendarDays, BedDouble, CheckCircle, Clock, Star } from 'lucide-react';
import { StatCard, PageSpinner, ReservationStatusBadge, PageHeader } from '../../components/ui';
import { format } from 'date-fns';
import { Link } from 'react-router-dom';

export default function GuestDashboard() {
  const { email } = useAuth();
  const [reservations, setReservations] = useState<Reservation[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    reservationsApi.getMyReservations().then(setReservations).finally(() => setLoading(false));
  }, []);

  const active = reservations.filter((r) => r.status === 'Confirmed' || r.status === 'CheckedIn');
  const upcoming = reservations.filter((r) => r.status === 'Confirmed');
  const checkedIn = reservations.filter((r) => r.status === 'CheckedIn');
  const firstName = email?.split('@')[0] ?? 'Guest';

  if (loading) return <PageSpinner />;

  return (
    <div className="p-8">
      {/* Greeting */}
      <div className="mb-8">
        <h1 className="font-display text-hotel-navy text-3xl">
          Welcome back, <span className="text-hotel-gold capitalize">{firstName}</span>
        </h1>
        <p className="text-hotel-muted font-body text-sm mt-1">
          {format(new Date(), 'EEEE, MMMM d, yyyy')}
        </p>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4 mb-8">
        <StatCard label="Total Reservations" value={reservations.length}
          icon={<CalendarDays size={20} className="text-hotel-gold" />} />
        <StatCard label="Upcoming" value={upcoming.length}
          icon={<Clock size={20} className="text-sky-600" />} accent="bg-sky-50" />
        <StatCard label="Currently Staying" value={checkedIn.length}
          icon={<BedDouble size={20} className="text-emerald-600" />} accent="bg-emerald-50" />
        <StatCard label="Completed Stays" value={reservations.filter((r) => r.status === 'CheckedOut').length}
          icon={<CheckCircle size={20} className="text-violet-600" />} accent="bg-violet-50" />
      </div>

      {/* CTA if no upcoming */}
      {upcoming.length === 0 && (
        <div className="card mb-6 flex items-center gap-6 bg-gradient-to-r from-hotel-navy to-hotel-navy/90 border-0">
          <div className="w-14 h-14 rounded-2xl bg-hotel-gold/20 flex items-center justify-center flex-shrink-0">
            <Star size={24} className="text-hotel-gold" />
          </div>
          <div className="flex-1">
            <h3 className="font-display text-white text-lg">Plan your next stay</h3>
            <p className="text-white/60 font-body text-sm mt-0.5">Browse our room selection and make a reservation.</p>
          </div>
          <Link to="/guest/rooms" className="btn-primary flex-shrink-0">Browse Rooms</Link>
        </div>
      )}

      {/* Active reservations */}
      {active.length > 0 && (
        <div className="card mb-6">
          <h2 className="font-display text-hotel-navy text-lg mb-4">Active Reservations</h2>
          <div className="space-y-3">
            {active.map((r) => (
              <div key={r.id} className="flex items-center justify-between p-4 rounded-xl bg-hotel-cream border border-hotel-border/50">
                <div className="flex items-center gap-4">
                  <div className="w-10 h-10 rounded-xl bg-hotel-gold/10 flex items-center justify-center">
                    <BedDouble size={18} className="text-hotel-gold" />
                  </div>
                  <div>
                    <p className="font-body font-semibold text-hotel-navy text-sm">Room {r.roomNumber} — {r.roomType}</p>
                    <p className="text-hotel-muted text-xs font-body mt-0.5">
                      {format(new Date(r.checkInDate), 'MMM d')} → {format(new Date(r.checkOutDate), 'MMM d, yyyy')} · {r.nights} night{r.nights !== 1 ? 's' : ''}
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-3">
                  <span className="text-hotel-navy font-semibold text-sm font-body">${r.totalPrice.toFixed(2)}</span>
                  <ReservationStatusBadge status={r.status} />
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Recent history */}
      <div className="card">
        <div className="flex items-center justify-between mb-4">
          <h2 className="font-display text-hotel-navy text-lg">Reservation History</h2>
          <Link to="/guest/reservations" className="text-hotel-gold text-sm font-body font-medium hover:text-hotel-gold-light transition-colors">
            View all →
          </Link>
        </div>
        {reservations.length === 0 ? (
          <p className="text-hotel-muted font-body text-sm text-center py-8">No reservations yet.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr>
                  {['Room', 'Check-in', 'Check-out', 'Total', 'Status'].map((h) => (
                    <th key={h} className="table-header text-left">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {reservations.slice(0, 5).map((r) => (
                  <tr key={r.id} className="hover:bg-hotel-cream/50 transition-colors">
                    <td className="table-cell font-medium text-hotel-navy">{r.roomNumber} <span className="text-hotel-muted font-normal">({r.roomType})</span></td>
                    <td className="table-cell">{format(new Date(r.checkInDate), 'MMM d, yyyy')}</td>
                    <td className="table-cell">{format(new Date(r.checkOutDate), 'MMM d, yyyy')}</td>
                    <td className="table-cell font-medium">${r.totalPrice.toFixed(2)}</td>
                    <td className="table-cell"><ReservationStatusBadge status={r.status} /></td>
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
