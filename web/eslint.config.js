import js from '@eslint/js'
import prettier from 'eslint-config-prettier'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import globals from 'globals'
import tseslint from 'typescript-eslint'

export default tseslint.config(
  { ignores: ['dist'] },
  {
    // eslint-plugin-react-hooks@7's flat presets still ship a legacy
    // `plugins: ["react-hooks"]` array, so the plugin/rules are wired manually below.
    extends: [js.configs.recommended, ...tseslint.configs.recommended, reactRefresh.configs.vite, prettier],
    files: ['**/*.{ts,tsx}'],
    plugins: {
      'react-hooks': reactHooks,
    },
    languageOptions: {
      ecmaVersion: 2023,
      globals: globals.browser,
    },
    rules: {
      ...reactHooks.configs['recommended-latest'].rules,
    },
  },
)
