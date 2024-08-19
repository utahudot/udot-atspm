import '@testing-library/jest-dom'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import AddButton from './AddButton' // Adjust the import path as necessary

describe('AddButton Component', () => {
  it('renders the button with the correct label and handles click', async () => {
    const label = 'Add Item'
    const onClick = jest.fn()

    render(<AddButton label={label} onClick={onClick} />)

    const button = screen.getByRole('button', { name: label })
    expect(button).toBeInTheDocument()

    await userEvent.click(button)

    expect(onClick).toHaveBeenCalledTimes(1)
  })
})
