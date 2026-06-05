// =============================================================================
// RegisterForm - controlled form, dispatch registerThunk, validate confirm pwd
// =============================================================================
import { useEffect, useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { useAppDispatch } from '@/hooks/redux';
import { clearError } from '@/features/auth/auth-slice';

export function RegisterForm() {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { register, status, error, isAuthenticated } = useAuth();

  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [terms, setTerms] = useState(false);
  const [localError, setLocalError] = useState<string | null>(null);

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
    setLocalError(null);

    if (password.length < 8) {
      setLocalError('Mat khau toi thieu 8 ky tu.');
      return;
    }
    if (!/[A-Z]/.test(password) || !/[a-z]/.test(password) || !/\d/.test(password)) {
      setLocalError('Mat khau phai co it nhat 1 chu HOA, 1 chu thuong va 1 chu so.');
      return;
    }
    if (password !== confirmPassword) {
      setLocalError('Xac nhan mat khau khong khop.');
      return;
    }
    if (!terms) {
      setLocalError('Ban can dong y dieu khoan de tiep tuc.');
      return;
    }

    try {
      await register({ fullName, email, password, isTermsAccepted: terms });
    } catch {
      /* error trong state.auth.error */
    }
  }

  const displayError = localError ?? error;

  return (
    <div className="flex flex-col justify-center p-6 md:p-10 lg:p-16 bg-surface w-full h-full">
      <div className="w-full max-w-md mx-auto">
        {/* Mobile Brand Header */}
        <div className="lg:hidden mb-8 text-center">
          <div className="text-2xl font-headline font-extrabold tracking-tighter text-primary">PharmaIntel AI</div>
        </div>

        {/* Form Header */}
        <div className="mb-8 text-center lg:text-left">
          <h1 className="font-headline text-2xl font-bold text-on-surface tracking-tight mb-2">Create Account</h1>
          <p className="font-body text-sm text-on-surface-variant">Enter your details to access the clinical intelligence platform.</p>
        </div>

        {/* Sign Up Form */}
        <form className="space-y-5" onSubmit={handleSubmit} noValidate>
          {/* Full Name Field */}
          <div className="space-y-1.5">
            <label className="block font-label text-sm font-medium text-on-surface" htmlFor="fullName">Full Name</label>
            <div className="relative flex items-center input-ghost-border rounded-lg bg-surface-container-low transition-colors duration-200">
              <span className="material-symbols-outlined absolute left-3 text-outline text-xl">person</span>
              <input
                className="w-full bg-transparent border-none py-2.5 pl-10 pr-4 text-on-surface text-sm font-body rounded-lg focus:ring-0 placeholder-outline-variant outline-none"
                id="fullName"
                name="fullName"
                placeholder="Dr. Jane Doe"
                required
                type="text"
                autoComplete="name"
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                disabled={isSubmitting}
              />
            </div>
          </div>

          {/* Email Field */}
          <div className="space-y-1.5">
            <label className="block font-label text-sm font-medium text-on-surface" htmlFor="email">Email Address</label>
            <div className="relative flex items-center input-ghost-border rounded-lg bg-surface-container-low transition-colors duration-200">
              <span className="material-symbols-outlined absolute left-3 text-outline text-xl">mail</span>
              <input
                className="w-full bg-transparent border-none py-2.5 pl-10 pr-4 text-on-surface text-sm font-body rounded-lg focus:ring-0 placeholder-outline-variant outline-none"
                id="email"
                name="email"
                placeholder="jane.doe@research.org"
                required
                type="email"
                autoComplete="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                disabled={isSubmitting}
              />
            </div>
          </div>

          {/* Password Grid */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {/* Password Field */}
            <div className="space-y-1.5">
              <label className="block font-label text-sm font-medium text-on-surface" htmlFor="password">Password</label>
              <div className="relative flex items-center input-ghost-border rounded-lg bg-surface-container-low transition-colors duration-200">
                <span className="material-symbols-outlined absolute left-3 text-outline text-xl">lock</span>
                <input
                  className="w-full bg-transparent border-none py-2.5 pl-10 pr-10 text-on-surface text-sm font-body rounded-lg focus:ring-0 placeholder-outline-variant outline-none"
                  id="password"
                  name="password"
                  placeholder="••••••••"
                  required
                  type="password"
                  autoComplete="new-password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  disabled={isSubmitting}
                />
              </div>
            </div>

            {/* Confirm Password Field */}
            <div className="space-y-1.5">
              <label className="block font-label text-sm font-medium text-on-surface" htmlFor="confirmPassword">Confirm Password</label>
              <div className="relative flex items-center input-ghost-border rounded-lg bg-surface-container-low transition-colors duration-200">
                <span className="material-symbols-outlined absolute left-3 text-outline text-xl">lock_reset</span>
                <input
                  className="w-full bg-transparent border-none py-2.5 pl-10 pr-4 text-on-surface text-sm font-body rounded-lg focus:ring-0 placeholder-outline-variant outline-none"
                  id="confirmPassword"
                  name="confirmPassword"
                  placeholder="••••••••"
                  required
                  type="password"
                  autoComplete="new-password"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  disabled={isSubmitting}
                />
              </div>
            </div>
          </div>
          <p className="font-body text-xs text-on-surface-variant ml-1 -mt-2 block">At least 8 chars, including 1 uppercase, 1 lowercase &amp; 1 number.</p>

          {/* Terms Checkbox */}
          <div className="flex items-start mt-4">
            <div className="flex items-center h-5">
              <input
                className="w-4 h-4 rounded border-outline-variant bg-surface-container-low text-primary focus:ring-primary focus:ring-offset-background transition-colors duration-200 cursor-pointer"
                id="terms"
                name="terms"
                required
                type="checkbox"
                checked={terms}
                onChange={(e) => setTerms(e.target.checked)}
                disabled={isSubmitting}
              />
            </div>
            <div className="ml-3 text-xs">
              <label className="font-body text-on-surface-variant cursor-pointer" htmlFor="terms">
                I agree to the <a className="text-primary hover:text-primary-container transition-colors duration-200 font-medium" href="#">Terms &amp; Conditions</a> and <a className="text-primary hover:text-primary-container transition-colors duration-200 font-medium" href="#">Privacy Policy</a>.
              </label>
            </div>
          </div>

          {displayError && (
            <div className="rounded-lg bg-error-container/40 border border-error/30 px-4 py-3 text-sm text-error font-body">
              {displayError}
            </div>
          )}

          {/* Submit Button */}
          <div className="pt-2">
            <button
              className="w-full flex justify-center items-center gap-2 py-2.5 px-4 rounded-full text-on-primary text-sm font-label font-medium bg-gradient-to-r from-primary to-primary-container hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary focus:ring-offset-background transition-all duration-300 shadow-sm disabled:opacity-60 disabled:cursor-not-allowed"
              type="submit"
              disabled={isSubmitting}
            >
              {isSubmitting ? (
                <>
                  <span className="material-symbols-outlined text-base animate-spin">progress_activity</span>
                  Creating account...
                </>
              ) : (
                'Create Account'
              )}
            </button>
          </div>
        </form>

        {/* Login Redirect */}
        <div className="mt-6 text-center text-sm font-body">
          <span className="text-on-surface-variant">Already have an account?</span>
          <Link className="font-medium text-primary hover:text-primary-container transition-colors duration-200 ml-1" to="/login">Sign In</Link>
        </div>
      </div>
    </div>
  );
}
