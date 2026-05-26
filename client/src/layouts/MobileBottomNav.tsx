import { Link, useLocation } from 'react-router-dom';

export function MobileBottomNav() {
  const location = useLocation();
  const path = location.pathname;

  return (
    <div className="md:hidden fixed bottom-0 left-0 right-0 bg-white dark:bg-slate-900 border-t border-slate-100 dark:border-slate-800 h-16 flex items-center justify-around z-50">
      <Link to="/" className={`flex flex-col items-center gap-1 ${path === '/' ? 'text-blue-700 dark:text-blue-400' : 'text-slate-500 hover:text-slate-700 dark:hover:text-slate-300'}`}>
        <span className="material-symbols-outlined" style={path === '/' ? { fontVariationSettings: "'FILL' 1" } : {}}>home</span>
        <span className={`text-[10px] ${path === '/' ? 'font-bold' : 'font-medium'}`}>Trang chủ</span>
      </Link>
      
      <Link to="/diagnostic" className={`flex flex-col items-center gap-1 ${path.startsWith('/diagnostic') ? 'text-blue-700 dark:text-blue-400' : 'text-slate-500 hover:text-slate-700 dark:hover:text-slate-300'}`}>
        <span className="material-symbols-outlined" style={path.startsWith('/diagnostic') ? { fontVariationSettings: "'FILL' 1" } : {}}>clinical_notes</span>
        <span className={`text-[10px] ${path.startsWith('/diagnostic') ? 'font-bold' : 'font-medium'}`}>Chẩn đoán</span>
      </Link>

      <Link to="/consultations" className={`flex flex-col items-center gap-1 ${path.startsWith('/consultations') ? 'text-blue-700 dark:text-blue-400' : 'text-slate-500 hover:text-slate-700 dark:hover:text-slate-300'}`}>
        <span className="material-symbols-outlined" style={path.startsWith('/consultations') ? { fontVariationSettings: "'FILL' 1" } : {}}>chat</span>
        <span className={`text-[10px] ${path.startsWith('/consultations') ? 'font-bold' : 'font-medium'}`}>Tư vấn</span>
      </Link>
      
      <Link to="/medicine" className={`flex flex-col items-center gap-1 ${path === '/medicine' ? 'text-blue-700 dark:text-blue-400' : 'text-slate-500 hover:text-slate-700 dark:hover:text-slate-300'}`}>
        <span className="material-symbols-outlined" style={path === '/medicine' ? { fontVariationSettings: "'FILL' 1" } : {}}>local_pharmacy</span>
        <span className={`text-[10px] ${path === '/medicine' ? 'font-bold' : 'font-medium'}`}>Tủ thuốc</span>
      </Link>

      <Link to="/profile" className={`flex flex-col items-center gap-1 ${path === '/profile' ? 'text-blue-700 dark:text-blue-400' : 'text-slate-500 hover:text-slate-700 dark:hover:text-slate-300'}`}>
        <span className="material-symbols-outlined" style={path === '/profile' ? { fontVariationSettings: "'FILL' 1" } : {}}>person</span>
        <span className={`text-[10px] ${path === '/profile' ? 'font-bold' : 'font-medium'}`}>Hồ sơ</span>
      </Link>
    </div>
  );
}
