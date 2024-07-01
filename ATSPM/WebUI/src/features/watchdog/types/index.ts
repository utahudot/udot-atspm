export type Watchdog = {
  Id: number
  LocationId: number
  LocationIdentifier: number
  Timestamp: string
  ComponentType: number
  ComponentId: number
  IssueType: number
  Details: string
  Phase: string | null
  Name: string
}
