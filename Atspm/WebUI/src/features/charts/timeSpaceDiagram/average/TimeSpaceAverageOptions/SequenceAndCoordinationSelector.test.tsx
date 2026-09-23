import '@testing-library/jest-dom'
import { fireEvent, render, screen } from '@testing-library/react'
import { useState } from 'react'
import type { TimeSpaceRouteRow } from '@/features/charts/timeSpaceDiagram/shared/components/TimeSpaceRouteSelect/TimeSpaceRouteSelect'
import SequenceAndCoordinationComponent from './SequenceAndCoordinationSelector'

type SequenceItem = {
  locationIdentifier: string
  sequence: number[][]
}

type CoordPhaseItem = {
  locationIdentifier: string
  coordinatedPhases: number[]
}

type RequestShape = {
  sequence: SequenceItem[]
  coordinatedPhases: CoordPhaseItem[]
}

const routeRows: TimeSpaceRouteRow[] = [
  {
    locationIdentifier: '7192',
    primaryPhaseDescription: 'NBT Ph2',
    primaryMph: 45,
    opposingPhaseDescription: 'SBT Ph6',
    opposingMph: 45,
    distance: 4723,
    order: 1,
  },
]

const upsertSequence = (
  items: SequenceItem[],
  nextItem: SequenceItem
): SequenceItem[] =>
  items.some((item) => item.locationIdentifier === nextItem.locationIdentifier)
    ? items.map((item) =>
        item.locationIdentifier === nextItem.locationIdentifier
          ? nextItem
          : item
      )
    : [...items, nextItem]

const upsertCoordPhase = (
  items: CoordPhaseItem[],
  nextItem: CoordPhaseItem
): CoordPhaseItem[] =>
  items.some((item) => item.locationIdentifier === nextItem.locationIdentifier)
    ? items.map((item) =>
        item.locationIdentifier === nextItem.locationIdentifier
          ? nextItem
          : item
      )
    : [...items, nextItem]

function SelectorHarness() {
  const [sequence, setSequence] = useState<SequenceItem[]>([
    {
      locationIdentifier: '7192',
      sequence: [
        [1, 2, 3, 4],
        [5, 6, 7, 8],
      ],
    },
  ])
  const [coordinatedPhases, setCoordinatedPhases] = useState<CoordPhaseItem[]>([
    {
      locationIdentifier: '7192',
      coordinatedPhases: [2, 6],
    },
  ])

  return (
    <>
      <SequenceAndCoordinationComponent
        routeRows={routeRows}
        locationWithSequence={sequence}
        updateLocationWithSequence={(nextItem) =>
          setSequence((current) => upsertSequence(current, nextItem))
        }
        locationWithCoordPhases={coordinatedPhases}
        updateLocationWithCoordPhases={(nextItem) =>
          setCoordinatedPhases((current) =>
            upsertCoordPhase(current, nextItem)
          )
        }
      />
      <pre data-testid="request-shape">
        {JSON.stringify({ sequence, coordinatedPhases })}
      </pre>
    </>
  )
}

function readRequestShape(): RequestShape {
  return JSON.parse(screen.getByTestId('request-shape').textContent ?? '{}')
}

function openSelect(index: number) {
  fireEvent.mouseDown(screen.getAllByRole('combobox')[index])
}

function chooseOption(name: RegExp) {
  fireEvent.click(screen.getByRole('option', { name }))
}

describe('SequenceAndCoordinationComponent', () => {
  it('writes custom rings and coordinated phases into the request shape', () => {
    render(<SelectorHarness />)

    openSelect(0)
    chooseOption(/custom/i)

    fireEvent.change(screen.getByLabelText('Ring 1'), {
      target: { value: '2,1,3,4' },
    })
    fireEvent.change(screen.getByLabelText('Ring 2'), {
      target: { value: '6,5,8,7' },
    })

    openSelect(1)
    chooseOption(/manual/i)

    fireEvent.change(screen.getByLabelText('Phases'), {
      target: { value: '1,5' },
    })

    expect(readRequestShape()).toEqual({
      sequence: [
        {
          locationIdentifier: '7192',
          sequence: [
            [2, 1, 3, 4],
            [6, 5, 8, 7],
          ],
        },
      ],
      coordinatedPhases: [
        {
          locationIdentifier: '7192',
          coordinatedPhases: [1, 5],
        },
      ],
    })
  })

  it('preserves comma input while parsing the numeric request shape', () => {
    render(<SelectorHarness />)

    openSelect(0)
    chooseOption(/custom/i)

    fireEvent.change(screen.getByLabelText('Ring 1'), {
      target: { value: '2,' },
    })

    expect(screen.getByLabelText('Ring 1')).toHaveValue('2,')
    expect(readRequestShape().sequence).toEqual([
      {
        locationIdentifier: '7192',
        sequence: [[2], [5, 6, 7, 8]],
      },
    ])
  })
})
