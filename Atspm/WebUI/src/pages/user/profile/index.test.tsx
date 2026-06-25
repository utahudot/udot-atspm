import {
  useApiKeys,
  useCreateApiKey,
  useIdentityClaims,
  useRevokeApiKey,
} from '@/features/identity/api/apiKeys'
import { useEditUserInfo } from '@/features/identity/api/editUserInfo'
import { useUserInfo } from '@/features/identity/api/getUserInfo'
import ProfilePage from '@/pages/user/profile'
import '@testing-library/jest-dom'
import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import Cookies from 'js-cookie'

jest.mock('@/features/identity/api/apiKeys', () => ({
  __esModule: true,
  useApiKeys: jest.fn(),
  useCreateApiKey: jest.fn(),
  useIdentityClaims: jest.fn(),
  useRevokeApiKey: jest.fn(),
}))

jest.mock('@/features/identity/api/editUserInfo', () => ({
  __esModule: true,
  useEditUserInfo: jest.fn(),
}))

jest.mock('@/features/identity/api/getUserInfo', () => ({
  __esModule: true,
  useUserInfo: jest.fn(),
}))

jest.mock('@/stores/notifications', () => ({
  useNotificationStore: () => ({
    addNotification: jest.fn(),
  }),
}))

jest.mock('@/stores/sidebar', () => ({
  useSidebarStore: () => ({
    toggleRightSidebar: jest.fn(),
  }),
}))

jest.mock('js-cookie', () => ({
  __esModule: true,
  default: {
    get: jest.fn(),
  },
}))

const mockUseApiKeys = useApiKeys as jest.Mock
const mockUseIdentityClaims = useIdentityClaims as jest.Mock
const mockUseCreateApiKey = useCreateApiKey as jest.Mock
const mockUseRevokeApiKey = useRevokeApiKey as jest.Mock
const mockUseEditUserInfo = useEditUserInfo as jest.Mock
const mockUseUserInfo = useUserInfo as jest.Mock
const mockCookiesGet = Cookies.get as jest.Mock

const apiKeys = [
  {
    id: 1,
    name: 'Active key',
    ownerId: 'user-1',
    ownerEmail: 'one@example.com',
    ownerName: 'User One',
    createdAt: '2026-06-16T12:00:00Z',
    expiresAt: null,
    isRevoked: false,
    claims: ['Data:View'],
  },
  {
    id: 2,
    name: 'Revoked key',
    ownerId: 'user-2',
    ownerEmail: 'two@example.com',
    ownerName: 'User Two',
    createdAt: '2026-06-15T12:00:00Z',
    expiresAt: null,
    isRevoked: true,
    claims: ['Report:View'],
  },
]

describe('ProfilePage', () => {
  beforeEach(() => {
    jest.clearAllMocks()
    mockCookiesGet.mockReturnValue('Admin,ApiKey:View,ApiKey:Create')
    mockUseUserInfo.mockReturnValue({
      data: {
        firstName: 'Admin',
        lastName: 'Admin',
        agency: 'Admin',
        email: 'admin@atspm.com',
        roles: ['Admin', 'WatchdogSubscriber'],
      },
    })
    mockUseEditUserInfo.mockReturnValue({
      mutate: jest.fn(),
    })
    mockUseApiKeys.mockImplementation((enabled: boolean) => ({
      data: enabled ? apiKeys : [],
      isLoading: false,
      refetch: jest.fn(),
    }))
    mockUseIdentityClaims.mockReturnValue({
      data: ['Admin', 'ApiKey:Create', 'ApiKey:Revoke', 'Data:View'],
      isLoading: false,
    })
    mockUseCreateApiKey.mockReturnValue({
      mutateAsync: jest.fn(),
      isLoading: false,
    })
    mockUseRevokeApiKey.mockReturnValue({
      mutateAsync: jest.fn(),
      isLoading: false,
    })
  })

  afterEach(() => {
    cleanup()
  })

  it('separates account, security, and API key management into tabs', async () => {
    const user = userEvent.setup()

    render(<ProfilePage />)

    expect(screen.getByRole('tab', { name: /account/i })).toBeInTheDocument()
    expect(screen.getByRole('tab', { name: /security/i })).toBeInTheDocument()
    expect(screen.getByRole('tab', { name: /api keys/i })).toBeInTheDocument()
    expect(screen.getByText('Your Information')).toBeInTheDocument()
    expect(screen.getByText('Admin')).toBeInTheDocument()
    expect(screen.getByText('WatchdogSubscriber')).toBeInTheDocument()
    expect(screen.queryByText('Active key')).not.toBeInTheDocument()

    await user.click(screen.getByRole('tab', { name: /security/i }))
    expect(
      screen.getByRole('button', { name: /update password/i })
    ).toBeInTheDocument()

    await user.click(screen.getByRole('tab', { name: /api keys/i }))
    expect(screen.getByRole('list', { name: /api keys/i })).toBeInTheDocument()
    expect(screen.getByText('Active key')).toBeInTheDocument()
    expect(screen.queryByText('Revoked key')).not.toBeInTheDocument()
    expect(screen.queryByRole('table')).not.toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: /show revoked keys/i }))
    expect(screen.getByText('Revoked key')).toBeInTheDocument()
  })
})
