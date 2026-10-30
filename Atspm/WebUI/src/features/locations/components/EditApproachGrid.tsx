import { memo } from 'react'
import { ApproachEditorGrid } from './editApproach/ApproachEditorGrid'
import {
  useLocationStore,
  type ConfigApproach,
} from './editLocation/locationStore'
function EditApproachGrid({ approach }: { approach: ConfigApproach }) {
  const { location, errors, updateApproach } = useLocationStore((state) => ({
    location: state.location,
    errors: state.errors,
    updateApproach: state.updateApproach,
  }))
  return (
    <ApproachEditorGrid
      approach={approach}
      errors={errors}
      updateApproach={updateApproach}
      pedsAre1to1={location?.pedsAre1to1 ?? false}
    />
  )
}
export default memo(EditApproachGrid)
