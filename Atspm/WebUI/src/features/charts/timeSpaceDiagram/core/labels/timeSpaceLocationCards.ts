// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - timeSpaceLocationCards.ts
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
import {
  formatOffsetSeconds,
  getOffsetDeltaVisuals,
  getOffsetSecondsValue,
  hasModifiedOffset,
  normalizeOffsetSeconds,
  normalizeOffsetToCycleLengthSeconds,
} from '@/features/charts/timeSpaceDiagram/core/offsets/timeSpaceOffsets'
import type {
  RawTimeSpaceAverageData,
  TimeSpaceUnwrappedData,
} from '@/features/charts/timeSpaceDiagram/shared/types'
import type { TimeSpaceDisplayDistanceOffset } from '@/features/charts/timeSpaceDiagram/core/types/timeSpaceCore.types'
import { Color } from '@/features/charts/utils'
import type {
  CustomSeriesRenderItemParams,
  CustomSeriesRenderItemReturn,
  GridComponentOption,
  SeriesOption,
} from 'echarts'

export const TIME_SPACE_LOCATION_CARD_LAYOUT = {
  gridGap: 35,
  dotOffset: 36,
  cardGapToDot: 18,
  cardWidth: 200,
  cardRadius: 4,
  verticalOffsetY: 15,
  headerHeight: 40,
  bodyHeight: 52,
  bodyPaddingLeft: 8,
  bodyPaddingRight: 8,
  headerActionSize: 12,
  headerActionRight: 10,
  headerActionOverlayOffsetX: 15,
  headerActionOverlayOffsetY: 15,
} as const

const TIME_SPACE_LOCATION_METRIC_GAP = 8
const TIME_SPACE_LOCATION_OFFSET_LABEL_WIDTH = 115

type TimeSpaceLocationCardGeometry = {
  bodyContentWidth: number
  bodyRightX: number
  bodyTop: number
  cardHeight: number
  cardLeft: number
  cardRight: number
  cardTop: number
  bottomMetricRowY: number
  leftMetricWidth: number
  middleMetricRowY: number
  offsetMetricX: number
  rightMetricWidth: number
  textX: number
  topMetricRowY: number
  xDot: number
  xLine: number
}

type TimeSpaceLocationOffsetBadgeLayout = {
  highlightHeight: number
  highlightWidth: number
  highlightX: number
  highlightY: number
  iconCenterX: number
  iconCenterY: number
  iconContainerHeight: number
  iconContainerWidth: number
  iconContainerX: number
  iconContainerY: number
  iconLeftX: number
  iconSize: number
  iconTopY: number
  overlayHeight: number
  overlayWidth: number
  overlayX: number
  overlayY: number
  textRightX: number
}

export const TIME_SPACE_LOCATION_AXIS_SERIES_ID = 'Location axis'

const TIME_SPACE_DISTANCE_VALUE_CARD_WIDTH = 96

export const TIME_SPACE_CYCLE_LABEL_CARD_LAYOUT = {
  cardWidth: 90,
  cardRadius: 2,
  headerHeight: 18,
  cardGapFromPlot: 5,
  cardGapBetween: 5,
  verticalOffsetY: 0,
  bodyPaddingX: 7,
  bodyPaddingY: 4,
  lineHeight: 13,
  minBodyHeight: 16,
} as const

export const TIME_SPACE_CARD_CONNECTOR_STROKE = '#CBD5E1'
export const TIME_SPACE_CARD_CONNECTOR_IGNORED_STROKE = '#D8E0E8'
export const TIME_SPACE_CARD_CONNECTOR_WIDTH = 2
export const TIME_SPACE_PHASE_CONNECTOR_INNER_OFFSET = 45
export const TIME_SPACE_PHASE_CONNECTOR_END_INSET = 20
export const TIME_SPACE_PHASE_CONNECTOR_ARROW_SIZE = 5
export const TIME_SPACE_PHASE_CONNECTOR_MIN_LENGTH = 12

