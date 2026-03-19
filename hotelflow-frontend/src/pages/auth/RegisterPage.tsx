import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { Hotel, Eye, EyeOff } from 'lucide-react';
import { ErrorAlert } from '../../components/ui';

export default function RegisterPage() {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({
    email: '', password: '', firstName: '', lastName: '', phone: '',
  });
  const [showPass, setShowPass] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const set = (k: string) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm((f) => ({ ...f, [k]: e.target.value }));

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await register(form);
      navigate('/guest');
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message ?? 'Registration failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-hotel-surface p-6">
      <div className="w-full max-w-lg">
        {/* Logo */}
        <div className="flex items-center gap-3 mb-8 justify-center">
          <div className="w-10 h-10 bg-hotel-navy rounded-xl flex items-center justify-center">
            <Hotel size={20} className="text-white" />
          </div>
          <span className="font-display text-hotel-navy text-2xl">HotelFlow</span>
        </div>

        <div className="card">
          <h2 className="font-display text-hotel-navy text-2xl mb-1">Create an account</h2>
          <p className="text-hotel-muted font-body text-sm mb-6">Register as a guest to make reservations.</p>

          {error && <ErrorAlert message={error} />}

          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <label className="block text-xs font-semibold font-body text-hotel-slate uppercase tracking-wide">First name</label>
                <input className="input-field" placeholder="Jane" value={form.firstName} onChange={set('firstName')} required />
              </div>
              <div className="space-y-1.5">
                <label className="block text-xs font-semibold font-body text-hotel-slate uppercase tracking-wide">Last name</label>
                <input className="input-field" placeholder="Smith" value={form.lastName} onChange={set('lastName')} required />
              </div>
            </div>

            <div className="space-y-1.5">
              <label className="block text-xs font-semibold font-body text-hotel-slate uppercase tracking-wide">Email address</label>
              <input type="email" className="input-field" placeholder="you@example.com" value={form.email} onChange={set('email')} required />
            </div>

            <div className="space-y-1.5">
              <label className="block text-xs font-semibold font-body text-hotel-slate uppercase tracking-wide">Phone number</label>
              <input type="tel" className="input-field" placeholder="+1 234 567 8900" value={form.phone} onChange={set('phone')} required />
            </div>

            <div className="space-y-1.5">
              <label className="block text-xs font-semibold font-body text-hotel-slate uppercase tracking-wide">Password</label>
              <div className="relative">
                <input
                  type={showPass ? 'text' : 'password'}
                  className="input-field pr-10"
                  placeholder="Min. 8 characters"
                  value={form.password}
                  onChange={set('password')}
                  required
                  minLength={6}
                />
                <button type="button" onClick={() => setShowPass((v) => !v)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-hotel-muted hover:text-hotel-slate transition-colors">
                  {showPass ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              </div>
            </div>

            <button type="submit" className="btn-primary w-full py-3 text-base mt-2" disabled={loading}>
              {loading ? 'Creating account…' : 'Create account'}
            </button>
          </form>
        </div>

        <p className="mt-5 text-center text-sm font-body text-hotel-muted">
          Already have an account?{' '}
          <Link to="/login" className="text-hotel-gold hover:text-hotel-gold-light font-semibold transition-colors">
            Sign in
          </Link>
        </p>
      </div>
    </div>
  );
}
