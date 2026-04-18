import { BrowserRouter, Routes, Route, Outlet } from 'react-router-dom';
import { HomePage } from '@/pages/home-page';
import { DiagnosticPage } from '@/pages/diagnostic-page';
import { DiagnosticResultPage } from '@/pages/diagnostic-result-page';
import { MedicineCabinetPage } from '@/pages/medicine-cabinet-page';
import { ProfilePage } from '@/pages/profile-page';
import { LoginPage } from '@/pages/login-page';
import { RegisterPage } from '@/pages/register-page';
import { MainLayout } from '@/layouts/MainLayout';

function MainLayoutWrapper() {
  return (
    <MainLayout>
      <Outlet />
    </MainLayout>
  );
}

export function AppRoutes() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Isolated route for Login without global layout */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />

        {/* Global layout for the rest of the application */}
        <Route element={<MainLayoutWrapper />}>
          <Route path="/" element={<HomePage />} />
          <Route path="/diagnostic" element={<DiagnosticPage />} />
          <Route path="/diagnostic/result" element={<DiagnosticResultPage />} />
          <Route path="/medicine" element={<MedicineCabinetPage />} />
          <Route path="/profile" element={<ProfilePage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
