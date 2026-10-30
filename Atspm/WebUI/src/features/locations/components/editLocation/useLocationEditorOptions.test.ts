import { renderHook } from '@testing-library/react'
import { useGetDetectionType, useGetLocationType } from '@/api/config'
import { useLocationEditorOptions } from './useLocationEditorOptions'

jest.mock('@/api/config', () => ({ useGetDetectionType: jest.fn(), useGetLocationType: jest.fn() }))
it.each([false, true])('shares lookup options for OData envelope: %s', (envelope) => {
  const detectionTypes = [{ id: 4, abbreviation: 'LLC' }]
  const locationTypes = [{ id: 1, name: 'Intersection' }]
  ;(useGetDetectionType as jest.Mock).mockReturnValue({ data: envelope ? { value: detectionTypes } : detectionTypes })
  ;(useGetLocationType as jest.Mock).mockReturnValue({ data: envelope ? { value: locationTypes } : locationTypes })
  const { result } = renderHook(() => useLocationEditorOptions(1))
  expect(result.current).toEqual({ detectionTypes, locationTypeName: 'Intersection' })
})
