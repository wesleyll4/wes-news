import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { feedsApi } from '../api/client'
import { Category, CategoryLabels } from '../types'
import type { CreateFeedSourceRequest } from '../types'
import { Trash2, Plus, ToggleLeft, ToggleRight, Loader2 } from 'lucide-react'

export default function SourcesPage() {
  const queryClient = useQueryClient()
  const [showForm, setShowForm] = useState(false)
  const [form, setForm] = useState<CreateFeedSourceRequest>({ name: '', url: '', category: Category.General })

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
    }
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

  return (
    <div className="max-w-2xl mx-auto p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-xl font-bold">RSS Sources</h1>
        <button
          onClick={() => setShowForm(!showForm)}
          className="flex items-center gap-2 px-3 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm rounded-lg transition-colors"
        >
          <Plus size={15} />
          Add Source
        </button>
      </div>

      {showForm && (
        <div className="mb-6 p-4 border border-gray-200 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-900">
          <h2 className="text-sm font-semibold mb-3">New Feed Source</h2>
          <div className="space-y-3">
            <input
              type="text"
              placeholder="Name"
              value={form.name}
              onChange={(e) => setForm({ ...form, name: e.target.value })}
              className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 rounded-lg bg-transparent outline-none focus:border-blue-500"
            />
            <input
              type="url"
              placeholder="RSS URL"
              value={form.url}
              onChange={(e) => setForm({ ...form, url: e.target.value })}
              className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 rounded-lg bg-transparent outline-none focus:border-blue-500"
            />
            <select
              value={form.category}
              onChange={(e) => setForm({ ...form, category: Number(e.target.value) as Category })}
              className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 rounded-lg bg-transparent outline-none focus:border-blue-500"
            >
              {categories.map((cat) => (
                <option key={cat} value={cat}>{CategoryLabels[cat]}</option>
              ))}
            </select>
            <div className="flex gap-2">
              <button
                onClick={() => createMutation.mutate(form)}
                disabled={createMutation.isPending}
                className="flex items-center gap-2 px-3 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm rounded-lg disabled:opacity-50"
              >
                {createMutation.isPending ? <Loader2 size={14} className="animate-spin" /> : <Plus size={14} />}
                Add
              </button>
              <button
                onClick={() => setShowForm(false)}
                className="px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      {isLoading ? (
        <div className="flex justify-center py-10">
          <Loader2 size={24} className="animate-spin text-blue-500" />
        </div>
      ) : (
        <div className="space-y-2">
          {feeds?.map((feed) => (
            <div key={feed.id} className="flex items-center justify-between p-3 border border-gray-200 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-900">
              <div className="min-w-0">
                <p className="text-sm font-medium truncate">{feed.name}</p>
                <p className="text-xs text-gray-400 truncate">{feed.url}</p>
                <span className="text-xs text-blue-500">{CategoryLabels[feed.category]}</span>
              </div>
              <div className="flex items-center gap-1 shrink-0">
                <button
                  onClick={() => toggleMutation.mutate({ id: feed.id, isActive: !feed.isActive })}
                  className="p-1.5 rounded hover:bg-gray-100 dark:hover:bg-gray-800 text-gray-400 hover:text-blue-500"
                >
                  {feed.isActive ? <ToggleRight size={18} className="text-blue-500" /> : <ToggleLeft size={18} />}
                </button>
                <button
                  onClick={() => deleteMutation.mutate(feed.id)}
                  className="p-1.5 rounded hover:bg-gray-100 dark:hover:bg-gray-800 text-gray-400 hover:text-red-500"
                >
                  <Trash2 size={15} />
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
