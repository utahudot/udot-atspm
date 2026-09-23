import { render } from '@testing-library/react'
import { Markup } from 'interweave'
import { stripTableWhitespace } from './stripTableWhitespace'

const formattedTable = `<table>
  <tbody>
    <tr>
      <td>A</td>
      <td>B</td>
    </tr>
  </tbody>
</table>`

describe('stripTableWhitespace', () => {
  it('removes whitespace between table structure tags', () => {
    expect(stripTableWhitespace(formattedTable)).toBe(
      '<table><tbody><tr><td>A</td><td>B</td></tr></tbody></table>'
    )
  })

  it('keeps whitespace inside cells and outside tables', () => {
    const html = '<p>Intro </p> <table><tr><td> x <b>y</b> </td></tr></table>'
    expect(stripTableWhitespace(html)).toBe(html)
  })

  it('lets interweave render formatted tables without DOM nesting warnings', () => {
    const consoleError = jest.spyOn(console, 'error').mockImplementation()
    try {
      render(<Markup content={formattedTable} />)
      expect(consoleError).toHaveBeenCalled()

      consoleError.mockClear()
      const { container } = render(
        <Markup content={stripTableWhitespace(formattedTable)} />
      )
      expect(consoleError).not.toHaveBeenCalled()
      expect(container.querySelectorAll('td')).toHaveLength(2)
    } finally {
      consoleError.mockRestore()
    }
  })
})
