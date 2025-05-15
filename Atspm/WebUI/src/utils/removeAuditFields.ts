type AuditKeys = 'created' | 'createdBy' | 'modified' | 'modifiedBy'

export const removeAuditFields = <
  T extends {
    created?: unknown
    createdBy?: unknown
    modified?: unknown
    modifiedBy?: unknown
  },
>(
  data: T
): Omit<T, AuditKeys> => {
  const { created, createdBy, modified, modifiedBy, ...rest } = data
  return rest
}
