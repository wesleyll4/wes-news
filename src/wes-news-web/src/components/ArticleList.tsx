import type { NewsArticleDto } from '../types'
import ArticleCard from './ArticleCard'

interface ArticleListProps {
  articles: NewsArticleDto[]
  selectedId: string | undefined
  onSelect: (article: NewsArticleDto) => void
}

export default function ArticleList({ articles, selectedId, onSelect }: ArticleListProps) {
  return (
    <div className="flex-1 overflow-y-auto">
      {articles.map((article) => (
        <ArticleCard
          key={article.id}
          article={article}
          isSelected={article.id === selectedId}
          onClick={() => onSelect(article)}
        />
      ))}
    </div>
  )
}
