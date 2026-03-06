import { NavLink, useNavigate } from 'react-router-dom'
import { Rss, Settings, BookOpen, Moon, Sun, List, X } from 'lucide-react'
import { Category, CategoryLabels, CategoryColors } from '../types'
import { useUiStore } from '../store/uiStore'

export default function Sidebar() {
  const {
    selectedCategory, setSelectedCategory,
    isDarkMode, toggleDarkMode,
    setUnreadOnly, unreadOnly,
    sidebarOpen, setSidebarOpen
  } = useUiStore()
  const navigate = useNavigate()

  const categories = Object.values(Category).filter((v): v is Category => typeof v === 'number')

  function handleCategory(cat: Category) {
    setSelectedCategory(cat)
    setUnreadOnly(false)
    navigate('/')
    setSidebarOpen(false)
  }

  function handleAllNews() {
    setSelectedCategory(undefined)
    setUnreadOnly(false)
    setSidebarOpen(false)
  }

  function handleUnreadOnly() {
    setUnreadOnly(!unreadOnly)
    navigate('/')
    setSidebarOpen(false)
  }

  const content = (
    <div className="flex flex-col h-full">
      <div className="px-5 py-5 flex items-center justify-between border-b border-zinc-100 dark:border-zinc-800">
        <div>
          <p className="font-display font-bold text-xl tracking-tight">WesNews</p>
          <p className="text-[11px] text-zinc-400 font-medium tracking-wider uppercase mt-0.5">Tech Feed</p>
        </div>
        <div className="flex items-center gap-1">
          <button
            onClick={toggleDarkMode}
            className="p-2 rounded-lg text-zinc-400 hover:text-zinc-700 dark:hover:text-zinc-200 hover:bg-zinc-100 dark:hover:bg-zinc-800 transition-colors"
          >
            {isDarkMode ? <Sun size={16} /> : <Moon size={16} />}
          </button>
          <button
            onClick={() => setSidebarOpen(false)}
            className="p-2 rounded-lg text-zinc-400 hover:text-zinc-700 dark:hover:text-zinc-200 hover:bg-zinc-100 dark:hover:bg-zinc-800 transition-colors md:hidden"
          >
            <X size={16} />
          </button>
        </div>
      </div>

      <nav className="flex-1 overflow-y-auto px-3 py-4 space-y-0.5">
        <NavLink
          to="/"
          onClick={handleAllNews}
          className={({ isActive }) =>
            `nav-item ${isActive && !selectedCategory && !unreadOnly ? 'nav-item-active' : ''}`
          }
        >
          <List size={16} />
          All News
        </NavLink>

        <button
          onClick={handleUnreadOnly}
          className={`nav-item w-full text-left ${unreadOnly ? 'nav-item-active' : ''}`}
        >
          <BookOpen size={16} />
          Unread Only
        </button>

        <div className="pt-4 pb-1 px-3">
          <p className="text-[10px] font-semibold text-zinc-400 dark:text-zinc-600 uppercase tracking-widest">
            Categories
          </p>
        </div>

        {categories.map((cat) => {
          const colors = CategoryColors[cat]
          const isActive = selectedCategory === cat
          return (
            <button
              key={cat}
              onClick={() => handleCategory(cat)}
              className={`nav-item w-full text-left ${isActive ? 'nav-item-active' : ''}`}
            >
              <span className={`w-2 h-2 rounded-full shrink-0 ${colors.dot}`} />
              {CategoryLabels[cat]}
            </button>
          )
        })}
      </nav>

      <div className="px-3 py-4 border-t border-zinc-100 dark:border-zinc-800 space-y-0.5">
        <NavLink
          to="/sources"
          onClick={() => setSidebarOpen(false)}
          className={({ isActive }) => `nav-item ${isActive ? 'nav-item-active' : ''}`}
        >
          <Rss size={16} />
          Sources
        </NavLink>
        <NavLink
          to="/settings"
          onClick={() => setSidebarOpen(false)}
          className={({ isActive }) => `nav-item ${isActive ? 'nav-item-active' : ''}`}
        >
          <Settings size={16} />
          Settings
        </NavLink>
      </div>
    </div>
  )

  return (
    <>
      {sidebarOpen && (
        <div
          className="fixed inset-0 bg-black/40 z-30 md:hidden animate-fade-in"
          onClick={() => setSidebarOpen(false)}
        />
      )}
      <aside
        className={`
          fixed inset-y-0 left-0 z-40 w-64 bg-white dark:bg-zinc-950 border-r border-zinc-100 dark:border-zinc-800
          transform transition-transform duration-250 ease-out
          ${sidebarOpen ? 'translate-x-0 animate-slide-in' : '-translate-x-full'}
          md:relative md:translate-x-0 md:flex md:flex-col md:w-56
        `}
      >
        {content}
      </aside>
    </>
  )
}
