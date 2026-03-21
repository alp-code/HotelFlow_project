import { useEffect, useRef, useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import api from '../../api/client';
import { User } from '../../types';
import { PageHeader, ErrorAlert, FormField, Spinner } from '../../components/ui';
import { Save, Phone, BadgeCheck, Camera, KeyRound, Eye, EyeOff } from 'lucide-react';

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
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    phone: '',
    gender: '',
    address: '',
    profilePicture: '',
  });

  const [pwForm, setPwForm] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  });
  const [showCurrent, setShowCurrent] = useState(false);
  const [showNew, setShowNew] = useState(false);
  const [pwError, setPwError] = useState('');
  const [pwSuccess, setPwSuccess] = useState('');
  const [pwSaving, setPwSaving] = useState(false);

  const loadProfile = () => {
    api.get<User>('/api/users/me')
      .then((r) => {
        setUser(r.data);
        setForm({
          firstName: r.data.profile?.firstName ?? '',
          lastName: r.data.profile?.lastName ?? '',
          phone: r.data.profile?.phone ?? '',
          gender: (r.data.profile as any)?.gender ?? '',
          address: (r.data.profile as any)?.address ?? '',
          profilePicture: (r.data.profile as any)?.profilePicture ?? '',
        });
      })
      .catch(() => setError('Failed to load profile.'))
      .finally(() => setLoading(false));
  };

  useEffect(() => { loadProfile(); }, []);

  const handleSave = async () => {
    setSaving(true);
    setError('');
    setSuccess('');
    try {
      await api.put('/api/users/me', form);
      setSuccess('Profile updated successfully!');
      loadProfile();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } };
      setError(e.response?.data?.message ?? 'Failed to update profile.');
    } finally {
      setSaving(false);
    }
  };

  const handlePasswordChange = async () => {
    setPwError('');
    setPwSuccess('');
    if (pwForm.newPassword !== pwForm.confirmPassword) {
      setPwError('New passwords do not match.');
      return;
    }
    if (pwForm.newPassword.length < 6) {
      setPwError('New password must be at least 6 characters.');
      return;
    }
    setPwSaving(true);
    try {
      await api.put('/api/users/me/password', {
        currentPassword: pwForm.currentPassword,
        newPassword: pwForm.newPassword,
      });
      setPwSuccess('Password changed successfully!');
      setPwForm({ currentPassword: '', newPassword: '', confirmPassword: '' });
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } };
      setPwError(e.response?.data?.message ?? 'Failed to change password.');
    } finally {
      setPwSaving(false);
    }
  };

  const handleImageUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    if (file.size > 2 * 1024 * 1024) {
      setError('Image must be under 2MB.');
      return;
    }
    const reader = new FileReader();
    reader.onload = () => {
      setForm((f) => ({ ...f, profilePicture: reader.result as string }));
    };
    reader.readAsDataURL(file);
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
            <div className="relative flex-shrink-0">
              {form.profilePicture ? (
                <img
                  src={form.profilePicture}
                  alt="Profile"
                  className="w-20 h-20 rounded-2xl object-cover"
                />
              ) : (
                <div className="w-20 h-20 rounded-2xl bg-hotel-navy flex items-center justify-center">
                  <span className="font-display text-white text-2xl">{initials}</span>
                </div>
              )}
              <button
                onClick={() => fileInputRef.current?.click()}
                className="absolute -bottom-2 -right-2 w-7 h-7 bg-hotel-gold rounded-full flex items-center justify-center hover:bg-hotel-gold-light transition-colors shadow-sm"
                title="Change photo"
              >
                <Camera size={13} className="text-white" />
              </button>
              <input
                ref={fileInputRef}
                type="file"
                accept="image/*"
                className="hidden"
                onChange={handleImageUpload}
              />
            </div>
            <div>
              <h2 className="font-display text-hotel-navy text-xl">
                {form.firstName && form.lastName
                  ? `${form.firstName} ${form.lastName}`
                  : 'No name set'}
              </h2>
              <p className="text-hotel-muted font-body text-sm mt-0.5">{email}</p>
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

          {/* Personal info form */}
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
                <input className="input-field" placeholder="Jane"
                  value={form.firstName}
                  onChange={(e) => setForm((f) => ({ ...f, firstName: e.target.value }))} />
              </FormField>
              <FormField label="Last name">
                <input className="input-field" placeholder="Smith"
                  value={form.lastName}
                  onChange={(e) => setForm((f) => ({ ...f, lastName: e.target.value }))} />
              </FormField>
            </div>

            <FormField label="Email address">
              <input className="input-field opacity-60 cursor-not-allowed" value={email ?? ''} disabled />
            </FormField>

            <div className="grid grid-cols-2 gap-4">
              <FormField label="Phone number">
                <div className="relative">
                  <Phone size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-hotel-muted" />
                  <input className="input-field pl-9" placeholder="+1 234 567 8900"
                    value={form.phone}
                    onChange={(e) => setForm((f) => ({ ...f, phone: e.target.value }))} />
                </div>
              </FormField>
              <FormField label="Gender">
                <select className="input-field" value={form.gender}
                  onChange={(e) => setForm((f) => ({ ...f, gender: e.target.value }))}>
                  <option value="">Prefer not to say</option>
                  <option value="Male">Male</option>
                  <option value="Female">Female</option>
                  <option value="Other">Other</option>
                </select>
              </FormField>
            </div>

            <FormField label="Address">
              <input className="input-field" placeholder="123 Main St, City, Country"
                value={form.address}
                onChange={(e) => setForm((f) => ({ ...f, address: e.target.value }))} />
            </FormField>

            <div className="pt-2">
              <button className="btn-primary flex items-center gap-2" onClick={handleSave} disabled={saving}>
                {saving ? <Spinner size="sm" /> : <Save size={15} />}
                {saving ? 'Saving…' : 'Save Changes'}
              </button>
            </div>
          </div>

          {/* Change password */}
          <div className="card space-y-4">
            <h3 className="font-display text-hotel-navy text-lg border-b border-hotel-border pb-3 flex items-center gap-2">
              <KeyRound size={18} className="text-hotel-gold" />
              Change Password
            </h3>

            {pwError && <ErrorAlert message={pwError} />}
            {pwSuccess && (
              <div className="bg-emerald-50 border border-emerald-200 text-emerald-700 rounded-xl px-4 py-3 text-sm font-body">
                {pwSuccess}
              </div>
            )}

            <FormField label="Current password">
              <div className="relative">
                <input
                  type={showCurrent ? 'text' : 'password'}
                  className="input-field pr-10"
                  placeholder="••••••••"
                  value={pwForm.currentPassword}
                  onChange={(e) => setPwForm((f) => ({ ...f, currentPassword: e.target.value }))}
                />
                <button type="button" onClick={() => setShowCurrent((v) => !v)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-hotel-muted hover:text-hotel-slate">
                  {showCurrent ? <EyeOff size={15} /> : <Eye size={15} />}
                </button>
              </div>
            </FormField>

            <div className="grid grid-cols-2 gap-4">
              <FormField label="New password">
                <div className="relative">
                  <input
                    type={showNew ? 'text' : 'password'}
                    className="input-field pr-10"
                    placeholder="Min. 6 characters"
                    value={pwForm.newPassword}
                    onChange={(e) => setPwForm((f) => ({ ...f, newPassword: e.target.value }))}
                  />
                  <button type="button" onClick={() => setShowNew((v) => !v)}
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-hotel-muted hover:text-hotel-slate">
                    {showNew ? <EyeOff size={15} /> : <Eye size={15} />}
                  </button>
                </div>
              </FormField>
              <FormField label="Confirm new password">
                <input
                  type="password"
                  className="input-field"
                  placeholder="Repeat new password"
                  value={pwForm.confirmPassword}
                  onChange={(e) => setPwForm((f) => ({ ...f, confirmPassword: e.target.value }))}
                />
              </FormField>
            </div>

            <div className="pt-2">
              <button className="btn-primary flex items-center gap-2" onClick={handlePasswordChange} disabled={pwSaving}>
                {pwSaving ? <Spinner size="sm" /> : <KeyRound size={15} />}
                {pwSaving ? 'Changing…' : 'Change Password'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
