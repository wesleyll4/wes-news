import { NavLink } from 'react-router-dom'
import { Rss, Settings, BookOpen, Moon, Sun, List } from 'lucide-react'
import { Category, CategoryLabels } from '../types'
import { useUiStore } from '../store/uiStore'

export default function Sidebar() {
  const { selectedCategory, setSelectedCategory, isDarkMode, toggleDarkMode, setUnreadOnly, unreadOnly } = useUiStore()

  const categories = Object.values(Category).filter((v): v is Category => typeof v === 'number')

  return (
    <aside className="w-56 flex flex-col border-r border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900">
      <div className="px-4 py-4 border-b border-gray-200 dark:border-gray-800 flex items-center justify-between">
        <span className="font-bold text-lg text-blue-600 dark:text-blue-400">WesNews</span>
        <button onClick={toggleDarkMode} className="p-1 rounded hover:bg-gray-100 dark:hover:bg-gray-800">
          {isDarkMode ? <Sun size={16} /> : <Moon size={16} />}
        </button>
      </div>

      <nav className="flex-1 overflow-y-auto px-2 py-3 space-y-1">
        <NavLink
          to="/"
          onClick={() => { setSelectedCategory(undefined); setUnreadOnly(false) }}
          className="flex items-center gap-2 px-3 py-2 rounded-lg text-sm hover:bg-gray-100 dark:hover:bg-gray-800"
        >
          <List size={16} />
          All News
        </NavLink>

        <div className="pt-2">
          <p className="px-3 text-xs font-semibold text-gray-400 uppercase tracking-wider mb-1">Categories</p>
          {categories.map((cat) => (
            <button
              key={cat}
              onClick={() => { setSelectedCategory(cat); setUnreadOnly(false) }}
              className={`w-full flex items-center gap-2 px-3 py-2 rounded-lg text-sm text-left hover:bg-gray-100 dark:hover:bg-gray-800 ${selectedCategory === cat ? 'bg-blue-50 dark:bg-blue-950 text-blue-600 dark:text-blue-400 font-medium' : ''}`}
            >
              {CategoryLabels[cat]}
            </button>
          ))}
        </div>

        <div className="pt-2">
          <button
            onClick={() => setUnreadOnly(!unreadOnly)}
            className={`w-full flex items-center gap-2 px-3 py-2 rounded-lg text-sm text-left hover:bg-gray-100 dark:hover:bg-gray-800 ${unreadOnly ? 'bg-blue-50 dark:bg-blue-950 text-blue-600 dark:text-blue-400 font-medium' : ''}`}
          >
            <BookOpen size={16} />
            Unread Only
          </button>
        </div>
      </nav>

      <div className="px-2 py-3 border-t border-gray-200 dark:border-gray-800 space-y-1">
        <NavLink
          to="/sources"
          className="flex items-center gap-2 px-3 py-2 rounded-lg text-sm hover:bg-gray-100 dark:hover:bg-gray-800"
        >
          <Rss size={16} />
          Sources
        </NavLink>
        <NavLink
          to="/settings"
          className="flex items-center gap-2 px-3 py-2 rounded-lg text-sm hover:bg-gray-100 dark:hover:bg-gray-800"
        >
          <Settings size={16} />
          Settings
        </NavLink>
      </div>
    </aside>
  )
}
