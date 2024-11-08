import axios from 'axios'
export interface EnvVariables {
  CONFIG_URL: string | undefined
  REPORTS_URL: string | undefined
  IDENTITY_URL: string | undefined
  DATA_URL: string | undefined
  SPEED_URL: string | undefined
  MAP_DEFAULT_LATITUDE: string | undefined
  MAP_DEFAULT_LONGITUDE: string | undefined
  MAP_TILE_LAYER: string | undefined
  MAP_TILE_ATTRIBUTION: string | undefined
  SPONSOR_IMAGE_URL: string | undefined
}
let cachedEnv: EnvVariables | null = null
export const getEnv = async (): Promise<EnvVariables | null> => {
  if (cachedEnv) {
    return cachedEnv
  }
  try {
    const response = await axios.get('/api/get-env')
    cachedEnv = response.data
    return cachedEnv
  } catch (error) {
    console.error('Failed to load environment variables:', error)
    throw error
  }
}
