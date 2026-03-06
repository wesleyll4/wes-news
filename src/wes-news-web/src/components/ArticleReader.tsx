import { ExternalLink, X, ArrowUpRight } from 'lucide-react'
import type { NewsArticleDto } from '../types'
import { CategoryLabels, CategoryColors } from '../types'

interface ArticleReaderProps {
  article: NewsArticleDto
  onClose: () => void
}

export default function ArticleReader({ article, onClose }: ArticleReaderProps) {
  const colors = CategoryColors[article.category]

  const date = new Date(article.publishedAt).toLocaleDateString('pt-BR', {
    weekday: 'short',
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  })

  return (
    <aside className="
      fixed inset-0 z-50 bg-white dark:bg-zinc-950 flex flex-col
      md:relative md:inset-auto md:z-auto md:w-[380px] md:border-l md:border-zinc-100 md:dark:border-zinc-800
    ">
      <div className="flex items-center justify-between px-5 py-4 border-b border-zinc-100 dark:border-zinc-800">
        <span className={`inline-flex items-center px-2.5 py-1 rounded-full text-[10px] font-semibold uppercase tracking-wider ${colors.bg} ${colors.text}`}>
          {CategoryLabels[article.category]}
        </span>
        <button
          onClick={onClose}
          className="p-2 rounded-lg text-zinc-400 hover:text-zinc-700 dark:hover:text-zinc-200 hover:bg-zinc-100 dark:hover:bg-zinc-800 transition-colors"
        >
          <X size={16} />
        </button>
      </div>

      <div className="flex-1 overflow-y-auto px-5 py-6">
        <p className="text-[11px] font-medium text-zinc-400 dark:text-zinc-500 uppercase tracking-wider mb-3">
          {article.feedSourceName} · {date}
        </p>

        <h2 className="font-display font-bold text-2xl leading-tight text-zinc-900 dark:text-zinc-100 mb-5">
          {article.title}
        </h2>

        {article.summary && (
          <>
            <div className="w-8 h-0.5 bg-zinc-200 dark:bg-zinc-700 mb-5" />
            <p className="text-sm text-zinc-600 dark:text-zinc-300 leading-relaxed">
              {article.summary}
            </p>
          </>
        )}
      </div>

      <div className="px-5 py-5 border-t border-zinc-100 dark:border-zinc-800">
        <a
          href={article.url}
          target="_blank"
          rel="noopener noreferrer"
          className="btn-primary w-full justify-center"
        >
          Read Full Article
          <ArrowUpRight size={15} />
        </a>
        <p className="text-center text-[11px] text-zinc-400 mt-3 flex items-center justify-center gap-1">
          <ExternalLink size={10} />
          Opens in a new tab
        </p>
      </div>
    </aside>
  )
}
