export enum Category {
  DotNet = 1,
  AI = 2,
  Architecture = 3,
  DevOps = 4,
  General = 5
}

export const CategoryLabels: Record<Category, string> = {
  [Category.DotNet]: '.NET',
  [Category.AI]: 'AI',
  [Category.Architecture]: 'Architecture',
  [Category.DevOps]: 'DevOps',
  [Category.General]: 'General'
}

export const CategoryColors: Record<Category, { bg: string; text: string; dot: string }> = {
  [Category.DotNet]:      { bg: 'bg-violet-100 dark:bg-violet-950/60', text: 'text-violet-700 dark:text-violet-400', dot: 'bg-violet-500' },
  [Category.AI]:          { bg: 'bg-rose-100 dark:bg-rose-950/60',     text: 'text-rose-600 dark:text-rose-400',    dot: 'bg-rose-500' },
  [Category.Architecture]:{ bg: 'bg-amber-100 dark:bg-amber-950/60',   text: 'text-amber-700 dark:text-amber-400',  dot: 'bg-amber-500' },
  [Category.DevOps]:      { bg: 'bg-emerald-100 dark:bg-emerald-950/60', text: 'text-emerald-700 dark:text-emerald-400', dot: 'bg-emerald-500' },
  [Category.General]:     { bg: 'bg-sky-100 dark:bg-sky-950/60',       text: 'text-sky-700 dark:text-sky-400',      dot: 'bg-sky-500' },
}

export interface NewsArticleDto {
  id: string
  title: string
  summary: string
  url: string
  imageUrl?: string
  publishedAt: string
  isRead: boolean
  category: Category
  feedSourceName: string
}

export interface FeedSourceDto {
  id: string
  name: string
  url: string
  category: Category
  isActive: boolean
  lastFetchedAt?: string
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

export interface DigestPreviewDto {
  html: string
  articleCount: number
}

export interface CreateFeedSourceRequest {
  name: string
  url: string
  category: Category
}

export interface UpdateFeedSourceRequest {
  name?: string
  isActive?: boolean
  category?: Category
}
