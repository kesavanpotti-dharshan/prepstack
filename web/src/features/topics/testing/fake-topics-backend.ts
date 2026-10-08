import { vi } from 'vitest'

// Test-only in-memory fake of the Topics API, so UI tests can exercise real
// create/edit/delete/tree-refetch flows without a chain of brittle
// mockResolvedValueOnce calls (mutations trigger TanStack Query refetches,
// so the mock needs to actually track state across calls).
interface FakeTopic {
  id: string
  slug: string
  name: string
  parentId: string | null
  path: string
  description: string | null
  visibility: 'private' | 'public'
  questionCount: number
  createdAt: string
  updatedAt: string
}

interface FakeTopicNode extends FakeTopic {
  children: FakeTopicNode[]
}

function slugify(name: string): string {
  return name
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/(^-+|-+$)/g, '')
}

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } })
}

function problemResponse(title: string, code: string, status: number): Response {
  return new Response(JSON.stringify({ title, code }), {
    status,
    headers: { 'Content-Type': 'application/problem+json' },
  })
}

export function createFakeTopicsBackend() {
  let topics: FakeTopic[] = [];
  let nextId = 1;

  function buildTree(): FakeTopicNode[] {
    const toNode = (topic: FakeTopic): FakeTopicNode => ({
      ...topic,
      children: topics.filter((t) => t.parentId === topic.id).map(toNode),
    });
    return topics.filter((t) => t.parentId === null).map(toNode);
  }

  const fetchMock = vi.fn(async (input: string | URL | Request, init?: RequestInit) => {
    const url = typeof input === 'string' ? input : input.toString();
    const method = (init?.method ?? 'GET').toUpperCase();

    if (url.endsWith('/auth/refresh')) {
      return new Response(null, { status: 401 });
    }

    if (url.endsWith('/topics') && method === 'GET') {
      return jsonResponse(buildTree());
    }

    if (url.endsWith('/topics') && method === 'POST') {
      const body = JSON.parse(init!.body as string) as {
        name: string;
        parentId: string | null;
        description: string | null;
        visibility: string | null;
      };

      const parent = body.parentId ? topics.find((t) => t.id === body.parentId) : undefined;
      if (body.parentId && !parent) {
        return problemResponse('Parent topic was not found.', 'topics.parent_not_found', 404);
      }

      const slug = slugify(body.name);
      const path = parent ? `${parent.path}/${slug}` : slug;
      if (topics.some((t) => t.path === path)) {
        return problemResponse(
          'A topic with this name already exists under the same parent.',
          'topics.duplicate_path',
          409,
        );
      }

      const now = new Date().toISOString();
      const topic: FakeTopic = {
        id: `topic-${nextId++}`,
        slug,
        name: body.name,
        parentId: body.parentId,
        path,
        description: body.description,
        visibility: body.visibility === 'public' ? 'public' : 'private',
        questionCount: 0,
        createdAt: now,
        updatedAt: now,
      };
      topics.push(topic);
      return jsonResponse(topic, 201);
    }

    const topicIdMatch = /\/topics\/([^/]+)$/.exec(url);
    if (topicIdMatch && method === 'PATCH') {
      const topic = topics.find((t) => t.id === topicIdMatch[1]);
      if (!topic) {
        return problemResponse('Topic was not found.', 'topics.not_found', 404);
      }

      const body = JSON.parse(init!.body as string) as { name: string; description: string | null; visibility: string };
      topic.name = body.name;
      topic.description = body.description;
      topic.visibility = body.visibility === 'public' ? 'public' : 'private';
      topic.updatedAt = new Date().toISOString();
      return jsonResponse(topic);
    }

    if (topicIdMatch && method === 'DELETE') {
      const id = topicIdMatch[1];
      const topic = topics.find((t) => t.id === id);
      if (!topic) {
        return problemResponse('Topic was not found.', 'topics.not_found', 404);
      }

      if (topics.some((t) => t.parentId === id)) {
        return problemResponse('Delete or move subtopics before deleting this topic.', 'topics.has_children', 409);
      }

      topics = topics.filter((t) => t.id !== id);
      return new Response(null, { status: 204 });
    }

    throw new Error(`Unhandled fetch in test: ${method} ${url}`);
  });

  return fetchMock;
}
