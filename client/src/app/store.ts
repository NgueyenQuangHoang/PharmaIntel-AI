import { configureStore } from '@reduxjs/toolkit'
import counterReducer from '@/features/counter/counter-slice'
import authReducer from '@/features/auth/auth-slice'
import categoriesReducer from '@/features/categories/categories-slice'
import medicationsReducer from '@/features/medications/medications-slice'
import cartReducer from '@/features/cart/cart-slice'
import diagnosticReducer from '@/features/diagnostic/diagnostic-slice'
import adminReducer from '@/features/admin/admin-slice'

import profileReducer from '@/features/profile/profile-slice'

export const store = configureStore({
  reducer: {
    counter: counterReducer,
    auth: authReducer,
    categories: categoriesReducer,
    medications: medicationsReducer,
    cart: cartReducer,
    diagnostic: diagnosticReducer,
    admin: adminReducer,
    profile: profileReducer,
  },
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch
export type AppStore = typeof store
