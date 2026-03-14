import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { feedsApi } from '../api/client'
import { Category, CategoryLabels, CategoryColors } from '../types'
import type { CreateFeedSourceRequest } from '../types'
import { Trash2, Plus, ToggleLeft, ToggleRight, Loader2, Rss } from 'lucide-react'

export default function SourcesPage() {
  const queryClient = useQueryClient()
  const [showForm, setShowForm] = useState(false)
  const [form, setForm] = useState<CreateFeedSourceRequest>({ name: '', url: '', category: Category.General })
  const [error, setError] = useState('')

  const { data: feeds, isLoading } = useQuery({
    queryKey: ['feeds'],
    queryFn: feedsApi.getAll
  })

  const createMutation = useMutation({
    mutationFn: feedsApi.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['feeds'] })
      setShowForm(false)
      setForm({ name: '', url: '', category: Category.General })
      setError('')
    },
    onError: () => setError('URL already exists or invalid data.')
  })

  const toggleMutation = useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      feedsApi.update(id, { isActive }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['feeds'] })
  })

  const deleteMutation = useMutation({
    mutationFn: feedsApi.delete,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['feeds'] })
  })

  const categories = Object.values(Category).filter((v): v is Category => typeof v === 'number')

  const grouped = categories.map((cat) => ({
    cat,
    items: feeds?.filter((f) => f.category === cat) ?? []
  })).filter((g) => g.items.length > 0)

  return (
    <div className="max-w-2xl mx-auto px-4 py-8">
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="font-display font-bold text-2xl tracking-tight">Sources</h1>
          <p className="text-sm text-zinc-500 dark:text-zinc-400 mt-0.5">
            {feeds?.length ?? 0} RSS feeds configured
          </p>
        </div>
        <button onClick={() => { setShowForm(!showForm); setError('') }} className="btn-primary">
          <Plus size={15} />
          Add Source
        </button>
      </div>

      {showForm && (
        <div className="mb-8 p-5 border border-zinc-200 dark:border-zinc-800 rounded-xl bg-zinc-50 dark:bg-zinc-900 animate-fade-in">
          <h2 className="font-semibold text-sm mb-4">New Feed Source</h2>
          {error && <p className="text-xs text-red-500 mb-3">{error}</p>}
          <div className="space-y-3">
            <input
              type="text"
              placeholder="Display name"
              value={form.name}
              onChange={(e) => setForm({ ...form, name: e.target.value })}
              className="input-field"
            />
            <input
              type="url"
              placeholder="https://example.com/feed.xml"
              value={form.url}
              onChange={(e) => setForm({ ...form, url: e.target.value })}
              className="input-field"
            />
            <select
              value={form.category}
              onChange={(e) => setForm({ ...form, category: Number(e.target.value) as Category })}
              className="input-field bg-white dark:bg-zinc-950"
            >
              {categories.map((cat) => (
                <option key={cat} value={cat}>{CategoryLabels[cat]}</option>
              ))}
            </select>
            <div className="flex gap-2 pt-1">
              <button
                onClick={() => createMutation.mutate(form)}
                disabled={createMutation.isPending || !form.name || !form.url}
                className="btn-primary"
              >
                {createMutation.isPending ? <Loader2 size={14} className="animate-spin" /> : <Plus size={14} />}
                Add Feed
              </button>
              <button onClick={() => { setShowForm(false); setError('') }} className="btn-ghost">
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      {isLoading ? (
        <div className="flex justify-center py-16">
          <Loader2 size={24} className="animate-spin text-zinc-400" />
        </div>
      ) : (
        <div className="space-y-8">
          {grouped.map(({ cat, items }) => {
            const colors = CategoryColors[cat]
            return (
              <div key={cat}>
                <div className="flex items-center gap-2 mb-3">
                  <span className={`w-2 h-2 rounded-full ${colors.dot}`} />
                  <h2 className="text-xs font-semibold uppercase tracking-widest text-zinc-400 dark:text-zinc-500">
                    {CategoryLabels[cat]}
                  </h2>
                  <span className="text-xs text-zinc-300 dark:text-zinc-700">({items.length})</span>
                </div>
                <div className="space-y-1">
                  {items.map((feed: any) => (
                    <div
                      key={feed.id}
                      className={`flex items-center justify-between px-4 py-3 rounded-xl border transition-colors ${feed.isActive ? 'border-zinc-100 dark:border-zinc-800 bg-white dark:bg-zinc-900' : 'border-zinc-100 dark:border-zinc-800/50 bg-zinc-50 dark:bg-zinc-900/40 opacity-50'}`}
                    >
                      <div className="flex items-center gap-3 min-w-0">
                        <Rss size={14} className="text-zinc-400 shrink-0" />
                        <div className="min-w-0">
                          <p className="text-sm font-medium truncate">{feed.name}</p>
                          <p className="text-[11px] text-zinc-400 truncate max-w-[200px] sm:max-w-xs">{feed.url}</p>
                        </div>
                      </div>
                      <div className="flex items-center gap-1 shrink-0 ml-3">
                        <button
                          onClick={() => toggleMutation.mutate({ id: feed.id, isActive: !feed.isActive })}
                          className="p-2 rounded-lg text-zinc-400 hover:text-zinc-700 dark:hover:text-zinc-200 hover:bg-zinc-100 dark:hover:bg-zinc-800 transition-colors"
                          title={feed.isActive ? 'Disable' : 'Enable'}
                        >
                          {feed.isActive
                            ? <ToggleRight size={18} className="text-zinc-900 dark:text-zinc-100" />
                            : <ToggleLeft size={18} />}
                        </button>
                        <button
                          onClick={() => deleteMutation.mutate(feed.id)}
                          className="p-2 rounded-lg text-zinc-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-950/40 transition-colors"
                        >
                          <Trash2 size={14} />
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )
          })}
        </div>
      )}
    </div>
  )
}
