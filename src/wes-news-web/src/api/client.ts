import axios from 'axios'
import { useAuthStore } from '../store/authStore'
import { Category, NewsArticleDto, PagedResult } from '../types'

const api = axios.create({
  baseURL: (import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000')
    .replace(/\/?$/, '')
    .concat('/api/'),
  headers: {
    'Content-Type': 'application/json'
  }
})

// Request interceptor for API tokens
api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

interface LoginRequest {
  email: string
  password: string
}

interface RegisterRequest {
  email: string
  password: string
  fullName: string
}

interface LoginResponse {
  token: string
  expiresAt: string
}

interface NewsQuery {
  category?: Category
  search?: string
  unreadOnly?: boolean
  page?: number
  pageSize?: number
}

export const authApi = {
  login: (data: LoginRequest) =>
    api.post<LoginResponse>('auth/login', data),
  register: (data: RegisterRequest) =>
    api.post('auth/register', data)
}

export const newsApi = {
  getAll: (params: NewsQuery = {}) =>
    api.get<PagedResult<NewsArticleDto>>('news', { params }).then(res => res.data),
  markAsRead: (id: string) =>
    api.patch(`news/${id}/read`).then(res => res.data),
  delete: (id: string) =>
    api.delete(`news/${id}`).then(res => res.data)
}

export const feedsApi = {
  getAll: () =>
    api.get<any[]>('feeds').then(res => res.data),
  create: (data: any) =>
    api.post('feeds', data).then(res => res.data),
  update: (id: string, data: any) =>
    api.put(`feeds/${id}`, data).then(res => res.data),
  delete: (id: string) =>
    api.delete(`feeds/${id}`).then(res => res.data)
}

export const digestApi = {
  preview: () =>
    api.get<{ html: string; articleCount: number }>('digest/preview').then(res => res.data),
  send: () =>
    api.post('digest/send').then(res => res.data)
}

export default api
