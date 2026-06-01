export enum DataSource {
  ATSPM = 1,
  PeMS = 2,
  ClearGuide = 3,
}

export function getDataSourceName(dataSource: number | undefined) {
  switch (dataSource) {
    case 1:
      return 'ATSPM'
    case 2:
      return 'PeMS'
    case 3:
      return 'ClearGuide'
  }
}
