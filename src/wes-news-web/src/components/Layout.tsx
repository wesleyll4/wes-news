import { Outlet } from 'react-router-dom'
import { Menu } from 'lucide-react'
import Sidebar from './Sidebar'
import { useUiStore } from '../store/uiStore'

export default function Layout() {
  const setSidebarOpen = useUiStore((s) => s.setSidebarOpen)

  return (
    <div className="relative flex h-screen bg-zinc-50 dark:bg-[#050505] text-zinc-900 dark:text-zinc-100 overflow-hidden font-sans">
      {/* Background Ambient Glows */}
      <div className="absolute top-[-10%] left-[-10%] w-[40%] h-[40%] bg-indigo-500/[0.04] dark:bg-indigo-500/5 blur-[120px] rounded-full animate-blob pointer-events-none" />
      <div className="absolute bottom-[10%] right-[-5%] w-[35%] h-[35%] bg-purple-500/[0.04] dark:bg-purple-500/5 blur-[120px] rounded-full animate-blob animation-delay-2000 pointer-events-none" />
      <div className="absolute top-[20%] right-[10%] w-[25%] h-[25%] bg-blue-500/[0.04] dark:bg-blue-500/5 blur-[120px] rounded-full animate-blob animation-delay-4000 pointer-events-none" />

      <Sidebar />

      <div className="relative flex-1 flex flex-col min-w-0 overflow-hidden z-10">
        <header className="md:hidden flex items-center gap-3 px-6 py-4 glass shrink-0">
          <button
            onClick={() => setSidebarOpen(true)}
            className="p-2 -ml-2 rounded-xl text-zinc-500 hover:text-zinc-900 dark:hover:text-zinc-100 hover:bg-zinc-100/50 dark:hover:bg-zinc-800/50 transition-all"
          >
            <Menu size={20} />
          </button>
          <span className="font-display font-bold text-xl tracking-tight bg-gradient-to-br from-zinc-900 to-zinc-500 dark:from-white dark:to-zinc-500 bg-clip-text text-transparent">
            WesNews
          </span>
        </header>

        <main className="flex-1 overflow-auto">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
