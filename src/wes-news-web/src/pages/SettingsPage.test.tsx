/**
 * Feature: user-digest-preference, Property 9: Toggle reflete o estado do servidor
 *
 * Validates: Requirements 4.1, 5.1
 */

import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router-dom'
import { vi, describe, it, beforeEach } from 'vitest'
import * as fc from 'fast-check'
import SettingsPage from './SettingsPage'

// --- Mocks ---

const mockNavigate = vi.fn()
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom')
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  }
})

vi.mock('../api/client', () => ({
  usersApi: {
    getMe: vi.fn(),
    updateDigestPreference: vi.fn(),
    deleteAccount: vi.fn(),
  },
  digestApi: {
    preview: vi.fn(),
    send: vi.fn(),
  },
}))

vi.mock('../store/authStore', () => ({
  useAuthStore: vi.fn(),
}))

import { usersApi, digestApi } from '../api/client'
import { useAuthStore } from '../store/authStore'

const mockUsersApi = usersApi as {
  getMe: ReturnType<typeof vi.fn>
  updateDigestPreference: ReturnType<typeof vi.fn>
  deleteAccount: ReturnType<typeof vi.fn>
}

const mockDigestApi = digestApi as {
  preview: ReturnType<typeof vi.fn>
  send: ReturnType<typeof vi.fn>
}

const mockUseAuthStore = useAuthStore as unknown as ReturnType<typeof vi.fn>

function makeQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  })
}

function renderSettingsPage(queryClient: QueryClient) {
  return render(
    <MemoryRouter>
      <QueryClientProvider client={queryClient}>
        <SettingsPage />
      </QueryClientProvider>
    </MemoryRouter>
  )
}

const mockLogout = vi.fn()

function makeAuthStoreMock(digestEnabled: boolean) {
  return (selector: (s: { digestEnabled: boolean; logout: () => void }) => unknown) => {
    const state = { digestEnabled, logout: mockLogout }
    return selector(state)
  }
}

describe('Property 9: Toggle reflete o estado do servidor', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    // Default auth store state
    mockUseAuthStore.mockImplementation(makeAuthStoreMock(false))
    // Default digest API mocks (not used in this property)
    mockDigestApi.preview.mockResolvedValue({ html: '', articleCount: 0 })
    mockDigestApi.send.mockResolvedValue({})
    mockUsersApi.updateDigestPreference.mockResolvedValue({ digestEnabled: false })
    mockUsersApi.deleteAccount.mockResolvedValue(undefined)
  })

  it('para qualquer valor de digestEnabled retornado pela API, o toggle reflete o estado correspondente', async () => {
    await fc.assert(
      fc.asyncProperty(fc.boolean(), async (digestEnabled) => {
        // Arrange: mock getMe to return the arbitrary digestEnabled value
        mockUsersApi.getMe.mockResolvedValue({ digestEnabled })
        mockUseAuthStore.mockImplementation(makeAuthStoreMock(digestEnabled))

        const queryClient = makeQueryClient()
        const { unmount } = renderSettingsPage(queryClient)

        // Act: wait for the query to resolve and the toggle to update
        const toggle = await waitFor(() =>
          screen.getByRole('switch', { name: /toggle daily digest email/i })
        )

        // Assert: aria-checked must match the value returned by the API
        await waitFor(() => {
          const checked = toggle.getAttribute('aria-checked')
          const expected = String(digestEnabled)
          if (checked !== expected) {
            throw new Error(
              `Expected aria-checked="${expected}" but got "${checked}" for digestEnabled=${digestEnabled}`
            )
          }
        })

        unmount()
      }),
      { numRuns: 100 }
    )
  })
})

/**
 * Feature: user-digest-preference, Property 10: Alteração do toggle dispara PATCH com valor correto
 *
 * Validates: Requirements 4.2
 */

