import { LoadingButton } from '@mui/lab'
import { Box } from '@mui/material'
import { jsPDF } from 'jspdf'
import Image from 'next/image'
import React, { useRef } from 'react'

//TODO: Move interfaces into its own file

interface AcceptableGapListItem {
  [key: string]: number
}

interface PercentCyclesWithPedsList {
  [key: string]: number
}

interface DemandList {
  [key: string]: number
}

interface PercentCyclesWithSplitFailList {
  [key: string]: number
}

interface LTGRData {
  startDate: string
  endDate: string
  approachDescription: string
  signalId: string
  location: string
  get24HourPeriod: boolean
  phaseType: string
  signalType: string
  speedLimit: number | null
  peakPeriodDescription: string | null
  startTime: string
  endTime: string
  cyclesWithSplitFailNum: number
  cyclesWithSplitFailPercent: number
  cyclesWithPedCallNum: number
  cyclesWithPedCallPercent: number
  crossProductValue: number
  calculatedVolumeBoundary: number
  gapDurationConsiderForStudy: boolean
  splitFailsConsiderForStudy: boolean
  pedActuationsConsiderForStudy: boolean
  volumesConsiderForStudy: boolean
  capacity: number
  demand: number
  gapOutPercent: number
  opposingLanes: number
  crossProductReview: boolean
  decisionBoundariesReview: boolean
  vcRatio: number
  leftTurnVolume: number
  opposingThroughVolume: number
  crossProductConsiderForStudy: boolean
  acceptableGapList: AcceptableGapListItem
  percentCyclesWithPedsList: PercentCyclesWithPedsList
  demandList: DemandList
  percentCyclesWithSplitFailList: PercentCyclesWithSplitFailList
  direction: string
  opposingDirection: string
}

interface ApproachData {
  leftTurnVolumeOk: boolean
  gapOutOk: boolean
  pedCycleOk: boolean
  insufficientDetectorEventCount: boolean
  insufficientCycleAggregation: boolean
  insufficientPhaseTermination: boolean
  insufficientPedAggregations: boolean
  insufficientSplitFailAggregations: boolean
  insufficientLeftTurnGapAggregations: boolean
  approachId: number
  approachDescription: string
  locationIdentifier: string
  locationDescription: string
  start: string
  end: string
}

interface LTGRReportViewProps {
  lTGRDataReport: LTGRData[]
  approaches: ApproachData[]
}

