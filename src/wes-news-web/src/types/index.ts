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
