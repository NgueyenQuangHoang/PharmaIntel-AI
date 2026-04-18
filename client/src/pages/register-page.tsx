import { RegisterBranding } from '@/features/auth/components/RegisterBranding';
import { RegisterForm } from '@/features/auth/components/RegisterForm';

export function RegisterPage() {
  return (
    <div className="bg-background text-on-surface min-h-screen grid grid-cols-1 lg:grid-cols-2 font-body antialiased">
      <RegisterBranding />
      <div className="flex items-center justify-center bg-surface w-full h-full p-4">
        <RegisterForm />
      </div>
    </div>
  );
}
