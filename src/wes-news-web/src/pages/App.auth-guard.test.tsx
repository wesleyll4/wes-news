/**
 * Feature: public-access, Property 2: AuthGuard redirects /settings for visitors
 *
 * Validates: Requirements 1.4, 4.3
 */

import { render } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { vi, describe, it, beforeEach } from 'vitest'
import * as fc from 'fast-check'

// --- Mocks ---

// Replace BrowserRouter with MemoryRouter so we can control the initial route
let currentInitialRoute = '/'

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom')
  return {
    ...actual,
    BrowserRouter: ({ children }: { children: React.ReactNode }) => (
      <actual.MemoryRouter initialEntries={[currentInitialRoute]}>
        {children}
      </actual.MemoryRouter>
    ),
  }
})

vi.mock('../store/authStore', () => ({
  useAuthStore: vi.fn(),
}))

vi.mock('../store/uiStore', () => ({
  useUiStore: vi.fn(),
}))

vi.mock('../api/client', () => ({
  newsApi: {
    getAll: vi.fn(),
    markAsRead: vi.fn(),
    delete: vi.fn(),
  },
  feedsApi: {
    getAll: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    delete: vi.fn(),
  },
  digestApi: {
    preview: vi.fn(),
    send: vi.fn(),
  },
  usersApi: {
    getMe: vi.fn(),
    updateDigestPreference: vi.fn(),
    deleteAccount: vi.fn(),
  },
}))

import App from '../App'
import { useAuthStore } from '../store/authStore'
import { useUiStore } from '../store/uiStore'
import { newsApi, feedsApi } from '../api/client'

const mockUseAuthStore = useAuthStore as unknown as ReturnType<typeof vi.fn>
const mockUseUiStore = useUiStore as unknown as ReturnType<typeof vi.fn>
const mockNewsApiGetAll = (newsApi as unknown as { getAll: ReturnType<typeof vi.fn> }).getAll
const mockFeedsApiGetAll = (feedsApi as unknown as { getAll: ReturnType<typeof vi.fn> }).getAll

const noopFn = vi.fn()

const authState = {
  isAuthenticated: false,
  token: null as null,
  role: null as null,
  digestEnabled: false,
  login: noopFn,
  logout: noopFn,
}

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

function makeQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  })
}

function renderAppAtRoute(route: string) {
  currentInitialRoute = route
  const queryClient = makeQueryClient()
  return render(
    <QueryClientProvider client={queryClient}>
      <App />
    </QueryClientProvider>
  )
}

describe('Property 2: AuthGuard redireciona /settings para visitantes', () => {
  beforeEach(() => {
    vi.clearAllMocks()

    // isAuthenticated = false for all runs
    authState.isAuthenticated = false
    mockUseAuthStore.mockImplementation(makeStoreHook(authState))
    mockUseUiStore.mockImplementation(makeStoreHook(uiState))

    mockNewsApiGetAll.mockResolvedValue({ items: [], totalCount: 0 })
    mockFeedsApiGetAll.mockResolvedValue([])
  })

  it('para qualquer navegação para /settings com isAuthenticated=false, a página de login é renderizada', async () => {
    await fc.assert(
      fc.asyncProperty(
        fc.constant('/settings'),
        async (route) => {
          const { unmount, container } = renderAppAtRoute(route)

          // Give React a tick to render and process any redirects
          await new Promise((resolve) => setTimeout(resolve, 0))

          // The login page has a password input that uniquely identifies it.
          // AuthGuard should redirect /settings → /login, so the login form must be present.
          const passwordInput = container.querySelector('input[type="password"]')
          const isOnLoginPage = passwordInput !== null

          unmount()

          if (!isOnLoginPage) {
            throw new Error(
              `Route "${route}" with isAuthenticated=false did NOT redirect to /login, but AuthGuard should have redirected`
            )
          }
        }
      ),
      { numRuns: 100 }
    )
  })
})
