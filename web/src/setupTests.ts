import '@testing-library/jest-dom/vitest'
import { cleanup } from '@testing-library/react'
import { afterEach } from 'vitest'

// `test.globals` is off, so testing-library's automatic afterEach(cleanup)
// detection doesn't fire — register it explicitly.
afterEach(() => {
  cleanup()
})
