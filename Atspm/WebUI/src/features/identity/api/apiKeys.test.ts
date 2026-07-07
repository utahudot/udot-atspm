import { normalizeApiKey } from '@/features/identity/api/apiKeys'

describe('normalizeApiKey', () => {
  it('defaults fields omitted by the all-keys endpoint', () => {
    expect(
      normalizeApiKey({
        id: 12,
        name: 'System integration',
        ownerId: 'user-2',
        createdAt: '2026-06-18T12:00:00Z',
        expiresAt: null,
      })
    ).toEqual({
      id: 12,
      name: 'System integration',
      ownerId: 'user-2',
      ownerEmail: '',
      ownerName: '',
      createdAt: '2026-06-18T12:00:00Z',
      expiresAt: null,
      isRevoked: false,
      claims: [],
    })
  })

  it('keeps only string claims', () => {
    expect(
      normalizeApiKey({
        id: 13,
        claims: ['Data:View', null, 42, 'Report:View'] as unknown as string[],
      }).claims
    ).toEqual(['Data:View', 'Report:View'])
  })
})
