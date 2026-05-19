// =============================================================================
// AppProviders - gom toan bo Provider toan cuc: Redux, Toaster, Google OAuth.
// GoogleOAuthProvider load script gsi (Google Identity Services) khi mount,
// can clientId tu VITE_GOOGLE_CLIENT_ID (xem .env.example).
// =============================================================================
import type { PropsWithChildren } from 'react'
import { Provider } from 'react-redux'
import { Toaster } from 'sonner'
import { GoogleOAuthProvider } from '@react-oauth/google'
import { store } from '@/app/store'

const googleClientId = import.meta.env.VITE_GOOGLE_CLIENT_ID ?? ''

export function AppProviders({ children }: PropsWithChildren) {
  const content = (
    <Provider store={store}>
      {children}
      <Toaster position="top-right" richColors closeButton />
    </Provider>
  )

  // Neu chua cau hinh VITE_GOOGLE_CLIENT_ID, bo qua provider de tranh warning -
  // nut Google se khong hoat dong nhung cac tinh nang khac van chay binh thuong.
  if (!googleClientId) return content

  return <GoogleOAuthProvider clientId={googleClientId}>{content}</GoogleOAuthProvider>
}
