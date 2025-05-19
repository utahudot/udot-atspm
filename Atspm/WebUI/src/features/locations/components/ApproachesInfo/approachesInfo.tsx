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

interface ApproachesInfoProps {
  location: LocationExpanded | undefined
}

const DataGridStyle: SxProps<Theme> = {
  '@media print': {
    '& .MuiDataGrid-main': {
      zoom: '0.63',
    },
  },
} as const

function CustomToolbar({ location }: { location: LocationExpanded }) {
  const fileName =
    `${location.primaryName} & ${location.secondaryName} Approaches Configuration`.replace(
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

const approachesHeaders: GridColDef[] = [
  {
    field: 'directionType',
    headerName: 'Direction',
    editable: false,
    flex: 1,
  },
  {
    field: 'description',
    headerName: 'Description',
    editable: false,
    flex: 1,
  },
  {
    field: 'protectedPhaseNumber',
    headerName: 'Protected Phase',
    editable: false,
    flex: 1,
  },
  {
    field: 'permissivePhaseNumber',
    headerName: 'Permissive Phase',
    editable: false,
    flex: 1,
  },
  {
    field: 'pedestrianPhaseNumber',
    headerName: 'Pedestrian Phase',
    editable: false,
    flex: 1,
  },
  {
    field: 'isProtectedPhaseOverlap',
    headerName: 'Protected Phase Overlap',
    align: 'center',
    headerAlign: 'center',
    editable: false,
    renderCell: (params) => {
      return params.value ? <CheckBoxIcon /> : <CheckBoxOutlineBlankIcon />
    },
    flex: 1,
  },
  {
    field: 'isPermissivePhaseOverlap',
    headerName: 'Perm. Phase Overlap',
    align: 'center',
    headerAlign: 'center',
    editable: false,
    renderCell: (params) => {
      return params.value ? <CheckBoxIcon /> : <CheckBoxOutlineBlankIcon />
    },
    flex: 1,
  },
  {
    field: 'isPedestrianPhaseOverlap',
    headerName: 'Ped. Phase Overlap',
    align: 'center',
    headerAlign: 'center',
    editable: false,
    renderCell: (params) => {
      return params.value ? <CheckBoxIcon /> : <CheckBoxOutlineBlankIcon />
    },
    flex: 1,
  },
  {
    field: 'pedestrianDetectors',
    headerName: 'Ped. Detector(s)',
    editable: false,
    flex: 1,
  },
  {
    field: 'mph',
    headerName: 'Approach Speed (MPH)',
    editable: false,
    flex: 1,
  },
]

function ApproachesInfo({ location }: ApproachesInfoProps) {
  if (!location) {
    return (
      <div>
        <h3>No approaches found</h3>
      </div>
    )
  }

  const { approaches } = location

  const data = approaches.map((approach) => {
    return {
      ...approach,
      directionType: approach?.directionType?.description,
    }
  })
  return (
    <Box sx={{ overflowX: 'auto' }}>
      <DataGrid
        autoHeight
        rows={data}
        hideFooter
        columns={approachesHeaders}
        getRowId={(row) => row.id}
        rowSelection={false}
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
          '& .MuiDataGrid-columnHeader': {
            height: 'unset !important',
          },
          '& .MuiDataGrid-columnHeaders': {
            maxHeight: '168px !important',
          },
          borderStyle: 'none',
        }}
      />
    </Box>
  )
}

export default ApproachesInfo