describe('Property 10: Alteração do toggle dispara PATCH com valor correto', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockDigestApi.preview.mockResolvedValue({ html: '', articleCount: 0 })
    mockDigestApi.send.mockResolvedValue({})
    mockUsersApi.deleteAccount.mockResolvedValue(undefined)
  })

  it('para qualquer estado atual do toggle, clicar chama updateDigestPreference com o valor oposto', async () => {
    await fc.assert(
      fc.asyncProperty(fc.boolean(), async (initialDigestEnabled) => {
        // Arrange: server returns the arbitrary initial state
        mockUsersApi.getMe.mockResolvedValue({ digestEnabled: initialDigestEnabled })
        mockUseAuthStore.mockImplementation(makeAuthStoreMock(initialDigestEnabled))
        mockUsersApi.updateDigestPreference.mockResolvedValue({ digestEnabled: !initialDigestEnabled })

        const queryClient = makeQueryClient()
        const { unmount } = renderSettingsPage(queryClient)

        // Wait for the toggle to reflect the initial server state
        const toggle = await waitFor(() =>
          screen.getByRole('switch', { name: /toggle daily digest email/i })
        )
        await waitFor(() => {
          const checked = toggle.getAttribute('aria-checked')
          if (checked !== String(initialDigestEnabled)) {
            throw new Error(
              `Toggle not yet initialised: expected "${initialDigestEnabled}" but got "${checked}"`
            )
          }
        })

        // Act: click the toggle
        await userEvent.click(toggle)

        // Assert: updateDigestPreference must have been called with the opposite value
        await waitFor(() => {
          const calls = mockUsersApi.updateDigestPreference.mock.calls
          if (calls.length === 0) {
            throw new Error('updateDigestPreference was not called after toggle click')
          }
          const calledWith = calls[calls.length - 1][0]
          const expected = !initialDigestEnabled
          if (calledWith !== expected) {
            throw new Error(
              `Expected updateDigestPreference to be called with ${expected} but got ${calledWith} (initialDigestEnabled=${initialDigestEnabled})`
            )
          }
        })

        unmount()
      }),
      { numRuns: 100 }
    )
  }, 60_000)
})

// ---------------------------------------------------------------------------
// Unit tests for Req 4.3, 4.4, 4.5, 5.3
// ---------------------------------------------------------------------------

describe('Toggle desabilitado durante requisição em andamento (Req 4.3)', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockDigestApi.preview.mockResolvedValue({ html: '', articleCount: 0 })
    mockDigestApi.send.mockResolvedValue({})
    mockUseAuthStore.mockImplementation(makeAuthStoreMock(false))
    mockUsersApi.getMe.mockResolvedValue({ digestEnabled: false })
    mockUsersApi.deleteAccount.mockResolvedValue(undefined)
  })

  it('o toggle fica desabilitado enquanto a mutação está pendente', async () => {
    // Never-resolving promise simulates pending state
    mockUsersApi.updateDigestPreference.mockReturnValue(new Promise(() => {}))

    const queryClient = makeQueryClient()
    renderSettingsPage(queryClient)

    const toggle = await waitFor(() =>
      screen.getByRole('switch', { name: /toggle daily digest email/i })
    )

    // Wait for initial state to settle
    await waitFor(() => {
      expect(toggle.getAttribute('aria-checked')).toBe('false')
    })

    // Click to trigger mutation
    await userEvent.click(toggle)

    // Toggle should now be disabled while mutation is pending
    await waitFor(() => {
      expect(toggle).toBeDisabled()
    })
  })
})

