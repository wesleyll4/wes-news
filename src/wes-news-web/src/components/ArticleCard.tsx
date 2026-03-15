import type { NewsArticleDto } from '../types'
import { CategoryLabels, CategoryColors } from '../types'
import { ExternalLink, Clock, ChevronRight } from 'lucide-react'
import { motion } from 'framer-motion'

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
    <motion.div
      initial={{ opacity: 0, y: 10 }}
      animate={{ opacity: 1, y: 0 }}
      layout
      className="px-4 py-2"
    >
      <button
        onClick={onClick}
        className={`
          w-full text-left group relative p-5 rounded-2xl transition-colors duration-300
          ${isSelected 
            ? 'glass-card ring-2 ring-indigo-500/20 dark:ring-indigo-400/20' 
            : 'hover:bg-white/50 dark:hover:bg-zinc-900/40 border border-transparent'
          }
        `}
      >
        <div className="flex items-start gap-4">
          <div className="flex-1 min-w-0 space-y-3">
            <div className="flex items-center gap-2.5">
              <span className={`inline-flex items-center px-2.5 py-1 rounded-lg text-[10px] font-bold uppercase tracking-wider ${colors.bg} ${colors.text} bg-opacity-10 backdrop-blur-sm border border-current border-opacity-10`}>
                {CategoryLabels[article.category]}
              </span>
              <div className="flex items-center gap-1.5 text-[10px] font-medium text-zinc-400 dark:text-zinc-500">
                <Clock size={12} className="opacity-70" />
                <span className="shrink-0">{timeAgo}</span>
              </div>
              {!article.isRead && (
                <span className="w-1.5 h-1.5 rounded-full bg-indigo-500 animate-pulse shrink-0" />
              )}
            </div>

            <p className={`text-sm leading-relaxed line-clamp-2 transition-colors ${!article.isRead ? 'font-bold text-zinc-900 dark:text-zinc-100' : 'font-medium text-zinc-500 dark:text-zinc-400'}`}>
              {article.title}
            </p>

            <div className="flex items-center justify-between group/footer">
              <span className="text-[11px] font-bold text-zinc-400 dark:text-zinc-600 tracking-wide uppercase">
                {article.feedSourceName}
              </span>
              
              <div className="flex items-center gap-2 opacity-0 group-hover:opacity-100 transition-all translate-x-1 group-hover:translate-x-0">
                <a
                  href={article.url}
                  target="_blank"
                  rel="noopener noreferrer"
                  onClick={(e) => e.stopPropagation()}
                  className="p-1.5 rounded-lg text-zinc-400 hover:text-indigo-500 hover:bg-indigo-50 dark:hover:bg-indigo-500/10 transition-colors"
                >
                  <ExternalLink size={14} />
                </a>
                <ChevronRight size={14} className="text-zinc-300" />
              </div>
            </div>
          </div>
        </div>

        {/* Selected Accent */}
        {isSelected && (
          <motion.div 
            layoutId="active-pill"
            className="absolute left-0 top-1/4 bottom-1/4 w-1 bg-indigo-500 rounded-r-full shadow-[0_0_12px_rgba(99,102,241,0.5)]"
          />
        )}
      </button>
    </motion.div>
  )
}
