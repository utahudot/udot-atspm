import '@testing-library/jest-dom'
import { act, fireEvent, render, screen } from '@testing-library/react'
import { DetectorReviewEditor } from './DetectorReviewEditor'
import { ConfigApproach, useLocationStore } from '../editLocation/locationStore'

jest.mock('../editLocation/useLocationEditorOptions', () => ({ useLocationEditorOptions: () => ({ detectionTypes: [] }) }))
beforeEach(() => {
  global.structuredClone = (value) => JSON.parse(JSON.stringify(value))
  const approaches = [{ id: 10, description: 'Northbound', detectors: [{ id: 1, approachId: 10, detectorChannel: 118, dectectorIdentifier: 'NB-18', laneType: 'V', movementType: 'T', detectionTypes: [] }] }] as unknown as ConfigApproach[]
  useLocationStore.setState({ approaches, savedApproaches: structuredClone(approaches), channelMap: new Map([[1, 118]]) })
})
it('stages removal, preserves the saved baseline, and restores it on reset', () => {
  const onAdded = jest.fn()
  render(<DetectorReviewEditor zone="NB-18" detectorId={1} allowRemove disabled={false} onAdded={onAdded} />)
  fireEvent.click(screen.getByRole('button', { name: 'Edit Channel 118' }))
  fireEvent.click(screen.getByRole('button', { name: 'Remove Channel 118 from draft' }))
  expect(useLocationStore.getState().approaches[0].detectors).toHaveLength(0)
  expect(useLocationStore.getState().savedApproaches[0].detectors).toHaveLength(1)
  expect(useLocationStore.getState().channelMap.has(1)).toBe(false)
  expect(useLocationStore.getState().hasUnsavedChanges()).toBe(true)
  act(() => useLocationStore.getState().resetApproaches())
  expect(useLocationStore.getState().approaches[0].detectors[0].detectorChannel).toBe(118)
  expect(useLocationStore.getState().channelMap.get(1)).toBe(118)
  expect(onAdded).toHaveBeenCalledTimes(1)
})
it('requires fresh review inputs before opening an editor', () => {
  render(<DetectorReviewEditor zone="NB-18" detectorId={1} allowRemove disabled onAdded={jest.fn()} />)
  expect(screen.getByRole('button', { name: 'Edit Channel 118' })).toBeDisabled()
})
