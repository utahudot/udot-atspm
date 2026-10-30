import { configRequest } from '@/lib/axios'

// Configuration stays in Config API. Its provider is selected from the saved device,
// independently of the report decoder that supplies observed movement counts.
export async function readDeviceZones(configurationDeviceId: number): Promise<string[]> {
  const names = await configRequest<string[]>({
    url: '/Detector/retrieveDetectionData',
    method: 'POST',
    data: { configurationDeviceId },
  })
  if (!Array.isArray(names) || names.some((name) => typeof name !== 'string' || !name.trim()))
    throw new Error('Invalid zone inventory response')
  if (new Set(names).size !== names.length)
    throw new Error('Duplicate zone names on the device')
  return names
}
