import axios from 'axios'
import type { NewsArticleDto, FeedSourceDto, PagedResult, DigestPreviewDto, CreateFeedSourceRequest, UpdateFeedSourceRequest, Category } from '../types'

const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' }
})

export interface NewsQueryParams {
  category?: Category
  search?: string
  unreadOnly?: boolean
  page?: number
  pageSize?: number
}

export const newsApi = {
  getAll: (params: NewsQueryParams = {}) =>
    api.get<PagedResult<NewsArticleDto>>('/news', { params }).then(r => r.data),

  markAsRead: (id: string) =>
    api.patch(`/news/${id}/read`),

  delete: (id: string) =>
    api.delete(`/news/${id}`)
}

export const feedsApi = {
  getAll: () =>
    api.get<FeedSourceDto[]>('/feeds').then(r => r.data),

  create: (data: CreateFeedSourceRequest) =>
    api.post<FeedSourceDto>('/feeds', data).then(r => r.data),

  update: (id: string, data: UpdateFeedSourceRequest) =>
    api.put(`/feeds/${id}`, data),

  delete: (id: string) =>
    api.delete(`/feeds/${id}`)
}

export const digestApi = {
  preview: () =>
    api.get<DigestPreviewDto>('/digest/preview').then(r => r.data),

  send: () =>
    api.post('/digest/send')
}
