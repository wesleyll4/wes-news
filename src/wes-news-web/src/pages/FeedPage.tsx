import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useUiStore } from '../store/uiStore'
import { newsApi } from '../api/client'
import ArticleCard from '../components/ArticleCard'
import ArticleReader from '../components/ArticleReader'
import SearchBar from '../components/SearchBar'
import { Loader2, RefreshCw, Inbox, Sparkles, Brain } from 'lucide-react'
import type { NewsArticleDto } from '../types'
import { CategoryLabels } from '../types'
import { motion, AnimatePresence } from 'framer-motion'

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

  const featuredArticles = data?.items.filter((a) => a.isFeatured) ?? []
  const regularArticles = data?.items.filter((a) => !a.isFeatured) ?? []

  const pageTitle = selectedCategory
    ? CategoryLabels[selectedCategory]
    : unreadOnly
    ? 'Unread'
    : 'All News'

  return (
    <div className="flex h-full overflow-hidden bg-transparent">
      <div className="flex-1 flex flex-col min-w-0 overflow-hidden relative">
        <header className="px-6 py-4 glass border-b z-20 flex items-center gap-4 shrink-0">
          <div className="flex-1">
            <SearchBar />
          </div>
          <button
            onClick={() => refetch()}
            className={`p-2.5 rounded-xl border border-zinc-200 dark:border-zinc-800 text-zinc-400 hover:text-indigo-500 hover:bg-indigo-50 dark:hover:bg-indigo-500/10 transition-all ${isFetching ? 'animate-spin text-indigo-500' : ''}`}
            title="Refresh"
          >
            <RefreshCw size={18} />
          </button>
        </header>

        <div className="px-6 py-5 flex items-center justify-between shrink-0 z-10">
          <div className="flex items-center gap-2">
            <Sparkles size={18} className="text-indigo-500" />
            <h1 className="font-display font-bold text-xl tracking-tight bg-gradient-to-br from-zinc-900 to-zinc-500 dark:from-white dark:to-zinc-500 bg-clip-text text-transparent">
              {pageTitle}
            </h1>
          </div>
          {data && (
            <span className="text-[11px] font-bold text-zinc-400 dark:text-zinc-600 tracking-wider uppercase tabular-nums">
              {data.totalCount} Articles
            </span>
          )}
        </div>

        <motion.div
          layout
          className="flex-1 overflow-y-auto custom-scrollbar pb-10"
        >
          <AnimatePresence>
            {isLoading ? (
              <motion.div
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                exit={{ opacity: 0 }}
                className="flex flex-col items-center justify-center h-64 gap-4 text-zinc-400"
              >
                <div className="relative">
                  <div className="absolute inset-0 bg-indigo-500 blur-[80px] opacity-10 animate-pulse" />
                  <Loader2 size={32} className="animate-spin text-indigo-500 relative" />
                </div>
                <span className="text-sm font-display font-medium tracking-widest uppercase opacity-60">Synthesizing feed...</span>
              </motion.div>
            ) : data?.items.length === 0 ? (
              <motion.div
                initial={{ opacity: 0, scale: 0.9 }}
                animate={{ opacity: 1, scale: 1 }}
                className="flex flex-col items-center justify-center h-64 gap-4 text-zinc-400"
              >
                <Inbox size={40} className="opacity-20" />
                <span className="text-sm font-medium">No articles in this frequency</span>
              </motion.div>
            ) : (
              <motion.div
                key={`feed-grid-${selectedCategory}-${unreadOnly}`}
                variants={{
                  hidden: { opacity: 0 },
                  show: { opacity: 1, transition: { staggerChildren: 0.05 } }
                }}
                initial="hidden"
                animate="show"
                className="grid grid-cols-1 gap-2 px-2"
              >
                {featuredArticles.length > 0 && (
                  <motion.div
                    variants={{ hidden: { opacity: 0, y: -8 }, show: { opacity: 1, y: 0 } }}
                    className="mb-1"
                  >
                    <div className="flex items-center gap-2 px-2 py-3">
                      <Brain size={14} className="text-indigo-500 shrink-0" />
                      <span className="text-[11px] font-bold uppercase tracking-widest text-indigo-500">
                        AI Picks
                      </span>
                      <div className="flex-1 h-px bg-indigo-200 dark:bg-indigo-900" />
                      <span className="text-[10px] text-zinc-400">{featuredArticles.length} selected by Gemini</span>
                    </div>
                    <div className="grid grid-cols-1 gap-2">
                      {featuredArticles.map((article) => (
                        <ArticleCard
                          key={article.id}
                          article={article}
                          isSelected={article.id === selectedArticleId}
                          onClick={() => handleArticleClick(article)}
                        />
                      ))}
                    </div>

                    {regularArticles.length > 0 && (
                      <div className="flex items-center gap-2 px-2 py-3 mt-2">
                        <span className="text-[11px] font-bold uppercase tracking-widest text-zinc-400 dark:text-zinc-600">
                          All Articles
                        </span>
                        <div className="flex-1 h-px bg-zinc-100 dark:bg-zinc-800" />
                      </div>
                    )}
                  </motion.div>
                )}

                {regularArticles.map((article) => (
                  <ArticleCard
                    key={article.id}
                    article={article}
                    isSelected={article.id === selectedArticleId}
                    onClick={() => handleArticleClick(article)}
                  />
                ))}
              </motion.div>
            )}
          </AnimatePresence>
        </motion.div>
      </div>

      <AnimatePresence>
        {selectedArticle && (
          <motion.div
            initial={{ width: 0, opacity: 0 }}
            animate={{ width: 'auto', opacity: 1 }}
            exit={{ width: 0, opacity: 0 }}
            transition={{ type: 'spring', damping: 30, stiffness: 300 }}
            className="hidden md:block overflow-hidden relative"
          >
            <div className="w-[420px] h-full">
              <ArticleReader
                article={selectedArticle}
                onClose={() => setSelectedArticleId(undefined)}
              />
            </div>
          </motion.div>
        )}
      </AnimatePresence>

      <AnimatePresence>
        {selectedArticle && (
          <div className="md:hidden">
            <ArticleReader
              article={selectedArticle}
              onClose={() => setSelectedArticleId(undefined)}
            />
          </div>
        )}
      </AnimatePresence>
    </div>
  )
}
