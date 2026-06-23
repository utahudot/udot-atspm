// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - timeSpaceCycleLabels.ts
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
  buildIdentifierAndNameTitle,
  splitIdentifierAndDescription,
  TIME_SPACE_CARD_CONNECTOR_IGNORED_STROKE,
  TIME_SPACE_CARD_CONNECTOR_STROKE,
  TIME_SPACE_CARD_CONNECTOR_WIDTH,
  TIME_SPACE_CYCLE_LABEL_CARD_LAYOUT,
  TIME_SPACE_PHASE_CONNECTOR_ARROW_SIZE,
  TIME_SPACE_PHASE_CONNECTOR_END_INSET,
  TIME_SPACE_PHASE_CONNECTOR_INNER_OFFSET,
  TIME_SPACE_PHASE_CONNECTOR_MIN_LENGTH,
} from '@/features/charts/timeSpaceDiagram/core/labels/timeSpaceLocationCards'
import { Color } from '@/features/charts/utils'
import { directionTypes as staticDirectionTypes } from '@/features/locations/components/editDetector/selectOptions'
import { getDirectionAccentColor } from '@/features/locations/utils/directionAccent'
import type {
  CustomSeriesRenderItemAPI,
  CustomSeriesRenderItemReturn,
  SeriesOption,
} from 'echarts'

type LabelColumn = 'left' | 'right'
type StaticDirectionTypeKey = keyof typeof staticDirectionTypes

const CARDINAL_DIRECTION_SVG_PATHS = ['m5 12 7-7 7 7', 'M12 19V5'] as const
const DIAGONAL_DIRECTION_SVG_PATHS = ['M17 17 7 7', 'M17 7H7v10'] as const
const DIRECTION_ICON_DATA_URLS = new Map<string, string>()

function getDirectionIconDataUrl(
  directionKey: StaticDirectionTypeKey,
  strokeColor = '#111111'
): string | null {
  const cacheKey = `${directionKey}:${strokeColor}`
  const cached = DIRECTION_ICON_DATA_URLS.get(cacheKey)
  if (cached) {
    return cached
  }

  const svgConfig = staticDirectionTypes[directionKey].chartSvg
  if (!svgConfig) {
    return null
  }

  const paths =
    svgConfig.family === 'diagonal'
      ? DIAGONAL_DIRECTION_SVG_PATHS
      : CARDINAL_DIRECTION_SVG_PATHS

  const svg = [
    `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="${strokeColor}" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">`,
    `<g transform="rotate(${svgConfig.rotationDeg} 12 12)">`,
    ...paths.map((path) => `<path d="${path}"/>`),
    '</g>',
    '</svg>',
  ].join('')

  const dataUrl = `data:image/svg+xml;utf8,${encodeURIComponent(svg)}`
  DIRECTION_ICON_DATA_URLS.set(cacheKey, dataUrl)
  return dataUrl
}

function getDirectionTypeKey(directionLabel: string): StaticDirectionTypeKey {
  const token = directionLabel.trim().split(/\s+/)[0]?.toUpperCase() ?? ''

  if ((token as StaticDirectionTypeKey) in staticDirectionTypes) {
    return token as StaticDirectionTypeKey
  }

  const prefixMatch = (
    Object.keys(staticDirectionTypes) as StaticDirectionTypeKey[]
  ).find((key) => key !== 'NA' && token.startsWith(key))

  return prefixMatch ?? 'NA'
}

function extractPercentValue(text: string): number | null {
  const match = text.match(/(\d+(?:\.\d+)?)%/)
  if (!match) {
    return null
  }

  const value = Number(match[1])
  if (!Number.isFinite(value)) {
    return null
  }

  return Math.max(0, Math.min(100, value))
}

export const CYCLE_LABEL_SERIES_ID_PREFIX = 'Cycle Labels '

