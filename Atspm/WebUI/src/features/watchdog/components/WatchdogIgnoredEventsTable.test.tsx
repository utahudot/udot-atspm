import '@testing-library/jest-dom'
import { render, screen } from '@testing-library/react'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFnsV3'

import WatchdogIgnoredEventsTable from './WatchdogIgnoredEventsTable'

const adminTableMock = jest.fn(() => <div>Admin Table</div>)

jest.mock('@/components/AdminTable/AdminTable', () => ({
  __esModule: true,
  default: (props: unknown) => adminTableMock(props),
}))

jest.mock('@/components/AdminTable/DeleteModal', () => ({
  __esModule: true,
  default: () => <div>Delete Modal</div>,
}))

jest.mock('@/features/watchdog/api/watchdogIgnoreEvents', () => ({
  __esModule: true,
  useGetWatchdogIgnoreEvents: () => ({
    data: {
      value: [
        {
          id: 1,
          locationId: 100,
          locationIdentifier: '001',
          start: '2026-03-01T00:00:00',
          end: '2026-03-31T00:00:00',
          issueType: 'RecordCount',
          componentType: 'Location',
          componentId: 100,
          phase: 2,
          created: '2026-03-01T00:00:00',
          modified: '2026-03-02T00:00:00',
          createdBy: 'tester',
          modifiedBy: 'tester',
        },
      ],
    },
    isLoading: false,
    refetch: jest.fn(),
  }),
  useEditWatchdogIgnoreEvents: () => ({
    mutateAsync: jest.fn(),
  }),
  useDeleteWatchdogIgnoreEvents: () => ({
    mutateAsync: jest.fn(),
  }),
}))

jest.mock('@/stores/notifications', () => ({
  __esModule: true,
  useNotificationStore: () => ({
    addNotification: jest.fn(),
  }),
}))

describe('WatchdogIgnoredEventsTable', () => {
  beforeEach(() => {
    adminTableMock.mockClear()
  })

  it('passes an edit modal into AdminTable', () => {
    render(
      <LocalizationProvider dateAdapter={AdapterDateFns}>
        <WatchdogIgnoredEventsTable />
      </LocalizationProvider>
    )

    expect(screen.getByText('Admin Table')).toBeInTheDocument()
    expect(adminTableMock).toHaveBeenCalledWith(
      expect.objectContaining({
        marginTop: 0,
        editModal: expect.any(Object),
      })
    )
  })
})
