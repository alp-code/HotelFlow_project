import { useEffect, useState } from 'react';
import { roomsApi } from '../../api/rooms';
import { reservationsApi } from '../../api/reservations';
import { RoomType, AvailableRoom } from '../../types';
import { Search, BedDouble, Users, Star, ChevronRight } from 'lucide-react';
import { PageHeader, PageSpinner, Modal, ErrorAlert, FormField } from '../../components/ui';
import { format } from 'date-fns';

export default function GuestRooms() {
  const [roomTypes, setRoomTypes] = useState<RoomType[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchOpen, setSearchOpen] = useState(false);
  const [selectedType, setSelectedType] = useState<RoomType | null>(null);
  const [availableRooms, setAvailableRooms] = useState<AvailableRoom[]>([]);
  const [searching, setSearching] = useState(false);
  const [booking, setBooking] = useState(false);
  const [bookError, setBookError] = useState('');
  const [bookSuccess, setBookSuccess] = useState('');
  const [selectedRoom, setSelectedRoom] = useState<AvailableRoom | null>(null);
  const [confirmOpen, setConfirmOpen] = useState(false);

  const today = format(new Date(), 'yyyy-MM-dd');
  const tomorrow = format(new Date(Date.now() + 86400000), 'yyyy-MM-dd');
  const [form, setForm] = useState({ checkIn: today, checkOut: tomorrow, guests: 1, specialRequests: '' });

  useEffect(() => {
    roomsApi.getRoomTypes().then(setRoomTypes).finally(() => setLoading(false));
  }, []);

  const set = (k: string) => (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) =>
    setForm((f) => ({ ...f, [k]: e.target.value }));

  const handleSearch = async () => {
    if (!selectedType) return;
    setSearching(true);
    setAvailableRooms([]);
    try {
      const rooms = await reservationsApi.findAvailableRooms(
        selectedType.name, form.checkIn, form.checkOut, form.guests
      );
      setAvailableRooms(rooms);
    } catch {
      setAvailableRooms([]);
    } finally {
      setSearching(false);
    }
  };

  const handleBook = async () => {
    if (!selectedRoom || !selectedType) return;
    setBooking(true);
    setBookError('');
    try {
      await reservationsApi.create({
        roomNumber: selectedRoom.roomNumber,
        roomTypeName: selectedType.name,
        checkInDate: new Date(form.checkIn).toISOString(),
        checkOutDate: new Date(form.checkOut).toISOString(),
        numberOfGuests: form.guests,
        specialRequests: form.specialRequests || undefined,
      });
      setBookSuccess(`Room ${selectedRoom.roomNumber} booked successfully!`);
      setConfirmOpen(false);
      setSearchOpen(false);
      setAvailableRooms([]);
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } };
      setBookError(e.response?.data?.message ?? 'Booking failed.');
    } finally {
      setBooking(false);
    }
  };

  if (loading) return <PageSpinner />;

  return (
    <div className="p-8">
      <PageHeader title="Browse Rooms" subtitle="Choose your perfect stay" />

      {bookSuccess && (
        <div className="bg-emerald-50 border border-emerald-200 text-emerald-800 rounded-xl px-4 py-3 text-sm font-body mb-6 flex items-center gap-2">
          <Star size={16} className="flex-shrink-0" />
          {bookSuccess}
        </div>
      )}

      {/* Room type cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-5">
        {roomTypes.map((rt) => (
          <div key={rt.id} className="card hover:shadow-card-hover transition-all duration-200 group cursor-pointer"
            onClick={() => { setSelectedType(rt); setSearchOpen(true); setAvailableRooms([]); setBookSuccess(''); }}>
            {/* Room type visual */}
            <div className="h-32 rounded-xl bg-gradient-to-br from-hotel-navy/90 to-hotel-navy mb-4 flex items-end p-4 relative overflow-hidden">
              <div className="absolute inset-0 opacity-20"
                style={{ backgroundImage: 'radial-gradient(circle at 70% 30%, #C8952A 0%, transparent 60%)' }} />
              <div className="relative">
                <p className="text-white/50 text-xs font-body uppercase tracking-widest">Type</p>
                <p className="font-display text-white text-xl">{rt.name}</p>
              </div>
            </div>

            <div className="flex items-start justify-between mb-3">
              <div>
                <p className="font-display text-hotel-navy text-2xl">${rt.pricePerNight}<span className="text-hotel-muted text-sm font-body font-normal">/night</span></p>
              </div>
              <div className="flex items-center gap-1 text-hotel-muted text-sm font-body">
                <Users size={14} />
                <span>Up to {rt.maxGuests}</span>
              </div>
            </div>

            {rt.description && (
              <p className="text-hotel-muted text-sm font-body mb-4 leading-relaxed">{rt.description}</p>
            )}

            <button className="w-full flex items-center justify-center gap-2 py-2.5 rounded-xl border border-hotel-gold text-hotel-gold text-sm font-body font-semibold hover:bg-hotel-gold hover:text-white transition-all duration-200 group-hover:bg-hotel-gold group-hover:text-white">
              Check Availability <ChevronRight size={15} />
            </button>
          </div>
        ))}
      </div>

      {/* Search / Book Modal */}
      <Modal title={`Search ${selectedType?.name ?? ''} Rooms`} open={searchOpen} onClose={() => setSearchOpen(false)} width="max-w-xl">
        <div className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Check-in date">
              <input type="date" className="input-field" value={form.checkIn} min={today}
                onChange={(e) => setForm((f) => ({ ...f, checkIn: e.target.value }))} />
            </FormField>
            <FormField label="Check-out date">
              <input type="date" className="input-field" value={form.checkOut} min={form.checkIn}
                onChange={(e) => setForm((f) => ({ ...f, checkOut: e.target.value }))} />
            </FormField>
          </div>

          <FormField label="Number of guests">
            <input type="number" className="input-field" min={1} max={selectedType?.maxGuests ?? 10}
              value={form.guests} onChange={(e) => setForm((f) => ({ ...f, guests: +e.target.value }))} />
          </FormField>

          <button onClick={handleSearch} disabled={searching} className="btn-primary w-full flex items-center justify-center gap-2">
            <Search size={15} />
            {searching ? 'Searching…' : 'Search Available Rooms'}
          </button>

          {/* Results */}
          {availableRooms.length === 0 && !searching && (
            <p className="text-center text-hotel-muted text-sm font-body py-4">
              {searching ? '' : 'No results yet. Click search above.'}
            </p>
          )}

          {availableRooms.map((room) => (
            <div key={room.id} className="flex items-center justify-between p-4 rounded-xl bg-hotel-cream border border-hotel-border">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-xl bg-hotel-gold/10 flex items-center justify-center">
                  <BedDouble size={18} className="text-hotel-gold" />
                </div>
                <div>
                  <p className="font-body font-semibold text-hotel-navy text-sm">Room {room.roomNumber}</p>
                  <p className="text-hotel-muted text-xs font-body">{room.nights} night{room.nights !== 1 ? 's' : ''} · ${room.totalPrice.toFixed(2)} total</p>
                </div>
              </div>
              <button className="btn-primary py-1.5 px-4"
                onClick={() => { setSelectedRoom(room); setConfirmOpen(true); }}>
                Book
              </button>
            </div>
          ))}
        </div>
      </Modal>

      {/* Confirm booking modal */}
      <Modal title="Confirm Reservation" open={confirmOpen} onClose={() => { setConfirmOpen(false); setBookError(''); }}>
        {bookError && <ErrorAlert message={bookError} />}
        {selectedRoom && (
          <div className="space-y-4">
            <div className="bg-hotel-cream rounded-xl p-4 space-y-2">
              {[
                ['Room', selectedRoom.roomNumber],
                ['Type', selectedType?.name ?? ''],
                ['Check-in', format(new Date(form.checkIn), 'MMMM d, yyyy')],
                ['Check-out', format(new Date(form.checkOut), 'MMMM d, yyyy')],
                ['Guests', form.guests],
                ['Total', `$${selectedRoom.totalPrice.toFixed(2)}`],
              ].map(([k, v]) => (
                <div key={String(k)} className="flex justify-between text-sm font-body">
                  <span className="text-hotel-muted">{k}</span>
                  <span className="font-semibold text-hotel-navy">{v}</span>
                </div>
              ))}
            </div>

            <FormField label="Special requests (optional)">
              <textarea className="input-field resize-none" rows={2} placeholder="Any special requests…"
                value={form.specialRequests} onChange={set('specialRequests')} />
            </FormField>

            <div className="flex gap-3 pt-2">
              <button className="btn-secondary flex-1" onClick={() => setConfirmOpen(false)}>Cancel</button>
              <button className="btn-primary flex-1" onClick={handleBook} disabled={booking}>
                {booking ? 'Booking…' : 'Confirm Booking'}
              </button>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}
