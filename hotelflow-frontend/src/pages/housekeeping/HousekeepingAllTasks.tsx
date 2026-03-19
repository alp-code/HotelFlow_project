import { useEffect, useState } from 'react';
import { housekeepingApi } from '../../api/housekeeping';
import { roomsApi } from '../../api/rooms';
import { HousekeepingTask, Room } from '../../types';
import { Plus, Wrench, Filter } from 'lucide-react';
import {
  PageHeader, PageSpinner, TaskStatusBadge, TaskTypeBadge,
  EmptyState, ErrorAlert, Modal, FormField,
} from '../../components/ui';
import { format } from 'date-fns';
import { useAuth } from '../../context/AuthContext';

const taskTypeOptions = [
  { label: 'Cleaning', value: 1 },
  { label: 'Maintenance', value: 2 },
  { label: 'Inspection', value: 3 },
  { label: 'Restocking', value: 4 },
  { label: 'Setup', value: 5 },
];

const statusFilters = ['All', 'Pending', 'InProgress', 'Completed', 'Cancelled', 'Failed'];

export default function HousekeepingAllTasks() {
  const { role } = useAuth();
  const isStaff = role === 'Staff';

  const [tasks, setTasks] = useState<HousekeepingTask[]>([]);
  const [rooms, setRooms] = useState<Room[]>([]);
  const [loading, setLoading] = useState(true);
  const [statusFilter, setStatusFilter] = useState('All');
  const [error, setError] = useState('');

  const [createModal, setCreateModal] = useState(false);
  const [form, setForm] = useState({
    roomId: '',
    type: 1,
    description: '',
    deadline: format(new Date(Date.now() + 3600000 * 4), "yyyy-MM-dd'T'HH:mm"),
  });
  const [creating, setCreating] = useState(false);
  const [createError, setCreateError] = useState('');

  const load = () => {
    setLoading(true);
    Promise.all([
      housekeepingApi.getAllTasks(),
      isStaff ? roomsApi.getAll() : Promise.resolve([] as Room[]),
    ]).then(([t, r]) => {
      setTasks(t);
      setRooms(r);
    }).finally(() => setLoading(false));
  };
  useEffect(load, []);

  const filtered = tasks.filter((t) => statusFilter === 'All' || t.status === statusFilter);

  const handleCreate = async () => {
    setCreating(true);
    setCreateError('');
    try {
      await housekeepingApi.createTask({
        roomId: form.roomId,
        type: form.type,
        description: form.description,
        deadline: new Date(form.deadline).toISOString(),
      });
      setCreateModal(false);
      load();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } };
      setCreateError(e.response?.data?.message ?? 'Failed to create task.');
    } finally {
      setCreating(false);
    }
  };

  if (loading) return <PageSpinner />;

  return (
    <div className="p-8">
      <PageHeader
        title="All Tasks"
        subtitle={`${tasks.length} total tasks`}
        action={
          isStaff ? (
            <button className="btn-primary flex items-center gap-2" onClick={() => {
              setForm({ roomId: rooms[0]?.id ?? '', type: 1, description: '', deadline: format(new Date(Date.now() + 3600000 * 4), "yyyy-MM-dd'T'HH:mm") });
              setCreateError('');
              setCreateModal(true);
            }}>
              <Plus size={15} /> Create Task
            </button>
          ) : undefined
        }
      />
      {error && <ErrorAlert message={error} />}

      {/* Status filter */}
      <div className="flex flex-wrap gap-1.5 mb-6">
        <Filter size={14} className="text-hotel-muted self-center mr-1" />
        {statusFilters.map((s) => (
          <button
            key={s}
            onClick={() => setStatusFilter(s)}
            className={`px-3 py-1.5 rounded-full text-xs font-body font-semibold transition-all ${
              statusFilter === s
                ? 'bg-hotel-navy text-white'
                : 'bg-white border border-hotel-border text-hotel-slate hover:border-hotel-navy'
            }`}
          >
            {s}
          </button>
        ))}
      </div>

      {filtered.length === 0 ? (
        <div className="card"><EmptyState message="No tasks match the current filter." icon={<Wrench size={40} />} /></div>
      ) : (
        <div className="card p-0 overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-hotel-border">
                  {['Room', 'Type', 'Description', 'Assigned To', 'Deadline', 'Status'].map((h) => (
                    <th key={h} className="table-header text-left">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {filtered.map((task) => {
                  const isOver = task.status !== 'Completed' && task.status !== 'Cancelled' && new Date(task.deadline) < new Date();
                  return (
                    <tr key={task.id} className={`hover:bg-hotel-cream/40 transition-colors ${isOver ? 'bg-red-50/30' : ''}`}>
                      <td className="table-cell font-semibold text-hotel-navy">{task.roomNumber}</td>
                      <td className="table-cell"><TaskTypeBadge type={task.taskType} /></td>
                      <td className="table-cell max-w-xs">
                        <p className="truncate text-sm">{task.description}</p>
                      </td>
                      <td className="table-cell text-hotel-muted text-sm">
                        {task.assignedToUser || <span className="italic">Unassigned</span>}
                      </td>
                      <td className="table-cell">
                        <span className={`text-sm font-body ${isOver ? 'text-red-600 font-semibold' : 'text-hotel-slate'}`}>
                          {format(new Date(task.deadline), 'MMM d, HH:mm')}
                        </span>
                        {isOver && <span className="ml-1.5 badge bg-red-100 text-red-700">Overdue</span>}
                      </td>
                      <td className="table-cell"><TaskStatusBadge status={task.status} /></td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Create task modal */}
      <Modal title="Create Housekeeping Task" open={createModal} onClose={() => setCreateModal(false)}>
        {createError && <ErrorAlert message={createError} />}
        <div className="space-y-4">
          <FormField label="Room">
            <select className="input-field" value={form.roomId} onChange={(e) => setForm((f) => ({ ...f, roomId: e.target.value }))}>
              {rooms.map((r) => (
                <option key={r.id} value={r.id}>Room {r.roomNumber} ({r.roomType})</option>
              ))}
            </select>
          </FormField>
          <FormField label="Task type">
            <select className="input-field" value={form.type} onChange={(e) => setForm((f) => ({ ...f, type: +e.target.value }))}>
              {taskTypeOptions.map((t) => <option key={t.value} value={t.value}>{t.label}</option>)}
            </select>
          </FormField>
          <FormField label="Description">
            <textarea className="input-field resize-none" rows={3} placeholder="Describe the task…"
              value={form.description} onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))} />
          </FormField>
          <FormField label="Deadline">
            <input type="datetime-local" className="input-field" value={form.deadline}
              onChange={(e) => setForm((f) => ({ ...f, deadline: e.target.value }))} />
          </FormField>
          <div className="flex gap-3 pt-2">
            <button className="btn-secondary flex-1" onClick={() => setCreateModal(false)}>Cancel</button>
            <button className="btn-primary flex-1" onClick={handleCreate} disabled={creating}>
              {creating ? 'Creating…' : 'Create Task'}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
