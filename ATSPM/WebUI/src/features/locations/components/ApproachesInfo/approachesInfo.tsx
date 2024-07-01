import { Approach } from '@/features/locations/types'
import CheckBoxIcon from '@mui/icons-material/CheckBox'
import CheckBoxOutlineBlankIcon from '@mui/icons-material/CheckBoxOutlineBlank'
import { Box } from '@mui/material'
import { DataGrid, GridColDef, GridToolbar } from '@mui/x-data-grid'

interface ApproachesInfoProps {
  approaches: Approach[] | undefined
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

function ApproachesInfo({ approaches }: ApproachesInfoProps) {
  if (!approaches) {
    return (
      <div>
        <h3>No approaches found</h3>
      </div>
    )
  }
  const data = approaches.map((approach) => {
    return {
      ...approach,
      directionType: approach.directionType.description,
    }
  })

  return (
    <Box sx={{ overflowX: 'auto' }}>
      <DataGrid
        autoHeight
        rows={data}
        columns={approachesHeaders}
        getRowId={(row) => row.id}
        slots={{ toolbar: GridToolbar }}
        pageSizeOptions={[{ value: 100, label: '100' }]}
        sx={{
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
