import { Search, X } from 'lucide-react'
import { useUiStore } from '../store/uiStore'
import { motion, AnimatePresence } from 'framer-motion'

export default function SearchBar() {
  const { searchTerm, setSearchTerm } = useUiStore()

  return (
    <div className="relative group max-w-md">
      <div className="absolute left-4 top-1/2 -translate-y-1/2 text-zinc-400 group-focus-within:text-indigo-500 transition-colors z-10">
        <Search size={16} />
      </div>
      <input
        type="text"
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
        placeholder="Search for signals..."
        className="input-field pl-11 pr-10"
      />
      <AnimatePresence>
        {searchTerm && (
          <motion.button
            initial={{ opacity: 0, scale: 0.8 }}
            animate={{ opacity: 1, scale: 1 }}
            exit={{ opacity: 0, scale: 0.8 }}
            onClick={() => setSearchTerm('')}
            className="absolute right-3 top-1/2 -translate-y-1/2 p-1.5 text-zinc-400 hover:text-zinc-600 dark:hover:text-zinc-200 hover:bg-zinc-100 dark:hover:bg-zinc-800 rounded-lg transition-all"
          >
            <X size={14} />
          </motion.button>
        )}
      </AnimatePresence>
    </div>
  )
}
