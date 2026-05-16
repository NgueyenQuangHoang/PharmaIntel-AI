// =============================================================================
// AdminBanksPage - Quản lý tài khoản ngân hàng để khách hàng chuyển khoản.
// (Giao diện Frontend mock data, cần kết nối API Backend sau)
// =============================================================================
import { useState } from 'react';
import { AdminPageShell } from '@/features/admin/components/AdminPageShell';

type BankAccount = {
  id: string;
  bankName: string;
  accountNumber: string;
  accountName: string;
  branch: string;
  isActive: boolean;
};

// Mock data ban đầu
const MOCK_BANKS: BankAccount[] = [
  {
    id: '1',
    bankName: 'Vietcombank',
    accountNumber: '1012345678',
    accountName: 'NGUYEN VAN A',
    branch: 'Chi nhánh Tân Bình',
    isActive: true,
  },
  {
    id: '2',
    bankName: 'Techcombank',
    accountNumber: '1903123456789',
    accountName: 'NGUYEN VAN A',
    branch: 'Chi nhánh Quận 10',
    isActive: false,
  },
];

export function AdminBanksPage() {
  const [banks, setBanks] = useState<BankAccount[]>(MOCK_BANKS);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingBank, setEditingBank] = useState<BankAccount | null>(null);

  // Form state
  const [formData, setFormData] = useState({
    bankName: '',
    accountNumber: '',
    accountName: '',
    branch: '',
    isActive: true,
  });

  const handleOpenModal = (bank?: BankAccount) => {
    if (bank) {
      setEditingBank(bank);
      setFormData({
        bankName: bank.bankName,
        accountNumber: bank.accountNumber,
        accountName: bank.accountName,
        branch: bank.branch,
        isActive: bank.isActive,
      });
    } else {
      setEditingBank(null);
      setFormData({
        bankName: '',
        accountNumber: '',
        accountName: '',
        branch: '',
        isActive: true,
      });
    }
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingBank(null);
  };

  const handleSave = (e: React.FormEvent) => {
    e.preventDefault();
    if (editingBank) {
      // Cập nhật
      setBanks((prev) =>
        prev.map((b) => (b.id === editingBank.id ? { ...b, ...formData } : b))
      );
    } else {
      // Thêm mới
      const newBank: BankAccount = {
        id: Math.random().toString(36).substring(7),
        ...formData,
      };
      setBanks((prev) => [...prev, newBank]);
    }
    handleCloseModal();
  };

  const handleDelete = (id: string) => {
    if (window.confirm('Bạn có chắc chắn muốn xóa tài khoản này không?')) {
      setBanks((prev) => prev.filter((b) => b.id !== id));
    }
  };

  const toggleStatus = (id: string) => {
    setBanks((prev) =>
      prev.map((b) => (b.id === id ? { ...b, isActive: !b.isActive } : b))
    );
  };

  return (
    <AdminPageShell
      title="Quản lý Ngân hàng"
      description="Quản lý danh sách tài khoản ngân hàng dùng để nhận chuyển khoản từ khách hàng."
      actions={
        <button
          onClick={() => handleOpenModal()}
          className="flex items-center gap-2 bg-primary text-on-primary px-4 py-2 rounded-xl font-semibold hover:bg-primary/90 transition-colors"
        >
          <span className="material-symbols-outlined text-[20px]">add</span>
          Thêm tài khoản
        </button>
      }
    >
      <div className="bg-surface-container/60 border border-outline-variant/40 rounded-2xl overflow-hidden backdrop-blur ambient-shadow">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="bg-surface-container-high/50 text-on-surface-variant">
              <tr>
                <th className="px-6 py-4 font-semibold">Ngân hàng</th>
                <th className="px-6 py-4 font-semibold">Số tài khoản</th>
                <th className="px-6 py-4 font-semibold">Chủ tài khoản</th>
                <th className="px-6 py-4 font-semibold">Chi nhánh</th>
                <th className="px-6 py-4 font-semibold text-center">Trạng thái</th>
                <th className="px-6 py-4 font-semibold text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-outline-variant/30 text-on-surface">
              {banks.length === 0 ? (
                <tr>
                  <td colSpan={6} className="px-6 py-8 text-center text-on-surface-variant">
                    Chưa có tài khoản ngân hàng nào. Hãy thêm mới.
                  </td>
                </tr>
              ) : (
                banks.map((bank) => (
                  <tr key={bank.id} className="hover:bg-surface-container-high/30 transition-colors">
                    <td className="px-6 py-4 font-medium text-primary">
                      {bank.bankName}
                    </td>
                    <td className="px-6 py-4 font-mono">{bank.accountNumber}</td>
                    <td className="px-6 py-4">{bank.accountName}</td>
                    <td className="px-6 py-4 text-on-surface-variant">{bank.branch || '—'}</td>
                    <td className="px-6 py-4 text-center">
                      <button
                        onClick={() => toggleStatus(bank.id)}
                        className={`inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold border transition-colors ${
                          bank.isActive
                            ? 'bg-primary-container/40 text-primary border-primary/30'
                            : 'bg-surface-container text-on-surface-variant border-outline-variant'
                        }`}
                      >
                        <span className={`w-1.5 h-1.5 rounded-full ${bank.isActive ? 'bg-primary' : 'bg-on-surface-variant'}`} />
                        {bank.isActive ? 'Đang dùng' : 'Tạm ẩn'}
                      </button>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex items-center justify-end gap-2">
                        <button
                          onClick={() => handleOpenModal(bank)}
                          className="p-2 text-on-surface-variant hover:text-primary hover:bg-primary-container/30 rounded-lg transition-colors"
                          title="Sửa"
                        >
                          <span className="material-symbols-outlined text-[20px]">edit</span>
                        </button>
                        <button
                          onClick={() => handleDelete(bank.id)}
                          className="p-2 text-on-surface-variant hover:text-error hover:bg-error-container/30 rounded-lg transition-colors"
                          title="Xóa"
                        >
                          <span className="material-symbols-outlined text-[20px]">delete</span>
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Modal Thêm/Sửa Ngân Hàng */}
      {isModalOpen && (
        <div className="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm">
          <div className="bg-surface text-on-surface w-full max-w-md rounded-3xl shadow-xl overflow-hidden border border-outline-variant/30 flex flex-col max-h-[90vh]">
            <div className="flex items-center justify-between px-6 py-4 border-b border-outline-variant/30">
              <h2 className="text-xl font-bold font-headline">
                {editingBank ? 'Sửa thông tin tài khoản' : 'Thêm tài khoản mới'}
              </h2>
              <button
                onClick={handleCloseModal}
                className="p-2 text-on-surface-variant hover:bg-surface-container rounded-full transition-colors"
              >
                <span className="material-symbols-outlined text-[24px]">close</span>
              </button>
            </div>

            <div className="p-6 overflow-y-auto">
              <form id="bank-form" onSubmit={handleSave} className="space-y-4">
                <div>
                  <label className="block text-sm font-semibold mb-1.5 text-on-surface">Tên ngân hàng <span className="text-error">*</span></label>
                  <input
                    type="text"
                    required
                    value={formData.bankName}
                    onChange={(e) => setFormData({ ...formData, bankName: e.target.value })}
                    placeholder="VD: Vietcombank"
                    className="w-full px-4 py-2.5 rounded-xl border border-outline-variant bg-surface focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary transition-all text-sm"
                  />
                </div>
                
                <div>
                  <label className="block text-sm font-semibold mb-1.5 text-on-surface">Số tài khoản <span className="text-error">*</span></label>
                  <input
                    type="text"
                    required
                    value={formData.accountNumber}
                    onChange={(e) => setFormData({ ...formData, accountNumber: e.target.value })}
                    placeholder="VD: 1012345678"
                    className="w-full px-4 py-2.5 rounded-xl border border-outline-variant bg-surface focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary transition-all text-sm font-mono"
                  />
                </div>

                <div>
                  <label className="block text-sm font-semibold mb-1.5 text-on-surface">Tên chủ tài khoản <span className="text-error">*</span></label>
                  <input
                    type="text"
                    required
                    value={formData.accountName}
                    onChange={(e) => setFormData({ ...formData, accountName: e.target.value.toUpperCase() })}
                    placeholder="VD: NGUYEN VAN A"
                    className="w-full px-4 py-2.5 rounded-xl border border-outline-variant bg-surface focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary transition-all text-sm uppercase"
                  />
                </div>

                <div>
                  <label className="block text-sm font-semibold mb-1.5 text-on-surface">Chi nhánh</label>
                  <input
                    type="text"
                    value={formData.branch}
                    onChange={(e) => setFormData({ ...formData, branch: e.target.value })}
                    placeholder="VD: Chi nhánh Tân Bình (Không bắt buộc)"
                    className="w-full px-4 py-2.5 rounded-xl border border-outline-variant bg-surface focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary transition-all text-sm"
                  />
                </div>

                <div className="flex items-center gap-3 pt-2">
                  <label className="relative inline-flex items-center cursor-pointer">
                    <input 
                      type="checkbox" 
                      className="sr-only peer" 
                      checked={formData.isActive}
                      onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                    />
                    <div className="w-11 h-6 bg-surface-container-high peer-focus:outline-none peer-focus:ring-2 peer-focus:ring-primary/50 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary"></div>
                    <span className="ml-3 text-sm font-semibold text-on-surface">Đang sử dụng (Hiển thị cho khách)</span>
                  </label>
                </div>
              </form>
            </div>

            <div className="px-6 py-4 bg-surface-container/30 border-t border-outline-variant/30 flex justify-end gap-3">
              <button
                type="button"
                onClick={handleCloseModal}
                className="px-5 py-2.5 rounded-xl font-semibold text-on-surface-variant hover:bg-surface-container-high transition-colors"
              >
                Hủy
              </button>
              <button
                type="submit"
                form="bank-form"
                className="px-5 py-2.5 rounded-xl font-semibold bg-primary text-on-primary hover:bg-primary/90 shadow-md shadow-primary/20 transition-all"
              >
                Lưu lại
              </button>
            </div>
          </div>
        </div>
      )}
    </AdminPageShell>
  );
}
