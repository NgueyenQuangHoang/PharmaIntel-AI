import React, { useEffect } from 'react';
import { Header } from './Header';
import { Footer } from './Footer';
import { MobileBottomNav } from './MobileBottomNav';
import { useAppDispatch, useAppSelector } from '@/hooks/redux';
import { fetchCartThunk } from '@/features/cart/cart-slice';
import { CartDrawer } from '@/features/medicine-cabinet/components/CartDrawer';

export function MainLayout({ children }: { children: React.ReactNode }) {
  const dispatch = useAppDispatch();
  const isAuthenticated = useAppSelector((s) => s.auth.status === 'authenticated' && !!s.auth.token);
  const cartLoaded = useAppSelector((s) => s.cart.cart !== null);

  useEffect(() => {
    if (isAuthenticated && !cartLoaded) {
      dispatch(fetchCartThunk());
    }
  }, [isAuthenticated, cartLoaded, dispatch]);

  return (
    <div className="bg-surface text-on-surface font-body antialiased min-h-screen flex flex-col">
      <Header />
      <main className="flex-1 pt-20">
        {children}
      </main>
      <Footer />
      <MobileBottomNav />
      <CartDrawer />
    </div>
  );
}
