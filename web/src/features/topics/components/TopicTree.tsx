import { useState } from 'react'
import { Button } from '../../../shared/ui/Button'
import { useCreateTopicMutation, useTopicTreeQuery } from '../api'
import { TopicForm } from './TopicForm'
import { TopicNode } from './TopicNode'

export function TopicTree() {
  const { data, isLoading, isError } = useTopicTreeQuery()
  const [addingRoot, setAddingRoot] = useState(false)
  const createTopic = useCreateTopicMutation()

  if (isLoading) {
    return <p className="text-sm text-slate-500 dark:text-slate-400">Loading topics…</p>
  }

  if (isError) {
    return <p className="text-sm text-red-600 dark:text-red-400">Could not load topics.</p>
  }

  const topics = data ?? []

  return (
    <div className="flex flex-col gap-4">
      {addingRoot ? (
        <TopicForm
          idPrefix="new-root-topic"
          submitLabel="Add topic"
          onSubmit={async (values) => {
            await createTopic.mutateAsync({ ...values, parentId: null })
            setAddingRoot(false)
          }}
          onCancel={() => setAddingRoot(false)}
        />
      ) : (
        <Button type="button" onClick={() => setAddingRoot(true)} className="self-start">
          New topic
        </Button>
      )}

      {topics.length === 0 ? (
        <p className="text-sm text-slate-500 dark:text-slate-400">No topics yet — create your first one above.</p>
      ) : (
        <ul className="flex flex-col">
          {topics.map((topic) => (
            <TopicNode key={topic.id} node={topic} />
          ))}
        </ul>
      )}
    </div>
  )
}
