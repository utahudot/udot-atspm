import { Segment } from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { AddButton } from '@/components/addButton'
import { PageNames, useViewPage } from '@/features/identity/pagesCheck'
import { useTransformedSegments } from '@/features/speedManagementTool/api/getSegments'
import { useSegmentEditorStore } from '@/features/speedManagementTool/components/SegmentEditor/segmentEditorStore'
import SegmentSelectMapWrapper from '@/features/speedManagementTool/components/SegmentSelectMap'
import { Box, Paper } from '@mui/material'
import { useRouter } from 'next/router'
import { memo, useCallback, useEffect } from 'react'

const SegmentsAdmin = () => {
  const pageAccess = useViewPage(PageNames.Segments)
  const router = useRouter()
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
    (segment: Segment | null) => {
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
        <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
          <AddButton
            label="New Segment"
            onClick={() => handleSegmentSelect(null)}
            sx={{ mb: 2, width: '160px' }}
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
            segments={fetchedSegments}
            selectedSegmentIds={[]}
            onSegmentSelect={handleSegmentSelect}
          />
        </Paper>
      </Box>
    </ResponsivePageLayout>
  )
}

export default memo(SegmentsAdmin)
