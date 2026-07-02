import {
  useApiKeys,
  useCreateApiKey,
  useIdentityClaims,
  useRevokeApiKey,
} from '@/features/identity/api/apiKeys'
import ApiKeyManagement from '@/features/identity/components/apiKeys/ApiKeyManagement'
import '@testing-library/jest-dom'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import Cookies from 'js-cookie'

jest.mock('@/features/identity/api/apiKeys', () => ({
  __esModule: true,
  useApiKeys: jest.fn(),
  useCreateApiKey: jest.fn(),
  useIdentityClaims: jest.fn(),
  useRevokeApiKey: jest.fn(),
}))

jest.mock('js-cookie', () => ({
  __esModule: true,
  default: {
    get: jest.fn(),
  },
}))

const mockAddNotification = jest.fn()

jest.mock('@/stores/notifications', () => ({
  useNotificationStore: () => ({
    addNotification: mockAddNotification,
  }),
}))

const mockUseApiKeys = useApiKeys as jest.Mock
const mockUseIdentityClaims = useIdentityClaims as jest.Mock
const mockUseCreateApiKey = useCreateApiKey as jest.Mock
const mockUseRevokeApiKey = useRevokeApiKey as jest.Mock
const mockCookiesGet = Cookies.get as jest.Mock
const mockCreateMutateAsync = jest.fn()

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

