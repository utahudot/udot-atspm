import { useGetDetectionType, useGetLocationType } from '@/api/config'
import DetectionTypesCell from '@/features/locations/components/editDetector/DetectionTypesCell'
import { hardwareTypeOptions } from '@/features/locations/components/editDetector/HardwareTypeCell'
import { laneTypeOptions } from '@/features/locations/components/editDetector/LaneTypeCell'
import { movementTypeOptions } from '@/features/locations/components/editDetector/MovementTypeCell'
import { LocationExpanded } from '@/features/locations/types'
import CheckBoxIcon from '@mui/icons-material/CheckBox'
import CheckBoxOutlineBlankIcon from '@mui/icons-material/CheckBoxOutlineBlank'
import { Box, SxProps, Theme } from '@mui/material'
import {
  DataGrid,
  GridColDef,
  GridToolbarColumnsButton,
  GridToolbarContainer,
  GridToolbarExport,
  GridToolbarFilterButton,
} from '@mui/x-data-grid'
import React from 'react'

const DataGridStyle: SxProps<Theme> = {
  '@media print': {
    '& .MuiDataGrid-main': {
      zoom: '.58',
    },
  },
} as const

interface CustomToolbarProps {
  location: LocationExpanded
}

function CustomToolbar({ location }: CustomToolbarProps) {
  const fileName =
    `${location.primaryName} & ${location.secondaryName} Detectors Configuration`.replace(
      / /g,
      '_'
    )
  return (
    <GridToolbarContainer>
      <GridToolbarColumnsButton />
      <GridToolbarFilterButton />
      <GridToolbarExport
        csvOptions={{ fileName }}
        printOptions={{
          hideFooter: true,
          hideToolbar: true,
        }}
      />
    </GridToolbarContainer>
  )
}

interface DetectorsInfoProps {
  location: LocationExpanded | undefined
}

