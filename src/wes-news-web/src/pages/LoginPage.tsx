import React, { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { LogIn, User, Lock, Loader2, Compass } from 'lucide-react'
import { authApi } from '../api/client'
import { motion } from 'framer-motion'

export default function LoginPage() {
  const navigate = useNavigate()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setIsLoading(true)
    setError(null)

    try {
      await authApi.login({ username, password })
      navigate('/')
    } catch (err: any) {
      setError(err.response?.data?.message || 'Invalid username or password')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="relative min-h-screen flex items-center justify-center bg-zinc-50 dark:bg-[#050505] px-4 overflow-hidden">
      {/* Background Ambient Glows */}
      <div className="absolute top-[-10%] left-[-10%] w-[50%] h-[50%] bg-indigo-500/10 blur-[120px] rounded-full animate-blob pointer-events-none" />
      <div className="absolute bottom-[-10%] right-[-10%] w-[50%] h-[50%] bg-purple-500/10 blur-[120px] rounded-full animate-blob animation-delay-2000 pointer-events-none" />

      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        className="max-w-md w-full z-10"
      >
        {/* Logo/Brand */}
        <div className="flex flex-col items-center mb-10 text-center">
          <motion.div
            whileHover={{ scale: 1.05 }}
            whileTap={{ scale: 0.95 }}
            className="w-16 h-16 rounded-[2rem] bg-indigo-600 dark:bg-indigo-500 flex items-center justify-center text-white shadow-2xl shadow-indigo-500/30 mb-6"
          >
            <Compass size={32} />
          </motion.div>
          <h1 className="text-4xl font-display font-bold text-zinc-900 dark:text-zinc-100 tracking-tight bg-gradient-to-br from-zinc-900 to-zinc-500 dark:from-white dark:to-zinc-500 bg-clip-text text-transparent">
            WesNews
          </h1>
          <p className="text-zinc-500 dark:text-zinc-400 mt-2 font-bold tracking-[0.2em] uppercase text-[10px] opacity-60">Core Feed</p>
        </div>

        <div className="glass-card rounded-[2.5rem] p-8 md:p-10">
          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="space-y-5">
              <div className="space-y-2">
                <label htmlFor="username" className="text-[11px] font-bold text-zinc-500 dark:text-zinc-400 uppercase tracking-widest ml-1">Usuário</label>
                <div className="relative group">
                  <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-zinc-400 group-focus-within:text-indigo-500 transition-colors">
                    <User size={18} />
                  </div>
                  <input
                    id="username"
                    type="text"
                    required
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                    className="input-field pl-12"
                    placeholder="Seu usuário"
                  />
                </div>
              </div>

              <div className="space-y-2">
                <label className="text-[11px] font-bold text-zinc-500 dark:text-zinc-400 uppercase tracking-widest ml-1">Password</label>
                <div className="relative group">
                  <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-zinc-400 group-focus-within:text-indigo-500 transition-colors">
                    <Lock size={18} />
                  </div>
                  <input
                    type="password"
                    required
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    className="input-field pl-12"
                    placeholder="••••••••"
                  />
                </div>
              </div>
            </div>

            {error && (
              <motion.div
                initial={{ opacity: 0, scale: 0.95 }}
                animate={{ opacity: 1, scale: 1 }}
                className="p-4 rounded-2xl bg-red-500/10 border border-red-500/20 text-red-500 text-xs font-bold uppercase tracking-wide"
              >
                {error}
              </motion.div>
            )}

            <button
              type="submit"
              disabled={isLoading}
              className="btn-primary w-full h-14 text-base"
            >
              {isLoading ? (
                <>
                  <Loader2 className="animate-spin" size={20} />
                  Authorizing...
                </>
              ) : (
                <>
                  <LogIn size={20} />
                  Authenticate
                </>
              )}
            </button>
          </form>

          <div className="mt-10 pt-8 border-t border-zinc-200/50 dark:border-zinc-800/50 space-y-4 text-center">
            <p className="text-zinc-500 dark:text-zinc-400 text-sm font-medium">
              New to the system?{' '}
              <Link to="/register" className="text-indigo-600 dark:text-indigo-400 font-bold hover:underline underline-offset-4">
                Initialize account
              </Link>
            </p>
            <button
              type="button"
              onClick={() => navigate('/')}
              className="text-zinc-400 dark:text-zinc-500 text-xs font-medium hover:text-zinc-600 dark:hover:text-zinc-300 transition-colors"
            >
              Continue as visitor →
            </button>
          </div>
        </div>

        {/* Footer info */}
        <p className="mt-10 text-center text-zinc-400 text-[10px] font-bold uppercase tracking-[0.2em] opacity-40">
          WesNews Protocol &copy; 2026 &bull; Secure Node
        </p>
      </motion.div>
    </div>
  )
}
