import {
    createDataZoom,
    createGrid,
    createLegend,
    createTitle,
    createTooltip,
    createYAxis,
  } from '@/features/charts/common/transformers'
  import { ExtendedEChartsOption } from '@/features/charts/types'
  import {
    Color,
    DashedLineSeriesSymbol,
    SolidLineSeriesSymbol,
    formatChartDateTimeRange,
  } from '@/features/charts/utils'
  import { SpeedComplianceResponse } from '@/features/speedManagementTool/api/getSpeedOverDistanceChart'
  import { round } from '@/utils/math'
  
  export default function transformSpeedComplianceData(
    response: SpeedComplianceResponse[],
    customSpeedLimit?: number | null 
  ) {
    const sortedResponse = response
      .map((segment) => {
        if (segment.startingMilePoint > segment.endingMilePoint) {
          return {
            ...segment,
            startingMilePoint: segment.endingMilePoint,
            endingMilePoint: segment.startingMilePoint,
          }
        }
        return segment
      })
      .sort((a, b) => a.startingMilePoint - b.startingMilePoint)
  
    const dateRange = formatChartDateTimeRange(
      response[0].startDate,
      response[0].endDate
    )
  
    const title = createTitle({
      title: 'Speed Compliance',
      dateRange: dateRange,
    })
  
    const xAxis = {
      type: 'value',
      name: 'Mile Points',
      min: round(
        Math.min(...sortedResponse.map((segment) => segment.startingMilePoint)),
        2
      ),
      max: round(
        Math.max(...sortedResponse.map((segment) => segment.endingMilePoint)),
        2
      ),
    }
  
    const yAxis = createYAxis(true, { name: 'Speed (mph)' })
  
    const grid = createGrid({
      top: 100,
      left: 60,
      right: 210,
    })
  
    const legend = createLegend({
      data: [
        { name: 'Average Speed', icon: SolidLineSeriesSymbol },
        { name: '85th Percentile Speed', icon: SolidLineSeriesSymbol },
        { name: 'Speed Limit', icon: DashedLineSeriesSymbol },
      ],
    })
  
    const dataZoom = createDataZoom()
  
    const tooltip = createTooltip()
  
    // Destructure the series data from mergeSeriesData
    const { averageSpeedData, eightyFifthPercentileData, speedLimitData, tableData } =
      mergeSeriesData(sortedResponse, customSpeedLimit)
  
    const series = [
      {
        name: 'Average Speed',
        data: averageSpeedData,
        type: 'line',
        step: 'start',
        showSymbol: false,
        color: Color.Blue,
      },
      {
        name: '85th Percentile Speed',
        data: eightyFifthPercentileData,
        type: 'line',
        step: 'start',
        showSymbol: false,
        color: Color.Red,
      },
      {
        name: 'Speed Limit',
        data: speedLimitData,
        type: 'line',
        step: 'start',
        showSymbol: false,
        color: Color.Black,
        lineStyle: {
          type: 'dashed',
        },
      },
    ]
  
    const chartOptions: ExtendedEChartsOption = {
      title,
      xAxis,
      yAxis,
      grid,
      legend,
      tooltip,
      series,
      dataZoom,
      tableData
    }
    
    return chartOptions
  }
  
  function mergeSeriesData(response: SpeedComplianceResponse[], customSpeedLimit?: number | null) {
    const averageSpeedData: [number, number | null][] = []
    const eightyFifthPercentileData: [number, number | null][] = []
    const speedLimitData: [number, number | null][] = []

    const tableData = {
      milePoints: [] as ([number, number] | null)[],
      speedLimit: [] as (number | null)[],        
      avgSpeed: [] as (number | null)[],           
      eightyFifth: [] as (number | null)[],         
    }
    
  
    response.forEach((segment, index) => {
      const {
        startingMilePoint,
        endingMilePoint,
        average,
        eightyFifth,
        speedLimit,
        avgVsBaseSpeed
      } = segment
      const finalSpeedLimit = customSpeedLimit ?? speedLimit

      tableData.milePoints.push(
        startingMilePoint !== undefined && endingMilePoint !== undefined 
          ? [startingMilePoint, endingMilePoint] 
          : null
      );
    
      tableData.speedLimit.push(finalSpeedLimit !== undefined ? finalSpeedLimit : null);
      tableData.avgSpeed.push(average !== undefined ? average : null);
      tableData.eightyFifth.push(eightyFifth !== undefined ? eightyFifth : null);


      const previousSegment = response[index - 1]

      // Ensure the startingMilePoint of the first segment is added
      if (index === 0) {
        if (average !== undefined) {
          averageSpeedData.push([startingMilePoint, average])
        }
        if (eightyFifth !== undefined) {
          eightyFifthPercentileData.push([startingMilePoint, eightyFifth])
        }
        if (finalSpeedLimit !== undefined) {
          speedLimitData.push([startingMilePoint, finalSpeedLimit])
        }
      }
  
      // Handle the break between non-continuous segments by adding [null, null]
      if (index > 0 && previousSegment.endingMilePoint !== startingMilePoint) {
        // Add a break for non-continuous segments
        averageSpeedData.push([null, null])
        eightyFifthPercentileData.push([null, null])
        speedLimitData.push([null, null])
  
        // Add the startingMilePoint of the current segment
        if (average !== undefined) {
          averageSpeedData.push([startingMilePoint, average])
        }
        if (eightyFifth !== undefined) {
          eightyFifthPercentileData.push([startingMilePoint, eightyFifth])
        }
        if (finalSpeedLimit !== undefined) {
          speedLimitData.push([startingMilePoint, finalSpeedLimit])
        }
      }
  
      // Always add the endingMilePoint for each segment
      if (average !== undefined) {
        averageSpeedData.push([endingMilePoint, average])
      }
      if (eightyFifth !== undefined) {
        eightyFifthPercentileData.push([endingMilePoint, eightyFifth])
      }
      if (finalSpeedLimit !== undefined) {
        speedLimitData.push([endingMilePoint, finalSpeedLimit])
      }
      
    })
  
    return {
      averageSpeedData,
      eightyFifthPercentileData,
      speedLimitData,
      tableData
    }
  }
  