function DetectorsInfo({ location }: DetectorsInfoProps) {
  const detectionRes = useGetDetectionType()
  const locationTypeRes = useGetLocationType()

  const locationType = locationTypeRes.data?.value.find(
    (lt) => lt.id === location?.locationTypeId
  )

  const availableDetectionTypes = React.useMemo(() => {
    if (!locationType || !detectionRes.data?.value) return []
    const all = detectionRes.data.value as any[]
    if (locationType.name === 'Intersection') {
      return all.filter((d) =>
        ['AC', 'AS', 'LLC', 'LLS', 'SBP', 'AP'].includes(d.abbreviation)
      )
    }
    if (locationType.name === 'Ramp') {
      return all.filter((d) => ['P', 'D', 'IQ', 'EQ'].includes(d.abbreviation))
    }
    return []
  }, [locationType, detectionRes.data])

  const detectors = location?.approaches?.flatMap(
    (approach) => approach.detectors || []
  )

  if (!location || !location.approaches?.length || !detectors?.length) {
    return (
      <div>
        <h3>No detectors found</h3>
      </div>
    )
  }

  const data = detectors
    .map((detector) => ({
      id: detector.id,
      dectectorIdentifier: detector.dectectorIdentifier,
      detectorChannel: detector.detectorChannel,
      direction: detector.approach?.directionType?.description,
      phase: detector.approach?.protectedPhaseNumber,
      permPhase: detector.approach?.permissivePhaseNumber,
      overlap: detector.approach?.isProtectedPhaseOverlap,
      detectionTypes: detector.detectionTypes,
      detectionHardware: detector.detectionHardware,
      latencyCorrection: detector.latencyCorrection,
      movementType: detector.movementType,
      laneNumber: detector.laneNumber,
      laneType: laneTypeOptions.find(
        (o) => o.abbreviation === detector.laneType
      )?.description,
      distanceFromStopBar: detector.distanceFromStopBar,
      decisionPoint: detector.decisionPoint,
      movementDelay: detector.movementDelay,
      minSpeedFilter: detector.minSpeedFilter,
      comment: detector.detectorComments
        ?.map((comment) => comment.comment)
        .join(', '),
    }))
    .sort((a, b) => {
      if (a.detectorChannel < b.detectorChannel) return -1
      if (a.detectorChannel > b.detectorChannel) return 1
    })

  const columns: GridColDef[] = [
    {
      field: 'dectectorIdentifier',
      ...{
        headerName: 'Detector Identifier',
        editable: false,
        flex: 1,
        minWidth: 100,
      },
    },
    {
      field: 'detectorChannel',
      headerName: 'Det. Channel',
      editable: false,
      flex: 1,
      minWidth: 100,
    },
    {
      field: 'direction',
      headerName: 'Direction',
      editable: false,
      flex: 1,
      minWidth: 100,
    },
    {
      field: 'phase',
      headerName: 'Phase',
      editable: false,
      flex: 1,
      minWidth: 70,
    },
    {
      field: 'permPhase',
      headerName: 'Perm. Phase',
      editable: false,
      flex: 1,
      minWidth: 70,
    },
    {
      field: 'overlap',
      headerName: 'Overlap',
      editable: false,
      flex: 1,
      minWidth: 70,
      renderCell: (params) =>
        params.value ? <CheckBoxIcon /> : <CheckBoxOutlineBlankIcon />,
    },
    {
      field: 'detectionTypes',
      headerName: 'Detection Types',
      editable: false,
      flex: 1,
      minWidth: 200,
      renderCell: (params) => (
        <DetectionTypesCell
          detector={params.row as any}
          detectionTypes={availableDetectionTypes as any}
          readonly
        />
      ),
    },
    {
      field: 'detectionHardware',
      headerName: 'Detection Hardware',
      editable: false,
      flex: 1,
      minWidth: 170,
      renderCell: (params) => (
        <Box display="grid" alignItems="center">
          {
            hardwareTypeOptions.find((opt) => opt.id === params.value)
              ?.description
          }
        </Box>
      ),
    },
    {
      field: 'latencyCorrection',
      headerName: 'Latency Correction',
      editable: false,
      flex: 1,
      minWidth: 70,
    },
    {
      field: 'movementType',
      headerName: 'Movement Type',
      editable: false,
      flex: 1,
      minWidth: 150,
      renderCell: (params) => {
        const mt = movementTypeOptions.find((opt) => opt.id === params.value)
        return (
          <Box display="flex" alignItems="center">
            <Box>{mt?.icon}</Box>
            <Box>{mt?.description}</Box>
          </Box>
        )
      },
    },
    {
      field: 'laneNumber',
      headerName: 'Lane Number',
      editable: false,
      flex: 1,
      minWidth: 70,
    },
    {
      field: 'laneType',
      headerName: 'Lane Type',
      editable: false,
      flex: 1,
      minWidth: 150,
      renderCell: (params) => {
        const lt = laneTypeOptions.find(
          (opt) => opt.description === params.value
        )
        return (
          <Box display="flex" alignItems="center">
            <Box>{lt?.icon}</Box>
            <Box ml={0.5}>{lt?.description}</Box>
          </Box>
        )
      },
    },
    {
      field: 'distanceFromStopBar',
      headerName: 'Dist. From Stop Bar',
      editable: false,
      flex: 1,
      minWidth: 70,
    },
    {
      field: 'decisionPoint',
      headerName: 'Decision Point',
      editable: false,
      flex: 1,
      minWidth: 70,
    },
    {
      field: 'movementDelay',
      headerName: 'Move. Delay',
      editable: false,
      flex: 1,
      minWidth: 70,
    },
    {
      field: 'minSpeedFilter',
      headerName: 'Min Speed Filter',
      editable: false,
      flex: 1,
      minWidth: 70,
    },
    {
      field: 'comment',
      headerName: 'Comment',
      editable: false,
      flex: 1,
      minWidth: 100,
      disableExport: true,
      renderCell: (params) => (
        <Box display="grid" alignItems="center">
          {(params.value as string)?.split(',').map((c) => (
            <Box key={c} mr={1}>
              {c}
            </Box>
          ))}
        </Box>
      ),
    },
  ]

  return (
    <Box sx={{ overflowX: 'auto' }}>
      <DataGrid
        autoHeight
        rows={data}
        columns={columns}
        getRowId={(row) => row.id}
        rowSelection={false}
        hideFooter
        slots={{ toolbar: CustomToolbar }}
        slotProps={{ toolbar: { location } }}
        pageSizeOptions={[{ value: 100, label: '100' }]}
        sx={{
          ...DataGridStyle,
          '& .MuiDataGrid-columnHeaderTitle': {
            whiteSpace: 'normal',
            lineHeight: 'normal',
            fontSize: '0.75rem',
          },
          borderStyle: 'none',
        }}
      />
    </Box>
  )
}

export default DetectorsInfo
