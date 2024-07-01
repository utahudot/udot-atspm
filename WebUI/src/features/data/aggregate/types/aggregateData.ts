import { StandardChart } from '@/features/charts/types'

export interface AggregateData {
  identifier: string
  series: AggregateSeries[]
}

export interface AggregateSeries {
  identifier: string
  dataPoints: AggregateDataPointTypes
}

export type AggregateDataPointTypes = AggregateDataForTimeStamp[]

export interface AggregateDataForTimeStamp {
  identifier: string
  start: string
  value: number
}

export interface TransformedAggregateData {
  data: {
    charts: StandardChart[]
  }
}
