import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFnsV3'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import '@testing-library/jest-dom'
import { fireEvent, render, screen } from '@testing-library/react'
import MultiDayCalendar from './MultiDayCalendar'

describe('MultiDayCalendar', () => {
  it('marks dates with partial or no location data', () => {
    render(
      <LocalizationProvider dateAdapter={AdapterDateFns}>
        <MultiDayCalendar
          selectedDays={[new Date(2026, 4, 10)]}
          onSelectedDaysChange={jest.fn()}
          dayAvailability={[
            {
              date: new Date(2026, 4, 11),
              availableLocationCount: 1,
              totalLocationCount: 2,
              locations: [
                { locationIdentifier: '1001', hasData: true },
                { locationIdentifier: '1002', hasData: false },
              ],
            },
            {
              date: new Date(2026, 4, 12),
              availableLocationCount: 0,
              totalLocationCount: 2,
              locations: [
                { locationIdentifier: '1001', hasData: false },
                { locationIdentifier: '1002', hasData: false },
              ],
            },
          ]}
        />
      </LocalizationProvider>
    )

    expect(
      screen.getByRole('img', { name: '1 location missing data' })
    ).toHaveTextContent('◐')
    expect(
      screen.getByRole('img', { name: 'No data available' })
    ).toHaveTextContent('✖')
  })

  it('reports the displayed month when navigating the calendar', () => {
    const handleMonthChange = jest.fn()

    render(
      <LocalizationProvider dateAdapter={AdapterDateFns}>
        <MultiDayCalendar
          selectedDays={[new Date(2026, 4, 10)]}
          onSelectedDaysChange={jest.fn()}
          onMonthChange={handleMonthChange}
        />
      </LocalizationProvider>
    )

    fireEvent.click(screen.getByLabelText('Next month'))

    expect(handleMonthChange).toHaveBeenCalledTimes(1)
    expect(handleMonthChange.mock.calls[0][0].getMonth()).toBe(5)
  })
})
