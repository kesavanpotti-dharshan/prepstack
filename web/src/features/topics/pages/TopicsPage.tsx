import { useAuth } from '../../auth/auth-context'
import { TopicTree } from '../components/TopicTree'

export function TopicsPage() {
  const { user } = useAuth()

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold text-slate-900 dark:text-slate-100">Topics</h1>
        <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">
          {user ? `Signed in as ${user.email}. ` : null}
          Pick a topic to study, or build out your tree below.
        </p>
      </div>
      <TopicTree />
    </div>
  )
}
