// import { useQueries } from 'react-query'
// import { getLeftTurnGapReportDataCheck } from './getLeftTurnGapReportDataCheck'

// export const useMultipleLeftTurnGapReportDataChecks = (approachIds, body) => {
//   // useQueries accepts an array of query configurations
//   return useQueries(
//     approachIds.map((approachId) => ({
//       queryKey: ['leftTurnGapReportDataCheck', approachId],
//       queryFn: () => getLeftTurnGapReportDataCheck({ body: { ...body, approachId: approachId } }),
//     }))
//   )
// }
