import { ExternalLink, X, ArrowUpRight, Calendar } from 'lucide-react'
import type { NewsArticleDto } from '../types'
import { CategoryLabels, CategoryColors } from '../types'
import { motion } from 'framer-motion'

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
    <motion.aside 
      initial={{ x: '100%', opacity: 0 }}
      animate={{ x: 0, opacity: 1 }}
      exit={{ x: '100%', opacity: 0 }}
      transition={{ type: 'spring', damping: 25, stiffness: 200 }}
      className="
        fixed inset-0 z-50 glass shadow-2xl flex flex-col
        md:static md:w-full md:h-full md:border-l
      "
    >
      <div className="flex items-center justify-between px-6 py-5 border-b border-zinc-200/50 dark:border-zinc-800/50">
        <span className={`inline-flex items-center px-3 py-1 rounded-xl text-[10px] font-bold uppercase tracking-widest ${colors.bg} ${colors.text} bg-opacity-10 backdrop-blur-sm border border-current border-opacity-10`}>
          {CategoryLabels[article.category]}
        </span>
        <button
          onClick={onClose}
          className="p-2.5 rounded-xl text-zinc-400 hover:text-zinc-900 dark:hover:text-zinc-100 hover:bg-zinc-100 dark:hover:bg-zinc-800 transition-all shadow-sm"
        >
          <X size={18} />
        </button>
      </div>

      <div className="flex-1 overflow-y-auto px-8 py-8 custom-scrollbar">
        <div className="flex items-center gap-2 text-[11px] font-bold text-zinc-400 dark:text-zinc-500 uppercase tracking-widest mb-4">
          <Calendar size={12} />
          {article.feedSourceName} · {date}
        </div>

        <h2 className="font-display font-bold text-3xl leading-tight text-zinc-900 dark:text-zinc-100 mb-8 bg-gradient-to-br from-zinc-900 to-zinc-500 dark:from-white dark:to-zinc-500 bg-clip-text text-transparent">
          {article.title}
        </h2>

        {article.summary && (
          <div className="space-y-6">
            <div className="w-12 h-1 bg-indigo-500/20 rounded-full" />
            <p className="text-sm text-zinc-600 dark:text-zinc-300 leading-loose font-medium selection:bg-indigo-500/20">
              {article.summary}
            </p>
          </div>
        )}
      </div>

      <div className="px-8 py-8 border-t border-zinc-200/50 dark:border-zinc-800/50 bg-white/30 dark:bg-zinc-900/10 backdrop-blur-md">
        <a
          href={article.url}
          target="_blank"
          rel="noopener noreferrer"
          className="btn-primary w-full h-12 text-base"
        >
          Read Full Article
          <ArrowUpRight size={18} />
        </a>
        <p className="text-center text-[10px] text-zinc-400 font-bold uppercase tracking-widest mt-4 flex items-center justify-center gap-1.5 opacity-60">
          <ExternalLink size={12} />
          Redirecting to original source
        </p>
      </div>
    </motion.aside>
  )
}