function splitPrimarySecondary(desc: string | undefined) {
  const raw = (desc ?? '').trim()
  const noId = raw.replace(/^\s*#?\d+\s*-\s*/, '')
  const [primary, secondary = ''] = noId.split(/\s*&\s*/, 2)

  return {
    primary: (primary ?? '').trim(),
    secondary: (secondary ?? '').trim(),
  }
}

export function splitIdentifierAndDescription(text: string | undefined) {
  const raw = (text ?? '').trim()
  const match = raw.match(/^\s*(#?\d+)\s*-\s*(.+)$/)

  return {
    identifier: match?.[1]?.trim() ?? '',
    description: match?.[2]?.trim() ?? raw,
  }
}

export function buildIdentifierAndNameTitle(
  identifier: string | undefined,
  description: string | undefined,
  maxCharacters?: number
) {
  const ident = (identifier ?? '').trim()
  const { primary, secondary } = splitPrimarySecondary(description)
  let name =
    primary && secondary ? `${primary} & ${secondary}` : primary || secondary
  const reservedIdentifierLength = ident ? ident.length + 3 : 0
  const maxNameCharacters =
    maxCharacters == null
      ? undefined
      : Math.max(0, maxCharacters - reservedIdentifierLength)

  if (maxNameCharacters != null && name.length > maxNameCharacters) {
    name = truncateWithEllipsis(name, maxNameCharacters)
  }

  if (ident && name) {
    return `{ident|${ident}}{name| - ${name}}`
  }

  if (ident) {
    return `{ident|${ident}}`
  }

  if (name) {
    return `{name|${name}}`
  }

  return ''
}

function truncateWithEllipsis(text: string, maxCharacters: number) {
  if (text.length <= maxCharacters) {
    return text
  }

  if (maxCharacters <= 3) {
    return '.'.repeat(Math.max(0, maxCharacters))
  }

  return `${text.slice(0, maxCharacters - 3).trimEnd()}...`
}

function getTimeSpaceLocationCardGeometry(
  gridLeft: number,
  y: number
): TimeSpaceLocationCardGeometry {
  const {
    gridGap,
    dotOffset,
    cardGapToDot,
    cardWidth,
    headerHeight,
    bodyHeight,
    bodyPaddingLeft,
    bodyPaddingRight,
    verticalOffsetY,
  } = TIME_SPACE_LOCATION_CARD_LAYOUT
  const cardHeight = headerHeight + bodyHeight
  const xTextRight = gridLeft - gridGap
  const xDot = xTextRight + dotOffset
  const cardRight = xDot - cardGapToDot
  const cardLeft = cardRight - cardWidth
  const xLine = cardRight + (gridLeft - cardRight) / 2
  const cardTop = y - cardHeight / 2 + verticalOffsetY
  const textX = cardLeft + bodyPaddingLeft
  const bodyTop = cardTop + headerHeight
  const bodyContentWidth = cardWidth - bodyPaddingLeft - bodyPaddingRight
  const bodyRightX = cardRight - bodyPaddingRight
  const topMetricContentWidth =
    bodyContentWidth - TIME_SPACE_LOCATION_METRIC_GAP
  const leftMetricWidth = Math.round(topMetricContentWidth * 0.45)
  const rightMetricWidth = topMetricContentWidth - leftMetricWidth
  const offsetMetricX = textX + leftMetricWidth + TIME_SPACE_LOCATION_METRIC_GAP
  const topMetricRowY = bodyTop + 8
  const middleMetricRowY = bodyTop + bodyHeight / 2
  const bottomMetricRowY = bodyTop + bodyHeight - 8

  return {
    bodyContentWidth,
    bodyRightX,
    bodyTop,
    cardHeight,
    cardLeft,
    cardRight,
    cardTop,
    bottomMetricRowY,
    leftMetricWidth,
    middleMetricRowY,
    offsetMetricX,
    rightMetricWidth,
    textX,
    topMetricRowY,
    xDot,
    xLine,
  }
}

export function getTimeSpaceLocationOffsetBadgeLayout(
  gridLeft: number,
  y: number,
  offsetText: string,
  showResetIcon: boolean
): TimeSpaceLocationOffsetBadgeLayout {
  const {
    bodyRightX,
    bottomMetricRowY,
    cardLeft,
    cardRight,
    middleMetricRowY,
    textX,
    topMetricRowY,
  } = getTimeSpaceLocationCardGeometry(gridLeft, y)
  void offsetText
  void showResetIcon
  const iconContainerSize = 0
  const iconContainerX = Math.round(bodyRightX)
  const textRightX = bodyRightX
  const overlayX = cardLeft
  const overlayY = (topMetricRowY + middleMetricRowY) / 2
  const overlayBottomY = (middleMetricRowY + bottomMetricRowY) / 2
  const overlayHeight = Math.max(0, overlayBottomY - overlayY)
  const overlayWidth = Math.max(0, cardRight - cardLeft)
  const highlightX = textX + TIME_SPACE_LOCATION_OFFSET_LABEL_WIDTH
  const highlightY = overlayY
  const highlightHeight = overlayHeight
  const highlightWidth = Math.max(0, cardRight - highlightX)
  const iconSize = 0
  const iconContainerY = Math.round(topMetricRowY - iconContainerSize / 2)
  const iconLeftX = iconContainerX + (iconContainerSize - iconSize) / 2
  const iconTopY = iconContainerY + (iconContainerSize - iconSize) / 2

  return {
    highlightHeight,
    highlightWidth,
    highlightX,
    highlightY,
    iconCenterX: iconContainerX + iconContainerSize / 2,
    iconCenterY: iconContainerY + iconContainerSize / 2,
    iconContainerHeight: iconContainerSize,
    iconContainerWidth: iconContainerSize,
    iconContainerX,
    iconContainerY,
    iconLeftX,
    iconSize,
    iconTopY,
    overlayHeight,
    overlayWidth,
    overlayX,
    overlayY,
    textRightX,
  }
}

function formatCycleLengthSummaryValue(value: unknown) {
  if (typeof value === 'number' && Number.isFinite(value) && value > 0) {
    return `${value}s`
  }

  return 'unknown'
}

function formatCycleLengthValue(value: unknown) {
  if (typeof value === 'number' && Number.isFinite(value) && value > 0) {
    return `${value}s`
  }

  return 'unknown'
}

function formatOffsetTotalSeconds(
  offsetValue: unknown,
  cycleLengthValue: unknown
) {
  const offsetSeconds = getOffsetSecondsValue(offsetValue)

  if (offsetSeconds == null) {
    return 'unknown'
  }

  const cycleLengthSeconds = getOffsetSecondsValue(cycleLengthValue)
  if (cycleLengthSeconds == null || cycleLengthSeconds <= 0) {
    return formatOffsetSeconds(Math.abs(offsetSeconds))
  }

  const wrappedOffsetSeconds =
    ((offsetSeconds % cycleLengthSeconds) + cycleLengthSeconds) %
    cycleLengthSeconds

  return formatOffsetSeconds(normalizeOffsetSeconds(wrappedOffsetSeconds))
}

function getLocationInitialOffsetSeconds(
  location: TimeSpaceUnwrappedData[number]
): number | null {
  const offset =
    'offset' in location ? getOffsetSecondsValue(location.offset) : null
  const offsetLengthChangeEvents =
    'offsetLengthChangeEvents' in location
      ? getOffsetSecondsValue(location.offsetLengthChangeEvents)
      : null

  return offset ?? offsetLengthChangeEvents
}

export function getLocationsLabelOption(
  data: TimeSpaceUnwrappedData,
  distanceData: number[],
  grid: GridComponentOption
): SeriesOption {
  const gridLeft = (grid.left as number) ?? 0

  const {
    cardWidth,
    cardRadius,
    headerHeight,
    bodyHeight,
    headerActionSize,
    headerActionRight,
  } = TIME_SPACE_LOCATION_CARD_LAYOUT

  return {
    id: TIME_SPACE_LOCATION_AXIS_SERIES_ID,
    name: TIME_SPACE_LOCATION_AXIS_SERIES_ID,
    type: 'custom',
    silent: true,
    clip: false,
    renderItem: (params, api) => {
      const idx = params.dataIndexInside ?? params.dataIndex
      const len = params.dataInsideLength ?? distanceData.length

      const [, y] = api.coord([api.value(0), api.value(1)])
      const {
        bodyContentWidth,
        bodyRightX,
        cardHeight,
        cardLeft,
        cardRight,
        cardTop,
        bottomMetricRowY,
        textX,
        middleMetricRowY,
        topMetricRowY,
        xLine,
      } = getTimeSpaceLocationCardGeometry(gridLeft, y)
      const iconLeft = cardRight - headerActionRight - headerActionSize
      const dividerX = iconLeft - 8
      const titleWidth = Math.max(0, dividerX - textX - 8)

      const children: CustomSeriesRenderItemReturn[] = []

      if (idx === 0 && len > 1) {
        const last = len - 1
        const [, yTop] = api.coord([api.value(0, 0), api.value(1, 0)])
        const [, yBottom] = api.coord([api.value(0, last), api.value(1, last)])

        children.push({
          type: 'line',
          shape: { x1: xLine, y1: yTop, x2: xLine, y2: yBottom },
          style: { stroke: Color.PlanB, lineWidth: 3 },
          z2: 1,
        })
      }

      const location = data.find(
        (loc) => loc.locationIdentifier === api.value(2).toString()
      )
      const isIgnored = Boolean(location?.isIgnoredLocation)

      children.push({
        type: 'line',
        shape: { x1: xLine, y1: y, x2: gridLeft, y2: y },
        style: {
          stroke: isIgnored
            ? TIME_SPACE_CARD_CONNECTOR_IGNORED_STROKE
            : TIME_SPACE_CARD_CONNECTOR_STROKE,
          lineWidth: TIME_SPACE_CARD_CONNECTOR_WIDTH,
        },
        z2: 2,
      })

      children.push({
        type: 'circle',
        shape: { cx: xLine, cy: y, r: 4 },
        style: {
          fill: isIgnored ? '#CBD5E1' : Color.LightBlue,
          stroke: '#FFFFFF',
          lineWidth: 1.5,
        },
        z2: 4,
      })

      const ident = String(api.value(2) ?? '')
      const maxTitleCharacters = Math.max(0, Math.floor(titleWidth / 6) * 2)
      const titleText = buildIdentifierAndNameTitle(
        ident,
        String(api.value(3) ?? ''),
        maxTitleCharacters
      )
      const cycleLengthValue = api.value(4)
      const currentOffsetValue = api.value(5)
      const actualOffsetValue = api.value(6)
      const userAdjustmentValue = api.value(7)
      const currentOffsetSecondsRaw = getOffsetSecondsValue(currentOffsetValue)
      const actualOffsetSecondsRaw =
        getOffsetSecondsValue(actualOffsetValue) ?? currentOffsetSecondsRaw
      const currentOffsetSeconds =
        currentOffsetSecondsRaw == null
          ? null
          : normalizeOffsetToCycleLengthSeconds(
              currentOffsetSecondsRaw,
              cycleLengthValue
            )
      const actualOffsetSeconds =
        actualOffsetSecondsRaw == null
          ? null
          : normalizeOffsetToCycleLengthSeconds(
              actualOffsetSecondsRaw,
              cycleLengthValue
            )
      const isDeltaOffsetModified = hasModifiedOffset(
        currentOffsetSeconds,
        actualOffsetSeconds,
        userAdjustmentValue
      )
      const userAdjustmentSeconds =
        getOffsetSecondsValue(userAdjustmentValue) ?? 0
      const offsetDirectionSeconds =
        Math.abs(userAdjustmentSeconds) >= 0.0001
          ? userAdjustmentSeconds
          : currentOffsetSeconds != null && actualOffsetSeconds != null
            ? currentOffsetSeconds - actualOffsetSeconds
            : 0
      const deltaOffsetVisuals = getOffsetDeltaVisuals(offsetDirectionSeconds)
      const offsetLabelWidth = TIME_SPACE_LOCATION_OFFSET_LABEL_WIDTH
      const offsetValueWidth = Math.max(0, bodyContentWidth - offsetLabelWidth)
      const cycleLabelWidth = 120
      const cycleValueWidth = Math.max(0, bodyContentWidth - cycleLabelWidth)
      const cycleText = formatCycleLengthValue(cycleLengthValue)
      const programmedOffsetText = formatOffsetTotalSeconds(
        actualOffsetSeconds,
        cycleLengthValue
      )
      const userAdjustedOffsetText = formatOffsetTotalSeconds(
        currentOffsetSeconds,
        cycleLengthValue
      )
      const deltaOffsetBadgeLayout = getTimeSpaceLocationOffsetBadgeLayout(
        gridLeft,
        y,
        userAdjustedOffsetText,
        false
      )
      const bodyChildren = isIgnored
        ? []
        : [
            ...(isDeltaOffsetModified
              ? [
                  {
                    type: 'rect' as const,
                    z2: 19,
                    shape: {
                      x: deltaOffsetBadgeLayout.highlightX,
                      y: deltaOffsetBadgeLayout.highlightY,
                      width: deltaOffsetBadgeLayout.highlightWidth,
                      height: deltaOffsetBadgeLayout.highlightHeight,
                      r: 0,
                    },
                    style: {
                      fill: deltaOffsetVisuals.highlightFill,
                      stroke: deltaOffsetVisuals.highlightStroke,
                      lineWidth: 1,
                    },
                  },
                ]
              : []),
            {
              type: 'text' as const,
              z2: 20,
              style: {
                x: textX,
                y: topMetricRowY,
                text: 'Programmed Offset',
                width: offsetLabelWidth,
                overflow: 'truncate',
                textAlign: 'left',
                textVerticalAlign: 'middle',
                fill: '#64748B',
                fontSize: 11,
                fontWeight: 500,
              },
            },
            {
              type: 'text' as const,
              z2: 20,
              style: {
                x: bodyRightX,
                y: topMetricRowY,
                text: programmedOffsetText,
                width: offsetValueWidth,
                overflow: 'truncate',
                textAlign: 'right',
                textVerticalAlign: 'middle',
                fill: '#111827',
                fontSize: 11,
                fontWeight: 700,
              },
            },
            {
              type: 'text' as const,
              z2: 20,
              style: {
                x: textX,
                y: middleMetricRowY,
                text: 'User-Adjusted Offset',
                width: offsetLabelWidth,
                overflow: 'truncate',
                textAlign: 'left',
                textVerticalAlign: 'middle',
                fill: '#64748B',
                fontSize: 11,
                fontWeight: 500,
              },
            },
            {
              type: 'text' as const,
              z2: 20,
              style: {
                x: bodyRightX,
                y: middleMetricRowY,
                text: userAdjustedOffsetText,
                width: offsetValueWidth,
                overflow: 'truncate',
                textAlign: 'right',
                textVerticalAlign: 'middle',
                fill: isDeltaOffsetModified
                  ? deltaOffsetVisuals.valueColor
                  : '#111827',
                fontSize: 11,
                fontWeight: 700,
              },
            },
            {
              type: 'text' as const,
              z2: 20,
              style: {
                x: textX,
                y: bottomMetricRowY,
                text: 'Cycle Length',
                width: cycleLabelWidth,
                overflow: 'truncate',
                textAlign: 'left',
                textVerticalAlign: 'middle',
                fill: '#64748B',
                fontSize: 11,
                fontWeight: 500,
              },
            },
            {
              type: 'text' as const,
              z2: 20,
              style: {
                x: bodyRightX,
                y: bottomMetricRowY,
                text: cycleText,
                width: cycleValueWidth,
                overflow: 'truncate',
                textAlign: 'right',
                textVerticalAlign: 'middle',
                fill: '#111827',
                fontSize: 11,
                fontWeight: 700,
              },
            },
          ]

      children.push({
        type: 'group',
        z2: 2,
        children: [
          {
            type: 'rect',
            z2: 10,
            shape: {
              x: cardLeft,
              y: cardTop,
              width: cardWidth,
              height: cardHeight,
              r: cardRadius,
            },
            style: {
              fill: isIgnored ? '#F8FAFC' : '#FFFFFF',
              stroke: isIgnored ? '#D5DCE5' : '#D9DEE6',
              lineWidth: 1,
              opacity: isIgnored ? 0.82 : 1,
            },
          },
          {
            type: 'rect',
            z2: 11,
            shape: {
              x: cardLeft,
              y: cardTop,
              width: cardWidth,
              height: headerHeight,
              r: bodyHeight > 0 ? [cardRadius, cardRadius, 0, 0] : cardRadius,
            },
            style: {
              fill: isIgnored ? '#F1F5F9' : '#EEF1F5',
              opacity: isIgnored ? 0.88 : 1,
            },
          },
          {
            type: 'text',
            z2: 20,
            style: {
              x: textX,
              y: cardTop + 6,
              text: titleText,
              width: titleWidth,
              height: 28,
              overflow: 'break',
              lineOverflow: 'truncate',
              lineHeight: 14,
              textAlign: 'left',
              textVerticalAlign: 'top',
              rich: {
                ident: {
                  fill: isIgnored ? '#334155' : '#111',
                  fontSize: 11,
                  fontWeight: 700,
                  opacity: isIgnored ? 0.6 : 1,
                },
                name: {
                  fill: isIgnored ? '#64748B' : '#111',
                  fontSize: 11,
                  fontWeight: 400,
                  opacity: isIgnored ? 0.72 : 1,
                },
              },
            },
          },
          {
            type: 'line',
            z2: 20,
            shape: {
              x1: dividerX,
              y1: cardTop + 6,
              x2: dividerX,
              y2: cardTop + headerHeight - 6,
            },
            style: {
              stroke: isIgnored ? '#D8E0E8' : '#CBD5E1',
              lineWidth: 1,
            },
          },
          ...bodyChildren,
        ],
      })

      return { type: 'group', children }
    },
    data: distanceData.map((distance, index) => {
      const location = data[index]
      const initialOffset = getLocationInitialOffsetSeconds(location)

      return [
        location.start,
        distance,
        location.locationIdentifier,
        location.locationDescription,
        location.cycleLength,
        initialOffset,
        initialOffset,
        0,
      ]
    }),
  }
}

export function getOffsetAndProgramSplitLabel(
  primaryPhaseData: RawTimeSpaceAverageData[],
  opposingPhaseData: RawTimeSpaceAverageData[],
  distanceData: number[],
  primaryDirection: string,
  opposingDirection: string,
  endDate: string
): SeriesOption {
  return {
    name: 'Labels offset and program split',
    type: 'custom',
    renderItem: (params: CustomSeriesRenderItemParams, api) => {
      const [, y] = api.coord([api.value(0), api.value(1)])
      const width = (params.coordSys as { width: number }).width

      return {
        type: 'group',
        position: [width + 140, y + 11],
        children: [
          {
            type: 'text',
            style: {
              x: 60,
              y: 10,
              textVerticalAlign: 'bottom',
              textAlign: 'center',
              text:
                'Cycle Length: ' +
                formatCycleLengthSummaryValue(api.value(2)) +
                '\n' +
                `Offset (${primaryDirection}: ${api.value(
                  3
                )}s | ${opposingDirection}: ${api.value(5)}s)\n` +
                `Split (${primaryDirection}: ${api.value(
                  4
                )}s | ${opposingDirection}: ${api.value(6)}s)\n`,
              textFill: '#000',
              fontSize: 10,
            },
          },
        ],
      }
    },
    data: distanceData.map((distance, index) => [
      endDate,
      distance,
      primaryPhaseData[index].cycleLength,
      primaryPhaseData[index].offset,
      primaryPhaseData[index].programmedSplit,
      opposingPhaseData[distanceData.length - 1 - index].offset,
      opposingPhaseData[distanceData.length - 1 - index].programmedSplit,
    ]),
  }
}

export function getDistancesLabelOption(
  data: TimeSpaceUnwrappedData,
  distanceData: number[],
  gridLeft: number,
  distanceScale = 1,
  displayDistanceOffset?: TimeSpaceDisplayDistanceOffset
): SeriesOption {
  const { gridGap, dotOffset, cardGapToDot, verticalOffsetY } =
    TIME_SPACE_LOCATION_CARD_LAYOUT
  const dataPoints = distanceData.map((distance, index) => {
    const distanceToNext =
      index !== distanceData.length - 1 ? data[index].distanceToNextLocation : 0
    const displayDistanceToNext =
      displayDistanceOffset?.(index, distanceToNext) ??
      distanceToNext * distanceScale

    return [
      data[index].end,
      distance,
      distanceToNext,
      index !== distanceData.length - 1 ? data[index].speed : '',
      displayDistanceToNext,
    ]
  })
  return {
    name: 'Labels distance',
    type: 'custom',
    z: 30,
    silent: true,
    selectedMode: false,
    tooltip: { show: false },
    renderItem: (params, api) => {
      if (params.dataIndex === dataPoints.length - 1) {
        return
      }

      const xDot = gridLeft - gridGap + dotOffset
      const cardRight = xDot - cardGapToDot
      const xLine = cardRight + (gridLeft - cardRight) / 2
      const valueCardWidth = TIME_SPACE_DISTANCE_VALUE_CARD_WIDTH
      const valueCardHeight = 26
      const valueCardRight = cardRight
      const valueCardLeft = valueCardRight - valueCardWidth
      const dividerX = valueCardLeft + valueCardWidth / 2
      const distanceText = `${(api.value(2) as number).toLocaleString()} ft`
      const speedText = `${api.value(3)} mph`
      const [, rawY] = api.coord([
        0,
        (api.value(1) as number) + (api.value(4) as number) / 2,
      ])
      const y = rawY + verticalOffsetY

      return {
        type: 'group',
        silent: true,
        emphasisDisabled: true,
        children: [
          {
            type: 'line',
            z2: 30,
            shape: {
              x1: xLine,
              y1: y,
              x2: valueCardRight,
              y2: y,
            },
            style: {
              stroke: Color.PlanB,
              lineWidth: 3,
            },
          },
          {
            type: 'rect',
            z2: 31,
            shape: {
              x: valueCardLeft,
              y: y - valueCardHeight / 2,
              width: valueCardWidth,
              height: valueCardHeight,
              r: 4,
            },
            style: {
              fill: '#e7f4fc',
              stroke: 'rgba(86, 180, 233, 0.38)',
              lineWidth: 1,
            },
          },
          {
            type: 'line',
            z2: 32,
            shape: {
              x1: dividerX,
              y1: y - 8,
              x2: dividerX,
              y2: y + 8,
            },
            style: {
              stroke: 'rgba(86, 180, 233, 0.34)',
              lineWidth: 1,
            },
          },
          {
            type: 'text',
            z2: 33,
            style: {
              x: valueCardLeft + valueCardWidth / 4,
              y,
              text: distanceText,
              textFill: '#000',
              fontSize: 10,
              fontWeight: 600,
              textAlign: 'center',
              textVerticalAlign: 'middle',
            },
          },
          {
            type: 'text',
            z2: 33,
            style: {
              x: valueCardLeft + (valueCardWidth * 3) / 4,
              y,
              text: speedText,
              textFill: '#2B4C68',
              fontSize: 10,
              textAlign: 'center',
              textVerticalAlign: 'middle',
            },
          },
        ],
      }
    },
    data: dataPoints,
  }
}

export function getDraggableOffsetabelOption(
  data: TimeSpaceUnwrappedData,
  distanceData: number[],
  phaseType?: string,
  extraLinesByIndex?: Array<string[] | undefined>
): SeriesOption[] {
  const seriesOptions: SeriesOption[] = []

  for (let i = 0; i < data.length; i++) {
    const location = data[i]
    const distance = distanceData[i]

    const dataPoint: [string, number, number, number][] = [
      [
        location.end,
        distance,
        i !== distanceData.length - 1 ? location.distanceToNextLocation : 0,
        0,
      ],
    ]

    seriesOptions.push({
      name: 'Offset amount',
      id: `Offset ${location.locationIdentifier} ${phaseType?.length ? phaseType : ''}`,
      type: 'custom',
      data: dataPoint,
      renderItem: (params, api) => {
        const coordSys = params.coordSys
        const [, y] = api.coord([0, api.value(1) as number])

        const textX = coordSys.x + coordSys.width + 40
        const offsetValue = api.value(3)
        const fontSize = 10
        const lineGap = 12
        const extra = extraLinesByIndex?.[params.dataIndex] ?? []
        const lines = [`Offset: ${offsetValue}s`, ...extra]
        const blockTopY = y - 6
        const lineX = textX - 6
        const lineY1 = blockTopY - 2
        const lineY2 = blockTopY + (lines.length - 1) * lineGap + fontSize + 2

        return {
          type: 'group',
          children: [
            {
              type: 'line',
              shape: { x1: lineX, y1: lineY1, x2: lineX, y2: lineY2 },
              style: { stroke: '#000', lineWidth: 1 },
            },
            ...lines.map((text, index) => ({
              type: 'text',
              style: {
                x: textX,
                y: blockTopY + index * lineGap,
                text,
                textFill: '#000',
                fontSize,
              },
            })),
          ],
        }
      },
      clip: false,
    })
  }

  return seriesOptions
}
