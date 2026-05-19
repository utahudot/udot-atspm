// src/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/SegmentDataSources.tsx
import {
  Entity,
  useSegmentEditorStore,
} from '@/features/speedManagementTool/components/SegmentEditor/segmentEditorStore'
import CloseIcon from '@mui/icons-material/Close'
import {
  Box,
  Divider,
  FormControl,
  IconButton,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
  useTheme,
} from '@mui/material'
import Skeleton from '@mui/material/Skeleton'
import { DataSource, getDataSourceName } from '../../enums'

const SOURCE_COLORS: Record<DataSource, string> = {
  [DataSource.ATSPM]: '#0e70c6',
  [DataSource.PeMS]: '#ec3131',
  [DataSource.ClearGuide]: '#bde865',
}

const getVersionsBySource = (
  entities: Entity[],
  source: DataSource
): string[] =>
  Array.from(
    new Set(
      entities
        .filter((entity) => entity.sourceId === source)
        .map((entity) => entity.version)
        .filter(Boolean)
    )
  ).sort()

export default function SegmentDataSources() {
  const theme = useTheme()
  const {
    nearByEntities,
    selectedEntityVersions,
    setSelectedEntityVersions,
    associatedEntityIds,
    setAssociatedEntityIds,
    isLoadingEntities,
  } = useSegmentEditorStore()

  const atspmVersions = getVersionsBySource(nearByEntities, DataSource.ATSPM)
  const pemsVersions = getVersionsBySource(nearByEntities, DataSource.PeMS)
  const clearGuideVersions = getVersionsBySource(
    nearByEntities,
    DataSource.ClearGuide
  )

  const handleVersionChange = (source: DataSource, version: string) => {
    setSelectedEntityVersions(source, version === 'None' ? [] : [version])
  }

  const renderSelect = (source: DataSource, versions: string[]) => (
    <FormControl sx={{ flex: 1, mx: 1 }} size="small">
      <InputLabel>{getDataSourceName(source)}</InputLabel>
      <Select
        value={(selectedEntityVersions[source] as string[])[0] ?? 'None'}
        label={getDataSourceName(source)}
        onChange={(e) => handleVersionChange(source, e.target.value as string)}
      >
        <MenuItem value="None">None</MenuItem>
        {versions.map((v) => (
          <MenuItem key={v} value={v}>
            {v}
          </MenuItem>
        ))}
      </Select>
    </FormControl>
  )

  const selected = nearByEntities.filter((e) =>
    associatedEntityIds.includes(e.id)
  )
  const showName = selected.some((e) => (e as any).name)
  const handleRemove = (id: string) =>
    setAssociatedEntityIds(associatedEntityIds.filter((x) => x !== id))

  return (
    <Box sx={{ minHeight: '559px' }}>
      <Box sx={{ mb: 2 }}>
        <Typography
          fontSize="12px"
          fontWeight="light"
          color={theme.palette.grey[600]}
          sx={{ mb: 2 }}
        >
          Source Versions
        </Typography>
        <Box display="flex">
          {renderSelect(DataSource.ATSPM, atspmVersions)}
          {renderSelect(DataSource.PeMS, pemsVersions)}
          {renderSelect(DataSource.ClearGuide, clearGuideVersions)}
        </Box>
      </Box>
      <Divider sx={{ mb: 2 }}>
        <Typography variant="caption">Associated Entities</Typography>
      </Divider>
      {isLoadingEntities ? (
        [0, 1, 2, 3, 4].map((i) => (
          <Skeleton
            key={i}
            variant="rectangular"
            height={40}
            sx={{ mb: 1, borderRadius: 1 }}
          />
        ))
      ) : selected.length === 0 ? (
        <Typography>No entities selected.</Typography>
      ) : (
        <TableContainer component={Paper} sx={{ maxHeight: 410 }}>
          <Table size="small" stickyHeader>
            <TableHead>
              <TableRow>
                <TableCell>Source</TableCell>
                <TableCell>ID</TableCell>
                {showName && <TableCell>Name</TableCell>}
                <TableCell>Type</TableCell>
                <TableCell>Dir</TableCell>
                <TableCell>Date</TableCell>
                <TableCell />
              </TableRow>
            </TableHead>
            <TableBody>
              {selected.map((e) => (
                <TableRow
                  key={e.id}
                  sx={{
                    borderLeft: `4px solid ${SOURCE_COLORS[e.sourceId]}`,
                    '&:last-child td': { borderBottom: 0 },
                  }}
                >
                  <TableCell>
                    <Typography variant="body2">
                      {getDataSourceName(e.sourceId)}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">{e.entityId}</Typography>
                  </TableCell>
                  {showName && (
                    <TableCell>
                      <Typography variant="body2">
                        {(e as any).name ?? ''}
                      </Typography>
                    </TableCell>
                  )}
                  <TableCell>
                    <Typography variant="body2">{e.entityType}</Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">{e.direction}</Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">
                      {e.startDate.split('T')[0]}
                    </Typography>
                  </TableCell>
                  <TableCell align="right">
                    <IconButton size="small" onClick={() => handleRemove(e.id)}>
                      <CloseIcon fontSize="small" />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Box>
  )
}
