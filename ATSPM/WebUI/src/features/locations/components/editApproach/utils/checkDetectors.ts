import { ApproachForConfig } from '@/features/locations/components/editLocation/editLocationConfigHandler'

export const hasUniqueDetectorChannels = (
  approaches: ApproachForConfig[]
): {
  isValid: boolean
  errors: Record<string, { error: string; id: string }>
} => {
  const channelMap = new Map<string, string[]>() // Map channel to array of detector IDs
  const errors: Record<string, { error: string; id: string }> = {}

  for (const approach of approaches) {
    for (const detector of approach.detectors) {
      if (detector.detectorChannel) {
        if (!channelMap.has(detector.detectorChannel)) {
          channelMap.set(detector.detectorChannel, [detector.id])
        } else {
          channelMap.get(detector.detectorChannel)!.push(detector.id)
        }
      }
    }
  }

  // Populate errors for duplicate channels
  for (const [channel, ids] of channelMap.entries()) {
    if (ids.length > 1) {
      ids.forEach((id) => {
        errors[id] = { error: 'Detector channels must be unique', id }
      })
    }
  }

  return {
    isValid: Object.keys(errors).length === 0,
    errors,
  }
}
