import { NavLink, useNavigate } from 'react-router-dom';
import { useState, useRef, useEffect, useCallback } from 'react';
import { useAppSelector, useAppDispatch } from '@/hooks/redux';
import { useAuth } from '@/hooks/useAuth';
import { useDarkMode } from '@/hooks/useDarkMode';
import { openCart } from '@/features/cart/cart-slice';

type Language = 'vi' | 'en';

const LANG_LABELS: Record<Language, { label: string; flag: string }> = {
  vi: { label: 'Tiếng Việt', flag: '🇻🇳' },
  en: { label: 'English', flag: '🇺🇸' },
};

function getStoredLang(): Language {
  const v = localStorage.getItem('app-lang');
  return v === 'en' ? 'en' : 'vi';
}

export function Header() {
  const role = useAppSelector((s) => s.auth.user?.role);
  const isAdmin = role?.toLowerCase() === 'admin';
  const cartTotalItems = useAppSelector((s) => s.cart.cart?.totalItems ?? 0);
  const dispatch = useAppDispatch();
  const { isAuthenticated, logout, user } = useAuth();
  const { theme, toggleTheme } = useDarkMode();
  const navigate = useNavigate();

  // Dropdown state
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);
  const btnRef = useRef<HTMLButtonElement>(null);

  // Language state
  const [lang, setLang] = useState<Language>(getStoredLang);

  // Close on outside click
  const handleClickOutside = useCallback((e: MouseEvent) => {
    if (
      menuRef.current &&
      !menuRef.current.contains(e.target as Node) &&
      btnRef.current &&
      !btnRef.current.contains(e.target as Node)
    ) {
      setMenuOpen(false);
    }
  }, []);

  useEffect(() => {
    if (menuOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [menuOpen, handleClickOutside]);

  // Close on Escape
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setMenuOpen(false);
    };
    document.addEventListener('keydown', onKey);
    return () => document.removeEventListener('keydown', onKey);
  }, []);

  const handleLangChange = (newLang: Language) => {
    setLang(newLang);
    localStorage.setItem('app-lang', newLang);
    // Future: trigger i18n context update here
  };

  const handleLogout = () => {
    logout();
    setMenuOpen(false);
    navigate('/login');
  };

  const handleLogin = () => {
    setMenuOpen(false);
    navigate('/login');
  };

  // Get user initials for avatar
  const initials = user?.fullName
    ? user.fullName
        .split(' ')
        .map((w: string) => w[0])
        .slice(0, 2)
        .join('')
        .toUpperCase()
    : null;

  return (
    <header className="fixed top-0 w-full z-50 bg-white/80 dark:bg-slate-900/80 backdrop-blur-xl shadow-sm dark:shadow-none h-20">
      <nav className="relative flex justify-between items-center px-8 h-full max-w-full mx-auto">
        {/* Brand */}
        <div className="flex items-center">
          <span className="text-2xl font-bold tracking-tighter text-blue-800 dark:text-blue-300 font-headline">PharmaIntel</span>
        </div>

        {/* Centered Navigation */}
        <div className="hidden md:flex items-center gap-8 absolute left-1/2 -translate-x-1/2">
          <NavLink
            to="/"
            end
            className={({ isActive }) => `font-manrope tracking-tight font-semibold duration-200 ease-in-out ${isActive ? 'text-blue-700 dark:text-blue-400 border-b-2 border-blue-600 pb-1' : 'text-slate-600 dark:text-slate-400 hover:text-blue-500'}`}
          >
            Trang chủ
          </NavLink>
          <NavLink
            to="/diagnostic"
            className={({ isActive }) => `font-manrope tracking-tight font-semibold duration-200 ease-in-out ${isActive ? 'text-blue-700 dark:text-blue-400 border-b-2 border-blue-600 pb-1' : 'text-slate-600 dark:text-slate-400 hover:text-blue-500'}`}
          >
            Chẩn đoán AI
          </NavLink>
          <NavLink
            to="/medicine"
            className={({ isActive }) => `font-manrope tracking-tight font-semibold duration-200 ease-in-out ${isActive ? 'text-blue-700 dark:text-blue-400 border-b-2 border-blue-600 pb-1' : 'text-slate-600 dark:text-slate-400 hover:text-blue-500'}`}
          >
            Tủ thuốc
          </NavLink>
          <NavLink
            to="/profile"
            className={({ isActive }) => `font-manrope tracking-tight font-semibold duration-200 ease-in-out ${isActive ? 'text-blue-700 dark:text-blue-400 border-b-2 border-blue-600 pb-1' : 'text-slate-600 dark:text-slate-400 hover:text-blue-500'}`}
          >
            Hồ sơ
          </NavLink>
          {isAdmin && (
            <NavLink
              to="/admin"
              className={({ isActive }) => `font-manrope tracking-tight font-semibold duration-200 ease-in-out inline-flex items-center gap-1 ${isActive ? 'text-blue-700 dark:text-blue-400 border-b-2 border-blue-600 pb-1' : 'text-slate-600 dark:text-slate-400 hover:text-blue-500'}`}
            >
              <span className="material-symbols-outlined text-[18px]">shield_person</span>
              Admin
            </NavLink>
          )}
        </div>

        {/* Icons */}
        <div className="flex items-center gap-4">
          <button 
            onClick={() => dispatch(openCart())}
            className="p-2 text-slate-600 hover:bg-slate-50 dark:hover:bg-slate-800 rounded-full transition-colors duration-200 relative"
            title="Giỏ hàng"
          >
            <span className="material-symbols-outlined">shopping_cart</span>
            {cartTotalItems > 0 && (
              <span className="absolute top-0 right-0 inline-flex items-center justify-center px-1.5 py-0.5 text-[10px] font-bold leading-none text-white transform translate-x-1/4 -translate-y-1/4 bg-red-600 rounded-full">
                {cartTotalItems}
              </span>
            )}
          </button>
          
          <button className="p-2 text-slate-600 hover:bg-slate-50 dark:hover:bg-slate-800 rounded-full transition-colors duration-200" title="Thông báo">
            <span className="material-symbols-outlined">notifications</span>
          </button>

          {/* ─── Avatar Button + Dropdown ─── */}
          <div className="relative">
            <button
              ref={btnRef}
              id="header-user-menu-btn"
              onClick={() => setMenuOpen((p) => !p)}
              className={`
                relative flex items-center justify-center w-10 h-10 rounded-full
                transition-all duration-200 select-none
                ${menuOpen
                  ? 'ring-2 ring-blue-500 ring-offset-2 dark:ring-offset-slate-900'
                  : 'hover:ring-2 hover:ring-slate-300 dark:hover:ring-slate-600 hover:ring-offset-1 dark:hover:ring-offset-slate-900'
                }
                ${isAuthenticated && initials
                  ? 'bg-gradient-to-br from-blue-600 to-indigo-600 text-white text-sm font-bold'
                  : 'text-slate-600 dark:text-slate-400 hover:bg-slate-50 dark:hover:bg-slate-800'
                }
              `}
              aria-haspopup="true"
              aria-expanded={menuOpen}
            >
              {isAuthenticated && initials ? (
                initials
              ) : (
                <span className="material-symbols-outlined text-[26px]">account_circle</span>
              )}
            </button>

            {/* Dropdown Menu */}
            <div
              ref={menuRef}
              id="header-user-dropdown"
              className={`
                absolute right-0 mt-2 w-64 origin-top-right
                rounded-2xl overflow-hidden
                bg-white dark:bg-slate-800
                shadow-[0_20px_60px_-12px_rgba(0,0,0,0.25)]
                dark:shadow-[0_20px_60px_-12px_rgba(0,0,0,0.5)]
                border border-slate-200/60 dark:border-slate-700/60
                transition-all duration-200 ease-out
                ${menuOpen
                  ? 'opacity-100 scale-100 translate-y-0 pointer-events-auto'
                  : 'opacity-0 scale-95 -translate-y-2 pointer-events-none'
                }
              `}
              role="menu"
            >
              {/* ── User Info (if logged in) ── */}
              {isAuthenticated && user && (
                <div className="px-4 py-3 border-b border-slate-100 dark:border-slate-700/60">
                  <p className="text-sm font-semibold text-slate-900 dark:text-slate-100 truncate">
                    {user.fullName || user.email}
                  </p>
                  {user.fullName && (
                    <p className="text-xs text-slate-500 dark:text-slate-400 truncate mt-0.5">{user.email}</p>
                  )}
                </div>
              )}

              {/* ── My Orders (chi hien khi da dang nhap) ── */}
              {isAuthenticated && (
                <div className="px-2 pt-2">
                  <button
                    id="header-my-orders-btn"
                    onClick={() => {
                      setMenuOpen(false);
                      navigate('/orders');
                    }}
                    className="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-left
                      text-sm font-medium text-slate-700 dark:text-slate-200
                      hover:bg-slate-100 dark:hover:bg-slate-700/60
                      transition-colors duration-150"
                    role="menuitem"
                  >
                    <span className="material-symbols-outlined text-[20px] text-blue-600 dark:text-blue-400">
                      receipt_long
                    </span>
                    <span>Đơn hàng của tôi</span>
                  </button>
                </div>
              )}

              {/* ── Theme Toggle ── */}
              <div className={isAuthenticated ? 'px-2 pt-1' : 'px-2 pt-2'}>
                <button
                  id="header-theme-toggle"
                  onClick={toggleTheme}
                  className="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-left
                    text-sm font-medium text-slate-700 dark:text-slate-200
                    hover:bg-slate-100 dark:hover:bg-slate-700/60
                    transition-colors duration-150"
                  role="menuitem"
                >
                  <span className="material-symbols-outlined text-[20px] text-amber-500 dark:text-amber-400">
                    {theme === 'dark' ? 'light_mode' : 'dark_mode'}
                  </span>
                  <span className="flex-1">
                    {theme === 'dark' ? 'Chế độ sáng' : 'Chế độ tối'}
                  </span>
                  <span className="text-xs px-2 py-0.5 rounded-full bg-slate-100 dark:bg-slate-700 text-slate-500 dark:text-slate-400">
                    {theme === 'dark' ? '☀️' : '🌙'}
                  </span>
                </button>
              </div>

              {/* ── Language Switcher ── */}
              <div className="px-2 pt-1">
                <div className="px-3 py-2">
                  <span className="text-[11px] font-semibold uppercase tracking-wider text-slate-400 dark:text-slate-500">
                    Ngôn ngữ
                  </span>
                </div>
                <div className="flex gap-1 px-1 pb-1">
                  {(Object.keys(LANG_LABELS) as Language[]).map((key) => (
                    <button
                      key={key}
                      id={`header-lang-${key}`}
                      onClick={() => handleLangChange(key)}
                      className={`
                        flex-1 flex items-center justify-center gap-1.5 py-2 rounded-lg text-sm font-medium
                        transition-all duration-150
                        ${lang === key
                          ? 'bg-blue-50 dark:bg-blue-500/20 text-blue-700 dark:text-blue-300 ring-1 ring-blue-200 dark:ring-blue-500/40'
                          : 'text-slate-500 dark:text-slate-400 hover:bg-slate-50 dark:hover:bg-slate-700/40'
                        }
                      `}
                      role="menuitem"
                    >
                      <span className="text-base">{LANG_LABELS[key].flag}</span>
                      <span>{LANG_LABELS[key].label}</span>
                    </button>
                  ))}
                </div>
              </div>

              {/* ── Divider ── */}
              <div className="mx-3 my-1 border-t border-slate-100 dark:border-slate-700/60" />

              {/* ── Login / Logout ── */}
              <div className="px-2 pb-2">
                {isAuthenticated ? (
                  <button
                    id="header-logout-btn"
                    onClick={handleLogout}
                    className="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-left
                      text-sm font-medium text-red-600 dark:text-red-400
                      hover:bg-red-50 dark:hover:bg-red-500/10
                      transition-colors duration-150"
                    role="menuitem"
                  >
                    <span className="material-symbols-outlined text-[20px]">logout</span>
                    <span>Đăng xuất</span>
                  </button>
                ) : (
                  <button
                    id="header-login-btn"
                    onClick={handleLogin}
                    className="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-left
                      text-sm font-medium text-blue-600 dark:text-blue-400
                      hover:bg-blue-50 dark:hover:bg-blue-500/10
                      transition-colors duration-150"
                    role="menuitem"
                  >
                    <span className="material-symbols-outlined text-[20px]">login</span>
                    <span>Đăng nhập</span>
                  </button>
                )}
              </div>
            </div>
          </div>
        </div>
      </nav>
    </header>
  );
}
