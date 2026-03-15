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
      variants={{
        hidden: { opacity: 0, y: 20, scale: 0.98 },
        show: { opacity: 1, y: 0, scale: 1 }
      }}
      transition={{ type: 'spring', damping: 25, stiffness: 120 }}
      whileHover={{ y: -6, transition: { duration: 0.3 } }}
      layout
      className="px-6 py-3"
    >
      <button
        onClick={onClick}
        className={`
          w-full text-left group relative p-6 rounded-[2rem] transition-all duration-500
          ${isSelected 
            ? 'glass-card ring-2 ring-indigo-500/40 shadow-2xl scale-[1.01]' 
            : 'glass-card hover:bg-white/60 dark:hover:bg-white/[0.12]'
          }
        `}
      >
        <div className="flex items-start gap-6">
          <div className="flex-1 min-w-0 space-y-4">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                <span className={`inline-flex items-center px-3 py-1 rounded-xl text-[10px] font-bold uppercase tracking-[0.15em] ${colors.bg} ${colors.text} bg-opacity-10 backdrop-blur-md border border-current border-opacity-15`}>
                  {CategoryLabels[article.category]}
                </span>
                <div className="flex items-center gap-1.5 text-[10px] font-bold text-zinc-400 dark:text-zinc-500 tracking-wider">
                  <Clock size={12} className="opacity-50" />
                  <span>{timeAgo.toUpperCase()}</span>
                </div>
              </div>
              {!article.isRead && (
                <div className="flex items-center gap-2">
                  <span className="text-[10px] font-bold text-indigo-500 dark:text-indigo-400 uppercase tracking-widest">New</span>
                  <span className="w-2 h-2 rounded-full bg-indigo-500 shadow-[0_0_10px_rgba(99,102,241,0.8)] animate-pulse" />
                </div>
              )}
            </div>

            <h3 className={`text-lg leading-tight transition-colors ${!article.isRead ? 'font-display font-bold text-zinc-900 dark:text-white' : 'font-display font-medium text-zinc-500 dark:text-zinc-400'}`}>
              {article.title}
            </h3>

            <div className="flex items-center justify-between pt-2 border-t border-zinc-200/50 dark:border-white/[0.05]">
              <div className="flex items-center gap-2">
                <div className="w-6 h-6 rounded-lg bg-zinc-100 dark:bg-white/5 flex items-center justify-center">
                  <span className="text-[10px] font-bold text-zinc-500">{article.feedSourceName.charAt(0)}</span>
                </div>
                <span className="text-[11px] font-bold text-zinc-400 dark:text-zinc-500 tracking-wide uppercase">
                  {article.feedSourceName}
                </span>
              </div>
              
              <div className="flex items-center gap-3 opacity-0 group-hover:opacity-100 transition-all duration-300 translate-x-2 group-hover:translate-x-0">
                <a
                  href={article.url}
                  target="_blank"
                  rel="noopener noreferrer"
                  onClick={(e) => e.stopPropagation()}
                  className="p-2 rounded-xl text-zinc-400 hover:text-indigo-500 hover:bg-indigo-50 dark:hover:bg-indigo-500/10 transition-colors border border-transparent hover:border-indigo-500/20"
                >
                  <ExternalLink size={16} />
                </a>
                <div className="w-8 h-8 rounded-full bg-indigo-500/10 flex items-center justify-center text-indigo-500">
                  <ChevronRight size={18} />
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Selected Accent */}
        {isSelected && (
          <motion.div 
            layoutId="active-pill"
            className="absolute left-[-1px] top-1/3 bottom-1/3 w-1.5 bg-indigo-500 rounded-r-full shadow-[0_0_20px_rgba(99,102,241,0.6)]"
          />
        )}
      </button>
    </motion.div>
  )
}
