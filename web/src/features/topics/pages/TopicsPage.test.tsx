import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { tokenStore } from '../../../shared/lib/token-store'
import { AuthProvider } from '../../auth/context'
import { createFakeTopicsBackend } from '../testing/fake-topics-backend'
import { TopicsPage } from './TopicsPage'

function renderTopicsPage() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  })
  return render(
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <TopicsPage />
      </AuthProvider>
    </QueryClientProvider>,
  )
}

async function createRootTopic(user: ReturnType<typeof userEvent.setup>, name: string) {
  await user.click(screen.getByRole('button', { name: 'New topic' }))
  await user.type(screen.getByLabelText('Name'), name)
  await user.click(screen.getByRole('button', { name: 'Add topic' }))
  await screen.findByText(name)
}

describe('TopicsPage', () => {
  beforeEach(() => {
    tokenStore.set(null)
    vi.stubGlobal('fetch', createFakeTopicsBackend())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('shows an empty state before any topics exist', async () => {
    renderTopicsPage()

    expect(await screen.findByText(/no topics yet/i)).toBeInTheDocument()
  })

  it('creates a root topic and shows it in the tree', async () => {
    const user = userEvent.setup()
    renderTopicsPage()
    await screen.findByText(/no topics yet/i)

    await createRootTopic(user, 'Dotnet')

    expect(screen.getByText('Dotnet')).toBeInTheDocument()
    expect(screen.queryByText(/no topics yet/i)).not.toBeInTheDocument()
  })

  it('creates a nested subtopic under an existing topic', async () => {
    const user = userEvent.setup()
    renderTopicsPage()
    await screen.findByText(/no topics yet/i)
    await createRootTopic(user, 'Dotnet')

    await user.click(screen.getByRole('button', { name: 'Add subtopic' }))
    await user.type(screen.getByLabelText('Name'), 'DI')
    await user.click(screen.getByRole('button', { name: 'Add' }))

    expect(await screen.findByText('DI')).toBeInTheDocument()
  })

  it("edits a topic's name and visibility", async () => {
    const user = userEvent.setup()
    renderTopicsPage()
    await screen.findByText(/no topics yet/i)
    await createRootTopic(user, 'Dotnet')

    await user.click(screen.getByRole('button', { name: 'Edit' }))
    const nameField = screen.getByLabelText('Name')
    await user.clear(nameField)
    await user.type(nameField, 'Dotnet (renamed)')
    await user.selectOptions(screen.getByLabelText('Visibility'), 'public')
    await user.click(screen.getByRole('button', { name: 'Save' }))

    await screen.findByText('Dotnet (renamed)')
    expect(screen.getByText('public')).toBeInTheDocument()
  })

  it('deletes a leaf topic after confirming', async () => {
    const user = userEvent.setup()
    renderTopicsPage()
    await screen.findByText(/no topics yet/i)
    await createRootTopic(user, 'Dotnet')

    await user.click(screen.getByRole('button', { name: 'Delete' }))
    await user.click(screen.getByRole('button', { name: 'Yes, delete' }))

    await waitFor(() => expect(screen.queryByText('Dotnet')).not.toBeInTheDocument())
    expect(await screen.findByText(/no topics yet/i)).toBeInTheDocument()
  })

  it('shows a conflict error when creating a duplicate topic', async () => {
    const user = userEvent.setup()
    renderTopicsPage()
    await screen.findByText(/no topics yet/i)
    await createRootTopic(user, 'Dotnet')

    await user.click(screen.getByRole('button', { name: 'New topic' }))
    await user.type(screen.getByLabelText('Name'), 'Dotnet')
    await user.click(screen.getByRole('button', { name: 'Add topic' }))

    expect(await screen.findByRole('alert')).toHaveTextContent(/already exists/i)
  })

  it('shows a conflict error when deleting a topic that has children', async () => {
    const user = userEvent.setup()
    renderTopicsPage()
    await screen.findByText(/no topics yet/i)
    await createRootTopic(user, 'Dotnet')
    await user.click(screen.getByRole('button', { name: 'Add subtopic' }))
    await user.type(screen.getByLabelText('Name'), 'DI')
    await user.click(screen.getByRole('button', { name: 'Add' }))
    await screen.findByText('DI')

    // Dotnet (the parent) renders before its DI child in document order.
    await user.click(screen.getAllByRole('button', { name: 'Delete' })[0])
    await user.click(screen.getByRole('button', { name: 'Yes, delete' }))

    expect(await screen.findByRole('alert')).toHaveTextContent(/delete or move subtopics/i)
  })
})
