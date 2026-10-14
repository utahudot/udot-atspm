import { isAxiosError } from 'axios'

export function getChartErrorMessage(error: unknown): string {
  const fallback =
    error instanceof Error && error.message
      ? error.message
      : 'Unable to generate charts.'

  if (!isAxiosError<unknown>(error)) return fallback

  const data = error.response?.data
  if (typeof data === 'string' && data.trim()) return data
  if (!data || typeof data !== 'object') return fallback

  const problem = data as Record<string, unknown>
  if (problem.errors && typeof problem.errors === 'object') {
    const messages = Object.values(problem.errors)
      .flat()
      .filter(
        (message): message is string =>
          typeof message === 'string' && message.trim().length > 0
      )
    if (messages.length > 0) return messages.join(' ')
  }

  for (const message of [problem.detail, problem.title]) {
    if (typeof message === 'string' && message.trim()) return message
  }

  return fallback
}