describe('Mensagem de erro e reversão do toggle em caso de falha no PATCH (Req 4.4)', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockDigestApi.preview.mockResolvedValue({ html: '', articleCount: 0 })
    mockDigestApi.send.mockResolvedValue({})
    mockUseAuthStore.mockImplementation(makeAuthStoreMock(false))
    mockUsersApi.getMe.mockResolvedValue({ digestEnabled: false })
    mockUsersApi.deleteAccount.mockResolvedValue(undefined)
  })

  it('exibe mensagem de erro e reverte o toggle quando o PATCH falha', async () => {
    mockUsersApi.updateDigestPreference.mockRejectedValue(new Error('Network error'))

    const queryClient = makeQueryClient()
    renderSettingsPage(queryClient)

    const toggle = await waitFor(() =>
      screen.getByRole('switch', { name: /toggle daily digest email/i })
    )

    // Wait for initial state (false)
    await waitFor(() => {
      expect(toggle.getAttribute('aria-checked')).toBe('false')
    })

    // Click to toggle (optimistically sets to true, then reverts on error)
    await userEvent.click(toggle)

    // Error message should appear
    await waitFor(() => {
      expect(
        screen.getByText('Failed to update digest preference. Please try again.')
      ).toBeInTheDocument()
    })

    // Toggle should revert back to original value (false)
    await waitFor(() => {
      expect(toggle.getAttribute('aria-checked')).toBe('false')
    })
  })
})

describe('Confirmação visual após sucesso no PATCH (Req 4.5)', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockDigestApi.preview.mockResolvedValue({ html: '', articleCount: 0 })
    mockDigestApi.send.mockResolvedValue({})
    mockUseAuthStore.mockImplementation(makeAuthStoreMock(false))
    mockUsersApi.getMe.mockResolvedValue({ digestEnabled: false })
    mockUsersApi.deleteAccount.mockResolvedValue(undefined)
  })

  it('exibe "Preference saved" após o PATCH ter sucesso', async () => {
    mockUsersApi.updateDigestPreference.mockResolvedValue({ digestEnabled: true })

    const queryClient = makeQueryClient()
    renderSettingsPage(queryClient)

    const toggle = await waitFor(() =>
      screen.getByRole('switch', { name: /toggle daily digest email/i })
    )

    // Wait for initial state
    await waitFor(() => {
      expect(toggle.getAttribute('aria-checked')).toBe('false')
    })

    // Click to trigger mutation
    await userEvent.click(toggle)

    // Success confirmation should appear
    await waitFor(() => {
      expect(screen.getByText('Preference saved')).toBeInTheDocument()
    })
  })
})

describe('Falha no GET /users/me exibe aviso e mantém último valor (Req 5.3)', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockDigestApi.preview.mockResolvedValue({ html: '', articleCount: 0 })
    mockDigestApi.send.mockResolvedValue({})
    mockUsersApi.updateDigestPreference.mockResolvedValue({ digestEnabled: true })
    mockUsersApi.deleteAccount.mockResolvedValue(undefined)
  })

  it('exibe aviso e usa o valor do authStore quando getMe falha', async () => {
    mockUsersApi.getMe.mockRejectedValue(new Error('Network error'))
    // authStore has the last known value
    mockUseAuthStore.mockImplementation(makeAuthStoreMock(true))

    // Use retryDelay: 0 so the component's retry: 1 resolves immediately
    const queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false, retryDelay: 0 },
        mutations: { retry: false },
      },
    })
    renderSettingsPage(queryClient)

    // Warning message should appear (allow extra time for retry: 1 in the component)
    await waitFor(() => {
      expect(
        screen.getByText(
          'Could not load your preferences from the server. Showing last known value.'
        )
      ).toBeInTheDocument()
    }, { timeout: 5000 })

    // Toggle should reflect the authStore fallback value (true)
    const toggle = screen.getByRole('switch', { name: /toggle daily digest email/i })
    await waitFor(() => {
      expect(toggle.getAttribute('aria-checked')).toBe('true')
    })
  })
})

// ---------------------------------------------------------------------------
// Feature: delete-account, Property 5: Confirmação é obrigatória antes da chamada à API
// Validates: Requirements 3.2, 3.4
// ---------------------------------------------------------------------------

