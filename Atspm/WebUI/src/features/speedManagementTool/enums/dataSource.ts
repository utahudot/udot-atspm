export enum DataSource {
  ATSPM = 1,
  PeMS = 2,
  ClearGuide = 3,
}

export function getDataSourceName(dataSource: DataSource): string {
  switch (dataSource) {
    case DataSource.ATSPM:
      return 'ATSPM'
    case DataSource.PeMS:
      return 'PeMS'
    case DataSource.ClearGuide:
      return 'ClearGuide'
  }
}
