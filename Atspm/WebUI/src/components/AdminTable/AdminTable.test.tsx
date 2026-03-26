import '@testing-library/jest-dom'
import { render, screen } from '@testing-library/react'

import AdminTable from './AdminTable'

jest.mock('@/stores/sidebar', () => ({
  __esModule: true,
  useSidebarStore: () => ({
    isSidebarOpen: false,
  }),
}))

type TestRow = {
  id: number
  name: string
  created: string
  createdBy: string
  modified: string
  modifiedBy: string
}

const cells = [{ key: 'name', label: 'Name' }]

const data: TestRow[] = [
  {
    id: 1,
    name: 'Alpha',
    created: '2026-03-01T00:00:00',
    createdBy: 'tester',
    modified: '2026-03-02T00:00:00',
    modifiedBy: 'tester',
  },
]

describe('AdminTable', () => {
  it('does not render an empty toolbar when there is no create modal', () => {
    const { container } = render(
      <AdminTable<TestRow> cells={cells} data={data} pageName="Thing" />
    )
    const root = container.firstChild as HTMLElement

    expect(
      screen.queryByRole('button', { name: 'New Thing' })
    ).not.toBeInTheDocument()
    expect(container.querySelector('.MuiToolbar-root')).not.toBeInTheDocument()
    expect(window.getComputedStyle(root).marginTop).toBe('24px')
  })

  it('renders the toolbar when a create modal is provided', () => {
    const { container } = render(
      <AdminTable<TestRow>
        cells={cells}
        data={data}
        pageName="Thing"
        createModal={<div>Create Modal</div>}
      />
    )
    const root = container.firstChild as HTMLElement

    expect(screen.getByRole('button', { name: 'New Thing' })).toBeInTheDocument()
    expect(container.querySelector('.MuiToolbar-root')).toBeInTheDocument()
    expect(window.getComputedStyle(root).marginTop).toBe('-32px')
  })

  it('keeps the actions column sticky while the table container scrolls', () => {
    const { container } = render(
      <AdminTable<TestRow>
        cells={cells}
        data={data}
        pageName="Thing"
        hasEditPrivileges
      />
    )

    const tableContainer = container.querySelector(
      '.MuiTableContainer-root'
    ) as HTMLElement
    const actionsHeader = screen.getByRole('columnheader', {
      name: 'Actions',
    })
    const actionsButton = screen.getByRole('button', { name: 'more' })
    const actionsCell = actionsButton.closest('td') as HTMLElement

    expect(window.getComputedStyle(tableContainer).overflowX).toBe('auto')
    expect(window.getComputedStyle(actionsHeader).position).toBe('sticky')
    expect(window.getComputedStyle(actionsHeader).right).toBe('0px')
    expect(window.getComputedStyle(actionsCell).position).toBe('sticky')
    expect(window.getComputedStyle(actionsCell).right).toBe('0px')
  })
})
