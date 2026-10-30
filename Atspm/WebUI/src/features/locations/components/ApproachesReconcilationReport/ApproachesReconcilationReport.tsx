import { useLocationStore } from '../editLocation/locationStore'
import { useLocationEditorOptions } from '../editLocation/useLocationEditorOptions'
import { ApproachesReconciliationView } from './ApproachesReconciliationView'
import type { LocationDiscrepancyReport } from './useDiscrepancyStatuses'

export default function ApproachesReconcilationReport({
  categories,
}: {
  categories: LocationDiscrepancyReport
}) {
  const {
    approaches,
    deleteApproach,
    updateApproach,
    deleteDetector,
    addApproach,
    addDetector,
    errors,
  } = useLocationStore((state) => ({
    approaches: state.approaches,
    deleteApproach: state.deleteApproach,
    updateApproach: state.updateApproach,
    deleteDetector: state.deleteDetector,
    addApproach: state.addApproach,
    addDetector: state.addDetector,
    errors: state.errors,
  }))
  const location = useLocationStore((s) => s.location)
  const options = useLocationEditorOptions(location?.locationTypeId)
  return (
    <ApproachesReconciliationView
      errors={errors}
      {...options}
      pedsAre1to1={location?.pedsAre1to1 ?? false}
      categories={categories}
      approaches={approaches}
      deleteApproach={deleteApproach}
      updateApproach={updateApproach}
      deleteDetector={deleteDetector}
      addApproach={addApproach}
      addDetector={addDetector}
    />
  )
}
