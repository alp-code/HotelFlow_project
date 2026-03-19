import { useEffect, useState } from 'react';
import { housekeepingApi } from '../../api/housekeeping';
import { HousekeepingTask } from '../../types';
import { ClipboardList, HandshakeIcon } from 'lucide-react';
import { PageHeader, PageSpinner, TaskStatusBadge, TaskTypeBadge, EmptyState, ErrorAlert } from '../../components/ui';
import { format } from 'date-fns';

export default function HousekeepingAvailable() {
  const [tasks, setTasks] = useState<HousekeepingTask[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [takingId, setTakingId] = useState<string | null>(null);

  const load = () => {
    setLoading(true);
    housekeepingApi.getAvailableTasks().then(setTasks).catch(() => setTasks([])).finally(() => setLoading(false));
  };
  useEffect(load, []);

  const takeTask = async (id: string) => {
    setTakingId(id);
    setError('');
    try {
      await housekeepingApi.takeTask(id);
      load();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } };
      setError(e.response?.data?.message ?? 'Failed to take task.');
    } finally {
      setTakingId(null);
    }
  };

  if (loading) return <PageSpinner />;

  return (
    <div className="p-8">
      <PageHeader
        title="Available Tasks"
        subtitle={`${tasks.length} unassigned task${tasks.length !== 1 ? 's' : ''} ready to take`}
      />
      {error && <ErrorAlert message={error} />}

      {tasks.length === 0 ? (
        <div className="card">
          <EmptyState message="No available tasks right now. Check back later." icon={<ClipboardList size={40} />} />
        </div>
      ) : (
        <div className="space-y-3">
          {tasks.map((task) => {
            const isOver = new Date(task.deadline) < new Date();
            return (
              <div key={task.id}
                className={`card hover:shadow-card-hover transition-all ${isOver ? 'border-amber-200' : ''}`}
              >
                <div className="flex items-start justify-between gap-4">
                  <div className="flex-1">
                    <div className="flex flex-wrap items-center gap-2 mb-2">
                      <span className="font-body font-bold text-hotel-navy">Room {task.roomNumber}</span>
                      <span className="text-hotel-muted text-sm font-body">({task.roomType})</span>
                      <TaskTypeBadge type={task.taskType} />
                      <TaskStatusBadge status={task.status} />
                      {isOver && <span className="badge bg-amber-100 text-amber-700">Urgent</span>}
                    </div>
                    <p className="text-hotel-slate text-sm font-body mb-2">{task.description}</p>
                    <div className="flex gap-4 text-xs text-hotel-muted font-body">
                      <span>Created: {format(new Date(task.createdAt), 'MMM d, HH:mm')}</span>
                      <span className={isOver ? 'text-amber-600 font-semibold' : ''}>
                        Deadline: {format(new Date(task.deadline), 'MMM d, HH:mm')}
                      </span>
                    </div>
                  </div>

                  <button
                    className="btn-primary flex-shrink-0 flex items-center gap-2"
                    onClick={() => takeTask(task.id)}
                    disabled={takingId === task.id}
                  >
                    {takingId === task.id ? (
                      'Taking…'
                    ) : (
                      <>
                        <svg className="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}>
                          <path d="M7 11V7a5 5 0 0 1 10 0v4M5 11h14a2 2 0 0 1 2 2v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-7a2 2 0 0 1 2-2z" />
                        </svg>
                        Take Task
                      </>
                    )}
                  </button>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
