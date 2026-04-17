const toBase64 = (bytes: Uint8Array): string => {
  let binary = ''
  const chunkSize = 0x8000

  for (let i = 0; i < bytes.length; i += chunkSize) {
    const chunk = bytes.subarray(i, i + chunkSize)
    binary += String.fromCharCode(...chunk)
  }

  return btoa(binary)
}

export const gzipAndBase64 = async (file: File): Promise<string> => {
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
