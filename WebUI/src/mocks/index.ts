async function initMocks() {
  if (typeof window === 'undefined') {
    const { server } = await import('./server')
    server.listen()
  } else {
    const { worker } = await import('./browser')
    worker.start({
      onUnhandledRequest: 'bypass', // This ensures unhandled requests bypass the service worker
    })
  }
}

export { initMocks }