describe('Property 5: Confirmação é obrigatória antes da chamada à API', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockDigestApi.preview.mockResolvedValue({ html: '', articleCount: 0 })
    mockDigestApi.send.mockResolvedValue({})
    mockUseAuthStore.mockImplementation(makeAuthStoreMock(false))
    mockUsersApi.getMe.mockResolvedValue({ digestEnabled: false })
    mockUsersApi.updateDigestPreference.mockResolvedValue({ digestEnabled: false })
    mockUsersApi.deleteAccount.mockResolvedValue(undefined)
  })

  it('para qualquer estado onde o diálogo não foi confirmado, deleteAccount não deve ter sido chamado', async () => {
    // Feature: delete-account, Property 5: Confirmação é obrigatória antes da chamada à API
    await fc.assert(
      fc.asyncProperty(
        fc.oneof(
          fc.constant('no-interaction'),
          fc.constant('open-then-cancel'),
        ),
        async (scenario) => {
          const queryClient = makeQueryClient()
          const { unmount } = renderSettingsPage(queryClient)

          // Wait for page to render
          await waitFor(() => screen.getByTestId('delete-account-trigger'))

          if (scenario === 'open-then-cancel') {
            // Open the dialog
            await userEvent.click(screen.getByTestId('delete-account-trigger'))

            // Dialog should be open — cancel it
            const cancelButton = await waitFor(() => screen.getByText('Cancel'))
            await userEvent.click(cancelButton)
          }
          // For 'no-interaction', we do nothing

          // In both cases, deleteAccount must NOT have been called
          if (mockUsersApi.deleteAccount.mock.calls.length !== 0) {
            throw new Error(
              `Expected deleteAccount not to be called for scenario "${scenario}", but it was called ${mockUsersApi.deleteAccount.mock.calls.length} time(s)`
            )
          }

          unmount()
        }
      ),
      { numRuns: 50 }
    )
  }, 60_000)
})

// ---------------------------------------------------------------------------
// Feature: delete-account, Property 6: Logout e redirecionamento após exclusão bem-sucedida
// Validates: Requirements 4.2, 4.3
// ---------------------------------------------------------------------------

describe('Property 6: Logout e redirecionamento após exclusão bem-sucedida', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockDigestApi.preview.mockResolvedValue({ html: '', articleCount: 0 })
    mockDigestApi.send.mockResolvedValue({})
    mockUsersApi.getMe.mockResolvedValue({ digestEnabled: false })
    mockUsersApi.updateDigestPreference.mockResolvedValue({ digestEnabled: false })
    mockUsersApi.deleteAccount.mockResolvedValue(undefined)
  })

  it('para qualquer estado autenticado, quando a API retorna 204, logout() é chamado e o usuário é redirecionado para /login', async () => {
    // Feature: delete-account, Property 6: Logout e redirecionamento após exclusão bem-sucedida
    await fc.assert(
      fc.asyncProperty(
        fc.boolean(), // digestEnabled value — represents "any authenticated state"
        async (digestEnabled) => {
          mockUseAuthStore.mockImplementation(makeAuthStoreMock(digestEnabled))
          mockUsersApi.getMe.mockResolvedValue({ digestEnabled })
          mockUsersApi.deleteAccount.mockResolvedValue(undefined)

          const queryClient = makeQueryClient()
          const { unmount } = renderSettingsPage(queryClient)

          // Wait for page to render
          await waitFor(() => screen.getByTestId('delete-account-trigger'))

          // Open the dialog
          await userEvent.click(screen.getByTestId('delete-account-trigger'))

          // Confirm deletion using the confirm button in the dialog
          const confirmBtn = await waitFor(() => screen.getByTestId('delete-account-confirm'))
          await userEvent.click(confirmBtn)

          // logout() must have been called
          await waitFor(() => {
            if (mockLogout.mock.calls.length === 0) {
              throw new Error('logout() was not called after successful deletion')
            }
          })

          // navigate('/login') must have been called
          await waitFor(() => {
            const navigateCalls = mockNavigate.mock.calls
            const redirectedToLogin = navigateCalls.some(call => call[0] === '/login')
            if (!redirectedToLogin) {
              throw new Error(
                `Expected navigate('/login') to be called, but got: ${JSON.stringify(navigateCalls)}`
              )
            }
          })

          unmount()
        }
      ),
      { numRuns: 20 }
    )
  }, 60_000)
})

// ---------------------------------------------------------------------------
// Unit tests for delete-account feature (Sub-task 5.3)
// ---------------------------------------------------------------------------

