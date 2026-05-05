// =============================================================================
// LoginForm - controlled form, dispatch loginThunk va navigate khi thanh cong
// =============================================================================
import { useEffect, useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { useAppDispatch } from '@/hooks/redux';
import { clearError } from '@/features/auth/auth-slice';

export function LoginForm() {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { login, status, error, isAuthenticated } = useAuth();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);

  const isSubmitting = status === 'loading';

  useEffect(() => {
    if (isAuthenticated) {
      navigate('/', { replace: true });
    }
  }, [isAuthenticated, navigate]);

  useEffect(() => {
    return () => {
      dispatch(clearError());
    };
  }, [dispatch]);

  async function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    if (isSubmitting) return;
    try {
      await login({ email, password });
    } catch {
      /* error da duoc luu trong state.auth.error */
    }
  }

  return (
    <div className="flex flex-col justify-center p-8 md:p-16 lg:px-24 bg-surface-container-lowest w-full h-full">
      {/* Mobile Logo */}
      <div className="md:hidden flex items-center gap-2 mb-8">
        <span className="material-symbols-outlined text-primary text-3xl fill-icon">biotech</span>
        <span className="font-headline font-bold text-2xl text-on-surface tracking-tighter">PharmaIntel AI</span>
      </div>

      <div className="mb-10">
        <h2 className="font-headline font-bold text-3xl text-on-surface mb-2 tracking-tight">Welcome back</h2>
        <p className="text-on-surface-variant font-body">Sign in to access your clinical workspace.</p>
      </div>

      <form className="space-y-6" onSubmit={handleSubmit} noValidate>
        <div>
          <label className="block text-sm font-medium text-on-surface mb-2 font-label" htmlFor="email">Email Address</label>
          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <span className="material-symbols-outlined text-outline">mail</span>
            </div>
            <input
              className="w-full pl-10 pr-4 py-3 bg-surface-container-low text-on-surface rounded-lg border-0 focus:ring-0 focus:bg-surface-container-lowest focus:shadow-[inset_0_0_0_2px_var(--color-primary)] transition-all duration-200 outline-none"
              id="email"
              name="email"
              placeholder="researcher@institution.edu"
              required
              type="email"
              autoComplete="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              disabled={isSubmitting}
            />
          </div>
        </div>

        <div>
          <div className="flex items-center justify-between mb-2">
            <label className="block text-sm font-medium text-on-surface font-label" htmlFor="password">Password</label>
            <a className="text-sm font-medium text-primary hover:text-primary-container transition-colors" href="#">Forgot Password?</a>
          </div>
          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <span className="material-symbols-outlined text-outline">lock</span>
            </div>
            <input
              className="w-full pl-10 pr-10 py-3 bg-surface-container-low text-on-surface rounded-lg border-0 focus:ring-0 focus:bg-surface-container-lowest focus:shadow-[inset_0_0_0_2px_var(--color-primary)] transition-all duration-200 outline-none"
              id="password"
              name="password"
              placeholder="••••••••"
              required
              type={showPassword ? 'text' : 'password'}
              autoComplete="current-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              disabled={isSubmitting}
            />
            <button
              className="absolute inset-y-0 right-0 pr-3 flex items-center text-outline hover:text-on-surface transition-colors"
              type="button"
              onClick={() => setShowPassword((v) => !v)}
              tabIndex={-1}
            >
              <span className="material-symbols-outlined">
                {showPassword ? 'visibility' : 'visibility_off'}
              </span>
            </button>
          </div>
        </div>

        {error && (
          <div className="rounded-lg bg-error-container/40 border border-error/30 px-4 py-3 text-sm text-error font-body">
            {error}
          </div>
        )}

        <button
          className="w-full py-3 px-4 bg-gradient-to-r from-primary to-primary-container text-on-primary rounded-full font-headline font-bold text-base hover:opacity-90 transition-opacity flex justify-center items-center gap-2 shadow-sm disabled:opacity-60 disabled:cursor-not-allowed"
          type="submit"
          disabled={isSubmitting}
        >
          {isSubmitting ? (
            <>
              <span className="material-symbols-outlined text-base animate-spin">progress_activity</span>
              Signing in...
            </>
          ) : (
            <>
              Login
              <span className="material-symbols-outlined text-sm">arrow_forward</span>
            </>
          )}
        </button>
      </form>

      <div className="mt-8">
        <div className="relative">
          <div className="absolute inset-0 flex items-center">
            <div className="w-full border-t border-surface-variant"></div>
          </div>
          <div className="relative flex justify-center text-sm">
            <span className="px-4 bg-surface-container-lowest text-outline font-body">Or continue with</span>
          </div>
        </div>

        <div className="mt-6 grid grid-cols-2 gap-4">
          <button className="flex justify-center items-center gap-2 py-2.5 px-4 bg-surface-container-low hover:bg-surface-variant rounded-lg text-on-surface font-medium transition-colors ghost-border" type="button" disabled>
            {/* Google SVG */}
            <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
              <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4"></path>
              <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853"></path>
              <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" fill="#FBBC05"></path>
              <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335"></path>
            </svg>
            Google
          </button>
          <button className="flex justify-center items-center gap-2 py-2.5 px-4 bg-surface-container-low hover:bg-surface-variant rounded-lg text-on-surface font-medium transition-colors ghost-border" type="button" disabled>
            <span className="material-symbols-outlined text-xl">ios</span>
            Apple
          </button>
        </div>
      </div>

      <p className="mt-10 text-center text-sm text-on-surface-variant font-body">
        Don't have an account?
        <Link className="font-medium text-primary hover:text-primary-container hover:underline transition-all ml-1" to="/register">Register</Link>
      </p>
    </div>
  );
}
