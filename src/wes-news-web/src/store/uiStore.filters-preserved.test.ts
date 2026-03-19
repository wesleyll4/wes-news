/**
 * Feature: public-access, Property 12: UI filters preserved after login
 *
 * Validates: Requirements 5.3
 *
 * Para qualquer combinação de `selectedCategory` e `searchTerm` definidos antes
 * do login, após login bem-sucedido esses valores no `uiStore` devem permanecer
 * inalterados.
 */

import { describe, it, beforeEach } from 'vitest'
import * as fc from 'fast-check'
import { useUiStore } from './uiStore'
import { useAuthStore } from './authStore'
import { Category } from '../types'

// Reset both stores to their initial state before each test run
function resetStores() {
  useUiStore.setState({
    selectedCategory: undefined,
    searchTerm: '',
    unreadOnly: false,
    selectedArticleId: undefined,
    sidebarOpen: false,
  })
  useAuthStore.setState({
    token: null,
    role: null,
    isAuthenticated: false,
    digestEnabled: false,
  })
}

// Arbitrary for Category enum values or undefined (null in the spec maps to undefined here)
const categoryArb = fc.option(
  fc.constantFrom(
    Category.DotNet,
    Category.AI,
    Category.Architecture,
    Category.DevOps,
    Category.General,
    Category.GitHubTrends
  ),
  { nil: undefined }
)

// Arbitrary for search term strings (including empty string)
const searchTermArb = fc.string()

describe('Property 12: Filtros de UI preservados após login', () => {
  beforeEach(() => {
    resetStores()
  })

  it('para qualquer selectedCategory e searchTerm, login não altera os filtros do uiStore', () => {
    fc.assert(
      fc.property(
        categoryArb,
        searchTermArb,
        fc.string({ minLength: 1 }), // token
        fc.constantFrom('User', 'Admin', 'Curator'), // role
        fc.boolean(), // digestEnabled
        (selectedCategory, searchTerm, token, role, digestEnabled) => {
          resetStores()

          // Set UI filters before login
          useUiStore.getState().setSelectedCategory(selectedCategory)
          useUiStore.getState().setSearchTerm(searchTerm)

          // Perform login
          useAuthStore.getState().login(token, role, digestEnabled)

          // Assert UI filters are unchanged
          const { selectedCategory: catAfter, searchTerm: termAfter } = useUiStore.getState()

          return catAfter === selectedCategory && termAfter === searchTerm
        }
      ),
      { numRuns: 100 }
    )
  })
})
