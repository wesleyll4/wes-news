import axios from 'axios'
import { useAuthStore } from '../store/authStore'
import { Category, NewsArticleDto, PagedResult } from '../types'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL
    ? import.meta.env.VITE_API_BASE_URL.replace(/\/?$/, '').concat('/api/')
    : '/api/',
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

// Response interceptor for handling token expiration
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      useAuthStore.getState().logout()
    }
    return Promise.reject(error)
  }
)

interface LoginRequest {
  username: string
  password: string
}

interface RegisterRequest {
  username: string
  email: string
  password: string
  fullName: string
}

interface LoginResponse {
  token: string
  role: string
  expiresAt: string
  digestEnabled: boolean
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
    api.post<LoginResponse>('auth/login', data).then(res => {
      useAuthStore.getState().login(res.data.token, res.data.role, res.data.digestEnabled)
      return res
    }),
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

export const usersApi = {
  getMe: () =>
    api.get<{ email: string; digestEnabled: boolean }>('users/me').then(r => r.data),
  updateDigestPreference: (digestEnabled: boolean) =>
    api.patch<{ digestEnabled: boolean }>('users/me/digest-preference', { digestEnabled }).then(r => r.data),
  updateEmail: (email: string) =>
    api.patch<{ email: string; digestEnabled: boolean }>('users/me/email', { email }).then(r => r.data),
  deleteAccount: () =>
    api.delete('users/me').then(r => r.data)
}

export default api
