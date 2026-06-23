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

  it('shows all API key list items with owner and revoked status for global admins', () => {
    render(<ApiKeyManagement />)

    expect(mockUseApiKeys).toHaveBeenCalledWith(true)
    expect(mockUseIdentityClaims).toHaveBeenCalledWith(true)
    expect(screen.getByRole('list', { name: /api keys/i })).toBeInTheDocument()
    expect(screen.queryByRole('table')).not.toBeInTheDocument()
    expect(screen.getByText('Active key')).toBeInTheDocument()
    expect(screen.getByText('Revoked key')).toBeInTheDocument()
    expect(screen.getByText('User Two (two@example.com)')).toBeInTheDocument()
    expect(screen.getByText('Active')).toBeInTheDocument()
    expect(screen.getByText('Revoked')).toBeInTheDocument()

    const revokeButtons = screen.getAllByRole('button', { name: /revoke/i })
    expect(revokeButtons[0]).toBeEnabled()
    expect(revokeButtons[1]).toBeDisabled()
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

    expect(mockUseApiKeys).toHaveBeenCalledWith(false)
    expect(screen.getByText('API Keys')).toBeInTheDocument()
    expect(
      screen.getByText('API key management is not enabled for your account.')
    ).toBeInTheDocument()
  })
})
