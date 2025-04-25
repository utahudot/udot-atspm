import DetectionTypesCell from '@/features/locations/components/editDetector/DetectionTypesCell'
import { hardwareTypeOptions } from '@/features/locations/components/editDetector/HardwareTypeCell'
import { laneTypeOptions } from '@/features/locations/components/editDetector/LaneTypeCell'
import { movementTypeOptions } from '@/features/locations/components/editDetector/MovementTypeCell'
import { LocationExpanded } from '@/features/locations/types'
import CheckBoxIcon from '@mui/icons-material/CheckBox'
import CheckBoxOutlineBlankIcon from '@mui/icons-material/CheckBoxOutlineBlank'
import { Box } from '@mui/material'
import { SxProps, Theme } from '@mui/system'
import {
  DataGrid,
  GridColDef,
  GridToolbarColumnsButton,
  GridToolbarContainer,
  GridToolbarExport,
  GridToolbarFilterButton,
} from '@mui/x-data-grid'

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

const defaults = {
  editable: false,
  flex: 1,
  minWidth: 100,
}

const detectorsHeaders: GridColDef[] = [
  {
    ...defaults,
    field: 'dectectorIdentifier',
    headerName: 'Detector Identifier',
  },
  {
    ...defaults,
    field: 'detectorChannel',
    headerName: 'Det. Channel',
  },
  {
    ...defaults,
    field: 'direction',
    headerName: 'Direction',
  },
  {
    ...defaults,
    field: 'phase',
    headerName: 'Phase',
    minWidth: 70,
  },
  {
    ...defaults,
    field: 'permPhase',
    headerName: 'Perm. Phase',
    minWidth: 70,
  },
  {
    ...defaults,
    field: 'overlap',
    headerName: 'Overlap',
    renderCell: (params) => {
      return params.value ? <CheckBoxIcon /> : <CheckBoxOutlineBlankIcon />
    },
    minWidth: 70,
  },
  {
    ...defaults,
    field: 'detectionTypes',
    headerName: 'Detection Types',
    minWidth: 200,
    renderCell: (params) => {
      if (!params.value) return null
      return <DetectionTypesCell detector={params.row} readonly />
    },
  },
  {
    ...defaults,
    field: 'detectionHardware',
    headerName: 'Detection Hardware',
    minWidth: 170,
    renderCell: (params) => {
      const hardware = hardwareTypeOptions.find(
        (option) => option.id === params.value
      )?.description
      return (
        <Box display={'grid'} alignItems={'center'}>
          {hardware}
        </Box>
      )
    },
  },
  {
    ...defaults,
    field: 'latencyCorrection',
    headerName: 'Latency Correction',
    minWidth: 70,
  },
  {
    ...defaults,
    field: 'movementType',
    headerName: 'Movement Type',
    minWidth: 150,
    renderCell: (params) => {
      const movementType = movementTypeOptions.find(
        (option) => option.id === params.value
      )
      return (
        <Box display={'flex'} alignItems={'center'}>
          <Box>{movementType?.icon}</Box>
          <Box>{movementType?.description}</Box>
        </Box>
      )
    },
  },
  {
    ...defaults,
    field: 'laneNumber',
    headerName: 'Lane Number',
    minWidth: 70,
  },
  {
    ...defaults,
    field: 'laneType',
    headerName: 'Lane Type',
    minWidth: 150,
    renderCell: (params) => {
      const laneType = laneTypeOptions.find(
        (option) => option.description === params.value
      )
      return (
        <Box display={'flex'} alignItems={'center'}>
          <Box>{laneType?.icon}</Box>
          <Box ml={0.5}>{laneType?.description}</Box>
        </Box>
      )
    },
  },
  {
    ...defaults,
    field: 'distanceFromStopBar',
    headerName: 'Dist. From Stop Bar',
    minWidth: 70,
  },
  {
    ...defaults,
    field: 'decisionPoint',
    headerName: 'Decision Point',
    minWidth: 70,
  },
  {
    ...defaults,
    field: 'movementDelay',
    headerName: 'Move. Delay',
    minWidth: 70,
  },
  {
    ...defaults,
    field: 'minSpeedFilter',
    headerName: 'Min Speed Filter',
    minWidth: 70,
  },
  {
    ...defaults,
    field: 'comment',
    headerName: 'Comment',
    disableExport: true,
    renderCell: (params) => {
      const comments = params.value?.split(',') as string[]
      return (
        <Box display={'grid'} alignItems={'center'}>
          {comments?.map((comment) => (
            <Box key={comment} mr={1}>
              {comment}
            </Box>
          ))}
        </Box>
      )
    },
  },
]

interface DetectorsInfoProps {
  location: LocationExpanded | undefined
}

function DetectorsInfo({ location }: DetectorsInfoProps) {
  const detectors = location?.approaches?.flatMap(
    (approach) => approach.detectors || []
  )

  if (!location || !location?.approaches || !detectors?.length) {
    return (
      <div>
        <h3>No detectors found</h3>
      </div>
    )
  }

  const data = detectors.map((detector) => {
    return {
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
    }
  })

  return (
    <Box sx={{ overflowX: 'auto' }}>
      <DataGrid
        autoHeight
        rows={data}
        columns={detectorsHeaders}
        getRowId={(row) => row.id}
        rowSelection={false}
        hideFooter={true}
        slots={{
          toolbar: CustomToolbar,
        }}
        slotProps={{
          toolbar: { location },
        }}
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
