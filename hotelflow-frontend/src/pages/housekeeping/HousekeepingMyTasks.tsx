import { useEffect, useState } from 'react';
import { housekeepingApi } from '../../api/housekeeping';
import { HousekeepingTask } from '../../types';
import { ListTodo, Play, CheckCircle2, ChevronDown } from 'lucide-react';
import { PageHeader, PageSpinner, TaskStatusBadge, TaskTypeBadge, EmptyState, ErrorAlert, Modal, FormField } from '../../components/ui';
import { format } from 'date-fns';

export default function HousekeepingMyTasks() {
  const [tasks, setTasks] = useState<HousekeepingTask[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [actionId, setActionId] = useState<string | null>(null);
  const [completeModal, setCompleteModal] = useState(false);
  const [completeTarget, setCompleteTarget] = useState<HousekeepingTask | null>(null);
  const [notes, setNotes] = useState('');

  const load = () => {
    setLoading(true);
    housekeepingApi.getMyTasks().then(setTasks).catch(() => setTasks([])).finally(() => setLoading(false));
  };
  useEffect(load, []);

  const act = async (fn: () => Promise<unknown>, id: string) => {
    setActionId(id);
    setError('');
    try { await fn(); load(); }
    catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } };
      setError(e.response?.data?.message ?? 'Action failed.');
    } finally { setActionId(null); }
  };

  const handleComplete = async () => {
    if (!completeTarget) return;
    await act(() => housekeepingApi.completeTask(completeTarget.id, notes || undefined), completeTarget.id);
    setCompleteModal(false);
    setNotes('');
  };

  const updateStatus = async (task: HousekeepingTask, status: number) => {
    await act(() => housekeepingApi.updateStatus(task.id, status), task.id);
  };

  if (loading) return <PageSpinner />;

  const active = tasks.filter((t) => t.status === 'Pending' || t.status === 'InProgress');
  const done = tasks.filter((t) => t.status === 'Completed' || t.status === 'Cancelled' || t.status === 'Failed');

  return (
    <div className="p-8">
      <PageHeader title="My Tasks" subtitle={`${active.length} active, ${done.length} closed`} />
      {error && <ErrorAlert message={error} />}

      {tasks.length === 0 ? (
        <div className="card"><EmptyState message="You have no assigned tasks." icon={<ListTodo size={40} />} /></div>
      ) : (
        <>
          {/* Active tasks */}
          {active.length > 0 && (
            <div className="mb-6">
              <h2 className="font-display text-hotel-navy text-lg mb-3">Active</h2>
              <div className="space-y-3">
                {active.map((task) => {
                  const isOver = new Date(task.deadline) < new Date();
                  return (
                    <div key={task.id} className={`card hover:shadow-card-hover transition-all ${isOver ? 'border-red-200' : ''}`}>
                      <div className="flex items-start justify-between gap-4">
                        <div className="flex-1">
                          <div className="flex flex-wrap items-center gap-2 mb-2">
                            <span className="font-body font-bold text-hotel-navy">Room {task.roomNumber}</span>
                            <span className="text-hotel-muted text-sm font-body">({task.roomType})</span>
                            <TaskTypeBadge type={task.taskType} />
                            <TaskStatusBadge status={task.status} />
                            {isOver && <span className="badge bg-red-100 text-red-700">Overdue</span>}
                          </div>
                          <p className="text-hotel-slate text-sm font-body mb-2">{task.description}</p>
                          <p className="text-hotel-muted text-xs font-body">
                            Deadline: <span className={isOver ? 'text-red-500 font-semibold' : ''}>
                              {format(new Date(task.deadline), 'HH:mm, MMM d, yyyy')}
                            </span>
                          </p>
                          {task.notes && <p className="text-hotel-muted text-xs font-body mt-1 italic">Note: {task.notes}</p>}
                        </div>

                        <div className="flex flex-col gap-2 flex-shrink-0">
                          {task.status === 'Pending' && (
                            <button
                              className="btn-secondary py-1.5 px-3 text-xs flex items-center gap-1.5"
                              onClick={() => updateStatus(task, 2)}
                              disabled={actionId === task.id}
                            >
                              <Play size={12} /> Start
                            </button>
                          )}
                          {task.status === 'InProgress' && (
                            <button
                              className="btn-primary py-1.5 px-3 text-xs flex items-center gap-1.5"
                              onClick={() => { setCompleteTarget(task); setCompleteModal(true); }}
                            >
                              <CheckCircle2 size={12} /> Complete
                            </button>
                          )}
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          )}

          {/* Done tasks */}
          {done.length > 0 && (
            <div>
              <h2 className="font-display text-hotel-navy text-lg mb-3">Completed / Closed</h2>
              <div className="space-y-2">
                {done.map((task) => (
                  <div key={task.id} className="card opacity-60 py-3">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-3">
                        <span className="font-body font-semibold text-hotel-navy text-sm">Room {task.roomNumber}</span>
                        <TaskTypeBadge type={task.taskType} />
                        <TaskStatusBadge status={task.status} />
                      </div>
                      {task.completedAt && (
                        <span className="text-hotel-muted text-xs font-body">
                          {format(new Date(task.completedAt), 'HH:mm, MMM d')}
                        </span>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </>
      )}

      {/* Complete modal */}
      <Modal title="Complete Task" open={completeModal} onClose={() => { setCompleteModal(false); setNotes(''); }}>
        <p className="text-hotel-muted text-sm font-body mb-4">
          Mark task for <strong className="text-hotel-navy">Room {completeTarget?.roomNumber}</strong> as completed.
        </p>
        <FormField label="Notes (optional)">
          <textarea className="input-field resize-none" rows={3} placeholder="Any observations or notes…"
            value={notes} onChange={(e) => setNotes(e.target.value)} />
        </FormField>
        <div className="flex gap-3 mt-4">
          <button className="btn-secondary flex-1" onClick={() => setCompleteModal(false)}>Cancel</button>
          <button className="btn-primary flex-1" onClick={handleComplete} disabled={actionId === completeTarget?.id}>
            {actionId === completeTarget?.id ? 'Completing…' : 'Mark Complete'}
          </button>
        </div>
      </Modal>
    </div>
  );
}
