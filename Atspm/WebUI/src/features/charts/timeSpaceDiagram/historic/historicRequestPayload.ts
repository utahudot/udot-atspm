import { TimeSpaceHistoricOptions } from '@/features/charts/timeSpaceDiagram/shared/types'

const toBase64 = (bytes: Uint8Array): string => {
  let binary = ''
  const chunkSize = 0x8000

  for (let i = 0; i < bytes.length; i += chunkSize) {
    const chunk = bytes.subarray(i, i + chunkSize)
    binary += String.fromCharCode(...chunk)
  }

  return btoa(binary)
}

const gzipAndBase64 = async (file: File): Promise<string> => {
  const rawBuffer = await file.arrayBuffer()
  const rawBytes = new Uint8Array(rawBuffer)

  if (typeof CompressionStream === 'undefined') {
    return toBase64(rawBytes)
  }

  const compressedStream = new Blob([rawBytes]).stream().pipeThrough(
    new CompressionStream('gzip')
  )
  const compressedBuffer = await new Response(compressedStream).arrayBuffer()
  return toBase64(new Uint8Array(compressedBuffer))
}

export const buildHistoricRequestPayload = async (
  options: TimeSpaceHistoricOptions
): Promise<TimeSpaceHistoricOptions> => {
  const payload: TimeSpaceHistoricOptions = { ...options }

  if (payload.includeSrmSearch && payload.srmCsvFile) {
    payload.srmCsvContentBase64 = await gzipAndBase64(payload.srmCsvFile)
  } else {
    payload.srmCsvContentBase64 = undefined
  }

  delete payload.srmCsvFile
  return payload
}
