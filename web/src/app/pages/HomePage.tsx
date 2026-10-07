import { useAuth } from '../../features/auth/auth-context'

export function HomePage() {
  const { user } = useAuth()

  return (
    <div>
      <h1 className="text-2xl font-semibold">Welcome back{user ? `, ${user.email}` : ''}.</h1>
      <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">
        The topic tree and question bank land later in Phase 1.
      </p>
    </div>
  )
}
