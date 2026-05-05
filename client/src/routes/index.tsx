import { BrowserRouter, Routes, Route, Outlet } from 'react-router-dom';
import { HomePage } from '@/pages/home-page';
import { DiagnosticPage } from '@/pages/diagnostic-page';
import { DiagnosticResultPage } from '@/pages/diagnostic-result-page';
import { MedicineCabinetPage } from '@/pages/medicine-cabinet-page';
import { ProfilePage } from '@/pages/profile-page';
import { LoginPage } from '@/pages/login-page';
import { RegisterPage } from '@/pages/register-page';
import { CheckoutPage } from '@/pages/checkout-page';
import { AdminDashboardPage } from '@/pages/admin-dashboard-page';
import { AdminUsersPage } from '@/pages/admin-users-page';
import { AdminCategoriesPage } from '@/pages/admin-categories-page';
import { AdminMedicationsPage } from '@/pages/admin-medications-page';
import { AdminOrdersPage } from '@/pages/admin-orders-page';
import { MainLayout } from '@/layouts/MainLayout';
import { ProtectedRoute } from '@/components/ProtectedRoute';

function ProtectedLayoutWrapper() {
  return (
    <ProtectedRoute>
      <MainLayout>
        <Outlet />
      </MainLayout>
    </ProtectedRoute>
  );
}

function AdminLayoutWrapper() {
  return (
    <ProtectedRoute requireRole="admin">
      <MainLayout>
        <Outlet />
      </MainLayout>
    </ProtectedRoute>
  );
}

export function AppRoutes() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Isolated route for Login without global layout */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />

        {/* Global layout - chi truy cap khi da dang nhap */}
        <Route element={<ProtectedLayoutWrapper />}>
          <Route path="/" element={<HomePage />} />
          <Route path="/diagnostic" element={<DiagnosticPage />} />
          <Route path="/diagnostic/result" element={<DiagnosticResultPage />} />
          <Route path="/medicine" element={<MedicineCabinetPage />} />
          <Route path="/profile" element={<ProfilePage />} />
          <Route path="/checkout" element={<CheckoutPage />} />
        </Route>

        {/* Admin area - yeu cau role=admin */}
        <Route element={<AdminLayoutWrapper />}>
          <Route path="/admin" element={<AdminDashboardPage />} />
          <Route path="/admin/users" element={<AdminUsersPage />} />
          <Route path="/admin/categories" element={<AdminCategoriesPage />} />
          <Route path="/admin/medications" element={<AdminMedicationsPage />} />
          <Route path="/admin/orders" element={<AdminOrdersPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
