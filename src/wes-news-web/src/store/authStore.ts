import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface AuthState {
  token: string | null
  role: string | null
  isAuthenticated: boolean
  login: (token: string, role: string) => void
  logout: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      role: null,
      isAuthenticated: false,
      login: (token: string, role: string) => set({ token, role, isAuthenticated: true }),
      logout: () => set({ token: null, role: null, isAuthenticated: false }),
    }),
    {
      name: 'wesnews-auth',
    }
  )
)
