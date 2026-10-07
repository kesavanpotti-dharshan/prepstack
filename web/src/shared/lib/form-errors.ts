import type { FieldValues, Path, UseFormSetError } from 'react-hook-form'
import { ApiError } from './api-error'

function lowercaseFirst(value: string): string {
  return value.length ? value.charAt(0).toLowerCase() + value.slice(1) : value
}

/**
 * Maps a failed API call onto react-hook-form errors. FluentValidation's
 * `errors` dict keys are PascalCase property names (e.g. "DisplayName");
 * form field names are camelCase, so the first letter is lowercased to match.
 */
export function applyApiErrorToForm<T extends FieldValues>(
  error: unknown,
  setError: UseFormSetError<T>,
  knownFields: readonly string[],
): void {
  if (error instanceof ApiError && error.fieldErrors) {
    let applied = false
    for (const [field, messages] of Object.entries(error.fieldErrors)) {
      const name = lowercaseFirst(field)
      const message = messages[0]
      if (knownFields.includes(name) && message) {
        setError(name as Path<T>, { message })
        applied = true
      }
    }
    if (applied) {
      return
    }
  }

  setError('root' as Path<T>, {
    message: error instanceof ApiError ? error.message : 'Something went wrong. Please try again.',
  })
}
