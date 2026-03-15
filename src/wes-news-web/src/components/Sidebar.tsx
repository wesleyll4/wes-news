import { NavLink, useNavigate } from 'react-router-dom'
import { Rss, Settings, BookOpen, Moon, Sun, List, LogOut, Compass } from 'lucide-react'
import { Category, CategoryLabels, CategoryColors } from '../types'
import { useUiStore } from '../store/uiStore'
import { useAuthStore } from '../store/authStore'
import { motion, AnimatePresence } from 'framer-motion'

export default function Sidebar() {
  const {
    selectedCategory, setSelectedCategory,
    isDarkMode, toggleDarkMode,
    setUnreadOnly, unreadOnly,
    sidebarOpen, setSidebarOpen
  } = useUiStore()
  const logout = useAuthStore((s) => s.logout)
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
      <div className="px-6 py-8 flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-2xl bg-indigo-600 dark:bg-indigo-500 flex items-center justify-center shadow-lg shadow-indigo-500/30">
            <Compass className="text-white" size={22} />
          </div>
          <div>
            <p className="font-display font-bold text-xl tracking-tight leading-none bg-gradient-to-br from-zinc-900 to-zinc-500 dark:from-white dark:to-zinc-500 bg-clip-text text-transparent">
              WesNews
            </p>
            <p className="text-[10px] text-zinc-400 font-bold tracking-[0.2em] uppercase mt-1.5 opacity-60">
              Core Feed
            </p>
          </div>
        </div>
      </div>

      <nav className="flex-1 overflow-y-auto px-4 py-2 space-y-1 custom-scrollbar">
        <div className="px-3 pb-2">
          <p className="text-[11px] font-bold text-zinc-400 dark:text-zinc-600 uppercase tracking-widest">
            Main Feed
          </p>
        </div>
        
        <NavLink
          to="/"
          onClick={handleAllNews}
          className={({ isActive }) =>
            `nav-item ${isActive && !selectedCategory && !unreadOnly ? 'nav-item-active' : ''}`
          }
        >
          <List size={18} />
          All News
        </NavLink>

        <button
          onClick={handleUnreadOnly}
          className={`nav-item w-full text-left ${unreadOnly ? 'nav-item-active' : ''}`}
        >
          <BookOpen size={18} />
          Unread Only
        </button>

        <div className="pt-8 pb-2 px-3">
          <p className="text-[11px] font-bold text-zinc-400 dark:text-zinc-600 uppercase tracking-widest">
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
              className={`nav-item w-full text-left group ${isActive ? 'nav-item-active' : ''}`}
            >
              <span className={`w-2 h-2 rounded-full shrink-0 ${colors.dot} group-hover:scale-125 transition-transform`} />
              {CategoryLabels[cat]}
            </button>
          )
        })}
      </nav>

      <div className="px-4 py-6 mt-auto space-y-1 border-t border-zinc-200/50 dark:border-zinc-800/50">
        <NavLink
          to="/sources"
          onClick={() => setSidebarOpen(false)}
          className={({ isActive }) => `nav-item ${isActive ? 'nav-item-active' : ''}`}
        >
          <Rss size={18} />
          Sources
        </NavLink>
        <NavLink
          to="/settings"
          onClick={() => setSidebarOpen(false)}
          className={({ isActive }) => `nav-item ${isActive ? 'nav-item-active' : ''}`}
        >
          <Settings size={18} />
          Settings
        </NavLink>
        
        <div className="flex items-center gap-2 pt-4 px-1">
          <button
            onClick={toggleDarkMode}
            className="flex-1 flex items-center justify-center gap-2 py-2.5 rounded-xl border border-zinc-200 dark:border-zinc-800 text-zinc-500 hover:text-zinc-900 dark:hover:text-zinc-100 transition-all text-xs font-medium"
          >
            {isDarkMode ? <Sun size={14} /> : <Moon size={14} />}
            {isDarkMode ? 'Light' : 'Dark'}
          </button>
          <button
            onClick={() => {
              logout()
              navigate('/login')
            }}
            className="w-11 h-11 flex items-center justify-center rounded-xl bg-red-500/10 text-red-500 hover:bg-red-500 hover:text-white transition-all"
            title="Sign Out"
          >
            <LogOut size={16} />
          </button>
        </div>
      </div>
    </div>
  )

  return (
    <>
      <AnimatePresence>
        {sidebarOpen && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 bg-black/20 dark:bg-black/40 backdrop-blur-sm z-30 md:hidden"
            onClick={() => setSidebarOpen(false)}
          />
        )}
      </AnimatePresence>

      <aside
        className={`
          fixed inset-y-0 left-0 z-40 w-72 glass border-r
          transform transition-transform duration-300 ease-[cubic-bezier(0.33,1,0.68,1)]
          ${sidebarOpen ? 'translate-x-0' : '-translate-x-full'}
          md:relative md:translate-x-0 md:flex md:flex-col md:w-64 md:bg-white/40 md:dark:bg-zinc-900/40
        `}
      >
        {content}
      </aside>
    </>
  )
}
