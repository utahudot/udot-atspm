import { useQuery } from 'react-query';
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import axios from 'axios';
import { Impact } from '../types/impact';



interface SpeedFromImpactResponse {
  impacts: Impact[];
}


// const getSpeedFromImpactSegment = async (segmentId: string): Promise<Impact[]> => {
//   const response = await axios.post<SpeedFromImpactResponse>(`https://localhost:44343/api/SpeedFromImpact/segment/${segmentId}`, {});
//   return response.data.impacts;
// };

// type QueryFnType = typeof getSpeedFromImpactSegment;

// export const useGetSpeedFromImpactSegment = (
//   segmentId: string,
//   config?: QueryConfig<QueryFnType>
// ) => {
//   return useQuery<ExtractFnReturnType<QueryFnType>>(
//     ['speedFromImpactSegment', segmentId],
//     () => getSpeedFromImpactSegment(segmentId),
//     config
//   );
// };
//! TODO: Remove this mock data and uncoment out the actual api call above
const mockImpacts: Impact[] = [
    {
      id: "1f9a8769-e712-4833-943a-a2b18c0e5760",
      description: "Bulldoze Ogden",
      start: "2024-05-16T16:17:24",
      end: "2024-08-06T16:17:24",
      startMile: 10.83056,
      endMile: 11.07612,
      impactTypeIds: ["79190953-f8cb-48e2-8436-7fcfc7680df3"],
      impactTypes: [
        {
          id: "79190953-f8cb-48e2-8436-7fcfc7680df3",
          name: "Ramp Closure",
          description: "Redirects traffic to alternative routes"
        }
      ],
      createdOn: "2024-08-16T17:14:10",
      createdBy: "8164d383-e8f6-432d-8e56-264092e30d15",
      updatedOn: null,
      updatedBy: null,
      deletedOn: null,
      deletedBy: null,
      segmentIds: [
        "e9d1b5bd-bc16-47dc-be5a-7780f66ea7ca",
        "581e8cf2-4ece-481a-b694-a981b4196664",
        "d878257b-e2f0-49d8-bee6-00211e148706",
        "c3b58632-f7d3-45d8-af78-1e0ef1475343"
      ]
    },
    {
      id: "2a9b9870-f823-5944-054b-b3c29d1f6871",
      description: "Repave Salt Lake City",
      start: "2024-06-01T08:00:00",
      end: "2024-09-30T17:00:00",
      startMile: 15.25000,
      endMile: 18.75000,
      impactTypeIds: ["89291064-g9dc-59f3-9547-8gfd8791df4"],
      impactTypes: [
        {
          id: "89291064-g9dc-59f3-9547-8gfd8791df4",
          name: "Lane Closure",
          description: "Reduces available lanes for traffic"
        }
      ],
      createdOn: "2024-09-01T09:30:15",
      createdBy: "9275e494-f9g7-543e-9f67-375193f41e26",
      updatedOn: null,
      updatedBy: null,
      deletedOn: null,
      deletedBy: null,
      segmentIds: [
        "f0e2c6ce-cd17-58ed-cf75-b891c5307775",
        "692f9dg3-5fdf-592b-c705-b092c5308886"
      ]
    },
    {
      id: "3b0c0981-g934-6055-165c-c4d30e2g7982",
      description: "Bridge Maintenance in Provo",
      start: "2024-07-15T22:00:00",
      end: "2024-07-20T05:00:00",
      startMile: 25.50000,
      endMile: 26.00000,
      impactTypeIds: ["90302175-h0ed-60g4-0658-9hge9802eg5"],
      impactTypes: [
        {
          id: "90302175-h0ed-60g4-0658-9hge9802eg5",
          name: "Night Work",
          description: "Construction during night hours"
        }
      ],
      createdOn: "2024-10-15T14:45:30",
      createdBy: "0386f505-g0h8-654f-0g78-486204g52f37",
      updatedOn: null,
      updatedBy: null,
      deletedOn: null,
      deletedBy: null,
      segmentIds: [
        "g1f3d7df-de28-69fe-dg86-c902d6419997"
      ]
    }
  ];
  
  const getSpeedFromImpactSegment = async (segmentId: string): Promise<Impact[]> => {
    // Simulating API call delay
    await new Promise(resolve => setTimeout(resolve, 1000));
    
    // Return mock data
    return mockImpacts;
  };
  
  type QueryFnType = typeof getSpeedFromImpactSegment;
  
  export const useGetSpeedFromImpactSegment = (
    segmentId: string,
    config?: QueryConfig<QueryFnType>
  ) => {
    return useQuery<ExtractFnReturnType<QueryFnType>>(
      ['speedFromImpactSegment', segmentId],
      () => getSpeedFromImpactSegment(segmentId),
      config
    );
  };
