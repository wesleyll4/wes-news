import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useUiStore } from '../store/uiStore'
import { newsApi } from '../api/client'
import ArticleCard from '../components/ArticleCard'
import ArticleReader from '../components/ArticleReader'
import SearchBar from '../components/SearchBar'
import { Loader2, RefreshCw } from 'lucide-react'
import type { NewsArticleDto } from '../types'

export default function FeedPage() {
  const { selectedCategory, searchTerm, unreadOnly, selectedArticleId, setSelectedArticleId } = useUiStore()
  const queryClient = useQueryClient()

  const { data, isLoading, refetch } = useQuery({
    queryKey: ['news', selectedCategory, searchTerm, unreadOnly],
    queryFn: () => newsApi.getAll({
      category: selectedCategory,
      search: searchTerm || undefined,
      unreadOnly,
      pageSize: 50
    })
  })

  const markAsReadMutation = useMutation({
    mutationFn: newsApi.markAsRead,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['news'] })
  })

  const handleArticleClick = (article: NewsArticleDto) => {
    setSelectedArticleId(article.id)
    if (!article.isRead) {
      markAsReadMutation.mutate(article.id)
    }
  }

  const selectedArticle = data?.items.find((a) => a.id === selectedArticleId)

  return (
    <div className="flex h-full">
      <div className="flex-1 flex flex-col min-w-0">
        <div className="px-4 py-3 border-b border-gray-200 dark:border-gray-800 flex items-center gap-3">
          <div className="flex-1">
            <SearchBar />
          </div>
          <button
            onClick={() => refetch()}
            className="p-2 rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
          >
            <RefreshCw size={16} />
          </button>
        </div>

        <div className="flex-1 overflow-y-auto">
          {isLoading ? (
            <div className="flex items-center justify-center h-40">
              <Loader2 size={24} className="animate-spin text-blue-500" />
            </div>
          ) : data?.items.length === 0 ? (
            <div className="flex items-center justify-center h-40 text-sm text-gray-400">
              No articles found
            </div>
          ) : (
            data?.items.map((article) => (
              <ArticleCard
                key={article.id}
                article={article}
                isSelected={article.id === selectedArticleId}
                onClick={() => handleArticleClick(article)}
              />
            ))
          )}
        </div>

        {data && (
          <div className="px-4 py-2 border-t border-gray-200 dark:border-gray-800 text-xs text-gray-400">
            {data.totalCount} articles
          </div>
        )}
      </div>

      {selectedArticle && (
        <ArticleReader
          article={selectedArticle}
          onClose={() => setSelectedArticleId(undefined)}
        />
      )}
    </div>
  )
}
