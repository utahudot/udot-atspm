export interface Role {
  id: number
  role: string
  claims: string[]
}
export type RolesResponse = Role[]
