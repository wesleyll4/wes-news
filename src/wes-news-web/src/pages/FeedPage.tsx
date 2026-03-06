import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useUiStore } from '../store/uiStore'
import { newsApi } from '../api/client'
import ArticleCard from '../components/ArticleCard'
import ArticleReader from '../components/ArticleReader'
import SearchBar from '../components/SearchBar'
import { Loader2, RefreshCw, Inbox } from 'lucide-react'
import type { NewsArticleDto } from '../types'
import { CategoryLabels } from '../types'

export default function FeedPage() {
  const { selectedCategory, searchTerm, unreadOnly, selectedArticleId, setSelectedArticleId } = useUiStore()
  const queryClient = useQueryClient()

  const { data, isLoading, refetch, isFetching } = useQuery({
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

  function handleArticleClick(article: NewsArticleDto) {
    setSelectedArticleId(article.id)
    if (!article.isRead) {
      markAsReadMutation.mutate(article.id)
    }
  }

  const selectedArticle = data?.items.find((a) => a.id === selectedArticleId)

  const pageTitle = selectedCategory
    ? CategoryLabels[selectedCategory]
    : unreadOnly
    ? 'Unread'
    : 'All News'

  return (
    <div className="flex h-full overflow-hidden">
      <div className="flex-1 flex flex-col min-w-0 overflow-hidden">
        <div className="px-4 py-3 border-b border-zinc-100 dark:border-zinc-800 flex items-center gap-3 shrink-0">
          <div className="flex-1">
            <SearchBar />
          </div>
          <button
            onClick={() => refetch()}
            className={`p-2 rounded-lg text-zinc-400 hover:text-zinc-700 dark:hover:text-zinc-200 hover:bg-zinc-100 dark:hover:bg-zinc-800 transition-colors ${isFetching ? 'animate-spin' : ''}`}
          >
            <RefreshCw size={15} />
          </button>
        </div>

        <div className="px-4 py-3 border-b border-zinc-100 dark:border-zinc-800 flex items-center justify-between shrink-0">
          <h1 className="font-display font-bold text-lg tracking-tight">{pageTitle}</h1>
          {data && (
            <span className="text-xs text-zinc-400 tabular-nums">{data.totalCount} articles</span>
          )}
        </div>

        <div className="flex-1 overflow-y-auto">
          {isLoading ? (
            <div className="flex flex-col items-center justify-center h-48 gap-3 text-zinc-400">
              <Loader2 size={22} className="animate-spin" />
              <span className="text-sm">Loading articles...</span>
            </div>
          ) : data?.items.length === 0 ? (
            <div className="flex flex-col items-center justify-center h-48 gap-3 text-zinc-400">
              <Inbox size={28} />
              <span className="text-sm">No articles found</span>
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
