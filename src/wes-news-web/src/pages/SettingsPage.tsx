import { useState, useEffect, useRef } from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { digestApi, usersApi } from '../api/client'
import { useAuthStore } from '../store/authStore'
import { Send, Eye, Loader2, CheckCircle2, Mail, Terminal, Bell, AlertTriangle, X, Trash2, User, Pencil } from 'lucide-react'

export default function SettingsPage() {
  const [showPreview, setShowPreview] = useState(false)
  const [sent, setSent] = useState(false)
  const [showDeleteDialog, setShowDeleteDialog] = useState(false)
  const [deleteError, setDeleteError] = useState<string | null>(null)

  const navigate = useNavigate()
  const logout = useAuthStore(s => s.logout)

  const deleteMutation = useMutation({
    mutationFn: usersApi.deleteAccount,
    onSuccess: () => { logout(); navigate('/login') },
    onError: () => setDeleteError('Failed to delete account. Please try again.')
  })

  // Digest preference state
  const authDigestEnabled = useAuthStore(s => s.digestEnabled)
  const [digestEnabled, setDigestEnabled] = useState(authDigestEnabled)
  const [digestSaved, setDigestSaved] = useState(false)
  const [digestError, setDigestError] = useState<string | null>(null)
  const [digestWarning, setDigestWarning] = useState<string | null>(null)

  // Email edit state
  const [editingEmail, setEditingEmail] = useState(false)
  const [emailValue, setEmailValue] = useState('')
  const [emailError, setEmailError] = useState<string | null>(null)
  const [emailSaved, setEmailSaved] = useState(false)
  const emailInputRef = useRef<HTMLInputElement>(null)

  const { data: meData, isError: meError } = useQuery({
    queryKey: ['users-me'],
    queryFn: usersApi.getMe,
    retry: 1,
  })

  useEffect(() => {
    if (meData !== undefined) {
      setDigestEnabled(meData.digestEnabled)
      setEmailValue(meData.email)
    } else if (meError) {
      setDigestEnabled(authDigestEnabled)
      setDigestWarning('Could not load your preferences from the server. Showing last known value.')
    }
  }, [meData, meError, authDigestEnabled])

  useEffect(() => {
    if (editingEmail) emailInputRef.current?.focus()
  }, [editingEmail])

  const digestMutation = useMutation({
    mutationFn: usersApi.updateDigestPreference,
    onSuccess: (data) => {
      setDigestEnabled(data.digestEnabled)
      setDigestError(null)
      setDigestSaved(true)
      setTimeout(() => setDigestSaved(false), 3000)
    },
    onError: (_err, variables) => {
      setDigestEnabled(!variables)
      setDigestError('Failed to update digest preference. Please try again.')
    }
  })

  const emailMutation = useMutation({
    mutationFn: (email: string) => usersApi.updateEmail(email),
    onSuccess: (data) => {
      setEmailValue(data.email)
      setEditingEmail(false)
      setEmailError(null)
      setEmailSaved(true)
      setTimeout(() => setEmailSaved(false), 3000)
    },
    onError: (err: any) => {
      const msg = err?.response?.data?.message
      setEmailError(msg ?? 'Failed to update email. Please try again.')
    }
  })

  const handleDigestToggle = () => {
    if (digestMutation.isPending) return
    const newValue = !digestEnabled
    setDigestEnabled(newValue)
    setDigestError(null)
    digestMutation.mutate(newValue)
  }

  const handleEmailSave = () => {
    const trimmed = emailValue.trim()
    if (!trimmed || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(trimmed)) {
      setEmailError('Please enter a valid email address.')
      return
    }
    if (trimmed === meData?.email) {
      setEditingEmail(false)
      return
    }
    emailMutation.mutate(trimmed)
  }

  const handleEmailKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') handleEmailSave()
    if (e.key === 'Escape') { setEditingEmail(false); setEmailValue(meData?.email ?? ''); setEmailError(null) }
  }

  const { data: preview, isLoading: previewLoading } = useQuery<{ html: string; articleCount: number }>({
    queryKey: ['digest-preview'],
    queryFn: digestApi.preview,
    enabled: showPreview
  })

  const sendMutation = useMutation({
    mutationFn: digestApi.send,
    onSuccess: () => { setSent(true); setTimeout(() => setSent(false), 4000) }
  })

  return (
    <div className="max-w-2xl mx-auto px-4 py-8">
      <div className="mb-8">
        <h1 className="font-display font-bold text-2xl tracking-tight">Settings</h1>
        <p className="text-sm text-zinc-500 dark:text-zinc-400 mt-0.5">Manage your account and notifications</p>
      </div>

      {/* Account Section */}
      <section className="mb-6">
        <div className="flex items-center gap-2 mb-3">
          <User size={14} className="text-zinc-400" />
          <h2 className="text-xs font-semibold uppercase tracking-widest text-zinc-400 dark:text-zinc-500">Account</h2>
        </div>

        <div className="p-5 border border-zinc-100 dark:border-zinc-800 rounded-xl bg-white dark:bg-zinc-900">
          <div className="flex items-center justify-between">
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium text-zinc-800 dark:text-zinc-200 mb-1">Email</p>
              {editingEmail ? (
                <div className="flex items-center gap-2">
                  <input
                    ref={emailInputRef}
                    type="email"
                    value={emailValue}
                    onChange={e => { setEmailValue(e.target.value); setEmailError(null) }}
                    onKeyDown={handleEmailKeyDown}
                    disabled={emailMutation.isPending}
                    className="flex-1 min-w-0 text-sm bg-zinc-50 dark:bg-zinc-800 border border-zinc-200 dark:border-zinc-700 rounded-lg px-3 py-1.5 text-zinc-800 dark:text-zinc-200 focus:outline-none focus:ring-2 focus:ring-indigo-500 disabled:opacity-50"
                  />
                  <button
                    onClick={handleEmailSave}
                    disabled={emailMutation.isPending}
                    className="px-3 py-1.5 text-xs font-medium text-white bg-indigo-600 hover:bg-indigo-500 rounded-lg transition-colors disabled:opacity-50 flex items-center gap-1.5"
                  >
                    {emailMutation.isPending ? <Loader2 size={12} className="animate-spin" /> : <CheckCircle2 size={12} />}
                    Save
                  </button>
                  <button
                    onClick={() => { setEditingEmail(false); setEmailValue(meData?.email ?? ''); setEmailError(null) }}
                    disabled={emailMutation.isPending}
                    className="p-1.5 text-zinc-400 hover:text-zinc-600 dark:hover:text-zinc-300 transition-colors"
                  >
                    <X size={14} />
                  </button>
                </div>
              ) : (
                <p className="text-sm text-zinc-500 dark:text-zinc-400 truncate">{emailValue || '—'}</p>
              )}
              {emailError && (
                <p className="text-xs text-red-500 mt-1.5">{emailError}</p>
              )}
              {emailSaved && !editingEmail && (
                <div className="flex items-center gap-1.5 mt-1.5">
                  <CheckCircle2 size={11} className="text-green-500" />
                  <span className="text-xs text-green-600 dark:text-green-400">Email updated</span>
                </div>
              )}
            </div>
            {!editingEmail && (
              <button
                onClick={() => setEditingEmail(true)}
                className="ml-4 p-2 rounded-lg text-zinc-400 hover:text-indigo-500 hover:bg-indigo-50 dark:hover:bg-indigo-500/10 transition-all"
                title="Edit email"
              >
                <Pencil size={14} />
              </button>
            )}
          </div>
        </div>
      </section>

      {/* Notifications */}
      <section className="mb-6">
        <div className="flex items-center gap-2 mb-3">
          <Bell size={14} className="text-zinc-400" />
          <h2 className="text-xs font-semibold uppercase tracking-widest text-zinc-400 dark:text-zinc-500">Notifications</h2>
        </div>

        <div className="p-5 border border-zinc-100 dark:border-zinc-800 rounded-xl bg-white dark:bg-zinc-900">
          {digestWarning && (
            <div className="flex items-start gap-2 mb-4 p-3 rounded-lg bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-800">
              <AlertTriangle size={14} className="text-amber-500 mt-0.5 shrink-0" />
              <p className="text-xs text-amber-700 dark:text-amber-400 flex-1">{digestWarning}</p>
              <button onClick={() => setDigestWarning(null)} className="text-amber-400 hover:text-amber-600"><X size={12} /></button>
            </div>
          )}

          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-zinc-800 dark:text-zinc-200">Daily Digest Email</p>
              <p className="text-xs text-zinc-500 dark:text-zinc-400 mt-0.5">
                Receive a curated email every morning with your top unread articles.
              </p>
            </div>
            <button
              role="switch"
              aria-checked={digestEnabled}
              aria-label="Toggle daily digest email"
              onClick={handleDigestToggle}
              disabled={digestMutation.isPending}
              className={`relative inline-flex h-6 w-11 shrink-0 cursor-pointer items-center rounded-full transition-colors duration-200 focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 ${
                digestEnabled ? 'bg-blue-600 dark:bg-blue-500' : 'bg-zinc-200 dark:bg-zinc-700'
              }`}
            >
              <span className={`inline-block h-4 w-4 transform rounded-full bg-white shadow-sm transition-transform duration-200 ${digestEnabled ? 'translate-x-6' : 'translate-x-1'}`} />
            </button>
          </div>

          {digestMutation.isPending && (
            <div className="flex items-center gap-1.5 mt-3">
              <Loader2 size={12} className="animate-spin text-zinc-400" />
              <span className="text-xs text-zinc-400">Saving...</span>
            </div>
          )}
          {digestSaved && !digestMutation.isPending && (
            <div className="flex items-center gap-1.5 mt-3">
              <CheckCircle2 size={12} className="text-green-500" />
              <span className="text-xs text-green-600 dark:text-green-400">Preference saved</span>
            </div>
          )}
          {digestError && (
            <div className="flex items-start gap-2 mt-3 p-2.5 rounded-lg bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800">
              <AlertTriangle size={12} className="text-red-500 mt-0.5 shrink-0" />
              <p className="text-xs text-red-600 dark:text-red-400">{digestError}</p>
              <button onClick={() => setDigestError(null)} className="text-red-400 hover:text-red-600 ml-auto"><X size={12} /></button>
            </div>
          )}
        </div>
      </section>

      {/* Daily Digest */}
      <section className="mb-6">
        <div className="flex items-center gap-2 mb-3">
          <Mail size={14} className="text-zinc-400" />
          <h2 className="text-xs font-semibold uppercase tracking-widest text-zinc-400 dark:text-zinc-500">Daily Digest</h2>
        </div>

        <div className="p-5 border border-zinc-100 dark:border-zinc-800 rounded-xl bg-white dark:bg-zinc-900">
          <p className="text-sm text-zinc-600 dark:text-zinc-300 mb-5">
            Sends a curated email every morning with the top unread articles per category.
          </p>
          <div className="flex flex-wrap gap-2">
            <button
              onClick={() => sendMutation.mutate()}
              disabled={sendMutation.isPending}
              className="btn-primary"
            >
              {sendMutation.isPending ? <Loader2 size={14} className="animate-spin" /> : sent ? <CheckCircle2 size={14} /> : <Send size={14} />}
              {sent ? 'Sent!' : 'Send Now'}
            </button>
            <button onClick={() => setShowPreview(!showPreview)} className="btn-ghost">
              <Eye size={14} />
              {showPreview ? 'Hide Preview' : 'Preview Digest'}
            </button>
          </div>
        </div>
      </section>

      {showPreview && (
        <section className="mb-6 animate-fade-in">
          <div className="flex items-center gap-2 mb-3">
            <Terminal size={14} className="text-zinc-400" />
            <h2 className="text-xs font-semibold uppercase tracking-widest text-zinc-400 dark:text-zinc-500">
              Email Preview
              {preview && <span className="ml-2 normal-case font-normal">— {preview.articleCount} articles</span>}
            </h2>
          </div>
          <div className="border border-zinc-100 dark:border-zinc-800 rounded-xl overflow-hidden">
            {previewLoading ? (
              <div className="flex justify-center py-12">
                <Loader2 size={22} className="animate-spin text-zinc-400" />
              </div>
            ) : preview ? (
              <iframe srcDoc={preview.html} className="w-full h-[480px]" title="Digest Preview" sandbox="allow-same-origin" />
            ) : null}
          </div>
        </section>
      )}

      {/* Danger Zone */}
      <section className="mb-6">
        <div className="flex items-center gap-2 mb-3">
          <Trash2 size={14} className="text-red-400" />
          <h2 className="text-xs font-semibold uppercase tracking-widest text-red-400">Danger Zone</h2>
        </div>
        <div className="p-5 border border-red-200 dark:border-red-900 rounded-xl bg-white dark:bg-zinc-900">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-zinc-800 dark:text-zinc-200">Delete Account</p>
              <p className="text-xs text-zinc-500 dark:text-zinc-400 mt-0.5">
                Permanently remove your account and all associated data.
              </p>
            </div>
            <button
              onClick={() => { setShowDeleteDialog(true); setDeleteError(null) }}
              data-testid="delete-account-trigger"
              className="px-3 py-1.5 text-xs font-medium text-red-600 dark:text-red-400 border border-red-300 dark:border-red-700 rounded-lg hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors"
            >
              Delete Account
            </button>
          </div>
        </div>
      </section>

      {/* Delete Account Dialog */}
      {showDeleteDialog && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50" role="dialog" aria-modal="true" aria-labelledby="delete-dialog-title">
          <div className="bg-white dark:bg-zinc-900 rounded-xl shadow-xl p-6 max-w-sm w-full mx-4">
            <div className="flex items-center gap-3 mb-4">
              <div className="p-2 rounded-full bg-red-100 dark:bg-red-900/30">
                <AlertTriangle size={18} className="text-red-600 dark:text-red-400" />
              </div>
              <h3 id="delete-dialog-title" className="text-base font-semibold text-zinc-900 dark:text-zinc-100">Delete Account</h3>
            </div>
            <p className="text-sm text-zinc-600 dark:text-zinc-400 mb-2">
              Are you sure you want to delete your account? <strong>This action is irreversible</strong> and all your data will be permanently removed.
            </p>
            {deleteError && (
              <div className="flex items-start gap-2 mt-3 mb-3 p-2.5 rounded-lg bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800">
                <AlertTriangle size={12} className="text-red-500 mt-0.5 shrink-0" />
                <p className="text-xs text-red-600 dark:text-red-400">{deleteError}</p>
              </div>
            )}
            <div className="flex gap-2 mt-5 justify-end">
              <button
                onClick={() => { setShowDeleteDialog(false); setDeleteError(null) }}
                disabled={deleteMutation.isPending}
                className="px-4 py-2 text-sm font-medium text-zinc-700 dark:text-zinc-300 border border-zinc-200 dark:border-zinc-700 rounded-lg hover:bg-zinc-50 dark:hover:bg-zinc-800 transition-colors disabled:opacity-50"
              >
                Cancel
              </button>
              <button
                onClick={() => deleteMutation.mutate()}
                disabled={deleteMutation.isPending}
                data-testid="delete-account-confirm"
                className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-red-600 hover:bg-red-700 rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {deleteMutation.isPending && <Loader2 size={14} className="animate-spin" />}
                {deleteMutation.isPending ? 'Deleting...' : 'Delete Account'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
