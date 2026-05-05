import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { AppProviders } from '@/app/providers'
import { store } from '@/app/store'
import { fetchMeThunk } from '@/features/auth/auth-slice'
import { fetchCartThunk } from '@/features/cart/cart-slice'
import { getToken } from '@/features/auth/token-storage'
import './index.css'
import App from './App'

// Hydrate auth state khi co token san trong localStorage. Sau khi xac thuc xong
// thi load gio hang. Neu /auth/me 401 -> response interceptor xu ly clear + redirect.
if (getToken()) {
  store
    .dispatch(fetchMeThunk())
    .unwrap()
    .then(() => store.dispatch(fetchCartThunk()))
    .catch(() => { /* ignored - interceptor handles 401 */ })
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <AppProviders>
      <App />
    </AppProviders>
  </StrictMode>,
)
