import { Box, Skeleton, Typography } from '@mui/material';
import 'leaflet/dist/leaflet.css';
import dynamic from 'next/dynamic';
import { memo } from 'react';
import { useGetSegments } from '../../api/getSegments';

const SegmentSelectMap = dynamic(() => import('./Map'), {
  // loading: () => <Skeleton variant="rectangular" />,
  ssr: false,
});

interface SegmentSelectMapProps {
  selectedSegmentIds: string[];
  onSegmentSelect: (id: string, startMile: number, endMile: number) => void;
  segmentData: any[]; // Add segments to props
  isLoadingSegments : any;
}

const MapWrapper:React.FC<SegmentSelectMapProps> = ({selectedSegmentIds, segmentData, isLoadingSegments , onSegmentSelect }) => {
  // const { data: segmentData, isLoading: isLoadingSegments } = useGetSegments();

  const segments = segmentData?.features
    ?.filter((feature) => feature.geometry.type === 'LineString')
    ?.map((feature) => ({
      ...feature,
      geometry: {
        ...feature.geometry,
        coordinates: feature.geometry.coordinates.map((coord) => [
          coord[1],
          coord[0],
        ]),
      },
    })) || [];
  return (
    <Box sx={{ height: '100%', width: '100%' }}>
      {!isLoadingSegments && segmentData ? (
        <SegmentSelectMap 
          segments={segments}
          selectedSegmentIds={selectedSegmentIds}
          onSegmentSelect={onSegmentSelect}
        /> 
      ) : (
        <Typography>Map is loading</Typography>
      )}
    </Box>
  );
};

export default memo(MapWrapper);