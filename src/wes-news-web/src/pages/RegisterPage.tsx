import React, { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { UserPlus, Mail, Lock, User, ArrowLeft, Loader2, CheckCircle2, Eye, EyeOff, ShieldCheck, ShieldAlert } from 'lucide-react'
import { authApi } from '../api/client'
import { motion, AnimatePresence } from 'framer-motion'

export default function RegisterPage() {
  const navigate = useNavigate()
  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [fullName, setFullName] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [isSuccess, setIsSuccess] = useState(false)

  const passwordsMatch = confirmPassword.length > 0 && password === confirmPassword
  const passwordsMismatch = confirmPassword.length > 0 && password !== confirmPassword

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

    if (password !== confirmPassword) {
      setError('Passwords do not match. Please try again.')
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
      <div className="relative min-h-screen flex items-center justify-center bg-zinc-50 dark:bg-[#050505] px-4 overflow-hidden">
        <div className="absolute top-[-10%] left-[-10%] w-[50%] h-[50%] bg-green-500/10 blur-[120px] rounded-full animate-blob pointer-events-none" />
        <motion.div
          initial={{ scale: 0.9, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          className="max-w-md w-full text-center space-y-6 z-10"
        >
          <div className="inline-flex items-center justify-center w-24 h-24 rounded-[2rem] bg-green-500/10 text-green-500 shadow-2xl shadow-green-500/20">
            <CheckCircle2 size={48} />
          </div>
          <h1 className="text-4xl font-display font-bold text-zinc-900 dark:text-zinc-100 tracking-tight">Signal Synchronized</h1>
          <p className="text-zinc-500 dark:text-zinc-400 font-medium tracking-wide">Welcome to WesNews. Initializing your node...</p>
        </motion.div>
      </div>
    )
  }

  return (
    <div className="relative min-h-screen flex items-center justify-center bg-zinc-50 dark:bg-[#050505] px-4 py-12 overflow-hidden">
      <div className="absolute top-[-10%] left-[-10%] w-[50%] h-[50%] bg-indigo-500/10 blur-[120px] rounded-full animate-blob pointer-events-none" />
      <div className="absolute bottom-[-10%] right-[-10%] w-[50%] h-[50%] bg-purple-500/10 blur-[120px] rounded-full animate-blob animation-delay-2000 pointer-events-none" />

      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        className="max-w-md w-full z-10"
      >
        <div className="glass-card rounded-[2.5rem] p-8 md:p-10">
          <div className="mb-10">
            <Link
              to="/login"
              className="inline-flex items-center gap-2 text-[10px] font-bold text-zinc-400 dark:text-zinc-500 hover:text-indigo-500 uppercase tracking-[0.2em] mb-8 transition-colors group"
            >
              <ArrowLeft size={14} className="group-hover:-translate-x-1 transition-transform" />
              Return To Gate
            </Link>

            <div className="flex items-center gap-4 mb-4">
              <div className="w-12 h-12 rounded-2xl bg-indigo-600 dark:bg-indigo-500 flex items-center justify-center text-white shadow-xl shadow-indigo-500/30">
                <UserPlus size={24} />
              </div>
              <div>
                <h1 className="text-3xl font-display font-bold text-zinc-900 dark:text-zinc-100 tracking-tight bg-gradient-to-br from-zinc-900 to-zinc-500 dark:from-white dark:to-zinc-500 bg-clip-text text-transparent">
                  Create Identity
                </h1>
                <p className="text-zinc-500 dark:text-zinc-400 text-xs font-medium">Join the premium news frequency</p>
              </div>
            </div>
          </div>

          <form onSubmit={handleSubmit} className="space-y-5">
            <div className="space-y-4">
              <div className="space-y-1.5">
                <label htmlFor="username" className="text-[10px] font-bold text-zinc-500 dark:text-zinc-400 uppercase tracking-widest ml-1">Usuário</label>
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
                    className="input-field pl-12 h-12"
                    placeholder="Seu usuário"
                  />
                </div>
              </div>

              <div className="space-y-1.5">
                <label className="text-[10px] font-bold text-zinc-500 dark:text-zinc-400 uppercase tracking-widest ml-1">Full Name</label>
                <div className="relative group">
                  <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-zinc-400 group-focus-within:text-indigo-500 transition-colors">
                    <User size={18} />
                  </div>
                  <input
                    type="text"
                    required
                    value={fullName}
                    onChange={(e) => setFullName(e.target.value)}
                    className="input-field pl-12 h-12"
                    placeholder="John Doe"
                  />
                </div>
              </div>

              <div className="space-y-1.5">
                <label className="text-[10px] font-bold text-zinc-500 dark:text-zinc-400 uppercase tracking-widest ml-1">Email Address</label>
                <div className="relative group">
                  <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-zinc-400 group-focus-within:text-indigo-500 transition-colors">
                    <Mail size={18} />
                  </div>
                  <input
                    type="email"
                    required
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    className="input-field pl-12 h-12"
                    placeholder="john@example.com"
                  />
                </div>
              </div>

              <div className="space-y-1.5">
                <label className="text-[10px] font-bold text-zinc-500 dark:text-zinc-400 uppercase tracking-widest ml-1">Password</label>
                <div className="relative group">
                  <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-zinc-400 group-focus-within:text-indigo-500 transition-colors">
                    <Lock size={18} />
                  </div>
                  <input
                    type={showPassword ? 'text' : 'password'}
                    required
                    minLength={6}
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    className="input-field pl-12 pr-12 h-12"
                    placeholder="••••••••"
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword((v) => !v)}
                    className="absolute inset-y-0 right-0 pr-4 flex items-center text-zinc-400 hover:text-indigo-500 transition-colors"
                    tabIndex={-1}
                  >
                    {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                  </button>
                </div>
                <p className="text-[9px] text-zinc-400 font-bold uppercase tracking-widest ml-1 opacity-60">Security Level: Standard (6+ chars)</p>
              </div>

              <div className="space-y-1.5">
                <label className="text-[10px] font-bold text-zinc-500 dark:text-zinc-400 uppercase tracking-widest ml-1">Confirm Password</label>
                <div className="relative group">
                  <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-zinc-400 group-focus-within:text-indigo-500 transition-colors">
                    <Lock size={18} />
                  </div>
                  <input
                    type={showConfirmPassword ? 'text' : 'password'}
                    required
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                    className={`input-field pl-12 pr-12 h-12 transition-colors ${
                      passwordsMismatch
                        ? 'border-red-500/60 focus:border-red-500'
                        : passwordsMatch
                        ? 'border-green-500/60 focus:border-green-500'
                        : ''
                    }`}
                    placeholder="••••••••"
                  />
                  <button
                    type="button"
                    onClick={() => setShowConfirmPassword((v) => !v)}
                    className="absolute inset-y-0 right-0 pr-4 flex items-center text-zinc-400 hover:text-indigo-500 transition-colors"
                    tabIndex={-1}
                  >
                    {showConfirmPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                  </button>
                </div>
                <AnimatePresence mode="wait">
                  {passwordsMatch && (
                    <motion.p
                      key="match"
                      initial={{ opacity: 0, y: -4 }}
                      animate={{ opacity: 1, y: 0 }}
                      exit={{ opacity: 0, y: -4 }}
                      className="flex items-center gap-1.5 text-[9px] font-bold uppercase tracking-widest ml-1 text-green-500"
                    >
                      <ShieldCheck size={11} />
                      Passwords match
                    </motion.p>
                  )}
                  {passwordsMismatch && (
                    <motion.p
                      key="mismatch"
                      initial={{ opacity: 0, y: -4 }}
                      animate={{ opacity: 1, y: 0 }}
                      exit={{ opacity: 0, y: -4 }}
                      className="flex items-center gap-1.5 text-[9px] font-bold uppercase tracking-widest ml-1 text-red-500"
                    >
                      <ShieldAlert size={11} />
                      Passwords do not match
                    </motion.p>
                  )}
                </AnimatePresence>
              </div>
            </div>

            <AnimatePresence>
              {error && (
                <motion.div
                  initial={{ opacity: 0, height: 0 }}
                  animate={{ opacity: 1, height: 'auto' }}
                  exit={{ opacity: 0, height: 0 }}
                  className="p-4 rounded-xl bg-red-500/10 border border-red-500/20 text-red-500 text-[11px] font-bold uppercase tracking-wide"
                >
                  {error}
                </motion.div>
              )}
            </AnimatePresence>

            <button
              type="submit"
              disabled={isLoading || passwordsMismatch}
              className="btn-primary w-full h-14 text-base"
            >
              {isLoading ? (
                <>
                  <Loader2 className="animate-spin" size={20} />
                  Transmitting...
                </>
              ) : (
                'Generate Identity'
              )}
            </button>
          </form>

          <div className="mt-10 pt-8 border-t border-zinc-200/50 dark:border-zinc-800/50 text-center">
            <p className="text-zinc-500 dark:text-zinc-400 text-sm font-medium">
              Already have an identity?{' '}
              <Link to="/login" className="text-indigo-600 dark:text-indigo-400 font-bold hover:underline underline-offset-4 transition-all">
                Authenticate
              </Link>
            </p>
          </div>
        </div>

        <p className="mt-10 text-center text-zinc-400 text-[10px] font-bold uppercase tracking-[0.2em] opacity-40">
          WesNews Hub &copy; 2026 &bull; Decentralized Access
        </p>
      </motion.div>
    </div>
  )
}
