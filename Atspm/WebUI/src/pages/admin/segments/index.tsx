import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { AddButton } from '@/components/addButton'
import { PageNames, useViewPage } from '@/features/identity/pagesCheck'
import {
  type AllSegmentsSegment,
  useTransformedSegments,
} from '@/features/speedManagementTool/api/getSegments'
import { useSegmentEditorStore } from '@/features/speedManagementTool/components/SegmentEditor/segmentEditorStore'
import SegmentSelectMapWrapper from '@/features/speedManagementTool/components/SegmentSelectMap'
import { Autocomplete, Box, Paper, TextField } from '@mui/material'
import { useRouter } from 'next/router'
import {
  memo,
  type SyntheticEvent,
  useCallback,
  useEffect,
  useMemo,
  useState,
} from 'react'

type SegmentLabelProperties = {
  name?: string | null
  Name?: string | null
}

const getSegmentLabel = (segment: AllSegmentsSegment) => {
  const properties = segment.properties as SegmentLabelProperties | undefined
  const name = properties?.name ?? properties?.Name

  return name || segment.id
}

const SegmentsAdmin = () => {
  const pageAccess = useViewPage(PageNames.Segments)
  const router = useRouter()
  const [focusedSegmentId, setFocusedSegmentId] = useState<string | null>(null)
  const { data: fetchedSegments, isLoading: isSegmentsLoading } =
    useTransformedSegments()
  const {
    setAllSegments,
    setAssociatedEntityIds,
    setIsLoadingSegments,
    reset,
  } = useSegmentEditorStore()

  // Update isLoadingSegments based on the fetch status
  useEffect(() => {
    setIsLoadingSegments(isSegmentsLoading)
  }, [isSegmentsLoading, setIsLoadingSegments])

  useEffect(() => {
    if (fetchedSegments) {
      setAllSegments(fetchedSegments)
    }
  }, [fetchedSegments, setAllSegments])

  const handleSegmentSelect = useCallback(
    (segment: AllSegmentsSegment | null) => {
      reset() // Clear other state, preserving allSegments
      if (segment == null) {
        router.push(`/admin/segments/new`)
      } else {
        setAssociatedEntityIds([segment.id])
        router.push({
          pathname: `/admin/segments/${segment.id}`,
        })
      }
    },
    [router, setAssociatedEntityIds, reset]
  )

  const segmentOptions = useMemo(
    () =>
      [...(fetchedSegments ?? [])].sort((a, b) =>
        getSegmentLabel(a).localeCompare(getSegmentLabel(b))
      ),
    [fetchedSegments]
  )

  const focusedSegment = useMemo(
    () =>
      segmentOptions.find((segment) => segment.id === focusedSegmentId) ?? null,
    [focusedSegmentId, segmentOptions]
  )

  const handleSegmentFocus = useCallback(
    (_: SyntheticEvent, segment: AllSegmentsSegment | null) => {
      setFocusedSegmentId(segment?.id ?? null)
    },
    []
  )

  if (pageAccess.isLoading) {
    return null // or a loading indicator
  }

  return (
    <ResponsivePageLayout title={'Manage Segments'} noBottomMargin>
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          height: '83vh',
        }}
      >
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'space-between',
            gap: 2,
            flexWrap: 'wrap',
            mb: 2,
            mt: 2,
          }}
        >
          <Autocomplete
            options={segmentOptions}
            value={focusedSegment}
            getOptionLabel={getSegmentLabel}
            isOptionEqualToValue={(option, value) => option.id === value.id}
            onChange={handleSegmentFocus}
            size="small"
            renderInput={(params) => (
              <TextField {...params} placeholder="Select segment" />
            )}
            sx={{ width: { xs: '100%', sm: 360} }}
          />
          <AddButton
            label="New Segment"
            onClick={() => handleSegmentSelect(null)}
            sx={{ width: '160px' }}
          />
        </Box>
        <Paper
          sx={{
            flexGrow: 1,
            display: 'flex',
            flexDirection: 'column',
            overflow: 'hidden',
            height: 'auto',
          }}
        >
          <SegmentSelectMapWrapper
            focusedSegmentId={focusedSegmentId}
            segments={fetchedSegments}
            selectedSegmentIds={focusedSegmentId ? [focusedSegmentId] : []}
            onSegmentSelect={handleSegmentSelect}
          />
        </Paper>
      </Box>
    </ResponsivePageLayout>
  )
}

export default memo(SegmentsAdmin)