describe('ApiKeyManagement', () => {
  beforeEach(() => {
    jest.clearAllMocks()
    mockCookiesGet.mockReturnValue('Admin')
    mockUseApiKeys.mockReturnValue({
      data: apiKeys,
      isLoading: false,
      refetch: jest.fn(),
    })
    mockUseIdentityClaims.mockReturnValue({
      data: [
        'Admin',
        'ApiKey:Create',
        'ApiKey:View',
        'ApiKey:Revoke',
        'Data:View',
      ],
      isLoading: false,
    })
    mockUseCreateApiKey.mockReturnValue({
      mutateAsync: mockCreateMutateAsync,
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

  it('hides revoked API keys by default and toggles them for global admins', async () => {
    const user = userEvent.setup()

    render(<ApiKeyManagement />)

    expect(mockUseApiKeys).toHaveBeenCalledWith({
      enabled: true,
      scope: 'mine',
    })
    expect(mockUseIdentityClaims).toHaveBeenCalledWith(true)
    expect(screen.getByRole('list', { name: /api keys/i })).toBeInTheDocument()
    expect(screen.queryByRole('table')).not.toBeInTheDocument()
    expect(screen.getByText('Active key')).toBeInTheDocument()
    expect(screen.queryByText('Revoked key')).not.toBeInTheDocument()
    expect(
      screen.queryByText('User Two (two@example.com)')
    ).not.toBeInTheDocument()
    expect(screen.getByText('Active')).toBeInTheDocument()
    expect(screen.queryByText('Revoked')).not.toBeInTheDocument()

    const revokeButtons = screen.getAllByRole('button', { name: /^revoke$/i })
    expect(revokeButtons).toHaveLength(1)
    expect(revokeButtons[0]).toBeEnabled()

    await user.click(screen.getByRole('button', { name: /show revoked keys/i }))

    expect(screen.getByText('Revoked key')).toBeInTheDocument()
    expect(screen.getByText('User Two (two@example.com)')).toBeInTheDocument()
    expect(screen.getByText('Revoked')).toBeInTheDocument()
    expect(
      screen.getByRole('button', { name: /hide revoked keys/i })
    ).toBeInTheDocument()

    const visibleRevokeButtons = screen.getAllByRole('button', {
      name: /^revoke$/i,
    })
    expect(visibleRevokeButtons).toHaveLength(2)
    expect(visibleRevokeButtons[0]).toBeEnabled()
    expect(visibleRevokeButtons[1]).toBeDisabled()
  })

  it('filters API key management claims from the create dialog', async () => {
    const user = userEvent.setup()
    render(<ApiKeyManagement />)

    await user.click(screen.getByRole('button', { name: /new api key/i }))

    expect(screen.getByLabelText('Data:View')).toBeInTheDocument()
    expect(screen.queryByLabelText('Admin')).not.toBeInTheDocument()
    expect(screen.queryByLabelText('ApiKey:Create')).not.toBeInTheDocument()
    expect(screen.queryByLabelText('ApiKey:View')).not.toBeInTheDocument()
    expect(screen.queryByLabelText('ApiKey:Revoke')).not.toBeInTheDocument()
  })

  it('shows the one-time raw key after creation', async () => {
    mockCreateMutateAsync.mockResolvedValue({
      key: 'raw-key-value',
      message: 'Copy this key now.',
    })
    const user = userEvent.setup()

    render(<ApiKeyManagement />)

    await user.click(screen.getByRole('button', { name: /new api key/i }))
    await user.type(
      await screen.findByRole('textbox', { name: /name/i }),
      'Integration key'
    )
    await user.click(screen.getByLabelText('Data:View'))
    await user.click(screen.getByRole('button', { name: /^create$/i }))

    await waitFor(() => {
      expect(mockCreateMutateAsync).toHaveBeenCalledWith({
        name: 'Integration key',
        expiresAt: null,
        claims: ['Data:View'],
      })
    })
    expect(screen.getByDisplayValue('raw-key-value')).toBeInTheDocument()
    expect(
      screen.getByText('Copy this key now. It cannot be retrieved again.')
    ).toBeInTheDocument()
  })

  it('does not query keys when the user lacks API key permissions', () => {
    mockCookiesGet.mockReturnValue('Data:View')

    render(<ApiKeyManagement />)

    expect(mockUseApiKeys).toHaveBeenCalledWith({
      enabled: false,
      scope: 'mine',
    })
    expect(screen.getByText('API Keys')).toBeInTheDocument()
    expect(
      screen.getByText('API key management is not enabled for your account.')
    ).toBeInTheDocument()
  })

  it('uses the all-keys scope in admin mode and keeps the normal key-management UI', async () => {
    const user = userEvent.setup()
    mockUseApiKeys.mockReturnValue({
      data: [
        {
          id: 10,
          name: 'System integration',
          ownerId: 'user-2',
          createdAt: '2026-06-18T12:00:00Z',
          expiresAt: null,
          isRevoked: false,
          claims: [],
        },
        {
          id: 11,
          name: 'Revoked integration',
          ownerId: 'user-3',
          createdAt: '2026-06-17T12:00:00Z',
          expiresAt: null,
          isRevoked: true,
          claims: [],
        },
      ],
      isLoading: false,
      refetch: jest.fn(),
    })

    render(<ApiKeyManagement mode="admin" />)

    expect(mockUseApiKeys).toHaveBeenCalledWith({
      enabled: true,
      scope: 'all',
    })
    expect(mockUseIdentityClaims).toHaveBeenCalledWith(true)
    expect(screen.getByText('System integration')).toBeInTheDocument()
    expect(screen.getByText('user-2')).toBeInTheDocument()
    expect(screen.queryByText('Revoked integration')).not.toBeInTheDocument()
    expect(
      screen.getByText('Create and revoke keys for API integrations.')
    ).toBeInTheDocument()
    expect(
      screen.getByRole('button', { name: /new api key/i })
    ).toBeInTheDocument()
    expect(
      screen.queryByRole('link', { name: /view all api keys/i })
    ).not.toBeInTheDocument()
    expect(screen.getByRole('button', { name: /^revoke$/i })).toBeEnabled()
    expect(screen.getByText('Claims')).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: /show revoked keys/i }))
    expect(screen.getByText('Revoked integration')).toBeInTheDocument()
    expect(screen.getByText('user-3')).toBeInTheDocument()

    const visibleRevokeButtons = screen.getAllByRole('button', {
      name: /^revoke$/i,
    })
    expect(visibleRevokeButtons).toHaveLength(2)
    expect(visibleRevokeButtons[0]).toBeEnabled()
    expect(visibleRevokeButtons[1]).toBeDisabled()
  })
})
