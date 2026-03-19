import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface AuthState {
  token: string | null
  role: string | null
  isAuthenticated: boolean
  digestEnabled: boolean
  login: (token: string, role: string, digestEnabled: boolean) => void
  logout: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      role: null,
      isAuthenticated: false,
      digestEnabled: false,
      login: (token: string, role: string, digestEnabled: boolean) =>
        set({ token, role, isAuthenticated: true, digestEnabled }),
      logout: () => set({ token: null, role: null, isAuthenticated: false, digestEnabled: false }),
    }),
    {
      name: 'wesnews-auth',
    }
  )
)
