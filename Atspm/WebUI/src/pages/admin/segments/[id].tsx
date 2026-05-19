import {
  getApiV1SegmentSegmentId,
  useDeleteApiV1SegmentDeleteSegmentSegmentId,
  usePostApiV1EntityGetEntitiesWithinRange,
  usePostApiV1SegmentAddSegment,
  usePostApiV1SegmentSegmentIdEntitiesReplace,
  usePutApiV1SegmentUpdateSegmentSegmentId,
} from '@/api/speedManagement/aTSPMSpeedManagementApi'
import { modalButtonLocation } from '@/components/GenericAdminChart'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { PageNames, useViewPage } from '@/features/identity/pagesCheck'
import { modalStyle } from '@/features/locations/components/editDetector/EditDetectors'
import { useTransformedSegments } from '@/features/speedManagementTool/api/getSegments'
import DataSources from '@/features/speedManagementTool/components/SegmentEditor/DataSources'
import SegmentEditorMapWrapper from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/SegmentEditorMapWrapper'
import SegmentProperties from '@/features/speedManagementTool/components/SegmentEditor/SegmentProperties'
import { useSegmentEditorStore } from '@/features/speedManagementTool/components/SegmentEditor/segmentEditorStore'
import { useNotificationStore } from '@/stores/notifications'
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft'
import SaveIcon from '@mui/icons-material/Save'
import { LoadingButton } from '@mui/lab'
import CircularProgress from '@mui/material/CircularProgress'

import {
  Alert,
  Backdrop,
  Box,
  Button,
  Divider,
  Modal,
  Paper,
  Snackbar,
  Tab,
  Tabs,
  Typography,
  useTheme,
} from '@mui/material'
import { useRouter } from 'next/router'
import { memo, useEffect, useRef, useState } from 'react'

