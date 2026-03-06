import { useState } from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { digestApi } from '../api/client'
import { Send, Eye, Loader2, CheckCircle } from 'lucide-react'

export default function SettingsPage() {
  const [showPreview, setShowPreview] = useState(false)
  const [sent, setSent] = useState(false)

  const { data: preview, isLoading: previewLoading } = useQuery({
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
    <div className="max-w-2xl mx-auto p-6">
      <h1 className="text-xl font-bold mb-6">Settings</h1>

      <section className="mb-8">
        <h2 className="text-sm font-semibold mb-3">Daily Digest</h2>
        <div className="p-4 border border-gray-200 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-900 space-y-3">
          <p className="text-sm text-gray-500 dark:text-gray-400">
            Configure the digest schedule and email address via environment variables
            <code className="mx-1 px-1 py-0.5 bg-gray-100 dark:bg-gray-800 rounded text-xs">RESEND_APITOKEN</code>,
            <code className="mx-1 px-1 py-0.5 bg-gray-100 dark:bg-gray-800 rounded text-xs">DIGEST_EMAIL_TO</code> and
            <code className="mx-1 px-1 py-0.5 bg-gray-100 dark:bg-gray-800 rounded text-xs">DIGEST_CRON</code>.
          </p>
          <div className="flex gap-2">
            <button
              onClick={() => sendMutation.mutate()}
              disabled={sendMutation.isPending}
              className="flex items-center gap-2 px-3 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm rounded-lg disabled:opacity-50 transition-colors"
            >
              {sendMutation.isPending ? <Loader2 size={14} className="animate-spin" /> : sent ? <CheckCircle size={14} /> : <Send size={14} />}
              {sent ? 'Sent!' : 'Send Now'}
            </button>
            <button
              onClick={() => setShowPreview(!showPreview)}
              className="flex items-center gap-2 px-3 py-2 border border-gray-300 dark:border-gray-600 text-sm rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
            >
              <Eye size={14} />
              Preview Digest
            </button>
          </div>
        </div>
      </section>

      {showPreview && (
        <section>
          <h2 className="text-sm font-semibold mb-3">
            Digest Preview
            {preview && <span className="ml-2 font-normal text-gray-400">({preview.articleCount} articles)</span>}
          </h2>
          <div className="border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden bg-white">
            {previewLoading ? (
              <div className="flex justify-center py-10">
                <Loader2 size={24} className="animate-spin text-blue-500" />
              </div>
            ) : preview ? (
              <iframe
                srcDoc={preview.html}
                className="w-full h-96"
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
