// featureFlags.ts
export interface Flags {
  speedManagementTool: boolean
}

export async function loadFlags(): Promise<Flags> {
  const resp = await fetch('/api/feature-flags')
  return resp.json()
}
