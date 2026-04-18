import { NavLink } from 'react-router-dom';

export function Header() {
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
        </div>

        {/* Icons */}
        <div className="flex items-center gap-4">
          <button className="p-2 text-slate-600 hover:bg-slate-50 dark:hover:bg-slate-800 rounded-full transition-colors duration-200">
            <span className="material-symbols-outlined">notifications</span>
          </button>
          <button className="p-2 text-slate-600 hover:bg-slate-50 dark:hover:bg-slate-800 rounded-full transition-colors duration-200">
            <span className="material-symbols-outlined">account_circle</span>
          </button>
        </div>
      </nav>
    </header>
  );
}
