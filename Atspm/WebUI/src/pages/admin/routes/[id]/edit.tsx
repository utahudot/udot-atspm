import {
  Route,
  RouteDistance,
  RouteLocation,
  useGetLocationLatestVersionOfAllLocations,
  useGetRouteDistance,
  useGetRouteRouteViewFromId,
  useUpsertRouteRoute,
} from '@/api/config'
import { PageNames, useViewPage } from '@/features/identity/pagesCheck'
import SelectLocation from '@/features/locations/components/selectLocation/SelectLocation'
import { Location } from '@/features/locations/types'
import RouteEditor from '@/features/routes/components/routeEditor'
import { ConfigEnum, useConfigEnums } from '@/hooks/useConfigEnums'
import { useNotificationStore } from '@/stores/notifications'
import { fetchRouteDistance } from '@/utils/fetchRouteDistance'
import { navigateToPage } from '@/utils/routes'
import { DropResult } from '@hello-pangea/dnd'
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft'
import { Box, Button, Paper, TextField } from '@mui/material'
import { useRouter } from 'next/router'
import React, { memo, useCallback, useEffect, useState } from 'react'

const RouteAdmin = () => {
  const pageAccess = useViewPage(PageNames.Routes)
  const router = useRouter()
  const { id } = router.query
  const { addNotification } = useNotificationStore()

  const { data: locationsData } = useGetLocationLatestVersionOfAllLocations()
  const { data: route } = useGetRouteRouteViewFromId(id as unknown as number)
  const { data: routeDistancesData } = useGetRouteDistance()
  const { mutate: updateRoute } = useUpsertRouteRoute()
  const { data: directionTypes } = useConfigEnums(ConfigEnum.DirectionTypes)

  const [location, setLocation] = useState<Location | null>(null)
  const [routePolyline, setRoutePolyline] = useState<number[][]>([])
  const [updatedRoute, setUpdatedRoute] = useState<Route>()
  const [hasLoaded, setHasLoaded] = useState(false)
  const [hasErrors, setHasErrors] = useState(false)
  const [routeDistances, setRouteDistances] = useState<RouteDistance[]>([])

  const locations = locationsData?.value

  useEffect(() => {
    if (routeDistancesData) {
      setRouteDistances(routeDistancesData.value)
    }
  }, [routeDistancesData])

  const fetchRouteDistanceAndUpdatePolyline = useCallback(
    async (routeLocations: RouteLocation[]) => {
      if (routeLocations.length < 2) {
        setRoutePolyline([])
        return
      }
      try {
        const polylineResponse = await fetchRouteDistance(routeLocations)
        if (polylineResponse) {
          setRoutePolyline(polylineResponse.shape)
        }
      } catch {
        addNotification({
          type: 'error',
          title: 'Error fetching route distance',
        })
      }
    },
    [addNotification]
  )

  useEffect(() => {
    if (hasLoaded) return

    if (!route || !locations || !routeDistances || pageAccess.isLoading) {
      return
    }

    const sortedLocations = [...route.routeLocations].sort(
      (a, b) => a.order - b.order
    )
    setUpdatedRoute({ ...route, routeLocations: sortedLocations })
    setHasLoaded(true)

    if (sortedLocations.length >= 2) {
      fetchRouteDistanceAndUpdatePolyline(sortedLocations)
    }
  }, [
    route,
    locations,
    routeDistances,
    pageAccess.isLoading,
    hasLoaded,
    fetchRouteDistanceAndUpdatePolyline,
  ])

  if (!updatedRoute || !locations || !routeDistances) {
    return null
  }

  const onDragEnd = (result: DropResult) => {
    if (!result.destination) return

    const items = Array.from(updatedRoute.routeLocations)
    const [moved] = items.splice(result.source.index, 1)
    items.splice(result.destination.index, 0, moved)

    const reIndexed = items.map((item, idx) => ({
      ...item,
      order: idx,
    }))

    let affectedIndices: number[] = [result.destination.index]
    if (result.destination.index > 0) {
      affectedIndices.push(result.destination.index - 1)
    }
    if (result.destination.index > result.source.index) {
      if (result.source.index > 0) {
        affectedIndices.push(result.source.index - 1)
      }
    } else {
      affectedIndices.push(result.source.index)
    }
    affectedIndices = Array.from(new Set(affectedIndices))

    affectedIndices.forEach((i) => {
      const curr = reIndexed[i]
      const next = reIndexed[i + 1]
      if (next) {
        curr.nextLocationDistance = findRouteDistance(
          curr,
          next.locationIdentifier,
          routeDistances
        )
      } else {
        curr.nextLocationDistance = null
      }
    })

    fetchRouteDistanceAndUpdatePolyline(reIndexed)

    setUpdatedRoute((prev) => ({
      ...prev!,
      routeLocations: reIndexed,
    }))
  }

  const onAddRoute = () => {
    if (!location || !updatedRoute) return
    const exists = updatedRoute.routeLocations.some(
      (link) => link.locationIdentifier === location.locationIdentifier
    )
    if (exists) return

    const newLink: RouteLocation = {
      routeId: updatedRoute.id,
      locationId: location.id,
      locationIdentifier: location.locationIdentifier,
      primaryName: location.primaryName,
      secondaryName: location.secondaryName,
      latitude: location.latitude,
      longitude: location.longitude,
      order: updatedRoute.routeLocations.length,
      primaryPhase: null,
      opposingPhase: null,
      primaryDirectionId: null,
      opposingDirectionId: null,
      primaryDirectionDescription: null,
      opposingDirectionDescription: null,
      isPrimaryOverlap: null,
      isOpposingOverlap: null,
      nextLocationDistance: null,
      nextLocationDistanceId: null,
    }

    const newList = [...updatedRoute.routeLocations, newLink]
    setUpdatedRoute((prev) => ({
      ...prev!,
      routeLocations: newList,
    }))

    fetchRouteDistanceAndUpdatePolyline(newList)
    setLocation(null)
  }

  const handleDistanceChange = (
    locationIdentifier: string,
    distance: number
  ) => {
    setUpdatedRoute((prevRoute) => {
      if (!prevRoute) return prevRoute

      // Find the index of the link:
      const idx = prevRoute.routeLocations.findIndex(
        (rl) => rl.locationIdentifier === locationIdentifier
      )
      if (idx === -1) return prevRoute

      const thisLink = prevRoute.routeLocations[idx]
      const nextLink = prevRoute.routeLocations[idx + 1]
      if (!nextLink) return prevRoute

      const existingDist = findRouteDistance(
        thisLink,
        nextLink.locationIdentifier,
        routeDistances
      )
      const newDistObj = existingDist
        ? existingDist.distance === distance
          ? existingDist
          : { ...existingDist, distance }
        : {
            locationIdentifierA: thisLink.locationIdentifier,
            locationIdentifierB: nextLink.locationIdentifier,
            distance,
          }

      if (!existingDist || existingDist.distance !== newDistObj.distance) {
        setRouteDistances((prev) => {
          const filtered = prev.filter(
            (d) =>
              !(
                d.locationIdentifierA === newDistObj.locationIdentifierA &&
                d.locationIdentifierB === newDistObj.locationIdentifierB
              )
          )
          return [...filtered, newDistObj]
        })
      }

      const newLocations = prevRoute.routeLocations.map((rl, i) =>
        i === idx
          ? {
              ...rl,
              nextLocationDistance: newDistObj,
            }
          : rl
      )

      return {
        ...prevRoute,
        routeLocations: newLocations,
      }
    })
  }

  const handleDirectionChange = (updatedLink: RouteLocation) => {
    setUpdatedRoute((prev) => ({
      ...prev,
      routeLocations: prev!.routeLocations.map((rl) =>
        rl.locationIdentifier === updatedLink.locationIdentifier
          ? updatedLink
          : rl
      ),
    }))
  }

  const handleDeleteLink = (link: RouteLocation) => {
    if (!updatedRoute) return
    const idx = updatedRoute.routeLocations.findIndex(
      (rl) => rl.locationIdentifier === link.locationIdentifier
    )
    if (idx === -1) return

    // Update the previous link’s distance (if any) to skip over the deleted one:
    if (idx > 0) {
      const prevLink = updatedRoute.routeLocations[idx - 1]
      const nextAfterDeleted = updatedRoute.routeLocations[idx + 1]
      prevLink.nextLocationDistance = nextAfterDeleted
        ? findRouteDistance(
            prevLink,
            nextAfterDeleted.locationIdentifier,
            routeDistances
          )
        : null
    }

    let filtered = updatedRoute.routeLocations.filter(
      (rl) => rl.locationIdentifier !== link.locationIdentifier
    )
    // Decrement order on everything after the deleted index:
    filtered = filtered.map((rl, i) =>
      i >= idx ? { ...rl, order: rl.order - 1 } : rl
    )

    fetchRouteDistanceAndUpdatePolyline(filtered)
    setUpdatedRoute((prev) => ({
      ...prev,
      routeLocations: filtered,
    }))
  }

  const handleEditRouteName = (e: React.ChangeEvent<HTMLInputElement>) => {
    setUpdatedRoute((prev) => ({
      ...prev,
      name: e.target.value,
    }))
  }

  const handleSaveRoute = async () => {
    if (!updatedRoute || !updatedRoute.id) return

    const hasErrorsLocal = updatedRoute.routeLocations.some((rl, i) => {
      const notLast = i !== updatedRoute.routeLocations.length - 1
      const missingDist = notLast && !rl.nextLocationDistance
      const missingDir = !rl.primaryDirectionId || !rl.opposingDirectionId
      return missingDist || missingDir
    })

    if (hasErrorsLocal) {
      setHasErrors(true)
      return
    }
    setHasErrors(false)

    const cloned: Route = JSON.parse(JSON.stringify(updatedRoute))
    cloned.routeLocations.forEach((rl) => {
      rl.primaryDirectionId =
        directionTypes?.find((dt) => dt.name === rl.primaryDirectionId)
          ?.value || null
      rl.opposingDirectionId =
        directionTypes?.find((dt) => dt.name === rl.opposingDirectionId)
          ?.value || null
    })

    updateRoute(
      { key: cloned.id, data: cloned },
      {
        onSuccess: (savedRoute) => {
          addNotification({
            type: 'success',
            title: 'Route saved successfully',
          })
          if (!savedRoute.routeLocations) return
          savedRoute.routeLocations.sort((a, b) => a.order - b.order)
          setUpdatedRoute(savedRoute)
        },
        onError: (err) => {
          addNotification({
            type: 'error',
            title: 'Error saving route',
            message: err.message,
          })
        },
      }
    )
  }

  return (
    <Box>
      <Box
        display="flex"
        flexDirection="column"
        width="200px"
        alignItems="start"
      >
        <Button
          onClick={() => navigateToPage('/admin/routes')}
          sx={{ pl: 0, mb: 2 }}
        >
          <ChevronLeftIcon /> Back to Routes
        </Button>
        <TextField
          label="Route Name"
          value={updatedRoute.name || ''}
          onChange={handleEditRouteName}
          sx={{ fontSize: '30px', mb: 2, minWidth: '250px' }}
        />
      </Box>
      <Box sx={{ display: 'flex' }}>
        <Paper sx={{ flexGrow: 1, minWidth: '400px', p: 3 }}>
          <SelectLocation
            location={location}
            setLocation={setLocation}
            route={routePolyline}
            center={
              routePolyline[Math.floor(routePolyline.length / 2)] as [
                number,
                number,
              ]
            }
            zoom={13}
            mapHeight="calc(100vh - 330px)"
          />
        </Paper>
        <RouteEditor
          hasErrors={hasErrors}
          route={updatedRoute}
          location={location}
          onAddRoute={onAddRoute}
          onDragEnd={onDragEnd}
          handleDistanceChange={handleDistanceChange}
          handleDirectionUpdate={handleDirectionChange}
          handleDeleteLink={handleDeleteLink}
          handleSaveRoute={handleSaveRoute}
        />
      </Box>
    </Box>
  )
}

export default memo(RouteAdmin)

function findRouteDistance(
  routeLocation: RouteLocation,
  nextLocationIdentifier: string,
  routeDistances: RouteDistance[]
) {
  return (
    routeDistances.find(
      (rd) =>
        routeLocation.locationIdentifier === rd.locationIdentifierA &&
        nextLocationIdentifier === rd.locationIdentifierB
    ) || null
  )
}
