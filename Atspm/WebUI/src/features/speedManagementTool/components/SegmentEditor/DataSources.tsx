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
  Select,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tooltip,
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

const DATA_SOURCES = [
  DataSource.ATSPM,
  DataSource.PeMS,
  DataSource.ClearGuide,
] as const

const ALL_VERSIONS = 'All'

type EntityDetails = Entity & {
  direction?: string | null
  name?: string | null
  startDate?: string | null
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

const getSourceLabel = (source: DataSource) =>
  getDataSourceName(source) ?? 'Unknown'

const pluralize = (count: number, singular: string, plural = `${singular}s`) =>
  `${count} ${count === 1 ? singular : plural}`

const formatDate = (date: string | null | undefined) =>
  date ? date.split('T')[0] : '-'

const matchesVersionFilter = (entity: EntityDetails, selectedVersion: string) =>
  selectedVersion === ALL_VERSIONS || entity.version === selectedVersion

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

  const handleVersionChange = (source: DataSource, version: string) => {
    setSelectedEntityVersions(source, version === ALL_VERSIONS ? [] : [version])
  }

  const associated = nearByEntities.filter((e) =>
    associatedEntityIds.includes(e.id)
  ) as EntityDetails[]

  const getSelectedVersion = (source: DataSource) =>
    (selectedEntityVersions[source] as string[] | undefined)?.[0] ??
    ALL_VERSIONS

  const filteredAssociated = associated.filter((entity) =>
    matchesVersionFilter(entity, getSelectedVersion(entity.sourceId))
  )

  const sourceSummaries = DATA_SOURCES.map((source) => {
    const versions = getVersionsBySource(nearByEntities, source)
    const nearbyCount = nearByEntities.filter(
      (e) => e.sourceId === source
    ).length
    const associatedCount = associated.filter(
      (e) => e.sourceId === source
    ).length
    const selectedVersion = getSelectedVersion(source)

    return {
      source,
      versions,
      nearbyCount,
      associatedCount,
      selectedVersion,
    }
  })

  const renderVersionSelect = (
    source: DataSource,
    versions: string[],
    selectedVersion: string
  ) => (
    <FormControl fullWidth size="small" disabled={versions.length === 0}>
      <InputLabel id={`${getSourceLabel(source)}-version-label`}>
        Version
      </InputLabel>
      <Select
        labelId={`${getSourceLabel(source)}-version-label`}
        value={selectedVersion}
        label="Version"
        inputProps={{
          'aria-label': `${getSourceLabel(source)} version`,
        }}
        onChange={(e) => handleVersionChange(source, e.target.value as string)}
      >
        <MenuItem value={ALL_VERSIONS}>
          {versions.length === 0 ? 'No versions' : 'All versions'}
        </MenuItem>
        {versions.map((v) => (
          <MenuItem key={v} value={v}>
            {v}
          </MenuItem>
        ))}
      </Select>
    </FormControl>
  )

  const handleRemove = (id: string) =>
    setAssociatedEntityIds(associatedEntityIds.filter((x) => x !== id))

  return (
    <Box sx={{ minHeight: '559px', pb: 1 }}>
      <Box sx={{ mb: 2.5 }}>
        <Stack direction="row" alignItems="center" spacing={2} sx={{ mb: 1.5 }}>
          <Typography variant="subtitle2" fontWeight={700}>
            Source Version Filters
          </Typography>
        </Stack>

        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns: {
              xs: '1fr',
              sm: 'repeat(3, minmax(0, 1fr))',
            },
            gap: 1.5,
          }}
        >
          {sourceSummaries.map(
            ({
              source,
              versions,
              selectedVersion,
              nearbyCount,
              associatedCount,
            }) => (
              <Box
                key={source}
                sx={{
                  border: `1px solid ${theme.palette.divider}`,
                  borderRadius: 1,
                  minWidth: 0,
                  p: 1.25,
                }}
              >
                <Stack
                  direction="row"
                  alignItems="center"
                  justifyContent="space-between"
                  spacing={1}
                  sx={{ mb: 1 }}
                >
                  <Stack
                    direction="row"
                    alignItems="center"
                    spacing={1}
                    sx={{ minWidth: 0 }}
                  >
                    <Box
                      sx={{
                        bgcolor: SOURCE_COLORS[source],
                        borderRadius: '50%',
                        height: 10,
                        width: 10,
                        flex: '0 0 auto',
                      }}
                    />
                    <Typography variant="body2" fontWeight={700} noWrap>
                      {getSourceLabel(source)}
                    </Typography>
                  </Stack>
                </Stack>
                <Typography
                  variant="caption"
                  color="text.secondary"
                  sx={{
                    display: 'block',
                    mb: versions.length > 1 ? 1.5 : 0,
                  }}
                >
                  {`${associatedCount} of ${nearbyCount} associated`}
                </Typography>
                {versions.length > 1 &&
                  renderVersionSelect(source, versions, selectedVersion)}
              </Box>
            )
          )}
        </Box>
      </Box>

      <Divider sx={{ mb: 1.5 }} />
      <Stack
        direction="row"
        alignItems="center"
        justifyContent="space-between"
        spacing={2}
        sx={{ mb: 1 }}
      >
        <Typography variant="subtitle2" fontWeight={700}>
          Associated Entities
        </Typography>
        <Typography variant="caption" color="text.secondary">
          {`${filteredAssociated.length} of ${pluralize(
            associated.length,
            'associated entity',
            'associated entities'
          )} shown`}
        </Typography>
      </Stack>
      {isLoadingEntities ? (
        [0, 1, 2, 3, 4].map((i) => (
          <Skeleton
            key={i}
            variant="rectangular"
            height={40}
            sx={{ mb: 1, borderRadius: 1 }}
          />
        ))
      ) : associated.length === 0 ? (
        <Box
          sx={{
            alignItems: 'center',
            border: `1px solid ${theme.palette.divider}`,
            borderRadius: 1,
            color: 'text.secondary',
            display: 'flex',
            justifyContent: 'center',
            minHeight: 160,
          }}
        >
          <Typography variant="body2">No entities selected.</Typography>
        </Box>
      ) : filteredAssociated.length === 0 ? (
        <Box
          sx={{
            alignItems: 'center',
            border: `1px solid ${theme.palette.divider}`,
            borderRadius: 1,
            color: 'text.secondary',
            display: 'flex',
            justifyContent: 'center',
            minHeight: 160,
          }}
        >
          <Typography variant="body2">
            No associated entities match the selected filters.
          </Typography>
        </Box>
      ) : (
        <TableContainer
          sx={{
            border: `1px solid ${theme.palette.divider}`,
            borderRadius: 1,
            maxHeight: 410,
          }}
        >
          <Table
            size="small"
            stickyHeader
            sx={{
              '& thead th': {
                bgcolor: theme.palette.grey[50],
                fontWeight: 700,
              },
            }}
          >
            <TableHead>
              <TableRow>
                <TableCell>Source</TableCell>
                <TableCell>Entity</TableCell>
                <TableCell>Type</TableCell>
                <TableCell>Direction</TableCell>
                <TableCell>Start Date</TableCell>
                <TableCell />
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredAssociated.map((e) => (
                <TableRow
                  key={e.id}
                  sx={{
                    borderLeft: `4px solid ${SOURCE_COLORS[e.sourceId]}`,
                    '&:last-child td': { borderBottom: 0 },
                  }}
                >
                    <TableCell>
                      <Stack direction="row" spacing={1} alignItems="center">
                      <Box
                        sx={{
                          bgcolor: SOURCE_COLORS[e.sourceId],
                          borderRadius: '50%',
                          height: 8,
                            width: 8,
                            flex: '0 0 auto',
                          }}
                        />
                      <Box sx={{ minWidth: 0 }}>
                        <Typography variant="body2" fontWeight={600} noWrap>
                          {getSourceLabel(e.sourceId)}
                        </Typography>
                          <Typography
                            variant="caption"
                            color="text.secondary"
                            noWrap
                          >
                            {e.version || 'No version'}
                          </Typography>
                        </Box>
                      </Stack>
                    </TableCell>
                    <TableCell sx={{ maxWidth: 180 }}>
                      <Typography variant="body2" fontWeight={600} noWrap>
                        {e.entityId}
                      </Typography>
                      {e.name && (
                        <Typography
                          variant="caption"
                          color="text.secondary"
                          noWrap
                        >
                          {e.name}
                        </Typography>
                      )}
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">{e.entityType}</Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">
                        {e.direction || '-'}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">
                        {formatDate(e.startDate)}
                      </Typography>
                    </TableCell>
                    <TableCell align="right">
                      <Tooltip title="Remove association">
                        <IconButton
                          aria-label={`Remove ${e.entityId}`}
                          size="small"
                          onClick={() => handleRemove(e.id)}
                        >
                          <CloseIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
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
