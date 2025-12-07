import { CompressedDataBase } from '@/api/data/aTSPMLogDataApi.schemas'
import { ResponseFormat } from '@/features/data/api/getEventLogs'
import { DataTypeOption } from '@/features/data/components/dataTypeSelector'
import { dateToTimestamp } from '@/utils/dateTime'

export const formatData = (data: CompressedDataBase[], type: string) => {
  if (type === 'application/json') {
    return JSON.stringify(data)
  }
  if (type === 'text/csv' || type === 'application/xml') {
    const eventLogData = data.flatMap((d) => d.data)
    if (eventLogData.length === 0) return ''

    const headers = Object.keys(eventLogData[0])
    if (type === 'text/csv') {
      // Build CSV string
      let csv = headers.join(',') + '\n'
      eventLogData.forEach((eventLog: any) => {
        const row = headers
          .map((header) => {
            if (header === 'timestamp') {
              return eventLog[header].replace('T', ' ')
            }
            // Wrap string with commas in quotes
            if (
              typeof eventLog[header] === 'string' &&
              eventLog[header].includes(',')
            ) {
              return `"${eventLog[header]}"`
            }
            return eventLog[header]
          })
          .join(',')
        csv += row + '\n'
      })
      return csv
    } else if (type === 'application/xml') {
      // Build XML string
      let xml = '<?xml version="1.0" encoding="UTF-8"?>\n<eventLogs>\n'
      eventLogData.forEach((eventLog: any) => {
        xml += '  <eventLog>\n'
        headers.forEach((header) => {
          xml += `    <${header}>${eventLog[header]}</${header}>\n`
        })
        xml += '  </eventLog>\n'
      })
      xml += '</eventLogs>'
      return xml
    }
  }
  return data
}

export const generateFilename = (
  location: Location | null,
  selectedDataType: DataTypeOption | null,
  start: Date,
  end: Date,
  downloadFormat: ResponseFormat
): string => {
  const startStr = dateToTimestamp(start)
  const endStr = dateToTimestamp(end)
  const baseName = `${location?.locationIdentifier}_${selectedDataType?.name.replace(/\s+/g, '')}_${startStr}`

  return selectedDataType?.type === 'raw'
    ? `${baseName}.${downloadFormat}`
    : `${baseName}_${endStr}.${downloadFormat}`
}

export const downloadData = (
  data: CompressedDataBase[],
  filename: string,
  mimeType: string
) => {
  const formattedData = formatData(data, mimeType)
  const blob = new Blob([formattedData], { type: mimeType })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}
