import { BrowserRouter, Routes, Route, Outlet } from 'react-router-dom';
import { HomePage } from '@/pages/home-page';
import { DiagnosticPage } from '@/pages/diagnostic-page';
import { DiagnosticResultPage } from '@/pages/diagnostic-result-page';
import { MedicineCabinetPage } from '@/pages/medicine-cabinet-page';
import { ProfilePage } from '@/pages/profile-page';
import { LoginPage } from '@/pages/login-page';
import { RegisterPage } from '@/pages/register-page';
import { CheckoutPage } from '@/pages/checkout-page';
import { OrderDetailPage } from '@/pages/order-detail-page';
import { OrdersListPage } from '@/pages/orders-list-page';
import { AdminDashboardPage } from '@/pages/admin-dashboard-page';
import { AdminUsersPage } from '@/pages/admin-users-page';
import { AdminCategoriesPage } from '@/pages/admin-categories-page';
import { AdminMedicationsPage } from '@/pages/admin-medications-page';
import { AdminOrdersPage } from '@/pages/admin-orders-page';
import { AdminBanksPage } from '@/pages/admin-banks-page';
import { AdminPharmacistsPage } from '@/pages/admin-pharmacists-page';
import { AdminRagKnowledgePage } from '@/pages/admin-rag-knowledge-page';
import { AdminRagFeedbackPage } from '@/pages/admin-rag-feedback-page';
import { AdminRagDashboardPage } from '@/pages/admin-rag-dashboard-page';
import { PrescriptionsPage } from '@/pages/prescriptions-page';
import { MedicationRemindersPage } from '@/pages/medication-reminders-page';
import { PrescriptionDetailPage } from '@/pages/prescription-detail-page';
import { PharmacistDashboardPage } from '@/pages/pharmacist-dashboard-page';
import { PharmacistPrescriptionDetailPage } from '@/pages/pharmacist-prescription-detail-page';
import { ConsultationsPage } from '@/pages/consultations-page';
import { NotFoundPage } from '@/pages/not-found-page';
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

function PharmacistLayoutWrapper() {
  return (
    <ProtectedRoute requireRole="pharmacist">
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
          <Route path="/consultations" element={<ConsultationsPage />} />
          <Route path="/medicine" element={<MedicineCabinetPage />} />
          <Route path="/profile" element={<ProfilePage />} />
          <Route path="/checkout" element={<CheckoutPage />} />
          <Route path="/orders" element={<OrdersListPage />} />
          <Route path="/orders/:id" element={<OrderDetailPage />} />
          <Route path="/prescriptions" element={<PrescriptionsPage />} />
          <Route path="/prescriptions/:id" element={<PrescriptionDetailPage />} />
          <Route path="/medication-reminders" element={<MedicationRemindersPage />} />
        </Route>

        {/* Admin area - yeu cau role=admin */}
        <Route element={<AdminLayoutWrapper />}>
          <Route path="/admin" element={<AdminDashboardPage />} />
          <Route path="/admin/users" element={<AdminUsersPage />} />
          <Route path="/admin/categories" element={<AdminCategoriesPage />} />
          <Route path="/admin/medications" element={<AdminMedicationsPage />} />
          <Route path="/admin/orders" element={<AdminOrdersPage />} />
          <Route path="/admin/pharmacists" element={<AdminPharmacistsPage />} />
          <Route path="/admin/banks" element={<AdminBanksPage />} />
          <Route path="/admin/rag/dashboard" element={<AdminRagDashboardPage />} />
          <Route path="/admin/rag/knowledge" element={<AdminRagKnowledgePage />} />
          <Route path="/admin/rag/feedback" element={<AdminRagFeedbackPage />} />
        </Route>

        {/* Pharmacist area - yeu cau role=pharmacist */}
        <Route element={<PharmacistLayoutWrapper />}>
          <Route path="/pharmacist" element={<PharmacistDashboardPage />} />
          <Route path="/pharmacist/prescriptions/:id" element={<PharmacistPrescriptionDetailPage />} />
        </Route>

        {/* 404 Fallback */}
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </BrowserRouter>
  );
}
