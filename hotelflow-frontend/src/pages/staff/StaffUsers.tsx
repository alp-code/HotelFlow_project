import { useEffect, useState } from 'react';
import { usersApi } from '../../api/users';
import { User } from '../../types';
import { Users, Trash2, RotateCcw, Shield } from 'lucide-react';
import { PageHeader, PageSpinner, EmptyState, ErrorAlert, Modal, FormField } from '../../components/ui';

const roleColors: Record<string, string> = {
  Staff: 'bg-amber-100 text-amber-800',
  Guest: 'bg-green-100 text-green-800',
  Housekeeping: 'bg-blue-100 text-blue-800',
};

export default function StaffUsers() {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [actionId, setActionId] = useState<string | null>(null);

  const [roleModal, setRoleModal] = useState(false);
  const [roleTarget, setRoleTarget] = useState<User | null>(null);
  const [newRole, setNewRole] = useState('Guest');

  const load = () => {
    setLoading(true);
    usersApi.getAllActive().then(setUsers).finally(() => setLoading(false));
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

  const openRoleModal = (u: User) => {
    setRoleTarget(u);
    setNewRole(u.role);
    setRoleModal(true);
  };

  const saveRole = async () => {
    if (!roleTarget) return;
    await act(() => usersApi.changeRole(roleTarget.id, newRole), roleTarget.id);
    setRoleModal(false);
  };

  if (loading) return <PageSpinner />;

  return (
    <div className="p-8">
      <PageHeader title="Users" subtitle={`${users.length} active user${users.length !== 1 ? 's' : ''}`} />
      {error && <ErrorAlert message={error} />}

      <div className="card p-0 overflow-hidden">
        {users.length === 0 ? (
          <EmptyState message="No active users found." icon={<Users size={40} />} />
        ) : (
          <table className="w-full">
            <thead>
              <tr className="border-b border-hotel-border">
                {['Name', 'Email', 'Role', 'Joined', 'Actions'].map((h) => (
                  <th key={h} className="table-header text-left">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {users.map((u) => (
                <tr key={u.id} className="hover:bg-hotel-cream/40 transition-colors">
                  <td className="table-cell font-medium text-hotel-navy">
                    {u.profile ? `${u.profile.firstName} ${u.profile.lastName}` : '—'}
                  </td>
                  <td className="table-cell text-hotel-muted">{u.email}</td>
                  <td className="table-cell">
                    <span className={`badge ${roleColors[u.role] ?? 'bg-gray-100 text-gray-600'}`}>{u.role}</span>
                  </td>
                  <td className="table-cell text-hotel-muted text-xs">
                    {new Date(u.createdAt).toLocaleDateString()}
                  </td>
                  <td className="table-cell">
                    <div className="flex gap-1.5">
                      <button
                        title="Change role"
                        className="p-1.5 rounded-lg text-hotel-muted hover:text-amber-600 hover:bg-amber-50 transition-colors"
                        onClick={() => openRoleModal(u)}
                      >
                        <Shield size={14} />
                      </button>
                      <button
                        title="Delete user"
                        className="p-1.5 rounded-lg text-hotel-muted hover:text-red-500 hover:bg-red-50 transition-colors"
                        onClick={() => confirm(`Delete ${u.email}?`) && act(() => usersApi.deleteUser(u.id), u.id)}
                        disabled={actionId === u.id}
                      >
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

      <Modal title="Change User Role" open={roleModal} onClose={() => setRoleModal(false)}>
        <div className="space-y-4">
          <p className="text-hotel-muted text-sm font-body">
            Changing role for <strong className="text-hotel-navy">{roleTarget?.email}</strong>
          </p>
          <FormField label="New role">
            <select className="input-field" value={newRole} onChange={(e) => setNewRole(e.target.value)}>
              {['Staff', 'Guest', 'Housekeeping'].map((r) => (
                <option key={r} value={r}>{r}</option>
              ))}
            </select>
          </FormField>
          <div className="flex gap-3 pt-2">
            <button className="btn-secondary flex-1" onClick={() => setRoleModal(false)}>Cancel</button>
            <button className="btn-primary flex-1" onClick={saveRole}>Save</button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