export function generateCycleLabels(
  distanceData: number[],
  direction: string,
  _gridLeft = 0,
  headerTextByIndex?: Array<string | undefined>,
  linesByIndex?: Array<string[] | undefined>,
  column: LabelColumn = 'left',
  ignoredByIndex?: boolean[]
): SeriesOption {
  void _gridLeft

  const {
    cardWidth,
    cardRadius,
    headerHeight,
    cardGapFromPlot,
    cardGapBetween,
    verticalOffsetY,
    bodyPaddingX,
    bodyPaddingY,
    lineHeight,
    minBodyHeight,
  } = TIME_SPACE_CYCLE_LABEL_CARD_LAYOUT

  const getCardBodyHeight = (index: number) => {
    const detailLines = (linesByIndex?.[index] ?? []).filter(Boolean)

    return detailLines.length
      ? Math.max(
          minBodyHeight,
          detailLines.length * lineHeight + bodyPaddingY * 2
        )
      : 0
  }

  const getCardMetrics = (api: CustomSeriesRenderItemAPI, index: number) => {
    const [, y] = api.coord([0, distanceData[index]])
    const bodyHeight = getCardBodyHeight(index)
    const cardHeight = headerHeight + bodyHeight
    const anchorY = y + verticalOffsetY
    const cardTop = anchorY - cardHeight / 2

    return {
      bodyHeight,
      cardHeight,
      cardTop,
      cardBottom: cardTop + cardHeight,
      isIgnored: Boolean(ignoredByIndex?.[index]),
    }
  }

  return {
    id: `${CYCLE_LABEL_SERIES_ID_PREFIX}${direction} ${column}`,
    name: `Cycles ${direction}`,
    type: 'custom',
    silent: true,
    clip: false,
    z: 7,
    renderItem: (params, api) => {
      const rowIndex = params.dataIndex
      const { bodyHeight, cardHeight, cardTop, cardBottom, isIgnored } =
        getCardMetrics(api, rowIndex)
      const coordSys = params.coordSys as { x: number; width: number }
      const primaryCardLeft = coordSys.x + coordSys.width + cardGapFromPlot
      const cardLeft =
        column === 'left'
          ? primaryCardLeft
          : primaryCardLeft + cardWidth + cardGapBetween

      const headerText = headerTextByIndex?.[rowIndex]?.trim() || direction
      const { identifier: headerIdentifier, description: headerDescription } =
        splitIdentifierAndDescription(headerText)
      const headerTitleText = buildIdentifierAndNameTitle(
        headerIdentifier,
        headerDescription
      )
      const headerAccentColor = getDirectionAccentColor(headerText)
      const headerDirectionKey = getDirectionTypeKey(headerText)
      const headerIconDataUrl = getDirectionIconDataUrl(
        headerDirectionKey,
        isIgnored ? '#94A3B8' : '#111111'
      )
      const detailLines = (linesByIndex?.[rowIndex] ?? []).filter(Boolean)
      const visibleDetailLines = isIgnored ? [] : detailLines
      const bodyTop = cardTop + headerHeight
      const textX = cardLeft + bodyPaddingX
      const iconSize = 10
      const headerTextX = textX + (headerIconDataUrl ? iconSize + 3 : 0)
      const detailPieRadius = 4.5
      const detailPieCenterX =
        cardLeft + cardWidth - bodyPaddingX - detailPieRadius
      const detailTextWidth = Math.max(
        0,
        detailPieCenterX - textX - detailPieRadius - 6
      )
      const detailMetricGap = 4
      const detailValueWidth = Math.min(26, Math.max(20, detailTextWidth * 0.4))
      const detailLabelWidth = Math.max(
        0,
        detailTextWidth - detailValueWidth - detailMetricGap
      )
      const connectorChildren: CustomSeriesRenderItemReturn[] = []
      const nextRowIndex = rowIndex + 1

      if (nextRowIndex < distanceData.length) {
        const nextCardMetrics = getCardMetrics(api, nextRowIndex)
        const upperCard =
          nextCardMetrics.cardTop < cardTop
            ? nextCardMetrics
            : { cardTop, cardBottom, isIgnored }
        const lowerCard =
          nextCardMetrics.cardTop < cardTop
            ? { cardTop, cardBottom, isIgnored }
            : nextCardMetrics
        const connectorTop =
          upperCard.cardBottom + TIME_SPACE_PHASE_CONNECTOR_END_INSET
        const connectorBottom =
          lowerCard.cardTop - TIME_SPACE_PHASE_CONNECTOR_END_INSET
        const connectorLength = connectorBottom - connectorTop

        if (connectorLength >= TIME_SPACE_PHASE_CONNECTOR_MIN_LENGTH) {
          const connectorX =
            column === 'left'
              ? cardLeft + cardWidth - TIME_SPACE_PHASE_CONNECTOR_INNER_OFFSET
              : cardLeft + TIME_SPACE_PHASE_CONNECTOR_INNER_OFFSET
          const connectorStroke =
            upperCard.isIgnored || lowerCard.isIgnored
              ? TIME_SPACE_CARD_CONNECTOR_IGNORED_STROKE
              : TIME_SPACE_CARD_CONNECTOR_STROKE
          const arrowTipY = column === 'left' ? connectorTop : connectorBottom
          const arrowBaseY =
            arrowTipY +
            (column === 'left'
              ? TIME_SPACE_PHASE_CONNECTOR_ARROW_SIZE
              : -TIME_SPACE_PHASE_CONNECTOR_ARROW_SIZE)

          connectorChildren.push(
            {
              type: 'line',
              z2: 8,
              shape: {
                x1: connectorX,
                y1: connectorTop,
                x2: connectorX,
                y2: connectorBottom,
              },
              style: {
                stroke: connectorStroke,
                lineWidth: TIME_SPACE_CARD_CONNECTOR_WIDTH,
                lineCap: 'round',
              },
            },
            {
              type: 'line',
              z2: 8,
              shape: {
                x1: connectorX,
                y1: arrowTipY,
                x2: connectorX - TIME_SPACE_PHASE_CONNECTOR_ARROW_SIZE,
                y2: arrowBaseY,
              },
              style: {
                stroke: connectorStroke,
                lineWidth: TIME_SPACE_CARD_CONNECTOR_WIDTH,
                lineCap: 'round',
              },
            },
            {
              type: 'line',
              z2: 8,
              shape: {
                x1: connectorX,
                y1: arrowTipY,
                x2: connectorX + TIME_SPACE_PHASE_CONNECTOR_ARROW_SIZE,
                y2: arrowBaseY,
              },
              style: {
                stroke: connectorStroke,
                lineWidth: TIME_SPACE_CARD_CONNECTOR_WIDTH,
                lineCap: 'round',
              },
            }
          )
        }
      }

      return {
        type: 'group',
        children: [
          ...connectorChildren,
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
              fill: '#FFFFFF',
              stroke: '#D9DEE6',
              lineWidth: 1,
            },
          },
          {
            type: 'rect',
            z2: 11,
            shape: {
              x: cardLeft,
              y: cardTop,
              width: 3,
              height: cardHeight,
              r: bodyHeight > 0 ? [cardRadius, 0, 0, cardRadius] : cardRadius,
            },
            style: {
              fill: headerAccentColor,
              opacity: isIgnored ? 0.5 : 1,
            },
          },
          {
            type: 'rect',
            z2: 12,
            shape: {
              x: cardLeft + 3,
              y: cardTop,
              width: cardWidth - 3,
              height: headerHeight,
              r:
                bodyHeight > 0
                  ? [0, cardRadius, 0, 0]
                  : [0, cardRadius, cardRadius, 0],
            },
            style: {
              fill: isIgnored ? '#F1F5F9' : '#EEF1F5',
              opacity: isIgnored ? 0.88 : 1,
            },
          },
          ...(headerIconDataUrl
            ? [
                {
                  type: 'image' as const,
                  z2: 20,
                  style: {
                    x: textX,
                    y: cardTop + (headerHeight - iconSize) / 2,
                    image: headerIconDataUrl,
                    width: iconSize,
                    height: iconSize,
                    opacity: isIgnored ? 0.4 : 1,
                  },
                },
              ]
            : [
                {
                  type: 'text' as const,
                  z2: 20,
                  style: {
                    x: textX,
                    y: cardTop + headerHeight / 2,
                    text: '?',
                    textAlign: 'left',
                    textVerticalAlign: 'middle',
                    fill: isIgnored ? '#94A3B8' : '#111',
                    fontSize: 10,
                    fontWeight: 700,
                  },
                },
              ]),
          {
            type: 'text',
            z2: 20,
            style: {
              x: headerTextX,
              y: cardTop + headerHeight / 2,
              text: headerTitleText,
              textAlign: 'left',
              textVerticalAlign: 'middle',
              fontSize: 10,
              rich: {
                ident: {
                  fill: isIgnored ? '#94A3B8' : '#111',
                  fontSize: 10,
                  fontWeight: 700,
                },
                name: {
                  fill: isIgnored ? '#94A3B8' : '#111',
                  fontSize: 10,
                  fontWeight: 400,
                },
              },
            },
          },
          ...visibleDetailLines.flatMap((line, index) => {
            const percentValue = extractPercentValue(line)
            const lineY = bodyTop + bodyPaddingY + index * lineHeight
            const isArrivalOnGreenLine = /^AOG:\s*/i.test(line)
            const arrivalOnGreenValue = isArrivalOnGreenLine
              ? line.replace(/^AOG:\s*/i, '')
              : null
            const pieChildren =
              percentValue === null
                ? []
                : [
                    {
                      type: 'circle' as const,
                      z2: 20,
                      shape: {
                        cx: detailPieCenterX,
                        cy: lineY + lineHeight / 2 - 1,
                        r: detailPieRadius,
                      },
                      style: {
                        fill: '#E2E8F0',
                        opacity: isIgnored ? 0.45 : 1,
                      },
                    },
                    ...(percentValue > 0
                      ? [
                          {
                            type: 'sector' as const,
                            z2: 21,
                            shape: {
                              cx: detailPieCenterX,
                              cy: lineY + lineHeight / 2 - 1,
                              r: detailPieRadius,
                              r0: 0,
                              startAngle: -Math.PI / 2,
                              endAngle:
                                -Math.PI / 2 +
                                (percentValue / 100) * Math.PI * 2,
                              clockwise: true,
                            },
                            style: {
                              fill: Color.Black,
                              opacity: isIgnored ? 0.55 : 1,
                            },
                          },
                        ]
                      : []),
                  ]

            return [
              ...(isArrivalOnGreenLine
                ? [
                    {
                      type: 'text' as const,
                      z2: 20,
                      style: {
                        x: textX,
                        y: lineY,
                        text: 'AOG',
                        width: detailLabelWidth,
                        overflow: 'truncate',
                        lineHeight,
                        textAlign: 'left',
                        textVerticalAlign: 'top',
                        fill: isIgnored ? '#94A3B8' : '#64748B',
                        fontSize: 10,
                        fontWeight: 500,
                      },
                    },
                    {
                      type: 'text' as const,
                      z2: 20,
                      style: {
                        x: textX + detailTextWidth,
                        y: lineY,
                        text: arrivalOnGreenValue ?? '',
                        width: detailValueWidth,
                        overflow: 'truncate',
                        lineHeight,
                        textAlign: 'right',
                        textVerticalAlign: 'top',
                        fill: isIgnored ? '#94A3B8' : '#222',
                        fontSize: 10,
                        fontWeight: 500,
                      },
                    },
                  ]
                : [
                    {
                      type: 'text' as const,
                      z2: 20,
                      style: {
                        x: textX,
                        y: lineY,
                        text: line,
                        width: detailTextWidth,
                        overflow: 'truncate',
                        lineHeight,
                        textAlign: 'left',
                        textVerticalAlign: 'top',
                        fill: isIgnored ? '#94A3B8' : '#222',
                        fontSize: 10,
                        fontWeight: 500,
                      },
                    },
                  ]),
              ...pieChildren,
            ]
          }),
        ],
      }
    },
    data: distanceData,
  } satisfies SeriesOption
}
