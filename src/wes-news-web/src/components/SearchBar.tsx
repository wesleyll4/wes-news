import { Search, X } from 'lucide-react'
import { useUiStore } from '../store/uiStore'

export default function SearchBar() {
  const { searchTerm, setSearchTerm } = useUiStore()

  return (
    <div className="relative">
      <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-zinc-400" />
      <input
        type="text"
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
        placeholder="Search articles..."
        className="w-full pl-9 pr-8 py-2 text-sm bg-zinc-100 dark:bg-zinc-800/80 border border-transparent focus:border-zinc-300 dark:focus:border-zinc-600 focus:bg-white dark:focus:bg-zinc-900 rounded-lg outline-none transition-colors placeholder:text-zinc-400 dark:placeholder:text-zinc-600"
      />
      {searchTerm && (
        <button
          onClick={() => setSearchTerm('')}
          className="absolute right-2 top-1/2 -translate-y-1/2 p-1 text-zinc-400 hover:text-zinc-600 dark:hover:text-zinc-300 rounded transition-colors"
        >
          <X size={12} />
        </button>
      )}
    </div>
  )
}
