import { useEffect, useState } from 'react';
import { roomsApi } from '../../api/rooms';
import { Room, RoomType } from '../../types';
import { Plus, Pencil, Trash2, BedDouble } from 'lucide-react';
import {
  PageHeader, PageSpinner, RoomStatusBadge, Modal,
  EmptyState, ErrorAlert, FormField,
} from '../../components/ui';

type Tab = 'rooms' | 'types';

const statusOptions = [
  { label: 'Available', value: 'Available' },
  { label: 'Occupied', value: 'Occupied' },
  { label: 'Needs Cleaning', value: 'NeedsCleaning' },
  { label: 'Out of Service', value: 'OutOfService' },
  { label: 'Cleaning', value: 'Cleaning' },
];


export default function StaffRooms() {
  const [tab, setTab] = useState<Tab>('rooms');
  const [rooms, setRooms] = useState<Room[]>([]);
  const [roomTypes, setRoomTypes] = useState<RoomType[]>([]);
  const [loading, setLoading] = useState(true);

  // Room form
  const [roomModal, setRoomModal] = useState(false);
  const [editRoom, setEditRoom] = useState<Room | null>(null);
  const [roomForm, setRoomForm] = useState({ roomNumber: '', roomTypeId: '', status: 'Available' });
  const [roomError, setRoomError] = useState('');
  const [roomSaving, setRoomSaving] = useState(false);

  // RoomType form
  const [typeModal, setTypeModal] = useState(false);
  const [editType, setEditType] = useState<RoomType | null>(null);
  const [typeForm, setTypeForm] = useState({ name: '', pricePerNight: 0, maxGuests: 1, description: '' });
  const [typeError, setTypeError] = useState('');
  const [typeSaving, setTypeSaving] = useState(false);

  const load = () => {
    setLoading(true);
    Promise.all([roomsApi.getAll(), roomsApi.getRoomTypes()])
      .then(([r, t]) => { setRooms(r); setRoomTypes(t); })
      .finally(() => setLoading(false));
  };
  useEffect(load, []);

  // ── Rooms ──
  const openCreateRoom = () => {
    setEditRoom(null);
    setRoomForm({ roomNumber: '', roomTypeId: roomTypes[0]?.id ?? '', status: 'Available' });
    setRoomError('');
    setRoomModal(true);
  };
  const openEditRoom = (r: Room) => {
    setEditRoom(r);
    const statusVal = statusOptions.find((s) => s.label.replace(' ', '') === r.status.replace(' ', ''))?.value ?? 1;
    setRoomForm({ roomNumber: r.roomNumber, roomTypeId: '', status: r.status });
    setRoomError('');
    setRoomModal(true);
  };
  const saveRoom = async () => {
    setRoomSaving(true);
    setRoomError('');
    try {
      if (editRoom) {
        await roomsApi.update(editRoom.id, { roomNumber: roomForm.roomNumber, status: roomForm.status });
      } else {
        await roomsApi.create({ roomNumber: roomForm.roomNumber, roomTypeId: roomForm.roomTypeId });
      }
      setRoomModal(false);
      load();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } };
      setRoomError(e.response?.data?.message ?? 'Save failed.');
    } finally {
      setRoomSaving(false);
    }
  };
  const deleteRoom = async (id: string) => {
    if (!confirm('Delete this room?')) return;
    await roomsApi.delete(id);
    load();
  };

  // ── Room Types ──
  const openCreateType = () => {
    setEditType(null);
    setTypeForm({ name: '', pricePerNight: 0, maxGuests: 1, description: '' });
    setTypeError('');
    setTypeModal(true);
  };
  const openEditType = (t: RoomType) => {
    setEditType(t);
    setTypeForm({ name: t.name, pricePerNight: t.pricePerNight, maxGuests: t.maxGuests, description: t.description ?? '' });
    setTypeError('');
    setTypeModal(true);
  };
  const saveType = async () => {
    setTypeSaving(true);
    setTypeError('');
    try {
      if (editType) {
        await roomsApi.updateRoomType(editType.id, typeForm);
      } else {
        await roomsApi.createRoomType(typeForm);
      }
      setTypeModal(false);
      load();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } };
      setTypeError(e.response?.data?.message ?? 'Save failed.');
    } finally {
      setTypeSaving(false);
    }
  };
  const deleteType = async (id: string) => {
    if (!confirm('Delete this room type?')) return;
    await roomsApi.deleteRoomType(id);
    load();
  };

  if (loading) return <PageSpinner />;

  return (
    <div className="p-8">
      <PageHeader
        title="Rooms & Types"
        action={
          <button
            className="btn-primary flex items-center gap-2"
            onClick={tab === 'rooms' ? openCreateRoom : openCreateType}
          >
            <Plus size={15} />
            {tab === 'rooms' ? 'Add Room' : 'Add Type'}
          </button>
        }
      />

      {/* Tabs */}
      <div className="flex gap-1 bg-hotel-cream border border-hotel-border rounded-xl p-1 w-fit mb-6">
        {(['rooms', 'types'] as Tab[]).map((t) => (
          <button
            key={t}
            onClick={() => setTab(t)}
            className={`px-5 py-2 rounded-lg text-sm font-body font-medium transition-all ${
              tab === t ? 'bg-white text-hotel-navy shadow-sm' : 'text-hotel-muted hover:text-hotel-navy'
            }`}
          >
            {t === 'rooms' ? `Rooms (${rooms.length})` : `Types (${roomTypes.length})`}
          </button>
        ))}
      </div>

      {/* Rooms tab */}
      {tab === 'rooms' && (
        <div className="card p-0 overflow-hidden">
          {rooms.length === 0 ? (
            <EmptyState message="No rooms yet." icon={<BedDouble size={40} />} />
          ) : (
            <table className="w-full">
              <thead>
                <tr className="border-b border-hotel-border">
                  {['Room Number', 'Type', 'Price/Night', 'Status', 'Actions'].map((h) => (
                    <th key={h} className="table-header text-left">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {rooms.map((r) => (
                  <tr key={r.id} className="hover:bg-hotel-cream/40 transition-colors">
                    <td className="table-cell font-semibold text-hotel-navy">{r.roomNumber}</td>
                    <td className="table-cell">{r.roomType}</td>
                    <td className="table-cell">${r.pricePerNight}/night</td>
                    <td className="table-cell"><RoomStatusBadge status={r.status} /></td>
                    <td className="table-cell">
                      <div className="flex gap-2">
                        <button onClick={() => openEditRoom(r)} className="p-1.5 rounded-lg text-hotel-muted hover:text-hotel-navy hover:bg-hotel-cream transition-colors">
                          <Pencil size={14} />
                        </button>
                        <button onClick={() => deleteRoom(r.id)} className="p-1.5 rounded-lg text-hotel-muted hover:text-red-500 hover:bg-red-50 transition-colors">
                          <Trash2 size={14} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}

      {/* Types tab */}
      {tab === 'types' && (
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
          {roomTypes.length === 0 && <EmptyState message="No room types." />}
          {roomTypes.map((t) => (
            <div key={t.id} className="card">
              <div className="flex items-start justify-between mb-3">
                <h3 className="font-display text-hotel-navy text-lg">{t.name}</h3>
                <div className="flex gap-1.5">
                  <button onClick={() => openEditType(t)} className="p-1.5 rounded-lg text-hotel-muted hover:text-hotel-navy hover:bg-hotel-cream transition-colors">
                    <Pencil size={14} />
                  </button>
                  <button onClick={() => deleteType(t.id)} className="p-1.5 rounded-lg text-hotel-muted hover:text-red-500 hover:bg-red-50 transition-colors">
                    <Trash2 size={14} />
                  </button>
                </div>
              </div>
              <p className="font-display text-2xl text-hotel-gold mb-1">${t.pricePerNight}<span className="text-sm text-hotel-muted font-body font-normal">/night</span></p>
              <p className="text-hotel-muted text-sm font-body">Up to {t.maxGuests} guests</p>
              {t.description && <p className="text-hotel-muted text-sm font-body mt-2 italic">{t.description}</p>}
            </div>
          ))}
        </div>
      )}

      {/* Room modal */}
      <Modal title={editRoom ? 'Edit Room' : 'Add Room'} open={roomModal} onClose={() => setRoomModal(false)}>
        {roomError && <ErrorAlert message={roomError} />}
        <div className="space-y-4">
          <FormField label="Room number">
            <input className="input-field" value={roomForm.roomNumber} onChange={(e) => setRoomForm((f) => ({ ...f, roomNumber: e.target.value }))} placeholder="e.g. 101" />
          </FormField>
          {!editRoom && (
            <FormField label="Room type">
              <select className="input-field" value={roomForm.roomTypeId} onChange={(e) => setRoomForm((f) => ({ ...f, roomTypeId: e.target.value }))}>
                {roomTypes.map((t) => <option key={t.id} value={t.id}>{t.name}</option>)}
              </select>
            </FormField>
          )}
          {editRoom && (
            <FormField label="Status">
              <select className="input-field" value={roomForm.status} onChange={(e) => setRoomForm((f) => ({ ...f, status: e.target.value }))}>
                {statusOptions.map((s) => <option key={s.value} value={s.value}>{s.label}</option>)}
              </select>
            </FormField>
          )}
          <div className="flex gap-3 pt-2">
            <button className="btn-secondary flex-1" onClick={() => setRoomModal(false)}>Cancel</button>
            <button className="btn-primary flex-1" onClick={saveRoom} disabled={roomSaving}>
              {roomSaving ? 'Saving…' : editRoom ? 'Update' : 'Create'}
            </button>
          </div>
        </div>
      </Modal>

      {/* Room type modal */}
      <Modal title={editType ? 'Edit Room Type' : 'Add Room Type'} open={typeModal} onClose={() => setTypeModal(false)}>
        {typeError && <ErrorAlert message={typeError} />}
        <div className="space-y-4">
          <FormField label="Name">
            <input className="input-field" value={typeForm.name} onChange={(e) => setTypeForm((f) => ({ ...f, name: e.target.value }))} placeholder="e.g. Deluxe Suite" />
          </FormField>
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Price per night ($)">
              <input type="number" className="input-field" value={typeForm.pricePerNight} min={0}
                onChange={(e) => setTypeForm((f) => ({ ...f, pricePerNight: +e.target.value }))} />
            </FormField>
            <FormField label="Max guests">
              <input type="number" className="input-field" value={typeForm.maxGuests} min={1}
                onChange={(e) => setTypeForm((f) => ({ ...f, maxGuests: +e.target.value }))} />
            </FormField>
          </div>
          <FormField label="Description (optional)">
            <input className="input-field" value={typeForm.description}
              onChange={(e) => setTypeForm((f) => ({ ...f, description: e.target.value }))} placeholder="Brief description…" />
          </FormField>
          <div className="flex gap-3 pt-2">
            <button className="btn-secondary flex-1" onClick={() => setTypeModal(false)}>Cancel</button>
            <button className="btn-primary flex-1" onClick={saveType} disabled={typeSaving}>
              {typeSaving ? 'Saving…' : editType ? 'Update' : 'Create'}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
