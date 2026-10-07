import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { Link, useNavigate } from 'react-router'
import { z } from 'zod'
import { applyApiErrorToForm } from '../../../shared/lib/form-errors'
import { Button } from '../../../shared/ui/Button'
import { TextField } from '../../../shared/ui/TextField'
import { useAuth } from '../auth-context'

const registerSchema = z.object({
  displayName: z.string().min(1, 'Display name is required').max(100),
  email: z.string().min(1, 'Email is required').email('Enter a valid email address').max(256),
  password: z.string().min(8, 'Password must be at least 8 characters').max(72),
})

type RegisterFormValues = z.infer<typeof registerSchema>

export function RegisterPage() {
  const { register: registerAccount } = useAuth()
  const navigate = useNavigate()
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<RegisterFormValues>({ resolver: zodResolver(registerSchema) })

  const onSubmit = async (values: RegisterFormValues) => {
    try {
      await registerAccount(values)
      navigate('/', { replace: true })
    } catch (error) {
      applyApiErrorToForm(error, setError, ['displayName', 'email', 'password'])
    }
  }

  return (
    <main className="flex min-h-svh items-center justify-center bg-white px-4 dark:bg-slate-950">
      <form
        onSubmit={handleSubmit(onSubmit)}
        className="flex w-full max-w-sm flex-col gap-4 rounded-lg border border-slate-200 p-6 dark:border-slate-800"
      >
        <h1 className="text-xl font-semibold text-slate-900 dark:text-slate-100">Create an account</h1>

        {errors.root ? (
          <p role="alert" className="text-sm text-red-600 dark:text-red-400">
            {errors.root.message}
          </p>
        ) : null}

        <TextField
          id="displayName"
          label="Display name"
          autoComplete="name"
          error={errors.displayName?.message}
          {...register('displayName')}
        />
        <TextField
          id="email"
          label="Email"
          type="email"
          autoComplete="email"
          error={errors.email?.message}
          {...register('email')}
        />
        <TextField
          id="password"
          label="Password"
          type="password"
          autoComplete="new-password"
          error={errors.password?.message}
          {...register('password')}
        />

        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Creating account…' : 'Create account'}
        </Button>

        <p className="text-sm text-slate-500 dark:text-slate-400">
          Already have an account?{' '}
          <Link to="/login" className="font-medium text-slate-900 underline dark:text-slate-100">
            Log in
          </Link>
        </p>
      </form>
    </main>
  )
}
