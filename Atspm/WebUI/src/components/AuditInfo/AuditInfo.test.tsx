import '@testing-library/jest-dom'
import { render, screen } from '@testing-library/react'

import { formatInstantAsLocalDate } from '@/utils/dateTime'
import AuditBadge from './AuditInfo'

describe('AuditBadge', () => {
  it('renders audit timestamps as local dates', () => {
    render(
      <AuditBadge
        obj={{
          created: '2026-05-20T20:30:00-06:00',
          createdBy: 'tester',
          modified: '2026-05-21T02:30:00+00:00',
          modifiedBy: 'tester',
        }}
      />
    )

    expect(
      screen.getByText(
        new RegExp(`Updated ${formatInstantAsLocalDate('2026-05-21T02:30:00+00:00')}`)
      )
    ).toBeInTheDocument()
  })
})
