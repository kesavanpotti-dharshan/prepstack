import { Link, Outlet } from 'react-router'
import { useAuth } from '../../features/auth/auth-context'
import { Button } from '../../shared/ui/Button'

export function AppLayout() {
  const { user, logout } = useAuth()

  return (
    <div className="flex min-h-svh flex-col bg-white text-slate-900 dark:bg-slate-950 dark:text-slate-100">
      <header className="flex items-center justify-between border-b border-slate-200 px-6 py-4 dark:border-slate-800">
        <Link to="/" className="text-lg font-semibold">
          Prepstack
        </Link>
        <nav className="flex items-center gap-4 text-sm">
          {user ? <span className="text-slate-500 dark:text-slate-400">{user.email}</span> : null}
          <Button type="button" onClick={() => void logout()} className="px-3 py-1.5">
            Log out
          </Button>
        </nav>
      </header>
      <main className="flex-1 px-6 py-8">
        <Outlet />
      </main>
    </div>
  )
}
