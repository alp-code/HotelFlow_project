import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { Hotel, Eye, EyeOff } from 'lucide-react';
import { ErrorAlert } from '../../components/ui';

export default function LoginPage() {
  const { login, role } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPass, setShowPass] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await login({ email, password });
      // role is set after login; read from localStorage fallback
      const token = localStorage.getItem('accessToken');
      if (token) {
        const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
        const r =
          payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
          payload['role'];
        if (r === 'Staff') navigate('/staff');
        else if (r === 'Housekeeping') navigate('/housekeeping');
        else navigate('/guest');
      }
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message ?? 'Invalid email or password.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex">
      {/* Left panel – decorative */}
      <div className="hidden lg:flex lg:w-1/2 bg-hotel-navy flex-col justify-between p-12 relative overflow-hidden">
        <div className="absolute inset-0 opacity-5"
          style={{
            backgroundImage: `radial-gradient(circle at 20% 50%, #C8952A 0%, transparent 60%),
                              radial-gradient(circle at 80% 20%, #C8952A 0%, transparent 50%)`,
          }}
        />
        <div className="relative flex items-center gap-3">
          <div className="w-10 h-10 bg-hotel-gold rounded-xl flex items-center justify-center">
            <Hotel size={20} className="text-white" />
          </div>
          <span className="text-white font-display text-2xl">HotelFlow</span>
        </div>
        <div className="relative">
          <blockquote className="font-display text-white/90 text-3xl leading-snug mb-6">
            "Excellence in every<br />stay, efficiency in<br />every process."
          </blockquote>
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-full bg-hotel-gold/20 flex items-center justify-center">
              <span className="text-hotel-gold text-sm font-semibold">HF</span>
            </div>
            <div>
              <p className="text-white text-sm font-body font-medium">Hotel Management Suite</p>
              <p className="text-white/40 text-xs font-body">v1.0 · Powered by ASP.NET Core</p>
            </div>
          </div>
        </div>
        <div className="relative flex gap-6">
          {['Reservations', 'Housekeeping', 'Staff'].map((f) => (
            <div key={f} className="text-center">
              <p className="text-white/30 text-xs font-body uppercase tracking-wider">{f}</p>
            </div>
          ))}
        </div>
      </div>

      {/* Right panel – form */}
      <div className="flex-1 flex items-center justify-center p-8 bg-hotel-surface">
        <div className="w-full max-w-md">
          <div className="lg:hidden flex items-center gap-3 mb-8">
            <div className="w-9 h-9 bg-hotel-navy rounded-xl flex items-center justify-center">
              <Hotel size={18} className="text-white" />
            </div>
            <span className="font-display text-hotel-navy text-xl">HotelFlow</span>
          </div>

          <h2 className="font-display text-hotel-navy text-3xl mb-1">Welcome back</h2>
          <p className="text-hotel-muted font-body text-sm mb-8">Sign in to your account to continue.</p>

          {error && <ErrorAlert message={error} />}

          <form onSubmit={handleSubmit} className="space-y-5">
            <div className="space-y-1.5">
              <label className="block text-xs font-semibold font-body text-hotel-slate uppercase tracking-wide">
                Email address
              </label>
              <input
                type="email"
                className="input-field"
                placeholder="you@example.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                autoFocus
              />
            </div>

            <div className="space-y-1.5">
              <label className="block text-xs font-semibold font-body text-hotel-slate uppercase tracking-wide">
                Password
              </label>
              <div className="relative">
                <input
                  type={showPass ? 'text' : 'password'}
                  className="input-field pr-10"
                  placeholder="••••••••"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                />
                <button
                  type="button"
                  onClick={() => setShowPass((v) => !v)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-hotel-muted hover:text-hotel-slate transition-colors"
                >
                  {showPass ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              </div>
            </div>

            <button type="submit" className="btn-primary w-full py-3 text-base" disabled={loading}>
              {loading ? 'Signing in…' : 'Sign in'}
            </button>
          </form>

          <p className="mt-6 text-center text-sm font-body text-hotel-muted">
            Don't have an account?{' '}
            <Link to="/register" className="text-hotel-gold hover:text-hotel-gold-light font-semibold transition-colors">
              Create one
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}
