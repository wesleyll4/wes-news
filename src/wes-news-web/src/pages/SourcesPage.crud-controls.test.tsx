/**
 * Feature: public-access, Property 11: SourcesPage CRUD controls conditional on role
 *
 * Validates: Requirements 4.4, 4.5
 */

import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router-dom'
import { vi, describe, it, beforeEach } from 'vitest'
import * as fc from 'fast-check'

// --- Mocks ---

vi.mock('../store/authStore', () => ({
  useAuthStore: vi.fn(),
}))

vi.mock('../api/client', () => ({
  feedsApi: {
    getAll: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    delete: vi.fn(),
  },
}))

import SourcesPage from './SourcesPage'
import { useAuthStore } from '../store/authStore'
import { feedsApi } from '../api/client'

const mockUseAuthStore = useAuthStore as unknown as ReturnType<typeof vi.fn>
const mockFeedsApi = feedsApi as unknown as { getAll: ReturnType<typeof vi.fn> }

function makeQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  })
}

function renderSourcesPage() {
  const queryClient = makeQueryClient()
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <SourcesPage />
      </MemoryRouter>
    </QueryClientProvider>
  )
}

// Zustand hooks can be called with or without a selector
function makeStoreHook<T>(state: T) {
  return (selector?: (s: T) => unknown) => {
    if (typeof selector === 'function') {
      return selector(state)
    }
    return state
  }
}

describe('Property 11: Controles CRUD em SourcesPage condicionais ao role', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockFeedsApi.getAll.mockResolvedValue([])
  })

  it('para qualquer role em [null, "User", "Admin"], o botão "Add Source" está desabilitado para não-Admin e habilitado para Admin', async () => {
    await fc.assert(
      fc.asyncProperty(
        fc.constantFrom(null, 'User', 'Admin'),
        async (role) => {
          const isAuthenticated = role !== null
          const isAdmin = role === 'Admin'

          const authState = {
            token: isAuthenticated ? 'mock-token' : null,
            role,
            isAuthenticated,
            digestEnabled: false,
            login: vi.fn(),
            logout: vi.fn(),
          }

          mockUseAuthStore.mockImplementation(makeStoreHook(authState))

          const { unmount } = renderSourcesPage()

          // Give React a tick to render
          await new Promise((resolve) => setTimeout(resolve, 0))

          const addButton = screen.getByRole('button', { name: /add source/i })

          if (isAdmin) {
            if (addButton.hasAttribute('disabled')) {
              unmount()
              throw new Error(
                `role="${role}": "Add Source" button should be ENABLED for Admin, but it is disabled`
              )
            }
          } else {
            if (!addButton.hasAttribute('disabled')) {
              unmount()
              throw new Error(
                `role="${role}": "Add Source" button should be DISABLED for non-Admin, but it is enabled`
              )
            }
          }

          unmount()
        }
      ),
      { numRuns: 100 }
    )
  })
})
