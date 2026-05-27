// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - fileEncoding.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion
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
