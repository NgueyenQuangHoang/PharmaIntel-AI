import { useState, useEffect } from 'react';

type Theme = 'light' | 'dark';

export function useDarkMode() {
  const [theme, setTheme] = useState<Theme>(() => {
    // 1. Kiểm tra ưu tiên trong Local Storage (nếu người dùng đã từng chuyển trước đó)
    const storedTheme = localStorage.getItem('theme');
    if (storedTheme === 'light' || storedTheme === 'dark') {
      return storedTheme;
    }

    // 2. Nếu không có ở bộ nhớ, lấy theo giao diện máy tính (Hệ Điều Hành) của thiết bị khách
    if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
      return 'dark';
    }

    // 3. Mặc định là Light Mode
    return 'light';
  });

  useEffect(() => {
    const root = window.document.documentElement;

    // Reset chuẩn class
    root.classList.remove('light', 'dark');
    
    // Gán class mới theo State
    root.classList.add(theme);

    // Lưu lựa chọn vào Local Storage để lần sau vào lại có luôn
    localStorage.setItem('theme', theme);
  }, [theme]);

  // Hàm đảo ngược dễ dùng cho Nút bấm:
  const toggleTheme = () => {
    setTheme((prevTheme) => (prevTheme === 'light' ? 'dark' : 'light'));
  };

  return { theme, toggleTheme, setTheme };
}
