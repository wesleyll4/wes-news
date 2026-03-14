import React, { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { UserPlus, Mail, Lock, User, ArrowLeft, Loader2, CheckCircle2 } from 'lucide-react'
import { authApi } from '../api/client'

export default function RegisterPage() {
  const navigate = useNavigate()
  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [fullName, setFullName] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [isSuccess, setIsSuccess] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setIsLoading(true)
    setError(null)

    if (username.length < 3) {
      setError('Username must be at least 3 characters.')
      setIsLoading(false)
      return
    }

    if (!email.includes('@')) {
      setError('Please enter a valid email address.')
      setIsLoading(false)
      return
    }

    if (password.length < 6) {
      setError('Password must be at least 6 characters.')
      setIsLoading(false)
      return
    }

    try {
      await authApi.register({ username, email, password, fullName })
      setIsSuccess(true)
      setTimeout(() => navigate('/login'), 2000)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to register. Please try again.')
    } finally {
      setIsLoading(false)
    }
  }

  if (isSuccess) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-zinc-50 dark:bg-zinc-950 px-4">
        <div className="max-w-md w-full text-center space-y-6 animate-in fade-in zoom-in duration-300">
          <div className="inline-flex items-center justify-center w-20 h-20 rounded-full bg-green-100 dark:bg-green-900/30 text-green-600 dark:text-green-400">
            <CheckCircle2 size={40} />
          </div>
          <h1 className="text-3xl font-display font-bold text-zinc-900 dark:text-zinc-100">Welcome to WesNews!</h1>
          <p className="text-zinc-500 dark:text-zinc-400">Your account has been created. Redirecting to login...</p>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-zinc-50 dark:bg-zinc-950 px-4 py-12">
      <div className="max-w-md w-full">
        <div className="bg-white dark:bg-zinc-900 rounded-3xl shadow-xl shadow-zinc-200/50 dark:shadow-none border border-zinc-100 dark:border-zinc-800 p-8 md:p-10">
          <div className="mb-10">
            <Link
              to="/login"
              className="inline-flex items-center gap-2 text-sm font-medium text-zinc-400 hover:text-zinc-600 dark:hover:text-zinc-200 mb-6 transition-colors"
            >
              <ArrowLeft size={16} />
              Back to Login
            </Link>
            <div className="flex items-center gap-3 mb-2">
              <div className="w-10 h-10 rounded-xl bg-primary-500/10 flex items-center justify-center text-primary-500">
                <UserPlus size={24} />
              </div>
              <h1 className="text-3xl font-display font-bold text-zinc-900 dark:text-zinc-100 italic">Create Account</h1>
            </div>
            <p className="text-zinc-500 dark:text-zinc-400">Join our modern tech news aggregator.</p>
          </div>

          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="space-y-5">
              <div className="space-y-2">
                <label htmlFor="username" className="text-sm font-semibold text-zinc-700 dark:text-zinc-300 ml-1">Usuário</label>
                <div className="relative group">
                  <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-zinc-400 group-focus-within:text-primary-500 transition-colors">
                    <User size={18} />
                  </div>
                  <input
                    id="username"
                    type="text"
                    required
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                    className="w-full bg-zinc-50 dark:bg-zinc-950 border border-zinc-200 dark:border-zinc-800 rounded-2xl py-3 pl-10 pr-4 outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all text-zinc-900 dark:text-zinc-100 placeholder:text-zinc-400"
                    placeholder="Seu usuário"
                  />
                </div>
              </div>

              <div className="space-y-2">
                <label className="text-sm font-semibold text-zinc-700 dark:text-zinc-300 ml-1">Full Name</label>
                <div className="relative group">
                  <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-zinc-400 group-focus-within:text-primary-500 transition-colors">
                    <User size={18} />
                  </div>
                  <input
                    type="text"
                    required
                    value={fullName}
                    onChange={(e) => setFullName(e.target.value)}
                    className="w-full bg-zinc-50 dark:bg-zinc-950 border border-zinc-200 dark:border-zinc-800 rounded-2xl py-3 pl-10 pr-4 outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all text-zinc-900 dark:text-zinc-100 placeholder:text-zinc-400"
                    placeholder="Jhon Doe"
                  />
                </div>
              </div>

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
                    minLength={6}
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    className="w-full bg-zinc-50 dark:bg-zinc-950 border border-zinc-200 dark:border-zinc-800 rounded-2xl py-3 pl-10 pr-4 outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all text-zinc-900 dark:text-zinc-100 placeholder:text-zinc-400"
                    placeholder="••••••••"
                  />
                </div>
                <p className="text-[11px] text-zinc-500 ml-1">Min. 6 characters</p>
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
                  Creating account...
                </>
              ) : (
                'Create Account'
              )}
            </button>
          </form>

          <div className="mt-10 pt-8 border-t border-zinc-100 dark:border-zinc-800 text-center">
            <p className="text-zinc-500 dark:text-zinc-400 text-sm">
              Already have an account?{' '}
              <Link to="/login" className="text-zinc-900 dark:text-zinc-100 font-bold hover:underline underline-offset-4">
                Sign In
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}
