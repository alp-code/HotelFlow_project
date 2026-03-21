import { useEffect, useState } from 'react';
import { housekeepingApi } from '../../api/housekeeping';
import { HousekeepingTask, Housekeeper } from '../../types';
import { ClipboardList, CheckCircle, Clock, AlertCircle, User } from 'lucide-react';
import { StatCard, PageSpinner, TaskStatusBadge, TaskTypeBadge, PageHeader } from '../../components/ui';
import { format } from 'date-fns';

export default function HousekeepingDashboard() {
  const [tasks, setTasks] = useState<HousekeepingTask[]>([]);
  const [myInfo, setMyInfo] = useState<Housekeeper | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    Promise.all([
      housekeepingApi.getTodayTasks(),
      housekeepingApi.getAvailableTasks(),
      housekeepingApi.getMyTasks(),
      housekeepingApi.getMyInfo(true).catch(() => null),
    ]).then(([todayTasks, available, myTasks, me]) => {
      const combined = [...todayTasks, ...available.filter(t => new Date(t.deadline).toDateString() === new Date().toDateString()),
      ...myTasks,
      ];
      const unique = combined.filter((t, i, self) => self.findIndex(x => x.id === t.id) === i);
      setTasks(unique);
      setMyInfo(me);
    }).finally(() => setLoading(false));
  }, []);

  const pending = tasks.filter((t) => t.status === 'Pending');
  const inProgress = tasks.filter((t) => t.status === 'InProgress');
  const completed = tasks.filter((t) => t.status === 'Completed');
  const overdue = tasks.filter((t) => t.status !== 'Completed' && t.status !== 'Cancelled' && new Date(t.deadline) < new Date());

  if (loading) return <PageSpinner />;

  return (
    <div className="p-8">
      <div className="flex items-start justify-between mb-8">
        <div>
          <h1 className="font-display text-hotel-navy text-3xl">
            Good {new Date().getHours() < 12 ? 'morning' : new Date().getHours() < 17 ? 'afternoon' : 'evening'},{' '}
            <span className="text-hotel-gold">{myInfo?.fullName?.split(' ')[0] ?? 'Housekeeper'}</span>
          </h1>
          <p className="text-hotel-muted font-body text-sm mt-1">{format(new Date(), 'EEEE, MMMM d, yyyy')}</p>
        </div>
        {myInfo && (
          <div className="flex items-center gap-3 bg-white border border-hotel-border rounded-2xl px-4 py-3">
            <div className="w-10 h-10 rounded-full bg-hotel-gold/10 flex items-center justify-center">
              <User size={18} className="text-hotel-gold" />
            </div>
            <div>
              <p className="font-body font-semibold text-hotel-navy text-sm">{myInfo.fullName}</p>
              <p className="text-hotel-muted text-xs font-body">{myInfo.activeTasksCount ?? 0} active · {myInfo.completedTasksToday ?? 0} done today</p>
            </div>
          </div>
        )}
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 xl:grid-cols-4 gap-4 mb-8">
        <StatCard label="Today's Tasks" value={tasks.length}
          icon={<ClipboardList size={20} className="text-hotel-gold" />} />
        <StatCard label="In Progress" value={inProgress.length}
          icon={<Clock size={20} className="text-sky-600" />} accent="bg-sky-50" />
        <StatCard label="Completed Today" value={completed.length}
          icon={<CheckCircle size={20} className="text-emerald-600" />} accent="bg-emerald-50" />
        <StatCard label="Overdue" value={overdue.length}
          icon={<AlertCircle size={20} className="text-red-500" />} accent="bg-red-50" />
      </div>

      {/* Task list */}
      <div className="card">
        <h2 className="font-display text-hotel-navy text-lg mb-4">Today's Schedule</h2>
        {tasks.length === 0 ? (
          <p className="text-center text-hotel-muted font-body text-sm py-8">No tasks assigned for today.</p>
        ) : (
          <div className="space-y-3">
            {tasks.map((task) => {
              const isOver = task.status !== 'Completed' && task.status !== 'Cancelled' && new Date(task.deadline) < new Date();
              return (
                <div key={task.id}
                  className={`flex items-start gap-4 p-4 rounded-xl border transition-all ${
                    isOver ? 'border-red-200 bg-red-50/50' : 'border-hotel-border bg-hotel-cream/40'
                  }`}
                >
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 mb-1 flex-wrap">
                      <span className="font-body font-semibold text-hotel-navy text-sm">Room {task.roomNumber}</span>
                      <TaskTypeBadge type={task.taskType} />
                      <TaskStatusBadge status={task.status} />
                      {isOver && <span className="badge bg-red-100 text-red-700">Overdue</span>}
                    </div>
                    <p className="text-hotel-muted text-sm font-body">{task.description}</p>
                    <p className="text-hotel-muted text-xs font-body mt-1">
                      Deadline: {format(new Date(task.deadline), 'HH:mm, MMM d')}
                    </p>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
}