const SegmentAdminPage = () => {
  const pageAccess = useViewPage(PageNames.Segments)
  const router = useRouter()
  const theme = useTheme()
  const { id } = router.query
  const { addNotification } = useNotificationStore()
  const [isLoading, setIsloading] = useState(false)
  const {
    segmentProperties,
    polylineCoordinates,
    associatedEntityIds,
    setAssociatedEntityIds,
    activeTab,
    allSegments,
    setAllSegments,
    nearByEntities,
    setNearByEntities,
    setIsLoadingEntities,
    setActiveTab,
    setPolylineCoordinates,
    setInitialPolylineCoordinates,
  } = useSegmentEditorStore()
  const [modalOpen, setModalOpen] = useState(false)
  const [selectedSegment, setSelectedSegment] = useState<number | null>(null)
  const [segmentData, setSegmentData] = useState<any>(null)
  const [alertOpen, setAlertOpen] = useState(false)
  const [alertMessage, setAlertMessage] = useState('')

  useEffect(() => {
    if (!id || id === 'new') return
    setSegmentData(null)
    getApiV1SegmentSegmentId(id as string)
      .then((res) => {
        setSegmentData(res)
        const { properties, geometry } = res.features[0]
        useSegmentEditorStore.getState().setSegmentProperties({
          id: properties.Id,
          udotRouteNumber: properties.UdotRouteNumber,
          startMilePoint: properties.StartMilePoint,
          endMilePoint: properties.EndMilePoint,
          functionalType: properties.FunctionalType,
          name: properties.Name,
          direction: properties.Direction,
          speedLimit: properties.SpeedLimit,
          region: properties.Region,
          city: properties.City,
          county: properties.County,
          alternateIdentifier: properties.AlternateIdentifier,
          accessCategory: properties.AccessCategory,
          offset: properties.Offset ?? 0,
          shape: properties.Shape,
          shapeWKT: properties.ShapeWKT,
        })
        setPolylineCoordinates(geometry.coordinates)
        setInitialPolylineCoordinates(geometry.coordinates)

        const entityIds = properties.RouteEntities?.map((e) => e.Id) ?? []
        setAssociatedEntityIds(entityIds)
      })
      .catch((err) => {
        console.error('Error loading segment:', err)
        addNotification({ title: 'Failed to load segment', type: 'error' })
      })
  }, [id, addNotification, setAssociatedEntityIds])

  const { mutateAsync: editMutation } =
    usePutApiV1SegmentUpdateSegmentSegmentId()

  const { mutateAsync: getNearByEntities, isLoading: entitiesAreLoading } =
    usePostApiV1EntityGetEntitiesWithinRange()

  const { mutateAsync: createSegment } = usePostApiV1SegmentAddSegment()

  const { mutateAsync: attachEntities } =
    usePostApiV1SegmentSegmentIdEntitiesReplace()

  const { mutateAsync: deleteEntities, isLoading: deleteIsLoading } =
    useDeleteApiV1SegmentDeleteSegmentSegmentId()

  const { data: fetchedSegments } = useTransformedSegments()

  const formRef = useRef<{ submitForm: () => Promise<boolean> }>(null)
  const { upsertSegment, removeSegment } = useSegmentEditorStore.getState()
  const originalCoordsRef = useRef<[number, number][]>([])
  const prevCoordsRef = useRef<[number, number][]>([])
  const fetchedOriginalRef = useRef(false)

  const samePoint = (a: [number, number], b: [number, number]) =>
    a[0] === b[0] && a[1] === b[1]

  //Functions ─────────────────────────────────────────────────────────────

  const buildDirections = (
    dir: 'NB' | 'SB' | 'EB' | 'WB' | undefined,
    polarity: string | undefined
  ): string[] => {
    if (!dir) return polarity ? [polarity] : []

    const variants: Record<'NB' | 'SB' | 'EB' | 'WB', string[]> = {
      NB: ['N', 'NB', 'NW', 'NE'],
      SB: ['S', 'SB', 'SW', 'SE'],
      EB: ['E', 'EB', 'NE', 'SE'],
      WB: ['W', 'WB', 'NW', 'SW'],
    }

    return polarity ? [...variants[dir], polarity] : variants[dir]
  }

  const handleGetEntities = async (coordinates: [number, number][]) => {
    if (polylineCoordinates.length < 2) return

    setIsLoadingEntities(true)
    try {
      const response = await getNearByEntities({
        data: {
          coordinates: coordinates,
          directions: buildDirections(
            segmentProperties.direction as
              | 'NB'
              | 'SB'
              | 'EB'
              | 'WB'
              | undefined,
            segmentProperties.polarity
          ),
        },
      })
      const filterTmcCodes = response.filter((e) => e.entityType !== 'TMC Code')

      setNearByEntities(filterTmcCodes)
    } catch (error) {
      console.error('Error fetching entities:', error)
      addNotification({ title: 'Failed to fetch entities', type: 'error' })
    } finally {
      setIsLoadingEntities(false)
    }
  }

  const handleSaveProperties = async () => {
    setIsloading(true)

    const {
      id,
      udotRouteNumber,
      startMilePoint,
      endMilePoint,
      functionalType,
      name,
      direction,
      speedLimit,
      region,
      city,
      county,
      shape,
      shapeWKT,
      alternateIdentifier,
      accessCategory,
      offset,
    } = segmentProperties

    const segmentPayload = {
      udotRouteNumber,
      startMilePoint,
      endMilePoint,
      functionalType,
      name,
      direction,
      speedLimit: Number(speedLimit),
      region,
      city,
      county,
      shape,
      shapeWKT,
      alternateIdentifier,
      accessCategory,
      offset,
    }

    const handleServerError = (error: any, fallbackMsg: string) => {
      if (error?.response?.status === 500) {
        addNotification({
          title:
            'Big Query is having issues. Please try again in 15-90 minutes',
          type: 'error',
        })
      } else {
        console.error(fallbackMsg, error)
        addNotification({ title: fallbackMsg, type: 'error' })
      }
    }

    try {
      // ────────── EDIT  ──────────
      if (router.query.id !== 'new') {
        await editMutation({ segmentId: id!, data: { id, ...segmentPayload } })
        upsertSegment({
          id: id!,
          geometry: { type: 'LineString', coordinates: polylineCoordinates },
          properties: { ...segmentPayload, Id: id! },
        })

        await attachEntities(
          { segmentId: id!, data: associatedEntityIds },
          {
            onSuccess: () =>
              addNotification({
                title: 'Segment updated successfully',
                type: 'success',
              }),
          }
        )

        return
      }

      // ────────── CREATE NEW ──────────
      const { features } = await createSegment(
        { data: segmentPayload },
        {
          onSuccess: () =>
            addNotification({
              title:
                'Segment created successfully. Please wait for entities to attach to segment...',
              type: 'success',
            }),
        }
      )
      const newSeg = features[0]

      upsertSegment(newSeg)
      // const newId = features[0].properties.Id
      if (associatedEntityIds.length > 0) {
        await attachEntities({
          segmentId: newSeg.properties.Id,
          data: associatedEntityIds,
        })
      }
      router.push(`/admin/segments/${newSeg.properties.Id}`).then(() => {
        setActiveTab(0)
      })
    } catch (error: any) {
      handleServerError(
        error,
        router.query.id !== 'new'
          ? 'Failed to update entities to segment'
          : 'Failed to create segment'
      )
    } finally {
      setIsloading(false)
    }
  }

  const handleTabChange = async (
    event: React.SyntheticEvent | null,
    newValue: number
  ) => {
    if (newValue === 1) {
      if (polylineCoordinates.length < 2) {
        setAlertMessage(
          'Please add two or more segment points to the map before proceeding'
        )
        setAlertOpen(true)
        return
      }

      if (formRef.current) {
        const isValid = await formRef.current.submitForm()
        if (!isValid) {
          setAlertMessage(
            'Please fill out all required segment properties before proceeding'
          )
          setAlertOpen(true)
          return
        }
      }
    }

    setActiveTab(newValue)
  }

  const handleDeleteClick = async (id: string) => {
    try {
      await deleteEntities(
        {
          segmentId: id,
        },
        {
          onSuccess: () => {
            addNotification({
              title: 'Segment deleted successfully',
              type: 'success',
            })
          },
        }
      )
      removeSegment(id)
      router.push('/admin/segments')
      setModalOpen(false)
    } catch (error: any) {
      if (error.response?.status === 500) {
        addNotification({
          title:
            'Big Query is having issues. Please try again in 15-90 minutes',
          type: 'error',
        })
        setModalOpen(false)
        return
      } else {
        console.error('Failed to delete segment', error)
        addNotification({
          title: 'Failed to delete segment',
          type: 'error',
        })
      }
      setModalOpen(false)
    }
  }

  const deleteModal = (id: string) => (
    <Modal open={modalOpen} onClose={() => setModalOpen(false)}>
      <Box sx={modalStyle}>
        <Typography sx={{ fontWeight: 'bold' }}>Delete Segment</Typography>
        <Divider sx={{ margin: '10px 0', backgroundColor: 'gray' }} />
        <Typography>Are you sure you want to delete this segment?</Typography>
        <Box sx={modalButtonLocation}>
          <Button
            onClick={() => {
              setModalOpen(false)
            }}
          >
            Cancel
          </Button>
          <Button
            onClick={() => {
              handleDeleteClick(id)
              setModalOpen(false)
            }}
            sx={{ color: 'red' }}
          >
            Delete Segment
          </Button>
        </Box>
      </Box>
    </Modal>
  )
  //useEffects ─────────────────────────────────────────────────────────────
  //1.
  useEffect(() => {
    if (!entitiesAreLoading && router.query.id === 'new') {
      const entitiesWithin50Ft = nearByEntities
        .filter((e) => e.isWithin50Ft)
        .map((e) => e.id)
      setAssociatedEntityIds(entitiesWithin50Ft)
    }
  }, [entitiesAreLoading])

  //2.
  useEffect(() => {
    if (!segmentData) return
    originalCoordsRef.current = segmentData.features[0].geometry.coordinates
    prevCoordsRef.current = segmentData.features[0].geometry.coordinates
  }, [segmentData])
  //3.
  useEffect(() => {
    if (!segmentData) return

    if (id === 'new') {
      // brand-new segment: wipe everything
      useSegmentEditorStore.getState().reset()
      setAssociatedEntityIds([])
      return
    }

    // existing segment: hydrate state from the API
    const { properties, geometry } = segmentData.features[0]

    const entityIds = properties.RouteEntities?.map((e) => e.Id) ?? []
    setAssociatedEntityIds(entityIds)

    useSegmentEditorStore.getState().setSegmentProperties({
      id: properties.Id,
      udotRouteNumber: properties.UdotRouteNumber,
      startMilePoint: properties.StartMilePoint,
      endMilePoint: properties.EndMilePoint,
      functionalType: properties.FunctionalType,
      name: properties.Name,
      direction: properties.Direction,
      speedLimit: properties.SpeedLimit,
      region: properties.Region,
      city: properties.City,
      county: properties.County,
      alternateIdentifier: properties.AlternateIdentifier,
      accessCategory: properties.AccessCategory,
      offset: properties.Offset ?? 0,
      shape: properties.Shape,
      shapeWKT: properties.ShapeWKT,
    })

    useSegmentEditorStore
      .getState()
      .setPolylineCoordinates(geometry.coordinates)

    setInitialPolylineCoordinates(geometry.coordinates)
  }, [segmentData, id])

  // 3a.  fetch entities for the *original* polyline ───────────────────────
  useEffect(() => {
    // run only when editing an existing segment
    if (router.query.id === 'new') return
    if (!segmentData) return // wait for data to load
    if (fetchedOriginalRef.current) return // already ran
    if (polylineCoordinates.length < 2) return

    fetchedOriginalRef.current = true
    handleGetEntities(polylineCoordinates) // full-line query, no auto-link
  }, [segmentData, polylineCoordinates])

  // 4.  auto associate new) ──────────────────────────────
  useEffect(() => {
    /* ───────── NEW SEGMENT ───────── */
    if (router.query.id === 'new') {
      if (polylineCoordinates.length >= 2 && activeTab == 1)
        handleGetEntities(polylineCoordinates)
      return
    }

    /* ───────── EDIT EXISTING ──────── */
    const original = originalCoordsRef.current
    const current = polylineCoordinates
    const prev = prevCoordsRef.current
    prevCoordsRef.current = current

    // only react when the user *adds* points
    if (current.length <= prev.length) return

    let added: [number, number][] = []

    // ① appended to the END
    if (
      current
        .slice(0, original.length)
        .every((c, i) => samePoint(c, original[i]))
    ) {
      added = current.slice(original.length - 1) // keep join point
    }
    // ② prepended to the START
    else if (
      current
        .slice(current.length - original.length)
        .every((c, i) => samePoint(c, original[i]))
    ) {
      added = current.slice(0, current.length - original.length + 1)
    }
    // ③ edited in the MIDDLE → fall back to whole line
    else {
      added = current
    }

    if (added.length < 2) return
    setIsLoadingEntities(true)
    // handleGetEntities(added)
    getNearByEntities({
      data: {
        coordinates: added,
        directions: buildDirections(
          segmentProperties.direction as 'NB' | 'SB' | 'EB' | 'WB' | undefined,
          segmentProperties.polarity
        ),
      },
    })
      .then((resp) => {
        // merge new entities without duplicates
        const arrayOfResp = [...resp]
        const allNearByEntities = [...nearByEntities, ...arrayOfResp]
        setNearByEntities(allNearByEntities)
        // auto-associate any newly-found ≤ 50 ft entities
        const newlyWithin = arrayOfResp.filter((e) => e.isWithin50Ft)
        const newIds = newlyWithin.map((e) => e.id)
        if (newIds.length) {
          setAssociatedEntityIds([...associatedEntityIds, ...newIds])
        }
      })
      .catch(() =>
        addNotification({ title: 'Failed to fetch entities', type: 'error' })
      )
      .finally(() => setIsLoadingEntities(false))
  }, [polylineCoordinates, activeTab])

  //5.
  useEffect(() => {
    if (fetchedSegments && (!allSegments || allSegments.length === 0)) {
      setAllSegments(fetchedSegments)
    }
  }, [fetchedSegments, allSegments, setAllSegments])

  // loading checks ──────────────────────────────────────────────────────────
  if (!segmentData && activeTab === 0 && id !== 'new') {
    return (
      <Backdrop open>
        <CircularProgress color="inherit" />
      </Backdrop>
    )
  }

  if (pageAccess.isLoading) {
    return null
  }

  return (
    <>
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          margin: 'auto',
          maxWidth: '1550px',
          minWidth: '375px',
          [theme.breakpoints.down('md')]: {},
        }}
      >
        <Box sx={{ display: 'flex', justifyContent: 'flex-start' }}>
          <Button onClick={() => router.push('/admin/segments')}>
            <ChevronLeftIcon /> Back to Segments
          </Button>
        </Box>
      </Box>
      <ResponsivePageLayout
        title={
          id !== 'new'
            ? `Edit Segment - ${segmentProperties.name || ''}`
            : `New Segment - ${segmentProperties.name || ''}`
        }
      >
        <Box sx={{ display: 'flex', flexDirection: 'row', gap: 2 }}>
          <Paper
            sx={{
              flex: 1.3,
              padding: '15px',
            }}
          >
            <Box
              sx={{
                display: 'flex',
                justifyContent: 'center',
                alignItems: 'center',
                height: '100%',
              }}
            >
              <SegmentEditorMapWrapper />
            </Box>
          </Paper>

          <Box sx={{ flex: 1 }}>
            <Paper
              sx={{
                width: 'auto',
                padding: '5px 20px',
                display: 'flex',
                flexDirection: 'column',
                gap: '15px',
              }}
            >
              <Tabs
                value={activeTab}
                onChange={handleTabChange}
                aria-label="segment tabs"
                sx={{ borderBottom: 1, borderColor: 'divider' }}
              >
                <Tab label="General" id="general-tab" />
                <Tab label="Data Sources" id="data-sources-tab" />
              </Tabs>

              {activeTab === 0 && <SegmentProperties ref={formRef} />}

              {activeTab === 1 && <DataSources />}
            </Paper>
          </Box>
        </Box>
        <Box
          sx={{ display: 'flex', justifyContent: 'flex-end', gap: 2, mt: 2 }}
        >
          <Snackbar
            open={alertOpen}
            autoHideDuration={6000}
            onClose={() => setAlertOpen(false)}
            anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
            sx={{
              position: 'absolute',
              bottom: 60,
              right: 10,
            }}
          >
            <Alert
              onClose={() => setAlertOpen(false)}
              severity="error"
              sx={{ width: '100%' }}
            >
              {alertMessage}
            </Alert>
          </Snackbar>
          <LoadingButton
            variant="outlined"
            aria-label="delete Segment"
            disabled={router.query.id === 'new'}
            onClick={() => {
              setSelectedSegment(id)
              setModalOpen(true)
            }}
            color="error"
            loading={deleteIsLoading}
          >
            {/* <DeleteIcon /> */} Delete
          </LoadingButton>
          {activeTab === 0 && (
            <Button
              variant="contained"
              onClick={() => handleTabChange(null, 1)}
            >
              Next
            </Button>
          )}
          {activeTab === 1 && (
            <>
              <Button
                variant="contained"
                onClick={() => handleTabChange(null, 0)}
              >
                back
              </Button>

              <LoadingButton
                loading={isLoading}
                startIcon={<SaveIcon />}
                loadingPosition="start"
                variant="contained"
                sx={{ pl: '10px', pr: '10px' }}
                onClick={handleSaveProperties}
              >
                save
              </LoadingButton>
            </>
          )}
        </Box>
      </ResponsivePageLayout>
      {modalOpen && selectedSegment && deleteModal(selectedSegment)}
    </>
  )
}

export default memo(SegmentAdminPage)
