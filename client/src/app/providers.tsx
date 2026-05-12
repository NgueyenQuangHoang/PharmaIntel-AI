import type { PropsWithChildren } from 'react'
import { Provider } from 'react-redux'
import { Toaster } from 'sonner'
import { store } from '@/app/store'

export function AppProviders({ children }: PropsWithChildren) {
  return (
    <Provider store={store}>
      {children}
      <Toaster position="top-right" richColors closeButton />
    </Provider>
  )
}