describe('Delete Account — testes unitários (Req 3.1, 3.2, 3.3, 3.4, 4.4, 4.5)', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockDigestApi.preview.mockResolvedValue({ html: '', articleCount: 0 })
    mockDigestApi.send.mockResolvedValue({})
    mockUseAuthStore.mockImplementation(makeAuthStoreMock(false))
    mockUsersApi.getMe.mockResolvedValue({ digestEnabled: false })
    mockUsersApi.updateDigestPreference.mockResolvedValue({ digestEnabled: false })
    mockUsersApi.deleteAccount.mockResolvedValue(undefined)
  })

  it('botão "Delete Account" está presente na página (Req 3.1)', async () => {
    const queryClient = makeQueryClient()
    renderSettingsPage(queryClient)

    await waitFor(() => {
      expect(screen.getByTestId('delete-account-trigger')).toBeInTheDocument()
    })
  })

  it('clicar em "Delete Account" abre o diálogo de confirmação (Req 3.2)', async () => {
    const queryClient = makeQueryClient()
    renderSettingsPage(queryClient)

    await waitFor(() => screen.getByTestId('delete-account-trigger'))
    await userEvent.click(screen.getByTestId('delete-account-trigger'))

    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeInTheDocument()
    })
  })

  it('diálogo exibe mensagem de irreversibilidade (Req 3.3)', async () => {
    const queryClient = makeQueryClient()
    renderSettingsPage(queryClient)

    await waitFor(() => screen.getByTestId('delete-account-trigger'))
    await userEvent.click(screen.getByTestId('delete-account-trigger'))

    await waitFor(() => {
      expect(screen.getByText(/irreversible/i)).toBeInTheDocument()
    })
  })

  it('cancelar o diálogo fecha sem chamar a API (Req 3.4)', async () => {
    const queryClient = makeQueryClient()
    renderSettingsPage(queryClient)

    await waitFor(() => screen.getByTestId('delete-account-trigger'))
    await userEvent.click(screen.getByTestId('delete-account-trigger'))

    const cancelButton = await waitFor(() => screen.getByText('Cancel'))
    await userEvent.click(cancelButton)

    await waitFor(() => {
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
    })

    expect(mockUsersApi.deleteAccount).not.toHaveBeenCalled()
  })

  it('botão de confirmação fica desabilitado e exibe loading durante a requisição (Req 4.4)', async () => {
    // Never-resolving promise simulates pending state
    mockUsersApi.deleteAccount.mockReturnValue(new Promise(() => {}))

    const queryClient = makeQueryClient()
    renderSettingsPage(queryClient)

    await waitFor(() => screen.getByTestId('delete-account-trigger'))
    await userEvent.click(screen.getByTestId('delete-account-trigger'))

    const confirmBtn = await waitFor(() => screen.getByTestId('delete-account-confirm'))
    await userEvent.click(confirmBtn)

    // Confirm button should be disabled and show loading text
    await waitFor(() => {
      expect(confirmBtn).toBeDisabled()
      expect(screen.getByText('Deleting...')).toBeInTheDocument()
    })
  })

  it('erro da API exibe mensagem e mantém sessão ativa (Req 4.5)', async () => {
    mockUsersApi.deleteAccount.mockRejectedValue(new Error('Server error'))

    const queryClient = makeQueryClient()
    renderSettingsPage(queryClient)

    await waitFor(() => screen.getByTestId('delete-account-trigger'))
    await userEvent.click(screen.getByTestId('delete-account-trigger'))

    const confirmBtn = await waitFor(() => screen.getByTestId('delete-account-confirm'))
    await userEvent.click(confirmBtn)

    // Error message should appear
    await waitFor(() => {
      expect(screen.getByText('Failed to delete account. Please try again.')).toBeInTheDocument()
    })

    // logout should NOT have been called (session stays active)
    expect(mockLogout).not.toHaveBeenCalled()

    // navigate should NOT have been called
    expect(mockNavigate).not.toHaveBeenCalled()
  })
})
