import {
  classifyLineWheelInput,
  clampZoom,
  getEasedZoom,
} from './smoothWheelZoomUtils'

describe('smooth wheel zoom utilities', () => {
  describe('classifyLineWheelInput', () => {
    it('keeps rapid intentional notches at full weight', () => {
      const result = classifyLineWheelInput({
        direction: 1,
        elapsedMs: 100,
        noisyLineBurst: false,
        previousDirection: 1,
      })

      expect(result).toEqual({
        isTailEvent: false,
        noisyLineBurst: false,
      })
    })

    it('classifies high-frequency follow-up events as a noisy tail', () => {
      const result = classifyLineWheelInput({
        direction: 1,
        elapsedMs: 17,
        noisyLineBurst: false,
        previousDirection: 1,
      })

      expect(result).toEqual({
        isTailEvent: true,
        noisyLineBurst: true,
      })
    })

    it('keeps an established noisy tail damped across a short pause', () => {
      const result = classifyLineWheelInput({
        direction: 1,
        elapsedMs: 300,
        noisyLineBurst: true,
        previousDirection: 1,
      })

      expect(result).toEqual({
        isTailEvent: true,
        noisyLineBurst: true,
      })
    })

    it('ends the noisy classification when direction changes', () => {
      const result = classifyLineWheelInput({
        direction: -1,
        elapsedMs: 17,
        noisyLineBurst: true,
        previousDirection: 1,
      })

      expect(result).toEqual({
        isTailEvent: false,
        noisyLineBurst: false,
      })
    })
  })

  it('clamps without snapping a fractional zoom', () => {
    expect(clampZoom(10.3, 0, 18)).toBe(10.3)
  })

  it('eases equally in both zoom directions', () => {
    const zoomInChange = getEasedZoom(8, 8.0003) - 8
    const zoomOutChange = 8 - getEasedZoom(8, 7.9997)

    expect(zoomInChange).toBeCloseTo(zoomOutChange, 10)
  })
})
