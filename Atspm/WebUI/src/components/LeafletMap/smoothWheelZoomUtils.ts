export const NOISY_LINE_EVENT_INTERVAL_MS = 40
export const NOISY_LINE_BURST_GRACE_MS = 600

type LineWheelInput = {
  direction: number
  elapsedMs: number
  noisyLineBurst: boolean
  previousDirection: number
}

export const classifyLineWheelInput = ({
  direction,
  elapsedMs,
  noisyLineBurst,
  previousDirection,
}: LineWheelInput) => {
  const sameDirection =
    direction !== 0 && direction === previousDirection
  const isRapidFollowup =
    sameDirection && elapsedMs <= NOISY_LINE_EVENT_INTERVAL_MS
  const continuesNoisyBurst =
    sameDirection &&
    noisyLineBurst &&
    elapsedMs <= NOISY_LINE_BURST_GRACE_MS

  return {
    isTailEvent: isRapidFollowup || continuesNoisyBurst,
    noisyLineBurst:
      sameDirection && (noisyLineBurst || isRapidFollowup),
  }
}

export const clampZoom = (zoom: number, minZoom: number, maxZoom: number) =>
  Math.max(minZoom, Math.min(maxZoom, zoom))

export const getEasedZoom = (
  currentZoom: number,
  goalZoom: number,
  easing = 0.3
) => currentZoom + (goalZoom - currentZoom) * easing
