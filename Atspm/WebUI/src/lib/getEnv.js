let envCache = null

export const getEnv = () => {
  if (envCache) {
    return envCache
  }

  if (typeof window !== 'undefined') {
    // Running on the client
    envCache = window.__ENV__
    console.log('Client-side getEnv envCache:', envCache)
  } else {
    // Running on the server
    envCache = {
      CONFIG_URL: process.env.CONFIG_URL,
      REPORTS_URL: process.env.REPORTS_URL,
      IDENTITY_URL: process.env.IDENTITY_URL,
      DATA_URL: process.env.DATA_URL,
      SPEED_URL: process.env.SPEED_URL,
      MAP_DEFAULT_LATITUDE: process.env.MAP_DEFAULT_LATITUDE,
      MAP_DEFAULT_LONGITUDE: process.env.MAP_DEFAULT_LONGITUDE,
      MAP_TILE_LAYER: process.env.MAP_TILE_LAYER,
      MAP_TILE_ATTRIBUTION: process.env.MAP_TILE_ATTRIBUTION,
    }

    console.log('Server-side getEnv envCache:', envCache)
  }

  return envCache
}
