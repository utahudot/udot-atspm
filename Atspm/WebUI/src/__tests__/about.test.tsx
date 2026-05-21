import type { GitHubReleaseDto } from '@/api/config'
import {
  useGetVersionCurrentVersion,
  useGetVersionLatestVersionFromPreRelease,
} from '@/api/config'
import About from '@/pages/about'
import '@testing-library/jest-dom'
import { render, screen } from '@testing-library/react'

jest.mock('@/api/config', () => ({
  __esModule: true,
  useGetVersionCurrentVersion: jest.fn(),
  useGetVersionLatestVersionFromPreRelease: jest.fn(),
}))

const mockUseGetVersionCurrentVersion =
  useGetVersionCurrentVersion as jest.Mock
const mockUseGetVersionLatestVersionFromPreRelease =
  useGetVersionLatestVersionFromPreRelease as jest.Mock

const release = (
  tagName: string,
  htmlUrl = `https://github.com/OpenSourceTransportation/Atspm/releases/tag/${tagName}`
): GitHubReleaseDto => ({
  tagName,
  htmlUrl,
})

const mockVersionHooks = ({
  currentVersion,
  latestVersion,
  isCurrentVersionError = false,
  isLatestVersionError = false,
  isCurrentVersionLoading = false,
  isLatestVersionLoading = false,
}: {
  currentVersion?: GitHubReleaseDto
  latestVersion?: GitHubReleaseDto
  isCurrentVersionError?: boolean
  isLatestVersionError?: boolean
  isCurrentVersionLoading?: boolean
  isLatestVersionLoading?: boolean
}) => {
  mockUseGetVersionCurrentVersion.mockReturnValue({
    data: currentVersion,
    isError: isCurrentVersionError,
    isLoading: isCurrentVersionLoading,
  })
  mockUseGetVersionLatestVersionFromPreRelease.mockReturnValue({
    data: latestVersion,
    isError: isLatestVersionError,
    isLoading: isLatestVersionLoading,
  })
}

describe('About page', () => {
  beforeEach(() => {
    jest.clearAllMocks()
  })

  it('shows an update prompt when the latest release is newer', () => {
    mockVersionHooks({
      currentVersion: release('v5.2.0'),
      latestVersion: release('v5.2.1'),
    })

    render(<About />)

    expect(
      screen.getByRole('heading', { name: 'About ATSPM' })
    ).toBeInTheDocument()
    expect(screen.getByText('Update available')).toBeInTheDocument()
    expect(screen.getByText('v5.2.0')).toBeInTheDocument()
    expect(screen.getByText('v5.2.1')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /view release/i })).toHaveAttribute(
      'href',
      'https://github.com/OpenSourceTransportation/Atspm/releases/tag/v5.2.1'
    )
    expect(mockUseGetVersionLatestVersionFromPreRelease).toHaveBeenCalledWith(
      false,
      undefined,
      expect.objectContaining({
        query: expect.objectContaining({ enabled: true }),
      })
    )
  })

  it('treats matching versions as up to date after normalizing the tag prefix', () => {
    mockVersionHooks({
      currentVersion: release('5.2.1'),
      latestVersion: release('v5.2.1'),
    })

    render(<About />)

    expect(screen.getByText('ATSPM is up to date')).toBeInTheDocument()
    expect(
      screen.queryByRole('link', { name: /view release/i })
    ).not.toBeInTheDocument()
  })

  it('shows an unavailable state when a version endpoint fails', () => {
    mockVersionHooks({
      currentVersion: undefined,
      latestVersion: release('v5.2.1'),
      isCurrentVersionError: true,
    })

    render(<About />)

    expect(screen.getByText('Version status unavailable')).toBeInTheDocument()
    expect(screen.getByText('Unavailable')).toBeInTheDocument()
    expect(
      screen.queryByText('ATSPM is up to date')
    ).not.toBeInTheDocument()
  })
})
