// =============================================================================
// LoginForm - controlled form, dispatch loginThunk va navigate khi thanh cong
// =============================================================================
import { useEffect, useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { GoogleLogin } from '@react-oauth/google';
import { useAuth } from '@/hooks/useAuth';
import { useAppDispatch } from '@/hooks/redux';
import { clearError } from '@/features/auth/auth-slice';

export function LoginForm() {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { login, loginWithGoogle, status, error, isAuthenticated } = useAuth();

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

        <div className="mt-6 flex justify-center w-full">
          {/* GoogleLogin: tu render nut chinh chu cua Google, callback tra
              "credential" la ID Token JWT - day len BE qua /api/auth/google. */}
          <div className="flex justify-center w-full max-w-[400px]">
            <GoogleLogin
              onSuccess={async (resp) => {
                if (!resp.credential) return;
                try {
                  await loginWithGoogle(resp.credential);
                } catch {
                  /* error da duoc luu trong state.auth.error */
                }
              }}
              onError={() => {
                /* user huy / Google tu choi - state.error giu nguyen */
              }}
              theme="outline"
              size="large"
              text="continue_with"
              shape="rectangular"
            />
          </div>
        </div>
      </div>

      <p className="mt-10 text-center text-sm text-on-surface-variant font-body">
        Don't have an account?
        <Link className="font-medium text-primary hover:text-primary-container hover:underline transition-all ml-1" to="/register">Register</Link>
      </p>
    </div>
  );
}
