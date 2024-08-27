// import { useRoutes } from '@/features/speedManagementTool/api/getRoutes'
// import { useUdotSpeedLimitRoutes } from '@/features/speedManagementTool/api/getUdotSpeedLimitRoutes'
// import SM_TopBar from '@/features/speedManagementTool/components/SM_Topbar'
// import SM_Popup from '@/features/speedManagementTool/components/speedManagementMap/SM_Popup'
// import { RouteRenderOption } from '@/features/speedManagementTool/enums'
// import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { Box } from '@mui/material'
import 'leaflet/dist/leaflet.css'
import dynamic from 'next/dynamic'
import { memo } from 'react'

const SegmentSelectMap = dynamic(() => import('./Map'), { ssr: false })

// const LocationMap = useMemo(
//     () =>
//       dynamic(() => import('@/components/LocationMap'), {
//         loading: () => (
//           <Skeleton variant="rectangular" height={mapHeight ?? 400} />
//         ),
//         ssr: false,
//       }),
//     [mapHeight]
//   )

const Map = () => {

  return (
    <Box sx={{ height: '100%', width: '100%' }}>
      <SegmentSelectMap />
    </Box>
  )
}

export default memo(Map)
