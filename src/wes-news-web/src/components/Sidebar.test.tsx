/**
 * Feature: public-access
 *
 * Property 8: Sidebar exibe ações corretas por estado de autenticação
 * Validates: Requirements 3.1, 3.2, 3.5
 *
 * Property 9: Sources sempre visível na Sidebar
 * Validates: Requirements 3.6
 */

import { render } from '@testing-library/react'
import { vi, describe, it, beforeEach } from 'vitest'
import * as fc from 'fast-check'

// --- Mocks ---

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom')
  return {
    ...actual,
    NavLink: ({ to, children, onClick }: { to: string; children: React.ReactNode; onClick?: () => void; className?: unknown }) => (
      <a href={to} onClick={onClick} data-testid={`navlink-${to.replace('/', '') || 'home'}`}>
        {children}
      </a>
    ),
    useNavigate: () => vi.fn(),
  }
})

vi.mock('../store/authStore', () => ({
  useAuthStore: vi.fn(),
}))

vi.mock('../store/uiStore', () => ({
  useUiStore: vi.fn(),
}))

vi.mock('framer-motion', () => ({
  motion: {
    div: ({ children, ...props }: React.HTMLAttributes<HTMLDivElement>) => <div {...props}>{children}</div>,
  },
  AnimatePresence: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}))

import Sidebar from './Sidebar'
import { useAuthStore } from '../store/authStore'
import { useUiStore } from '../store/uiStore'

const mockUseAuthStore = useAuthStore as unknown as ReturnType<typeof vi.fn>
const mockUseUiStore = useUiStore as unknown as ReturnType<typeof vi.fn>

const noopFn = vi.fn()

const uiState = {
  isDarkMode: false,
  selectedCategory: undefined as undefined,
  searchTerm: '',
  unreadOnly: false,
  selectedArticleId: undefined as undefined,
  sidebarOpen: false,
  setSelectedArticleId: noopFn,
  setSelectedCategory: noopFn,
  toggleDarkMode: noopFn,
  setUnreadOnly: noopFn,
  setSidebarOpen: noopFn,
}

function makeStoreHook<T>(state: T) {
  return (selector?: (s: T) => unknown) => {
    if (typeof selector === 'function') {
      return selector(state)
    }
    return state
  }
}

function renderSidebar(isAuthenticated: boolean) {
  const authState = {
    isAuthenticated,
    token: isAuthenticated ? 'token' : null,
    role: isAuthenticated ? 'User' : null,
    digestEnabled: false,
    login: noopFn,
    logout: noopFn,
  }
  mockUseAuthStore.mockImplementation(makeStoreHook(authState))
  mockUseUiStore.mockImplementation(makeStoreHook(uiState))
  return render(<Sidebar />)
}

describe('Property 8: Sidebar exibe ações corretas por estado de autenticação', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('para qualquer valor de isAuthenticated, a Sidebar exibe os elementos corretos', () => {
    fc.assert(
      fc.property(
        fc.boolean(),
        (isAuthenticated) => {
          const { container, unmount } = renderSidebar(isAuthenticated)

          const text = container.textContent ?? ''
          const links = Array.from(container.querySelectorAll('a'))
          const linkHrefs = links.map((a) => a.getAttribute('href'))

          if (isAuthenticated) {
            // Must have Settings link
            const hasSettings = linkHrefs.includes('/settings')
            // Must have logout button
            const hasLogout = container.querySelector('[title="Sign Out"]') !== null
            // Must NOT have Entrar or Cadastrar
            const hasEntrar = text.includes('Entrar')
            const hasCadastrar = text.includes('Cadastrar')

            unmount()

            if (!hasSettings) throw new Error('isAuthenticated=true: Settings link not found')
            if (!hasLogout) throw new Error('isAuthenticated=true: Logout button not found')
            if (hasEntrar) throw new Error('isAuthenticated=true: "Entrar" should not be visible')
            if (hasCadastrar) throw new Error('isAuthenticated=true: "Cadastrar" should not be visible')
          } else {
            // Must have Entrar and Cadastrar links
            const hasEntrar = linkHrefs.includes('/login') && text.includes('Entrar')
            const hasCadastrar = linkHrefs.includes('/register') && text.includes('Cadastrar')
            // Must NOT have Settings link
            const hasSettings = linkHrefs.includes('/settings')
            // Must NOT have logout button
            const hasLogout = container.querySelector('[title="Sign Out"]') !== null

            unmount()

            if (!hasEntrar) throw new Error('isAuthenticated=false: "Entrar" link to /login not found')
            if (!hasCadastrar) throw new Error('isAuthenticated=false: "Cadastrar" link to /register not found')
            if (hasSettings) throw new Error('isAuthenticated=false: Settings link should not be visible')
            if (hasLogout) throw new Error('isAuthenticated=false: Logout button should not be visible')
          }
        }
      ),
      { numRuns: 100 }
    )
  })
})

describe('Property 9: Sources sempre visível na Sidebar', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('para qualquer estado de autenticação, o link Sources está sempre presente', () => {
    fc.assert(
      fc.property(
        fc.boolean(),
        (isAuthenticated) => {
          const { container, unmount } = renderSidebar(isAuthenticated)

          const links = Array.from(container.querySelectorAll('a'))
          const hasSourcesLink = links.some((a) => a.getAttribute('href') === '/sources')

          unmount()

          if (!hasSourcesLink) {
            throw new Error(
              `isAuthenticated=${isAuthenticated}: Sources link (/sources) not found in Sidebar`
            )
          }
        }
      ),
      { numRuns: 100 }
    )
  })
})
