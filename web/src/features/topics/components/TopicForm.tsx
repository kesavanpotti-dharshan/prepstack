import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { applyApiErrorToForm } from '../../../shared/lib/form-errors'
import { Button } from '../../../shared/ui/Button'
import { Select } from '../../../shared/ui/Select'
import { TextField } from '../../../shared/ui/TextField'

const topicFormSchema = z.object({
  name: z.string().min(1, 'Name is required').max(100),
  description: z.string().max(2_000),
  visibility: z.enum(['private', 'public']),
})

export type TopicFormValues = z.infer<typeof topicFormSchema>

interface TopicFormProps {
  idPrefix: string
  submitLabel: string
  defaultValues?: TopicFormValues
  onSubmit: (values: TopicFormValues) => Promise<void>
  onCancel: () => void
}

export function TopicForm({ idPrefix, submitLabel, defaultValues, onSubmit, onCancel }: TopicFormProps) {
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<TopicFormValues>({
    resolver: zodResolver(topicFormSchema),
    defaultValues: defaultValues ?? { name: '', description: '', visibility: 'private' },
  })

  const submit = async (values: TopicFormValues) => {
    try {
      await onSubmit(values)
    } catch (error) {
      applyApiErrorToForm(error, setError, ['name', 'description', 'visibility'])
    }
  }

  return (
    <form
      onSubmit={handleSubmit(submit)}
      className="flex flex-col gap-3 rounded-md border border-slate-200 p-3 dark:border-slate-800"
    >
      {errors.root ? (
        <p role="alert" className="text-sm text-red-600 dark:text-red-400">
          {errors.root.message}
        </p>
      ) : null}

      <TextField id={`${idPrefix}-name`} label="Name" error={errors.name?.message} {...register('name')} />
      <TextField
        id={`${idPrefix}-description`}
        label="Description (optional)"
        error={errors.description?.message}
        {...register('description')}
      />
      <Select
        id={`${idPrefix}-visibility`}
        label="Visibility"
        error={errors.visibility?.message}
        {...register('visibility')}
      >
        <option value="private">Private</option>
        <option value="public">Public</option>
      </Select>

      <div className="flex gap-2">
        <Button type="submit" disabled={isSubmitting} className="px-3 py-1.5 text-xs">
          {isSubmitting ? 'Saving…' : submitLabel}
        </Button>
        <Button
          type="button"
          onClick={onCancel}
          className="bg-slate-200 px-3 py-1.5 text-xs text-slate-900 hover:bg-slate-300 dark:bg-slate-800 dark:text-slate-100 dark:hover:bg-slate-700"
        >
          Cancel
        </Button>
      </div>
    </form>
  )
}
