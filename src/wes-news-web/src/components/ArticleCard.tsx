import type { NewsArticleDto } from '../types'
import { CategoryLabels, CategoryColors } from '../types'
import { ExternalLink } from 'lucide-react'

interface ArticleCardProps {
  article: NewsArticleDto
  isSelected: boolean
  onClick: () => void
}

export default function ArticleCard({ article, isSelected, onClick }: ArticleCardProps) {
  const colors = CategoryColors[article.category]

  const timeAgo = (() => {
    const diff = Date.now() - new Date(article.publishedAt).getTime()
    const h = Math.floor(diff / 3600000)
    const d = Math.floor(diff / 86400000)
    if (h < 1) return 'just now'
    if (h < 24) return `${h}h ago`
    if (d < 7) return `${d}d ago`
    return new Date(article.publishedAt).toLocaleDateString('pt-BR', { day: '2-digit', month: 'short' })
  })()

  return (
    <button
      onClick={onClick}
      className={`
        w-full text-left group px-4 py-4 border-b border-zinc-100 dark:border-zinc-800/80
        transition-colors hover:bg-zinc-50 dark:hover:bg-zinc-900
        ${isSelected ? 'bg-zinc-50 dark:bg-zinc-900 border-l-2 border-l-zinc-900 dark:border-l-zinc-100' : ''}
      `}
    >
      <div className="flex items-start gap-3">
        <div className="flex-1 min-w-0 space-y-1.5">
          <div className="flex items-center gap-2">
            <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-semibold uppercase tracking-wider ${colors.bg} ${colors.text}`}>
              {CategoryLabels[article.category]}
            </span>
            {!article.isRead && (
              <span className="w-1.5 h-1.5 rounded-full bg-zinc-900 dark:bg-zinc-100 shrink-0" />
            )}
          </div>

          <p className={`text-[13px] leading-snug line-clamp-2 ${!article.isRead ? 'font-semibold text-zinc-900 dark:text-zinc-100' : 'font-normal text-zinc-500 dark:text-zinc-400'}`}>
            {article.title}
          </p>

          <div className="flex items-center gap-1.5 text-[11px] text-zinc-400 dark:text-zinc-500">
            <span className="truncate max-w-[120px]">{article.feedSourceName}</span>
            <span>·</span>
            <span className="shrink-0">{timeAgo}</span>
          </div>
        </div>

        <a
          href={article.url}
          target="_blank"
          rel="noopener noreferrer"
          onClick={(e) => e.stopPropagation()}
          className="shrink-0 mt-0.5 p-1.5 rounded-md text-zinc-300 dark:text-zinc-600 hover:text-zinc-700 dark:hover:text-zinc-300 hover:bg-zinc-100 dark:hover:bg-zinc-800 transition-colors opacity-0 group-hover:opacity-100"
        >
          <ExternalLink size={13} />
        </a>
      </div>
    </button>
  )
}
