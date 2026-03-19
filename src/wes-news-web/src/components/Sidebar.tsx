import { NavLink, useNavigate } from 'react-router-dom'
import { Rss, Settings, BookOpen, Moon, Sun, List, LogOut, Compass, LogIn, UserPlus } from 'lucide-react'
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
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
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
    if (!isAuthenticated) return
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
        <button
          onClick={toggleDarkMode}
          className="w-8 h-8 flex items-center justify-center rounded-xl text-zinc-400 hover:text-zinc-900 dark:hover:text-zinc-100 hover:bg-zinc-100 dark:hover:bg-zinc-800 transition-all"
          title={isDarkMode ? 'Switch to Light' : 'Switch to Dark'}
        >
          {isDarkMode ? <Sun size={15} /> : <Moon size={15} />}
        </button>
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
        {isAuthenticated ? (
          <>
            <NavLink
              to="/settings"
              onClick={() => setSidebarOpen(false)}
              className={({ isActive }) => `nav-item ${isActive ? 'nav-item-active' : ''}`}
            >
              <Settings size={18} />
              Settings
            </NavLink>
          </>
        ) : null}
        <a
          href="https://github.com/wesleyll4/wes-news"
          target="_blank"
          rel="noopener noreferrer"
          className="nav-item"
        >
          <svg viewBox="0 0 24 24" width="18" height="18" fill="currentColor" aria-hidden="true">
            <path d="M12 2C6.477 2 2 6.477 2 12c0 4.418 2.865 8.166 6.839 9.489.5.092.682-.217.682-.482 0-.237-.009-.868-.013-1.703-2.782.604-3.369-1.34-3.369-1.34-.454-1.156-1.11-1.463-1.11-1.463-.908-.62.069-.608.069-.608 1.003.07 1.531 1.03 1.531 1.03.892 1.529 2.341 1.087 2.91.832.092-.647.35-1.088.636-1.338-2.22-.253-4.555-1.11-4.555-4.943 0-1.091.39-1.984 1.029-2.683-.103-.253-.446-1.27.098-2.647 0 0 .84-.269 2.75 1.025A9.578 9.578 0 0 1 12 6.836a9.59 9.59 0 0 1 2.504.337c1.909-1.294 2.747-1.025 2.747-1.025.546 1.377.202 2.394.1 2.647.64.699 1.028 1.592 1.028 2.683 0 3.842-2.339 4.687-4.566 4.935.359.309.678.919.678 1.852 0 1.336-.012 2.415-.012 2.743 0 .267.18.578.688.48C19.138 20.163 22 16.418 22 12c0-5.523-4.477-10-10-10z" />
          </svg>
          GitHub
        </a>

        <div className="flex items-center justify-between pt-4 px-1">
          {isAuthenticated ? (
            <button
              onClick={() => { logout(); navigate('/') }}
              className="w-9 h-9 flex items-center justify-center rounded-xl text-zinc-400 hover:bg-red-500/10 hover:text-red-500 transition-all"
              title="Sign Out"
            >
              <LogOut size={16} />
            </button>
          ) : (
            <div className="flex items-center gap-1.5 w-full">
              <NavLink
                to="/login"
                onClick={() => setSidebarOpen(false)}
                className="flex-1 flex items-center justify-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium text-zinc-600 dark:text-zinc-400 border border-zinc-200 dark:border-zinc-700 hover:border-indigo-400 hover:text-indigo-500 transition-all"
              >
                <LogIn size={13} />
                Entrar
              </NavLink>
              <NavLink
                to="/register"
                onClick={() => setSidebarOpen(false)}
                className="flex-1 flex items-center justify-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium text-white bg-indigo-600 hover:bg-indigo-500 transition-all"
              >
                <UserPlus size={13} />
                Cadastrar
              </NavLink>
            </div>
          )}
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
          md:relative md:translate-x-0 md:flex md:flex-col md:w-64 md:bg-zinc-50/50 md:dark:bg-zinc-900/40
        `}
      >
        {content}
      </aside>
    </>
  )
}
