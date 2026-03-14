import { useState } from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { digestApi } from '../api/client'
import { Send, Eye, Loader2, CheckCircle2, Mail, Terminal } from 'lucide-react'

export default function SettingsPage() {
  const [showPreview, setShowPreview] = useState(false)
  const [sent, setSent] = useState(false)

  const { data: preview, isLoading: previewLoading } = useQuery<{ html: string; articleCount: number }>({
    queryKey: ['digest-preview'],
    queryFn: digestApi.preview,
    enabled: showPreview
  })

  const sendMutation = useMutation({
    mutationFn: digestApi.send,
    onSuccess: () => {
      setSent(true)
      setTimeout(() => setSent(false), 4000)
    }
  })

  return (
    <div className="max-w-2xl mx-auto px-4 py-8">
      <div className="mb-8">
        <h1 className="font-display font-bold text-2xl tracking-tight">Settings</h1>
        <p className="text-sm text-zinc-500 dark:text-zinc-400 mt-0.5">Manage your digest and notifications</p>
      </div>

      <section className="mb-6">
        <div className="flex items-center gap-2 mb-3">
          <Mail size={14} className="text-zinc-400" />
          <h2 className="text-xs font-semibold uppercase tracking-widest text-zinc-400 dark:text-zinc-500">Daily Digest</h2>
        </div>

        <div className="p-5 border border-zinc-100 dark:border-zinc-800 rounded-xl bg-white dark:bg-zinc-900">
          <p className="text-sm text-zinc-600 dark:text-zinc-300 mb-1">
            Sends a curated email every morning with the top unread articles per category.
          </p>
          <p className="text-xs text-zinc-400 dark:text-zinc-500 mb-5">
            Configure schedule and destination via environment variables.
          </p>

          <div className="flex flex-wrap gap-2 mb-5">
            {['RESEND_APITOKEN', 'DIGEST_EMAIL_TO', 'DIGEST_CRON'].map((v) => (
              <code key={v} className="px-2 py-1 text-[11px] font-mono bg-zinc-100 dark:bg-zinc-800 text-zinc-600 dark:text-zinc-400 rounded-md">
                {v}
              </code>
            ))}
          </div>

          <div className="flex flex-wrap gap-2">
            <button
              onClick={() => sendMutation.mutate()}
              disabled={sendMutation.isPending}
              className="btn-primary"
            >
              {sendMutation.isPending
                ? <Loader2 size={14} className="animate-spin" />
                : sent
                ? <CheckCircle2 size={14} />
                : <Send size={14} />}
              {sent ? 'Sent!' : 'Send Now'}
            </button>
            <button
              onClick={() => setShowPreview(!showPreview)}
              className="btn-ghost"
            >
              <Eye size={14} />
              {showPreview ? 'Hide Preview' : 'Preview Digest'}
            </button>
          </div>
        </div>
      </section>

      {showPreview && (
        <section className="animate-fade-in">
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
              <iframe
                srcDoc={preview.html}
                className="w-full h-[480px]"
                title="Digest Preview"
                sandbox="allow-same-origin"
              />
            ) : null}
          </div>
        </section>
      )}
    </div>
  )
}
