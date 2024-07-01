import { StyledPaper } from '@/components/StyledPaper'
import { PageNames, useViewPage } from '@/features/identity/pagesCheck'
import { useLatestVersionOfAllLocations } from '@/features/locations/api'
import SelectLocation from '@/features/locations/components/selectLocation/SelectLocation'
import { Location } from '@/features/locations/types'
import { useGetRoute, usePutRoute } from '@/features/routes/api'
import { useGetRouteDistances } from '@/features/routes/api/updateRouteDistance'
import RouteEditor from '@/features/routes/components/routeEditor'
import { Route, RouteDistance, RouteLocation } from '@/features/routes/types'
import { ConfigEnum, useConfigEnums } from '@/hooks/useConfigEnums'
import { fetchRouteDistance } from '@/utils/fetchRouteDistance'
import { navigateToPage } from '@/utils/routes'
import { DropResult } from '@hello-pangea/dnd'
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft'
import { Box, Button, TextField } from '@mui/material'
import { useRouter } from 'next/router'
import React, { memo, useEffect, useState } from 'react'
const RouteAdmin = () => {
  const pageAccess = useViewPage(PageNames.Routes)

  const router = useRouter()
  const { id } = router.query

  const { data: locationsData } = useLatestVersionOfAllLocations()
  const { data: route } = useGetRoute(id as string)
  const { data: routeDistancesData } = useGetRouteDistances()
  const { mutate: updateRoute } = usePutRoute()

  const directionTypesData = useConfigEnums(ConfigEnum.DirectionTypes)

  const [location, setLocation] = useState<Location | null>(null)
  const [routePolyline, setRoutePolyline] = useState<number[][]>([])
  const [updatedRoute, setUpdatedRoute] = useState<Route>()
  const [hasErrors, setHasErrors] = useState(false)
  const [routeDistances, setRouteDistances] = useState<RouteDistance[]>([])

  const locations = locationsData?.value
  const directionTypes = directionTypesData?.data?.members

  useEffect(() => {
    if (routeDistancesData) {
      setRouteDistances(routeDistancesData.value)
    }
  }, [routeDistancesData])

  useEffect(() => {
    if (!(route && locations && routeDistances) || pageAccess.isLoading) {
      return
    }

    route.routeLocations.sort((a, b) => a.order - b.order)
    setUpdatedRoute(route)
    if (route.routeLocations.length >= 2) {
      fetchRouteDistanceAndUpdatePolyline(route.routeLocations)
    }
  }, [route, locations, routeDistances, pageAccess.isLoading])

  const fetchRouteDistanceAndUpdatePolyline = async (
    routeLocations: RouteLocation[]
  ) => {
    const polylineResponse = await fetchRouteDistance(routeLocations)
    if (polylineResponse) {
      setRoutePolyline(polylineResponse.shape)
    }
  }

  if (!updatedRoute || !locations || !routeDistances) {
    return
  }

  const onDragEnd = (result: DropResult) => {
    if (!result.destination || !updatedRoute) return

    if (result.destination.index === result.source.index) return

    const items = Array.from(updatedRoute.routeLocations)
    const [reorderedItem] = items.splice(result.source.index, 1)
    items.splice(result.destination.index, 0, reorderedItem)

    // Update the order property of each routeLocation based on the new order
    const updatedItems = items.map((item, index) => ({
      ...item,
      order: index,
    }))

    let affectedIndices = [result.destination.index]

    // Find the affected links based on the source and destination indices
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

    // Ensure unique indices in case of overlap
    affectedIndices = [...new Set(affectedIndices)]

    // Update the distances for the affected links
    affectedIndices.forEach((index) => {
      const currentItem = updatedItems[index]
      const nextItem = updatedItems[index + 1]

      if (nextItem) {
        const distance = findRouteDistance(
          currentItem,
          nextItem.locationIdentifier,
          routeDistances
        )
        currentItem.nextLocationDistance = distance
      } else {
        currentItem.nextLocationDistance = null
      }
    })

    fetchRouteDistanceAndUpdatePolyline(updatedItems)

    setUpdatedRoute((prevRoute) => ({
      ...prevRoute,
      routeLocations: updatedItems,
    }))
  }

  const onAddRoute = () => {
    if (!location || !updatedRoute) return
    const locationExists = updatedRoute.routeLocations.find(
      (link) => link.locationIdentifier === location.locationIdentifier
    )
    if (locationExists) return

    const newRouteLocation = {
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

    const updatedRouteLocations = [
      ...updatedRoute.routeLocations,
      newRouteLocation,
    ]

    setUpdatedRoute((prevRoute) => ({
      ...prevRoute,
      routeLocations: updatedRouteLocations,
    }))
    fetchRouteDistanceAndUpdatePolyline(updatedRouteLocations)
    setLocation(null)
  }

  const handleDistanceChange = (link: RouteLocation, distance: number) => {
    const routeLocationIndex = updatedRoute.routeLocations.findIndex(
      (routeLocation) =>
        routeLocation.locationIdentifier === link.locationIdentifier
    )

    if (routeLocationIndex === -1) return

    const nextLink = updatedRoute.routeLocations[routeLocationIndex + 1]
    if (!nextLink) return // Ensure nextLink exists

    // Find or create the routeDistance
    let routeDistance = findRouteDistance(
      link,
      nextLink.locationIdentifier,
      routeDistances
    ) || {
      locationIdentifierA: link.locationIdentifier,
      locationIdentifierB: nextLink.locationIdentifier,
      distance,
    }

    // Update the routeDistance if it already exists
    if (routeDistance.distance !== distance) {
      routeDistance = { ...routeDistance, distance }
    }

    // Update the route locations
    const updatedRouteLocations = updatedRoute.routeLocations.map(
      (routeLocation, idx) => {
        if (idx === routeLocationIndex) {
          return { ...routeLocation, nextLocationDistance: routeDistance }
        }
        return routeLocation
      }
    )

    setUpdatedRoute({ ...updatedRoute, routeLocations: updatedRouteLocations })
  }

  const handleDirectionChange = (updatedLink: RouteLocation) => {
    const updatedLinks = updatedRoute.routeLocations.map((routeLocation) => {
      if (routeLocation.locationIdentifier === updatedLink.locationIdentifier) {
        return updatedLink
      }
      return routeLocation
    })
    setUpdatedRoute({ ...updatedRoute, routeLocations: updatedLinks })
  }

  const handleDeleteLink = (link: RouteLocation) => {
    // Find index of the link to be deleted
    const index = updatedRoute.routeLocations.findIndex(
      (routeLocation) =>
        routeLocation.locationIdentifier === link.locationIdentifier
    )

    // If there is a preceding link, set its nextLocationDistance to null
    if (index > 0) {
      updatedRoute.routeLocations[index - 1].nextLocationDistance =
        findRouteDistance(
          updatedRoute.routeLocations[index - 1],
          updatedRoute.routeLocations[index + 1]?.locationIdentifier,
          routeDistances
        )
    }

    // Filter out the deleted link from the route
    let updatedLinks = updatedRoute.routeLocations.filter(
      (routeLocation) =>
        routeLocation.locationIdentifier !== link.locationIdentifier
    )

    // Decrement the order of all links after the deleted one
    if (index < updatedRoute.routeLocations.length - 1) {
      updatedLinks = updatedLinks.map((routeLocation, i) => {
        if (i >= index) {
          return { ...routeLocation, order: routeLocation.order - 1 }
        }
        return routeLocation
      })
    }

    setUpdatedRoute({ ...updatedRoute, routeLocations: updatedLinks })
    fetchRouteDistanceAndUpdatePolyline(updatedRoute.routeLocations)
  }

  const handleEditRouteName = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = event.target.value
    setUpdatedRoute({ ...updatedRoute, name: value })
  }

  const handleSaveRoute = async () => {
    if (!updatedRoute || !updatedRoute.id) return

    const hasErrors = updatedRoute.routeLocations.some(
      (routeLocation, index) => {
        const isNotLast = index !== updatedRoute.routeLocations.length - 1
        const hasDistanceError =
          isNotLast && !routeLocation.nextLocationDistance
        const hasDirectionError =
          !routeLocation.primaryDirectionId ||
          !routeLocation.opposingDirectionId
        return hasDistanceError || hasDirectionError
      }
    )

    if (hasErrors) {
      setHasErrors(true)
      return
    }
    setHasErrors(false)

    const clonedRoute = JSON.parse(JSON.stringify(updatedRoute)) as Route
    clonedRoute.routeLocations.forEach((routeLocation) => {
      routeLocation.primaryDirectionId =
        directionTypes?.find(
          (directionType) =>
            directionType.name === routeLocation.primaryDirectionId
        )?.value || null
      routeLocation.opposingDirectionId =
        directionTypes?.find(
          (directionType) =>
            directionType.name === routeLocation.opposingDirectionId
        )?.value || null
    })

    updateRoute(clonedRoute, {
      onSuccess: (data: Route) => {
        if (!data.routeLocations) return
        data.routeLocations.sort((a, b) => a.order - b.order)
        setUpdatedRoute(data)
      },
    })
  }

  return (
    <Box>
      <Box
        display={'flex'}
        flexDirection={'column'}
        width={'200px'}
        alignItems={'start'}
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
        <StyledPaper sx={{ flexGrow: 1, minWidth: '400px', p: 3 }}>
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
            mapHeight={'calc(100vh - 330px)'}
          />
        </StyledPaper>
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
      (routeDistance) =>
        routeLocation.locationIdentifier ===
          routeDistance.locationIdentifierA &&
        nextLocationIdentifier === routeDistance.locationIdentifierB
    ) || null
  )
}
