import CommentCell from '../Cell/CommentCell'
import { useLocationStore } from '../editLocation/locationStore'
import { useLocationEditorOptions } from '../editLocation/useLocationEditorOptions'
import { DetectorEditor, EditDetectorsProps } from './DetectorEditor'
export { AuditIcon } from './DetectorEditor'

export default function EditDetectors(props: EditDetectorsProps) {
  const { updateDetector, errors, setErrors, warnings, location, channelMap } =
    useLocationStore((state) => ({
      updateDetector: state.updateDetector,
      errors: state.errors,
      setErrors: state.setErrors,
      warnings: state.warnings,
      location: state.location,
      channelMap: state.channelMap,
    }))
  const options = useLocationEditorOptions(location?.locationTypeId)
  return (
    <DetectorEditor
      {...props}
      {...{ updateDetector, errors, setErrors, warnings, channelMap }}
      {...options}
      renderComment={(detector, row, rowCount, colCount) => (
        <CommentCell
          key={detector.id}
          approachId={props.approach.id}
          detector={detector}
          row={row}
          col={9}
          rowCount={rowCount}
          colCount={colCount}
        />
      )}
    />
  )
}
