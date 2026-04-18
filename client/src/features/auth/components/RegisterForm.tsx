import { Link } from 'react-router-dom';

export function RegisterForm() {
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
        <form className="space-y-5">
          {/* Full Name Field */}
          <div className="space-y-1.5">
            <label className="block font-label text-sm font-medium text-on-surface" htmlFor="fullName">Full Name</label>
            <div className="relative flex items-center input-ghost-border rounded-lg bg-surface-container-low transition-colors duration-200">
              <span className="material-symbols-outlined absolute left-3 text-outline text-xl">person</span>
              <input className="w-full bg-transparent border-none py-2.5 pl-10 pr-4 text-on-surface text-sm font-body rounded-lg focus:ring-0 placeholder-outline-variant outline-none" id="fullName" name="fullName" placeholder="Dr. Jane Doe" required type="text" />
            </div>
          </div>

          {/* Email Field */}
          <div className="space-y-1.5">
            <label className="block font-label text-sm font-medium text-on-surface" htmlFor="email">Email Address</label>
            <div className="relative flex items-center input-ghost-border rounded-lg bg-surface-container-low transition-colors duration-200">
              <span className="material-symbols-outlined absolute left-3 text-outline text-xl">mail</span>
              <input className="w-full bg-transparent border-none py-2.5 pl-10 pr-4 text-on-surface text-sm font-body rounded-lg focus:ring-0 placeholder-outline-variant outline-none" id="email" name="email" placeholder="jane.doe@research.org" required type="email" />
            </div>
          </div>

          {/* Password Grid */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {/* Password Field */}
            <div className="space-y-1.5">
              <label className="block font-label text-sm font-medium text-on-surface" htmlFor="password">Password</label>
              <div className="relative flex items-center input-ghost-border rounded-lg bg-surface-container-low transition-colors duration-200">
                <span className="material-symbols-outlined absolute left-3 text-outline text-xl">lock</span>
                <input className="w-full bg-transparent border-none py-2.5 pl-10 pr-10 text-on-surface text-sm font-body rounded-lg focus:ring-0 placeholder-outline-variant outline-none" id="password" name="password" placeholder="••••••••" required type="password" />
              </div>
            </div>

            {/* Confirm Password Field */}
            <div className="space-y-1.5">
              <label className="block font-label text-sm font-medium text-on-surface" htmlFor="confirmPassword">Confirm Password</label>
              <div className="relative flex items-center input-ghost-border rounded-lg bg-surface-container-low transition-colors duration-200">
                <span className="material-symbols-outlined absolute left-3 text-outline text-xl">lock_reset</span>
                <input className="w-full bg-transparent border-none py-2.5 pl-10 pr-4 text-on-surface text-sm font-body rounded-lg focus:ring-0 placeholder-outline-variant outline-none" id="confirmPassword" name="confirmPassword" placeholder="••••••••" required type="password" />
              </div>
            </div>
          </div>
          <p className="font-body text-xs text-on-surface-variant ml-1 -mt-2 block">Must be at least 8 chars.</p>

          {/* Terms Checkbox */}
          <div className="flex items-start mt-4">
            <div className="flex items-center h-5">
              <input className="w-4 h-4 rounded border-outline-variant bg-surface-container-low text-primary focus:ring-primary focus:ring-offset-background transition-colors duration-200 cursor-pointer" id="terms" name="terms" required type="checkbox" />
            </div>
            <div className="ml-3 text-xs">
              <label className="font-body text-on-surface-variant cursor-pointer" htmlFor="terms">
                I agree to the <a className="text-primary hover:text-primary-container transition-colors duration-200 font-medium" href="#">Terms &amp; Conditions</a> and <a className="text-primary hover:text-primary-container transition-colors duration-200 font-medium" href="#">Privacy Policy</a>.
              </label>
            </div>
          </div>

          {/* Submit Button */}
          <div className="pt-2">
            <button className="w-full flex justify-center py-2.5 px-4 rounded-full text-on-primary text-sm font-label font-medium bg-gradient-to-r from-primary to-primary-container hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary focus:ring-offset-background transition-all duration-300 shadow-sm" type="submit">
              Create Account
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
