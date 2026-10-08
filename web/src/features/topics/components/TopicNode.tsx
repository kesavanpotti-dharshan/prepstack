import { useState } from 'react'
import { ApiError } from '../../../shared/lib/api-error'
import { Button } from '../../../shared/ui/Button'
import { useCreateTopicMutation, useDeleteTopicMutation, useUpdateTopicMutation, type TopicTreeNode } from '../api'
import { TopicForm } from './TopicForm'

type Panel = 'none' | 'edit' | 'add-child' | 'confirm-delete'

export function TopicNode({ node }: { node: TopicTreeNode }) {
  const [panel, setPanel] = useState<Panel>('none')
  const [expanded, setExpanded] = useState(true)
  const updateTopic = useUpdateTopicMutation()
  const createTopic = useCreateTopicMutation()
  const deleteTopic = useDeleteTopicMutation()

  const hasChildren = node.children.length > 0

  return (
    <li>
      <div className="flex flex-wrap items-center gap-2 py-1">
        {hasChildren ? (
          <button
            type="button"
            onClick={() => setExpanded((value) => !value)}
            aria-label={expanded ? `Collapse ${node.name}` : `Expand ${node.name}`}
            className="w-4 text-slate-400"
          >
            {expanded ? '▾' : '▸'}
          </button>
        ) : (
          <span className="w-4" aria-hidden="true" />
        )}

        <span className="font-medium text-slate-900 dark:text-slate-100">{node.name}</span>
        <span className="rounded-full bg-slate-100 px-2 py-0.5 text-xs text-slate-600 dark:bg-slate-800 dark:text-slate-300">
          {node.visibility}
        </span>
        <span className="text-xs text-slate-400">{node.questionCount} questions</span>

        <span className="ml-auto flex gap-3 text-sm">
          <button
            type="button"
            className="text-slate-500 hover:underline dark:text-slate-400"
            onClick={() => setPanel(panel === 'add-child' ? 'none' : 'add-child')}
          >
            Add subtopic
          </button>
          <button
            type="button"
            className="text-slate-500 hover:underline dark:text-slate-400"
            onClick={() => setPanel(panel === 'edit' ? 'none' : 'edit')}
          >
            Edit
          </button>
          <button
            type="button"
            className="text-red-600 hover:underline dark:text-red-400"
            onClick={() => setPanel(panel === 'confirm-delete' ? 'none' : 'confirm-delete')}
          >
            Delete
          </button>
        </span>
      </div>

      {panel === 'edit' ? (
        <TopicForm
          idPrefix={`topic-${node.id}-edit`}
          submitLabel="Save"
          defaultValues={{
            name: node.name,
            description: node.description ?? '',
            visibility: node.visibility === 'public' ? 'public' : 'private',
          }}
          onSubmit={async (values) => {
            await updateTopic.mutateAsync({ id: node.id, request: values })
            setPanel('none')
          }}
          onCancel={() => setPanel('none')}
        />
      ) : null}

      {panel === 'add-child' ? (
        <TopicForm
          idPrefix={`topic-${node.id}-add-child`}
          submitLabel="Add"
          onSubmit={async (values) => {
            await createTopic.mutateAsync({ ...values, parentId: node.id })
            setPanel('none')
          }}
          onCancel={() => setPanel('none')}
        />
      ) : null}

      {panel === 'confirm-delete' ? (
        <div className="flex flex-wrap items-center gap-2 py-1 text-sm">
          <span>Delete &quot;{node.name}&quot;?</span>
          <Button
            type="button"
            onClick={() => {
              deleteTopic.mutate(node.id, { onSuccess: () => setPanel('none') })
            }}
            className="bg-red-600 px-2 py-1 text-xs hover:bg-red-700 dark:bg-red-600 dark:hover:bg-red-700"
          >
            Yes, delete
          </Button>
          <Button
            type="button"
            onClick={() => setPanel('none')}
            className="bg-slate-200 px-2 py-1 text-xs text-slate-900 hover:bg-slate-300 dark:bg-slate-800 dark:text-slate-100 dark:hover:bg-slate-700"
          >
            Cancel
          </Button>
          {deleteTopic.isError ? (
            <span role="alert" className="text-red-600 dark:text-red-400">
              {deleteTopic.error instanceof ApiError ? deleteTopic.error.message : 'Could not delete this topic.'}
            </span>
          ) : null}
        </div>
      ) : null}

      {hasChildren && expanded ? (
        <ul className="ml-6 border-l border-slate-200 pl-3 dark:border-slate-800">
          {node.children.map((child) => (
            <TopicNode key={child.id} node={child} />
          ))}
        </ul>
      ) : null}
    </li>
  )
}
