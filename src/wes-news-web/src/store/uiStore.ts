import { create } from 'zustand'
import type { Category } from '../types'

interface UiState {
  selectedCategory: Category | undefined
  searchTerm: string
  unreadOnly: boolean
  selectedArticleId: string | undefined
  isDarkMode: boolean
  sidebarOpen: boolean
  setSelectedCategory: (category: Category | undefined) => void
  setSearchTerm: (term: string) => void
  setUnreadOnly: (value: boolean) => void
  setSelectedArticleId: (id: string | undefined) => void
  toggleDarkMode: () => void
  setSidebarOpen: (open: boolean) => void
}

export const useUiStore = create<UiState>((set) => ({
  selectedCategory: undefined,
  searchTerm: '',
  unreadOnly: false,
  selectedArticleId: undefined,
  isDarkMode: localStorage.getItem('darkMode') !== 'false',
  sidebarOpen: false,

  setSelectedCategory: (category) => set({ selectedCategory: category, selectedArticleId: undefined }),
  setSearchTerm: (term) => set({ searchTerm: term }),
  setUnreadOnly: (value) => set({ unreadOnly: value }),
  setSelectedArticleId: (id) => set({ selectedArticleId: id }),
  setSidebarOpen: (open) => set({ sidebarOpen: open }),
  toggleDarkMode: () =>
    set((state) => {
      const next = !state.isDarkMode
      localStorage.setItem('darkMode', String(next))
      return { isDarkMode: next }
    }),
}))
