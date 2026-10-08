import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type { components } from '../../shared/api/schema'
import { apiFetch } from '../../shared/lib/http'

export type TopicVisibility = 'private' | 'public'
export type Topic = components['schemas']['TopicResponse']
export type TopicTreeNode = components['schemas']['TopicTreeNodeResponse']
export type CreateTopicRequest = components['schemas']['CreateTopicRequest']
export type UpdateTopicRequest = components['schemas']['UpdateTopicRequest']

const topicTreeKey = ['topics', 'tree'] as const

export function getTopicTree(): Promise<TopicTreeNode[]> {
  return apiFetch<TopicTreeNode[]>('/topics')
}

export function createTopic(request: CreateTopicRequest): Promise<Topic> {
  return apiFetch<Topic>('/topics', { method: 'POST', body: request })
}

export function updateTopic(id: string, request: UpdateTopicRequest): Promise<Topic> {
  return apiFetch<Topic>(`/topics/${id}`, { method: 'PATCH', body: request })
}

export function deleteTopic(id: string): Promise<void> {
  return apiFetch<void>(`/topics/${id}`, { method: 'DELETE' })
}

export function useTopicTreeQuery() {
  return useQuery({ queryKey: topicTreeKey, queryFn: getTopicTree })
}

export function useCreateTopicMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: createTopic,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: topicTreeKey })
    },
  })
}

export function useUpdateTopicMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: UpdateTopicRequest }) => updateTopic(id, request),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: topicTreeKey })
    },
  })
}

export function useDeleteTopicMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: deleteTopic,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: topicTreeKey })
    },
  })
}
