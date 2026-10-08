import { createBrowserRouter, type RouteObject } from 'react-router'
import { RequireAuth } from '../features/auth/components/RequireAuth'
import { LoginPage } from '../features/auth/pages/LoginPage'
import { RegisterPage } from '../features/auth/pages/RegisterPage'
import { TopicsPage } from '../features/topics/pages/TopicsPage'
import { AppLayout } from './layout/AppLayout'

export const routes: RouteObject[] = [
  { path: '/login', element: <LoginPage /> },
  { path: '/register', element: <RegisterPage /> },
  {
    element: <RequireAuth />,
    children: [
      {
        element: <AppLayout />,
        children: [{ path: '/', element: <TopicsPage /> }],
      },
    ],
  },
]

export const router = createBrowserRouter(routes)
