import React, { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { LogIn, Mail, Lock, Loader2, Newspaper } from 'lucide-react'
import { useAuthStore } from '../store/authStore'
import { authApi } from '../api/client'

export default function LoginPage() {
  const navigate = useNavigate()
  const setToken = useAuthStore((s) => s.login)
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setIsLoading(true)
    setError(null)

    try {
      const response = await authApi.login({ email, password })
      setToken(response.data.token)
      navigate('/')
    } catch (err: any) {
      setError(err.response?.data?.message || 'Invalid email or password')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-zinc-50 dark:bg-zinc-950 px-4">
      <div className="max-w-md w-full">
        {/* Logo/Brand */}
        <div className="flex flex-col items-center mb-10 text-center">
          <div className="w-16 h-16 rounded-3xl bg-zinc-950 dark:bg-white flex items-center justify-center text-white dark:text-zinc-950 shadow-2xl shadow-zinc-950/20 dark:shadow-white/10 mb-6 active:scale-95 transition-transform">
            <Newspaper size={32} />
          </div>
          <h1 className="text-4xl font-display font-bold text-zinc-900 dark:text-zinc-100 tracking-tight italic">WesNews</h1>
          <p className="text-zinc-500 dark:text-zinc-400 mt-2 font-medium">Log in to your account</p>
        </div>

        <div className="bg-white dark:bg-zinc-900 rounded-3xl shadow-xl shadow-zinc-200/50 dark:shadow-none border border-zinc-100 dark:border-zinc-800 p-8 md:p-10">
          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="space-y-5">
              <div className="space-y-2">
                <label className="text-sm font-semibold text-zinc-700 dark:text-zinc-300 ml-1">Email Address</label>
                <div className="relative group">
                  <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-zinc-400 group-focus-within:text-primary-500 transition-colors">
                    <Mail size={18} />
                  </div>
                  <input
                    type="email"
                    required
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    className="w-full bg-zinc-50 dark:bg-zinc-950 border border-zinc-200 dark:border-zinc-800 rounded-2xl py-3 pl-10 pr-4 outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all text-zinc-900 dark:text-zinc-100 placeholder:text-zinc-400"
                    placeholder="jhon@example.com"
                  />
                </div>
              </div>

              <div className="space-y-2">
                <label className="text-sm font-semibold text-zinc-700 dark:text-zinc-300 ml-1">Password</label>
                <div className="relative group">
                  <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-zinc-400 group-focus-within:text-primary-500 transition-colors">
                    <Lock size={18} />
                  </div>
                  <input
                    type="password"
                    required
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    className="w-full bg-zinc-50 dark:bg-zinc-950 border border-zinc-200 dark:border-zinc-800 rounded-2xl py-3 pl-10 pr-4 outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all text-zinc-900 dark:text-zinc-100 placeholder:text-zinc-400"
                    placeholder="••••••••"
                  />
                </div>
              </div>
            </div>

            {error && (
              <div className="p-4 rounded-2xl bg-red-50 dark:bg-red-900/20 border border-red-100 dark:border-red-900/30 text-red-600 dark:text-red-400 text-sm font-medium animate-in slide-in-from-top-2 duration-200">
                {error}
              </div>
            )}

            <button
              type="submit"
              disabled={isLoading}
              className="w-full bg-zinc-950 dark:bg-white text-white dark:text-zinc-950 rounded-2xl py-4 font-bold hover:shadow-lg hover:shadow-zinc-950/20 dark:hover:shadow-white/10 active:scale-[0.98] transition-all disabled:opacity-50 disabled:scale-100 flex items-center justify-center gap-2"
            >
              {isLoading ? (
                <>
                  <Loader2 className="animate-spin" size={20} />
                  Logging in...
                </>
              ) : (
                <>
                  <LogIn size={20} />
                  Sign In
                </>
              )}
            </button>
          </form>

          <div className="mt-10 pt-8 border-t border-zinc-100 dark:border-zinc-800 text-center">
            <p className="text-zinc-500 dark:text-zinc-400 text-sm">
              Don't have an account?{' '}
              <Link to="/register" className="text-zinc-900 dark:text-zinc-100 font-bold hover:underline underline-offset-4">
                Register free
              </Link>
            </p>
          </div>
        </div>

        {/* Footer info */}
        <p className="mt-10 text-center text-zinc-400 text-[11px] font-medium uppercase tracking-[0.2em]">
          WesNews &copy; 2026 &bull; Modern Tech Aggregator
        </p>
      </div>
    </div>
  )
}
