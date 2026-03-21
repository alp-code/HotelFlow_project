import { useEffect, useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import api from '../../api/client';
import { User } from '../../types';
import { PageHeader, ErrorAlert, FormField, Spinner } from '../../components/ui';
import { UserCircle, Save, Mail, Phone, BadgeCheck } from 'lucide-react';

const roleColors: Record<string, string> = {
  Staff: 'bg-amber-100 text-amber-800',
  Guest: 'bg-green-100 text-green-800',
  Housekeeping: 'bg-blue-100 text-blue-800',
};

export default function ProfilePage() {
  const { email, role } = useAuth();
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    phone: '',
  });

  useEffect(() => {
    api.get<User>('/api/users/me')
      .then((r) => {
        setUser(r.data);
        setForm({
          firstName: r.data.profile?.firstName ?? '',
          lastName: r.data.profile?.lastName ?? '',
          phone: r.data.profile?.phone ?? '',
        });
      })
      .catch(() => setError('Failed to load profile.'))
      .finally(() => setLoading(false));
  }, []);

  const handleSave = async () => {
    setSaving(true);
    setError('');
    setSuccess('');
    try {
      await api.put('/api/users/me', form);
      setSuccess('Profile updated successfully!');
      // Refresh user data
      const r = await api.get<User>('/api/users/me');
      setUser(r.data);
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } };
      setError(e.response?.data?.message ?? 'Failed to update profile.');
    } finally {
      setSaving(false);
    }
  };

  const initials = form.firstName && form.lastName
    ? `${form.firstName[0]}${form.lastName[0]}`.toUpperCase()
    : email?.[0]?.toUpperCase() ?? '?';

  return (
    <div className="p-8 max-w-2xl">
      <PageHeader title="My Profile" subtitle="View and update your personal information" />

      {loading ? (
        <div className="flex justify-center py-16"><Spinner size="lg" /></div>
      ) : (
        <div className="space-y-6">
          {/* Avatar & role card */}
          <div className="card flex items-center gap-6">
            <div className="w-20 h-20 rounded-2xl bg-hotel-navy flex items-center justify-center flex-shrink-0">
              <span className="font-display text-white text-2xl">{initials}</span>
            </div>
            <div>
              <h2 className="font-display text-hotel-navy text-xl">
                {form.firstName && form.lastName
                  ? `${form.firstName} ${form.lastName}`
                  : 'No name set'}
              </h2>
              <p className="text-hotel-muted font-body text-sm mt-0.5 flex items-center gap-1.5">
                <Mail size={13} /> {email}
              </p>
              <div className="flex items-center gap-2 mt-2">
                <span className={`badge ${roleColors[role ?? ''] ?? 'bg-gray-100 text-gray-600'}`}>
                  <BadgeCheck size={11} className="mr-1" />{role}
                </span>
                {user?.createdAt && (
                  <span className="text-hotel-muted text-xs font-body">
                    Member since {new Date(user.createdAt).toLocaleDateString()}
                  </span>
                )}
              </div>
            </div>
          </div>

          {/* Edit form */}
          <div className="card space-y-4">
            <h3 className="font-display text-hotel-navy text-lg border-b border-hotel-border pb-3">
              Personal Information
            </h3>

            {error && <ErrorAlert message={error} />}

            {success && (
              <div className="bg-emerald-50 border border-emerald-200 text-emerald-700 rounded-xl px-4 py-3 text-sm font-body">
                {success}
              </div>
            )}

            <div className="grid grid-cols-2 gap-4">
              <FormField label="First name">
                <input
                  className="input-field"
                  placeholder="Jane"
                  value={form.firstName}
                  onChange={(e) => setForm((f) => ({ ...f, firstName: e.target.value }))}
                />
              </FormField>
              <FormField label="Last name">
                <input
                  className="input-field"
                  placeholder="Smith"
                  value={form.lastName}
                  onChange={(e) => setForm((f) => ({ ...f, lastName: e.target.value }))}
                />
              </FormField>
            </div>

            <FormField label="Email address">
              <div className="relative">
                <input
                  className="input-field opacity-60 cursor-not-allowed"
                  value={email ?? ''}
                  disabled
                />
                <span className="absolute right-3 top-1/2 -translate-y-1/2 text-xs text-hotel-muted font-body">
                  Cannot be changed
                </span>
              </div>
            </FormField>

            <FormField label="Phone number">
              <div className="relative">
                <Phone size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-hotel-muted" />
                <input
                  className="input-field pl-9"
                  placeholder="+1 234 567 8900"
                  value={form.phone}
                  onChange={(e) => setForm((f) => ({ ...f, phone: e.target.value }))}
                />
              </div>
            </FormField>

            <div className="pt-2">
              <button
                className="btn-primary flex items-center gap-2"
                onClick={handleSave}
                disabled={saving}
              >
                {saving ? <Spinner size="sm" /> : <Save size={15} />}
                {saving ? 'Saving…' : 'Save Changes'}
              </button>
            </div>
          </div>

          {/* Account info */}
          <div className="card space-y-3">
            <h3 className="font-display text-hotel-navy text-lg border-b border-hotel-border pb-3">
              Account Information
            </h3>
            {[
              ['User ID', user?.id?.slice(0, 8) + '…'],
              ['Role', role],
              ['Account status', 'Active'],
            ].map(([k, v]) => (
              <div key={String(k)} className="flex justify-between text-sm font-body py-1.5 border-b border-hotel-border/40">
                <span className="text-hotel-muted">{k}</span>
                <span className="font-semibold text-hotel-navy">{v}</span>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
