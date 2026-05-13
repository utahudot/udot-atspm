import type {
  EChartsOption,
  TitleComponentOption,
  ToolboxComponentOption,
} from 'echarts'

function getPrimaryToolbox(
  option?: EChartsOption
): ToolboxComponentOption | undefined {
  if (!option?.toolbox) return undefined

  return Array.isArray(option.toolbox)
    ? (option.toolbox[0] as ToolboxComponentOption | undefined)
    : (option.toolbox as ToolboxComponentOption)
}

function getTitleEntries(option?: EChartsOption): TitleComponentOption[] {
  if (!option?.title) return []

  return Array.isArray(option.title)
    ? (option.title as TitleComponentOption[])
    : ([option.title] as TitleComponentOption[])
}

function stripRichText(text: string) {
  return text.replace(/\{[^|}]+\|([^}]+)\}/g, '$1')
}

function normalizeHeaderText(text: string) {
  return stripRichText(text).replace(/\s+/g, ' ').trim()
}

function sanitizeFileName(name: string) {
  return name
    .trim()
    .replace(/[<>:"/\\|?*\x00-\x1F]/g, '-')
    .replace(/\s+/g, ' ')
}

export function getChartDownloadName(option?: EChartsOption) {
  const toolbox = getPrimaryToolbox(option)
  const saveAsImage = toolbox?.feature?.saveAsImage as
    | { name?: unknown }
    | undefined

  if (typeof saveAsImage?.name === 'string' && saveAsImage.name.trim()) {
    return sanitizeFileName(saveAsImage.name)
  }

  const title = option?.title
  const rawTitle = Array.isArray(title)
    ? title
        .map((entry) =>
          typeof entry?.text === 'string' ? entry.text.trim() : ''
        )
        .filter(Boolean)
        .join(' - ')
    : typeof title?.text === 'string'
      ? title.text.trim()
      : ''

  return sanitizeFileName(rawTitle || 'time-space-diagram')
}

export function extractHeaderContent(option?: EChartsOption) {
  const titleEntries = getTitleEntries(option)
  const primaryTitle =
    typeof titleEntries[0]?.text === 'string' ? titleEntries[0].text : ''
  const secondaryTitle =
    typeof titleEntries[1]?.text === 'string' ? titleEntries[1].text : ''

  const titleText = normalizeHeaderText(
    primaryTitle.split('\n')[0] ?? ''
  ).replace(/[,\s]+$/, '')
  const rangeText = normalizeHeaderText(secondaryTitle)

  const remainingTitles =
    titleEntries.length > 2 ? titleEntries.slice(2) : undefined

  return {
    titleText,
    rangeText,
    remainingTitles,
  }
}
