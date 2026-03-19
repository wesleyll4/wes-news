/**
 * Feature: public-access, Property 10: mark-as-read hidden for visitors
 *
 * Validates: Requirements 4.2
 *
 * Para `isAuthenticated=false`, clicar em qualquer artigo não deve chamar
 * `markAsRead` (nenhuma requisição PATCH enviada).
 */

import { render, fireEvent, act } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router-dom'
import { vi, describe, it, beforeEach } from 'vitest'
import * as fc from 'fast-check'

// --- Mocks ---

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
  },
}))

// framer-motion: render children directly to avoid animation complexity in tests
vi.mock('framer-motion', () => ({
  motion: {
    div: ({ children, ...props }: React.HTMLAttributes<HTMLDivElement> & { children?: React.ReactNode }) =>
      <div {...props}>{children}</div>,
  },
  AnimatePresence: ({ children }: { children?: React.ReactNode }) => <>{children}</>,
}))

import FeedPage from './FeedPage'
import { useAuthStore } from '../store/authStore'
import { useUiStore } from '../store/uiStore'
import { newsApi } from '../api/client'
import type { NewsArticleDto } from '../types'
import { Category } from '../types'

const mockUseAuthStore = useAuthStore as unknown as ReturnType<typeof vi.fn>
const mockUseUiStore = useUiStore as unknown as ReturnType<typeof vi.fn>
const mockMarkAsRead = newsApi.markAsRead as ReturnType<typeof vi.fn>
const mockGetAll = newsApi.getAll as ReturnType<typeof vi.fn>

const noopFn = vi.fn()

const baseUiState = {
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
    if (typeof selector === 'function') return selector(state)
    return state
  }
}

function buildArticle(id: string, isRead = false): NewsArticleDto {
  return {
    id,
    title: `Article ${id}`,
    summary: 'Summary',
    url: `https://example.com/${id}`,
    publishedAt: new Date().toISOString(),
    isRead,
    isFeatured: false,
    category: Category.General,
    feedSourceName: 'Test Feed',
  }
}

function renderFeedPage(articles: NewsArticleDto[]) {
  // Pre-populate the query cache so FeedPage renders articles immediately
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  })
  queryClient.setQueryData(
    ['news', undefined, '', false],
    { items: articles, totalCount: articles.length, page: 1, pageSize: 50, totalPages: 1, hasNextPage: false, hasPreviousPage: false }
  )

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <FeedPage />
      </MemoryRouter>
    </QueryClientProvider>
  )
}

describe('Property 10: Controle de marcar como lido oculto para visitantes', () => {
  beforeEach(() => {
    vi.clearAllMocks()

    mockUseAuthStore.mockImplementation(
      makeStoreHook({ isAuthenticated: false, token: null, role: null, digestEnabled: false, login: noopFn, logout: noopFn })
    )
    mockUseUiStore.mockImplementation(makeStoreHook(baseUiState))
    mockMarkAsRead.mockResolvedValue(undefined)
    mockGetAll.mockResolvedValue({ items: [], totalCount: 0, page: 1, pageSize: 50, totalPages: 0, hasNextPage: false, hasPreviousPage: false })
  })

  it('para isAuthenticated=false, clicar em qualquer artigo não deve chamar markAsRead', async () => {
    await fc.assert(
      fc.asyncProperty(
        // Generate 1–5 article IDs
        fc.array(fc.uuid(), { minLength: 1, maxLength: 5 }),
        // Pick which article button to click (index)
        fc.nat({ max: 4 }),
        async (articleIds, clickIndexRaw) => {
          vi.clearAllMocks()

          mockUseAuthStore.mockImplementation(
            makeStoreHook({ isAuthenticated: false, token: null, role: null, digestEnabled: false, login: noopFn, logout: noopFn })
          )
          mockUseUiStore.mockImplementation(makeStoreHook({ ...baseUiState, setSelectedArticleId: noopFn }))
          mockMarkAsRead.mockResolvedValue(undefined)

          const articles = articleIds.map((id) => buildArticle(id, false))

          let container!: HTMLElement
          let unmount!: () => void

          await act(async () => {
            const result = renderFeedPage(articles)
            container = result.container
            unmount = result.unmount
          })

          // Find article buttons rendered by ArticleCard
          const buttons = container.querySelectorAll('button')

          if (buttons.length > 0) {
            const clickIndex = clickIndexRaw % buttons.length
            await act(async () => {
              fireEvent.click(buttons[clickIndex])
            })
          }

          // Allow any async mutations to settle
          await act(async () => {
            await new Promise((resolve) => setTimeout(resolve, 0))
          })

          const wasCalled = mockMarkAsRead.mock.calls.length > 0

          unmount()

          if (wasCalled) {
            throw new Error(
              `markAsRead foi chamado mesmo com isAuthenticated=false (artigos: ${articleIds.join(', ')})`
            )
          }
        }
      ),
      { numRuns: 100 }
    )
  }, 30000)
})
