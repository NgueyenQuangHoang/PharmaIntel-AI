import { LoginBranding } from '@/features/auth/components/LoginBranding';
import { LoginForm } from '@/features/auth/components/LoginForm';

export function LoginPage() {
  return (
    <div className="min-h-screen flex flex-col antialiased selection:bg-primary-container selection:text-on-primary-container bg-background text-on-background">
      <main className="flex-grow flex items-center justify-center p-4 md:p-8">
        <div className="w-full max-w-6xl grid grid-cols-1 md:grid-cols-2 bg-surface-container-lowest rounded-xl ambient-shadow ghost-border overflow-hidden">
          <LoginBranding />
          <LoginForm />
        </div>
      </main>

      <footer className="bg-surface-container-low text-on-surface-variant py-6 border-t border-surface-variant/50 w-full mt-auto">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 flex flex-col md:flex-row justify-between items-center gap-4 text-sm">
          <div>© 2024 PharmaIntel AI. All rights reserved.</div>
          <div className="flex gap-6">
            <a className="hover:text-primary transition-colors" href="#">Privacy Policy</a>
            <a className="hover:text-primary transition-colors" href="#">Terms of Service</a>
            <a className="hover:text-primary transition-colors" href="#">Contact Support</a>
          </div>
        </div>
      </footer>
    </div>
  );
}
