import { ExternalLink, X } from 'lucide-react'
import type { NewsArticleDto } from '../types'
import { CategoryLabels } from '../types'

interface ArticleReaderProps {
  article: NewsArticleDto
  onClose: () => void
}

export default function ArticleReader({ article, onClose }: ArticleReaderProps) {
  const publishedDate = new Date(article.publishedAt).toLocaleDateString('pt-BR', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })

  return (
    <aside className="w-96 border-l border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 flex flex-col overflow-hidden">
      <div className="px-4 py-3 border-b border-gray-200 dark:border-gray-800 flex items-center justify-between">
        <span className="text-xs font-semibold text-blue-500 dark:text-blue-400">
          {CategoryLabels[article.category]}
        </span>
        <button onClick={onClose} className="p-1 rounded hover:bg-gray-100 dark:hover:bg-gray-800">
          <X size={16} />
        </button>
      </div>

      <div className="flex-1 overflow-y-auto p-4">
        <h2 className="text-base font-bold leading-snug mb-2">{article.title}</h2>
        <p className="text-xs text-gray-400 mb-4">
          {article.feedSourceName} · {publishedDate}
        </p>
        {article.summary && (
          <p className="text-sm text-gray-600 dark:text-gray-300 leading-relaxed mb-6">
            {article.summary}
          </p>
        )}
        <a
          href={article.url}
          target="_blank"
          rel="noopener noreferrer"
          className="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm rounded-lg transition-colors"
        >
          <ExternalLink size={14} />
          Read Full Article
        </a>
      </div>
    </aside>
  )
}
