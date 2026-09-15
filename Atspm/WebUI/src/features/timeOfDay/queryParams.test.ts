import {
  getLocationLocationsForSearch,
  type SearchLocation,
} from '@/api/config'
import { resolveSearchLocationsByIdentifier } from './queryParams'

jest.mock('@/api/config', () => ({
  getLocationLocationsForSearch: jest.fn(),
}))
jest.mock('nuqs', () => ({
  createParser: (parser: unknown) => parser,
}))

const getLocationsForSearchMock = jest.mocked(getLocationLocationsForSearch)

describe('resolveSearchLocationsByIdentifier', () => {
  beforeEach(() => {
    getLocationsForSearchMock.mockReset()
  })

  it('hydrates URL location identifiers from an OData response', async () => {
    const locations: SearchLocation[] = [
      {
        id: 1005,
        locationIdentifier: '1005',
        primaryName: 'Main Street',
        secondaryName: '100 South',
      },
      {
        id: 7418,
        locationIdentifier: '7418',
        primaryName: 'State Street',
        secondaryName: '200 North',
      },
    ]
    getLocationsForSearchMock.mockResolvedValue({
      value: locations,
    } as unknown as SearchLocation[])

    await expect(
      resolveSearchLocationsByIdentifier(['1005', '7418'])
    ).resolves.toEqual(locations)
    expect(getLocationsForSearchMock).toHaveBeenCalledWith({
      filter: `locationIdentifier eq '1005' or locationIdentifier eq '7418'`,
    })
  })

  it('keeps URL order and falls back for an identifier not returned by the API', async () => {
    getLocationsForSearchMock.mockResolvedValue([
      {
        id: 7418,
        locationIdentifier: '7418',
        primaryName: 'State Street',
        secondaryName: '200 North',
      },
    ])

    await expect(
      resolveSearchLocationsByIdentifier(['1005', '7418'])
    ).resolves.toEqual([
      { locationIdentifier: '1005' },
      expect.objectContaining({
        locationIdentifier: '7418',
        primaryName: 'State Street',
        secondaryName: '200 North',
      }),
    ])
  })
})