const LTGRReportView: React.FC<LTGRReportViewProps> = ({
  lTGRDataReport = [],
  approaches = [],
}) => {
  const reportRef = useRef<HTMLDivElement>(null)

  const downloadPDF = () => {
    const doc = new jsPDF('p', 'mm', 'letter')

    // Set the desired font size for the PDF
    doc.setFontSize(12)

    // Add logo and location description header on the first page
    const logoWidth = 36
    const logoHeight = 14
    const logoX = 10
    const logoY = 10
    doc.addImage(
      '/images/new-atspm-logo.png',
      'PNG',
      logoX,
      logoY,
      logoWidth,
      logoHeight
    )

    doc.setFontSize(16)
    doc.text('Left Turn Phase', logoX + logoWidth + 40, logoY + 6)
    doc.text('Analysis Report', logoX + logoWidth + 40, logoY + 14)

    doc.setDrawColor('#5a87c6')
    doc.setLineWidth(0.3)
    doc.line(10, logoY + 20, 205, logoY + 20)

    doc.setFontSize(14)
    doc.text('Signal ID:', logoX, logoY + 29)
    doc.setFont('helvetica', 'bold')
    doc.text(approaches[0].locationDescription, logoX + 24, logoY + 29)
    doc.setFont('helvetica', 'normal')
    // end of header

    let yPos = logoY + logoHeight + 30
    lTGRDataReport.forEach((reportData, index) => {
      const approachDirection = reportData.direction || 'NA'
      const approach = approaches.find(
        (item) => item.approachDescription.split(' ')[0] === approachDirection
      )

      const prevApproachDirection =
        index > 0 ? lTGRDataReport[index - 1].direction || 'NA' : null

      if (approachDirection !== prevApproachDirection) {
        // Approach header

        doc.setFillColor('#F0F0F0')
        doc.rect(10, yPos - 9, 195, 19, 'F')

        doc.setFontSize(8)
        doc.setFont('helvetica', 'normal')
        doc.text('Left Turn Approach: ', 17, yPos - 3)
        const textWidth = doc.getTextWidth('Left Turn Approach: ')
        doc.setFont('helvetica', 'bold')
        doc.text(approachDirection, 20 + textWidth, yPos - 3)
        doc.setFont('helvetica', 'normal')

        doc.setFontSize(8)
        doc.text('Opposing Approach: ', 17, yPos + 5)
        const opposingApproachTextWidth = doc.getTextWidth(
          'Opposing Approach: '
        )
        doc.setFont('helvetica', 'bold')
        doc.text(
          reportData.opposingDirection || '-',
          20 + opposingApproachTextWidth,
          yPos + 5
        )
        doc.setFont('helvetica', 'normal')

        doc.text('Phase Type: ', 79, yPos - 3)
        const phaseTypeTextWidth = doc.getTextWidth('Phase Type: ')
        doc.setFont('helvetica', 'bold')
        doc.text(reportData.phaseType ?? '-', 82 + phaseTypeTextWidth, yPos - 3)
        doc.setFont('helvetica', 'normal')

        doc.text('Number of Thru Lanes: ', 79, yPos + 5)
        const thruLanesTextWidth = doc.getTextWidth('Number of Thru Lanes: ')
        doc.setFont('helvetica', 'bold')
        doc.text(
          `${reportData.opposingLanes || '-'}`,
          82 + thruLanesTextWidth,
          yPos + 5
        )
        doc.setFont('helvetica', 'normal')

        doc.text('Signal Head Type: ', 142, yPos - 3)
        const signalHeadTypeTextWidth = doc.getTextWidth('Signal Head Type: ')
        doc.setFont('helvetica', 'bold')
        doc.text(
          reportData.signalType || '-',
          145 + signalHeadTypeTextWidth,
          yPos - 3
        )
        doc.setFont('helvetica', 'normal')

        doc.text('Speed Limit: ', 142, yPos + 5)
        const speedLimitTextWidth = doc.getTextWidth('Speed Limit: ')
        doc.setFont('helvetica', 'bold')
        doc.text(
          `${reportData.speedLimit || '-'}`,
          145 + speedLimitTextWidth,
          yPos + 5
        )
        doc.setFont('helvetica', 'normal')
        yPos += 34
      } else {
        yPos += 5
      }

      // Period header
      doc.setFillColor('#2F6BAC')
      doc.rect(10, yPos - 24.5, 195, 11.5, 'F')
      doc.setTextColor(255, 255, 255)
      doc.setFontSize(8)
      doc.text('Period: ', 13, yPos - 18)
      const periodTextWidth = doc.getTextWidth('Period: ')
      doc.setFont('helvetica', 'bold')
      doc.text(
        reportData.peakPeriodDescription || '-',
        13 + periodTextWidth,
        yPos - 18
      )
      doc.setFont('helvetica', 'normal')

      doc.text('Start: ', 50, yPos - 18)
      const startTextWidth = doc.getTextWidth('Start: ')
      doc.setFont('helvetica', 'bold')
      doc.text(reportData.startTime || '-', 50 + startTextWidth, yPos - 18)
      doc.setFont('helvetica', 'normal')

      doc.text('End: ', 85, yPos - 18)
      const endTextWidth = doc.getTextWidth('End: ')
      doc.setFont('helvetica', 'bold')
      doc.text(reportData.endTime || '-', 85 + endTextWidth, yPos - 18)
      doc.setFont('helvetica', 'normal')

      doc.text('Start Date: ', 115, yPos - 18)
      const startDateTextWidth = doc.getTextWidth('Start Date: ')
      doc.setFont('helvetica', 'bold')
      doc.text(
        approach?.start.substring(0, 10) || '-',
        115 + startDateTextWidth,
        yPos - 18
      )
      doc.setFont('helvetica', 'normal')

      doc.text('End Date: ', 160, yPos - 18)
      const endDateTextWidth = doc.getTextWidth('End Date: ')
      doc.setFont('helvetica', 'bold')
      doc.text(
        approach?.end.substring(0, 10) || '-',
        160 + endDateTextWidth,
        yPos - 18
      )
      doc.setFont('helvetica', 'normal')

      doc.setTextColor(0, 0, 0)
      yPos += 2

      // Analysis results
      doc.setFontSize(11)
      doc.setTextColor('#2F6BAC')
      doc.text('Left Turn Gap Analysis Results', 81, yPos - 8)
      doc.setTextColor(0, 0, 0)
      yPos += 2
      doc.setFontSize(8)

      doc.text('Capacity: ', 17, yPos)
      const capacityTextWidth = doc.getTextWidth('Capacity: ')
      doc.setFont('helvetica', 'bold')
      doc.text(
        Math.round(reportData.capacity).toLocaleString() || '-',
        17 + capacityTextWidth,
        yPos
      )
      doc.setFont('helvetica', 'normal')

      doc.text('Demand: ', 67, yPos)
      const demandTextWidth = doc.getTextWidth('Demand: ')
      doc.setFont('helvetica', 'bold')
      doc.text(
        Math.round(reportData.demand).toLocaleString() || '-',
        67 + demandTextWidth,
        yPos
      )
      doc.setFont('helvetica', 'normal')

      doc.text('V/C Ratio: ', 117, yPos)
      const vcRatioTextWidth = doc.getTextWidth('V/C Ratio: ')
      doc.setFont('helvetica', 'bold')
      doc.text(
        reportData.vcRatio.toFixed(2).toLocaleString() || '-',
        117 + vcRatioTextWidth,
        yPos
      )
      doc.setFont('helvetica', 'normal')

      doc.text('Phase: ', 152, yPos)
      const phaseTextWidth = doc.getTextWidth('Phase: ')
      doc.setFont('helvetica', 'bold')
      doc.text(reportData.phaseType || '-', 152 + phaseTextWidth, yPos)
      doc.setFont('helvetica', 'normal')

      yPos += 10

      if (reportData.gapDurationConsiderForStudy) {
        doc.setTextColor('#007b40')
        doc.text('Consider for study', 97, yPos - 2)
      } else {
        doc.setTextColor('#E0E0E0')
        doc.text("Don't consider for study", 97, yPos - 2)
      }

      doc.setTextColor(0, 0, 0)
      yPos += 7

      // Alternative measures
      doc.setFillColor('#F0F0F0')
      doc.rect(10, yPos - 5, 195, 65, 'F')
      doc.setFontSize(11)
      yPos += 1
      doc.setTextColor('#2F6BAC')
      doc.text('Alternative Measures Analysis Results', 75, yPos)

      doc.setTextColor(0, 0, 0)
      doc.setFontSize(10)
      yPos += 10
      doc.text('Split Failure and Pedestrians', 20, yPos)
      doc.setFontSize(8)
      yPos += 10

      doc.text('Cycles With Split Failure: ', 15, yPos)

      doc.setFont('helvetica', 'bold')
      doc.text(
        `${reportData.cyclesWithSplitFailNum || ' '} (${
          reportData.cyclesWithSplitFailPercent !== undefined
            ? parseFloat(
                reportData.cyclesWithSplitFailPercent.toFixed(2)
              ).toLocaleString()
            : ' '
        }%)`,
        65,
        yPos
      )
      doc.setFont('helvetica', 'normal')
      doc.setFontSize(8)
      doc.setTextColor(
        reportData.splitFailsConsiderForStudy ? '#007b40' : '#CC0000'
      )
      doc.text(
        reportData.splitFailsConsiderForStudy
          ? 'Consider for study'
          : "Don't consider for study",
        15,
        yPos + 4
      )
      doc.setTextColor(0, 0, 0)
      doc.setFontSize(8)
      yPos += 10

      doc.text('Cycles With Pedestrian Calls: ', 15, yPos + 10)
      const pedCallsTextWidth = doc.getTextWidth(
        'Cycles With Pedestrian Calls: '
      )
      doc.setFont('helvetica', 'bold')
      doc.text(
        `${reportData.cyclesWithPedCallNum || ' '} (${
          reportData.cyclesWithPedCallPercent !== undefined
            ? parseFloat(
                reportData.cyclesWithPedCallPercent.toFixed(2)
              ).toLocaleString()
            : ' '
        }%)`,
        65,
        yPos + 10
      )
      doc.setFont('helvetica', 'normal')
      doc.setFontSize(8)
      doc.setTextColor(
        reportData.pedActuationsConsiderForStudy ? '#007b40' : '#CC0000'
      )
      doc.text(
        reportData.pedActuationsConsiderForStudy
          ? 'Consider for study'
          : "Don't consider for study",
        15,
        yPos + 14
      )
      doc.setFontSize(11)
      doc.setTextColor(0, 0, 0)
      yPos += 10
      doc.setFontSize(10)
      doc.text('Volume Cross Product and Boundaries', 108, yPos - 30)

      doc.setFontSize(8)

      doc.text('Left Turn Movement Volume: ', 105, yPos - 20)
      const leftTurnVolumeTextWidth = doc.getTextWidth(
        'Left Turn Movement Volume: '
      )
      doc.setFont('helvetica', 'bold')
      doc.text(
        `${
          reportData.leftTurnVolume !== undefined
            ? reportData.leftTurnVolume.toLocaleString()
            : ''
        }`,
        165,
        yPos - 20
      )
      doc.setFont('helvetica', 'normal')

      doc.text('Opposing Through Movement Volume: ', 105, yPos - 10)
      doc.setFont('helvetica', 'bold')
      doc.text(
        `${
          reportData.opposingThroughVolume !== undefined
            ? reportData.opposingThroughVolume.toLocaleString()
            : ''
        }`,
        165,
        yPos - 10
      )
      doc.setFont('helvetica', 'normal')

      doc.text('Cross Product Value: ', 105, yPos)
      doc.setFont('helvetica', 'bold')
      doc.text(
        `${
          reportData.crossProductValue !== undefined
            ? reportData.crossProductValue.toLocaleString()
            : ''
        }`,
        165,
        yPos
      )
      doc.setFont('helvetica', 'normal')
      doc.setTextColor(
        reportData.crossProductConsiderForStudy ? '#007b40' : '#CC0000'
      )
      doc.setFontSize(8)
      doc.text(
        reportData.crossProductConsiderForStudy
          ? 'Consider for study'
          : "Don't consider for study",
        105,
        yPos + 4
      )
      doc.setTextColor(0, 0, 0)
      doc.setFontSize(8)
      yPos += 10

      doc.text('Calculated Volume Boundary: ', 105, yPos)
      doc.setFont('helvetica', 'bold')
      doc.text(
        `${
          reportData.calculatedVolumeBoundary !== undefined
            ? parseFloat(
                reportData.calculatedVolumeBoundary.toFixed(2)
              ).toLocaleString()
            : ''
        }`,
        165,
        yPos
      )
      doc.setFont('helvetica', 'normal')
      doc.setTextColor(
        reportData.decisionBoundariesReview ? '#007b40' : '#CC0000'
      )
      doc.setFontSize(8)
      doc.text(
        reportData.decisionBoundariesReview
          ? 'Consider for study'
          : "Don't consider for study",
        105,
        yPos + 4
      )
      doc.setTextColor(0, 0, 0)
      doc.setFontSize(11)
      yPos += 10

      if (approachDirection !== prevApproachDirection) {
        // border
        doc.setDrawColor('#2F6BAC')
        doc.setLineWidth(0.5)
        doc.rect(10, yPos - 125, 195, 124, 'S')
      } else {
        // border
        doc.setDrawColor('#2F6BAC')
        doc.setLineWidth(0.5)
        doc.rect(10, yPos - 106.5, 195, 105.5, 'S')
      }

      if (reportData.peakPeriodDescription == 'AM Peak') {
        yPos += 21
        doc.setFontSize(8)
      } else {
        if (index < lTGRDataReport.length - 1) {
          doc.addPage()
          yPos = 20
        }
      }
    })

    doc.save('LTGR_Report.pdf')
  }

  return (
    <div
      ref={reportRef}
      id="report"
      style={{ display: 'flex', flexDirection: 'column', gap: 5 }}
    >
      <div style={{ display: 'flex', flexDirection: 'row', gap: 2 }}>
        <div
          style={{
            width: '150px',
            margin: '20px',
          }}
        >
          <Image
            alt="ATSPM Logo"
            src="/images/new-atspm-logo.png"
            width={500}
            height={300}
            style={{ width: '100%', height: 'auto' }}
          />
        </div>
        <h1>Left Turn Gap Report - </h1>
        <h2 style={{ marginTop: '1.1em' }}>
          {approaches[0].locationDescription}
        </h2>
      </div>
      {lTGRDataReport.map((reportData, index) => {
        const approachDirection = reportData.direction || 'NA'
        const approach = approaches.find(
          (item) => item.approachDescription.split(' ')[0] === approachDirection
        )
        const prevApproachDirection =
          index > 0 ? lTGRDataReport[index - 1].direction || 'NA' : null

        return (
          <div key={index} style={{ border: '1px solid black' }}>
            {approachDirection !== prevApproachDirection && (
              <div
                style={{
                  display: 'flex',
                  flexDirection: 'column',
                  backgroundColor: '#F0F0F0',
                  gap: 1,
                  padding: '.5rem',
                }}
              >
                <div
                  style={{
                    display: 'flex',
                    flexDirection: 'row',
                    justifyContent: 'space-between',
                  }}
                >
                  <div style={{ flex: 1 }}>
                    <p style={{ textAlign: 'left', fontSize: '1.2rem' }}>
                      Left Turn Approach: {approachDirection}
                    </p>
                    <p style={{ textAlign: 'left' }}>
                      Opposing Approach: {reportData?.opposingDirection || '-'}
                    </p>
                  </div>
                  <div style={{ flex: 1 }}>
                    <p style={{ textAlign: 'left' }}>
                      Phase Type: {reportData?.phaseType || '-'}
                    </p>
                    <p style={{ textAlign: 'left' }}>
                      Number of Thru Lanes: {reportData?.opposingLanes || '-'}
                    </p>
                  </div>
                  <div style={{ flex: 1 }}>
                    <p style={{ textAlign: 'left' }}>
                      Signal Head Type: {reportData?.signalId || '-'}
                    </p>
                    <p style={{ textAlign: 'left' }}>
                      Speed Limit: {reportData?.speedLimit || '-'}
                    </p>
                  </div>
                </div>
              </div>
            )}

            <div>
              <div
                style={{
                  display: 'flex',
                  flexDirection: 'row',
                  justifyContent: 'space-around',
                  backgroundColor: '#2F6BAC',
                  color: 'white',
                  padding: '10px',
                }}
              >
                <p style={{ flexGrow: 1 }}>
                  period:{' '}
                  {reportData?.peakPeriodDescription ||
                    (reportData?.get24HourPeriod ? '24-Hour' : '-')}{' '}
                </p>
                <p style={{ flexGrow: 1 }}>
                  Start: {reportData?.startTime || '-'}
                </p>
                <p style={{ flexGrow: 1 }}>End: {reportData?.endTime || '-'}</p>
                <p style={{ flexGrow: 1 }}>
                  Start Date: {approach?.start.substring(0, 10) || '-'}
                </p>
                <p style={{ flexGrow: 1 }}>
                  End Date: {approach?.end.substring(0, 10) || '-'}{' '}
                </p>
              </div>
            </div>

            <div
              style={{
                display: 'flex',
                flexDirection: 'column',
                justifyContent: 'space-around',
                alignItems: 'center',
                height: '8rem',
              }}
            >
              <div>
                <p
                  style={{
                    color: '#2F6BAC',
                    fontWeight: 'bold',
                  }}
                >
                  Left Turn Gap Analysis Results
                </p>
              </div>

              <div
                style={{
                  display: 'flex',
                  justifyContent: 'space-around',
                  flexDirection: 'row',
                  width: '70%',
                }}
              >
                <div>
                  Capacity:{' '}
                  {reportData?.capacity
                    ? Math.round(reportData.capacity).toLocaleString()
                    : '-'}
                </div>
                <div>
                  Demand:{' '}
                  {reportData?.demand
                    ? Math.round(reportData.demand).toLocaleString()
                    : '-'}
                </div>
                <div>
                  V/C Ratio:{' '}
                  {reportData.vcRatio.toFixed(2).toLocaleString() || '-'}
                </div>
                <div>Phase: {reportData?.phaseType || '-'}</div>
              </div>

              <div>
                <p
                  style={{
                    color: '#007b40',
                    fontWeight: 'bold',
                  }}
                >
                  Consider for study
                </p>
              </div>
            </div>

            <div
              style={{
                display: 'flex',
                flexDirection: 'column',
                justifyContent: 'space-between',
                alignItems: 'center',
                backgroundColor: '#F0F0F0',
              }}
            >
              <div
                style={{
                  display: 'flex',
                  justifyContent: 'center',
                  width: '100%',
                }}
              >
                <p
                  style={{
                    color: '#2F6BAC',
                    fontWeight: 'bold',
                    margin: '1em',
                  }}
                >
                  Alternative Measures Analysis Results
                </p>
              </div>

              <div
                style={{
                  display: 'flex',
                  width: '70%',
                  paddingBottom: '2rem',
                }}
              >
                <div
                  style={{
                    display: 'flex',
                    flexDirection: 'column',
                    padding: '1rem',
                    alignItems: 'center',
                    width: '100%',
                  }}
                >
                  <p style={{ fontWeight: 'bold' }}>
                    Split Failure and Pedestrians
                  </p>

                  <div style={{ display: 'flex', alignItems: 'center' }}>
                    <span>
                      Cycles with Split failure:{' '}
                      {`${reportData?.cyclesWithSplitFailNum || ' '} (${
                        reportData?.cyclesWithSplitFailPercent !== undefined
                          ? parseFloat(
                              reportData.cyclesWithSplitFailPercent.toFixed(2)
                            ).toLocaleString()
                          : ' '
                      }%)`}
                    </span>
                    <span
                      style={{
                        color: reportData.splitFailsConsiderForStudy
                          ? '#007b40'
                          : '#CC0000',
                        fontSize: '0.8em',
                        marginLeft: '8px',
                      }}
                    >
                      {reportData.splitFailsConsiderForStudy
                        ? 'Consider for study'
                        : "Don't consider for study"}
                    </span>
                  </div>

                  <div style={{ display: 'flex' }}>
                    <span>
                      Cycles With Pedestrian Calls:{' '}
                      {`${reportData?.cyclesWithPedCallNum || ' '} (${
                        reportData?.cyclesWithPedCallPercent !== undefined
                          ? parseFloat(
                              reportData.cyclesWithPedCallPercent.toFixed(2)
                            ).toLocaleString()
                          : ' '
                      }%)`}
                    </span>
                    <span
                      style={{
                        color: reportData.pedActuationsConsiderForStudy
                          ? '#007b40'
                          : '#CC0000',
                        fontSize: '0.8em',
                        marginTop: '4px',
                        marginLeft: '8px',
                      }}
                    >
                      {reportData.pedActuationsConsiderForStudy
                        ? 'Consider for study'
                        : "Don't consider for study"}
                    </span>
                  </div>
                </div>
                <div
                  style={{
                    display: 'flex',
                    flexDirection: 'column',
                    padding: '1rem',
                    width: '100%',
                  }}
                >
                  <p style={{ fontWeight: 'bold' }}>
                    Volume Cross Product and Boundaries
                  </p>
                  <div style={{ display: 'flex', flexDirection: 'column' }}>
                    <div>
                      Left Turn Movement Volume:{' '}
                      {reportData?.leftTurnVolume !== undefined
                        ? reportData.leftTurnVolume.toLocaleString()
                        : ''}
                    </div>
                    <div>
                      Opposing Through Movement Volume:{' '}
                      {reportData?.opposingThroughVolume !== undefined
                        ? reportData.opposingThroughVolume.toLocaleString()
                        : ''}
                    </div>
                    <div style={{ display: 'flex' }}>
                      <span>
                        Cross Product Value:{' '}
                        {reportData?.crossProductValue !== undefined
                          ? reportData.crossProductValue.toLocaleString()
                          : ''}
                      </span>
                      <span
                        style={{
                          color: reportData?.crossProductReview
                            ? '#007b40'
                            : '#CC0000',
                          fontSize: '0.8em',
                          marginTop: '2px',
                          marginLeft: '8px',
                        }}
                      >
                        {reportData?.crossProductReview
                          ? 'Consider for Study'
                          : "Don't consider for Study"}
                      </span>
                    </div>
                  </div>
                  <div style={{ display: 'flex' }}>
                    <span>
                      Calculated Volume Boundary:{' '}
                      {reportData?.calculatedVolumeBoundary !== undefined
                        ? parseFloat(
                            reportData.calculatedVolumeBoundary.toFixed(2)
                          ).toLocaleString()
                        : ''}
                    </span>
                    <span
                      style={{
                        color: reportData?.decisionBoundariesReview
                          ? '#007b40'
                          : '#CC0000',
                        fontSize: '0.8em',
                        marginTop: '2px',
                        marginLeft: '8px',
                      }}
                    >
                      {reportData?.decisionBoundariesReview
                        ? 'Consider for Study'
                        : "Don't consider for Study"}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        )
      })}
      <Box className="ignore-print">
        <LoadingButton
          variant="contained"
          color="primary"
          style={{ padding: '10px' }}
          onClick={downloadPDF}
        >
          Download PDF
        </LoadingButton>
      </Box>
    </div>
  )
}

export default LTGRReportView
