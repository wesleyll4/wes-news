import { ExternalLink } from 'lucide-react'
import type { NewsArticleDto } from '../types'
import { CategoryLabels } from '../types'

interface ArticleCardProps {
  article: NewsArticleDto
  isSelected: boolean
  onClick: () => void
}

export default function ArticleCard({ article, isSelected, onClick }: ArticleCardProps) {
  const publishedDate = new Date(article.publishedAt).toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: 'short',
    hour: '2-digit',
    minute: '2-digit'
  })

  return (
    <button
      onClick={onClick}
      className={`w-full text-left px-4 py-3 border-b border-gray-100 dark:border-gray-800 hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors ${isSelected ? 'bg-blue-50 dark:bg-blue-950/40 border-l-2 border-l-blue-500' : ''} ${!article.isRead ? '' : 'opacity-60'}`}
    >
      <div className="flex items-start justify-between gap-2">
        <div className="flex-1 min-w-0">
          <p className={`text-sm leading-snug line-clamp-2 ${!article.isRead ? 'font-semibold' : 'font-normal'}`}>
            {article.title}
          </p>
          <div className="flex items-center gap-2 mt-1">
            <span className="text-xs text-blue-500 dark:text-blue-400 font-medium">
              {CategoryLabels[article.category]}
            </span>
            <span className="text-xs text-gray-400">·</span>
            <span className="text-xs text-gray-400 truncate">{article.feedSourceName}</span>
            <span className="text-xs text-gray-400">·</span>
            <span className="text-xs text-gray-400">{publishedDate}</span>
          </div>
        </div>
        <a
          href={article.url}
          target="_blank"
          rel="noopener noreferrer"
          onClick={(e) => e.stopPropagation()}
          className="shrink-0 p-1 text-gray-400 hover:text-blue-500 rounded"
        >
          <ExternalLink size={13} />
        </a>
      </div>
    </button>
  )
}
