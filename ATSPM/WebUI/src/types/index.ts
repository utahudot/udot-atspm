export type ApiResponse<T> = {
  '@odata.context': string
  value: T[]
}
