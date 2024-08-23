export const getEnv = async () => {
  let url

  if (typeof window === 'undefined') {
    // Running on the server, use the full URL
    const baseUrl = process.env.BASE_URL || 'http://localhost:3000'
    url = `${baseUrl}/api/env`
  } else {
    // Running on the client, use relative URL
    url = '/api/env'
  }

  const response = await fetch(url)
  return await response.json()
}
