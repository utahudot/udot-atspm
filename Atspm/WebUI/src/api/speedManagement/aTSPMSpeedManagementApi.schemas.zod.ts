/**
 * Generated by orval v7.1.1 🍺
 * Do not edit manually.
 * ATSPM Speed Management Api
 * ATSPM Log Data with OpenAPI, Swashbuckle, and API versioning.
 * OpenAPI spec version: 1.0
 */
export type PostApiViolationsAndExtremeViolationsGetReportDataParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type PostApiV1SpeedViolationsGetReportDataParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type PostApiV1SpeedVariabilityGetReportDataParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type PostApiV1SpeedOverTimeGetReportDataParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type PostApiV1SpeedOverDistanceGetReportDataParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type PostApiV1SpeedManagementGetHistoricalSpeedsParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type GetRouteSpeedsParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type PostApiV1SpeedFromImpactSegmentSegmentIdParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type PostApiV1SpeedFromImpactImpactImpactIdParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type PostApiV1SpeedComplianceGetReportDataParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type PostApiV1SegmentSegmentIdSpeedsParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type PostApiV1SegmentSpeedsParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type GetApiV1SegmentSegmentIdParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type GetApiV1SegmentAllSegmentsParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type PostApiV1SegmentParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type GetApiV1SegmentParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type GetApiV1RegionParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type GetApiV1MonthlyAggregationSegmentsSegmentIdParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type PostApiV1MonthlyAggregationSegmentParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type PostApiV1MonthlyAggregationSourceSourceIdParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type DeleteApiV1MonthlyAggregationParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type PostApiV1MonthlyAggregationParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type DeleteApiV1ImpactTypeIdParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type PutApiV1ImpactTypeIdParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type GetApiV1ImpactTypeIdParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type PostApiV1ImpactTypeParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type GetApiV1ImpactTypeParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type DeleteApiV1ImpactIdImpactTypeImpactTypeIdParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type PutApiV1ImpactIdImpactTypeImpactTypeIdParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type DeleteApiV1ImpactIdSegmentsSegmentIdParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type PutApiV1ImpactIdSegmentsSegmentIdParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type DeleteApiV1ImpactIdParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type PutApiV1ImpactIdParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type GetApiV1ImpactIdParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type PostApiV1ImpactParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type GetApiV1ImpactParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type GetApiV1FunctionalTypeParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type PostApiV1EffectivenessOfStrategiesGetReportDataParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type PostApiV1DataQualityGetReportDataParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type GetApiV1CountyParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type PostApiV1CongestionTrackingGetReportDataParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type GetApiV1CityParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export type GetApiV1AccessCategoryParams = {
/**
 * The requested API version
 */
'api-version'?: string;
};

export interface ViolationsAndExtremeViolationsOptions {
  endDate?: string;
  /** @nullable */
  segmentIds?: string[] | null;
  startDate?: string;
}

export interface ViolationsAndExtremeViolationsDto {
  endingMilePoint?: number;
  exteremeViolations?: number;
  flow?: number;
  segmentId?: string;
  /** @nullable */
  segmentName?: string | null;
  speedLimit?: number;
  startingMilePoint?: number;
  time?: string;
  violations?: number;
}

export interface TimeSegmentEffectiveness {
  averageEightyFifthSpeed?: number;
  averageSpeed?: number;
  endDate?: string;
  flow?: number;
  maxSpeed?: number;
  minSpeed?: number;
  percentExtremeViolations?: number;
  percentViolations?: number;
  startDate?: string;
  variability?: number;
}

export type TimeOptionsEnum = typeof TimeOptionsEnum[keyof typeof TimeOptionsEnum];


// eslint-disable-next-line @typescript-eslint/no-redeclare
export const TimeOptionsEnum = {
  NUMBER_0: 0,
  NUMBER_1: 1,
  NUMBER_2: 2,
} as const;

export interface SpeedViolationsOptions {
  /** @nullable */
  dayOfWeek?: number | null;
  endDate?: string;
  /** @nullable */
  endTime?: string | null;
  /** @nullable */
  segmentIds?: string[] | null;
  /** @nullable */
  specificDays?: string[] | null;
  startDate?: string;
  /** @nullable */
  startTime?: string | null;
}

export interface SpeedViolationsDto {
  /** @nullable */
  dailySpeedViolationsDto?: DailySpeedViolationsDto[] | null;
  percentExtremeViolations?: number;
  percentViolations?: number;
  segmentId?: string;
  /** @nullable */
  segmentName?: string | null;
  speedLimit?: number;
  totalExtremeViolationsCount?: number;
  totalFlow?: number;
  totalViolationsCount?: number;
}

export interface SpeedVariabilityOptions {
  /** @nullable */
  daysOfWeek?: DayOfWeek[] | null;
  endDate?: string;
  /** @nullable */
  isHolidaysFiltered?: boolean | null;
  /** @nullable */
  segmentId?: string | null;
  sourceId?: number;
  startDate?: string;
}

export interface SpeedVariabilityDataDto {
  avgSpeed?: number;
  date?: string;
  maxSpeed?: number;
  minSpeed?: number;
  speedVariability?: number;
}

export interface SpeedVariabilityDto {
  /** @nullable */
  data?: SpeedVariabilityDataDto[] | null;
  endDate?: string;
  endingMilePoint?: number;
  segmentId?: string;
  /** @nullable */
  segmentName?: string | null;
  speedLimit?: number;
  startDate?: string;
  startingMilePoint?: number;
}

export interface SpeedOverTimeOptions {
  endDate?: string;
  /** @nullable */
  endTime?: string | null;
  /** @nullable */
  segmentId?: string | null;
  sourceId?: number;
  startDate?: string;
  /** @nullable */
  startTime?: string | null;
  timeOptions?: TimeOptionsEnum;
}

export interface SpeedOverTimeDto {
  /** @nullable */
  data?: SpeedDataDto[] | null;
  endDate?: string;
  endingMilePoint?: number;
  segmentId?: string;
  /** @nullable */
  segmentName?: string | null;
  speedLimit?: number;
  startDate?: string;
  startingMilePoint?: number;
  timeOptions?: TimeOptionsEnum;
}

export interface SpeedOverDistanceOptions {
  endDate?: string;
  /** @nullable */
  segmentIds?: string[] | null;
  startDate?: string;
}

export interface SpeedOverDistanceDto {
  average?: number;
  eightyFifth?: number;
  endDate?: string;
  endingMilePoint?: number;
  segmentId?: string;
  /** @nullable */
  segmentName?: string | null;
  speedLimit?: number;
  startDate?: string;
  startingMilePoint?: number;
}

export interface SpeedFromImpactDto {
  endDate?: string;
  /** @nullable */
  hourlySpeeds?: HourlySpeed[] | null;
  /** @nullable */
  impacts?: Impact[] | null;
  /** @nullable */
  segments?: Segment[] | null;
  startDate?: string;
}

export interface SpeedDataDto {
  date?: string;
  series?: AverageAndEightyFifthSeriesData;
}

export interface SpeedComplianceOptions {
  endDate?: string;
  /** @nullable */
  segmentIds?: string[] | null;
  startDate?: string;
}

export interface SpeedComplianceDto {
  average?: number;
  avgVsBaseSpeed?: number;
  eightyFifth?: number;
  eightyFifthPercentileVsBaseSpeed?: number;
  endDate?: string;
  endingMilePoint?: number;
  segmentId?: string;
  /** @nullable */
  segmentName?: string | null;
  speedLimit?: number;
  startDate?: string;
  startingMilePoint?: number;
}

export interface SegmentRequestDto {
  endDate?: string;
  /** @nullable */
  segmentIds?: string[] | null;
  startDate?: string;
}

export interface Segment {
  /** @nullable */
  accessCategory?: string | null;
  /** @nullable */
  alternateIdentifier?: string | null;
  /** @nullable */
  city?: string | null;
  /** @nullable */
  county?: string | null;
  /** @nullable */
  direction?: string | null;
  endMilePoint?: number;
  /** @nullable */
  entities?: SegmentEntity[] | null;
  /** @nullable */
  functionalType?: string | null;
  id?: string;
  /** @nullable */
  name?: string | null;
  /** @nullable */
  offset?: number | null;
  /** @nullable */
  region?: string | null;
  shape?: Geometry;
  /** @nullable */
  shapeWKT?: string | null;
  speedLimit?: number;
  startMilePoint?: number;
  /** @nullable */
  udotRouteNumber?: string | null;
}

export interface SegmentEntity {
  entityId?: number;
  /** @nullable */
  entityType?: string | null;
  length?: number;
  segment?: Segment;
  segmentId?: string;
  sourceId?: number;
}

export interface RouteSpeedOptions {
  /** @nullable */
  accessCategory?: string | null;
  /** @nullable */
  city?: string | null;
  /** @nullable */
  county?: string | null;
  /** @nullable */
  daysOfWeek?: DayOfWeek[] | null;
  endDate?: string;
  endTime?: string;
  /** @nullable */
  functionalType?: string | null;
  /** @nullable */
  region?: string | null;
  sourceId?: number;
  startDate?: string;
  startTime?: string;
  violationThreshold?: number;
}

export interface ProblemDetails {
  /** @nullable */
  detail?: string | null;
  /** @nullable */
  instance?: string | null;
  /** @nullable */
  status?: number | null;
  /** @nullable */
  title?: string | null;
  /** @nullable */
  type?: string | null;
  [key: string]: unknown;
}

export type PrecisionModels = typeof PrecisionModels[keyof typeof PrecisionModels];


// eslint-disable-next-line @typescript-eslint/no-redeclare
export const PrecisionModels = {
  NUMBER_0: 0,
  NUMBER_1: 1,
  NUMBER_2: 2,
} as const;

export interface PrecisionModel {
  readonly gridSize?: number;
  readonly isFloating?: boolean;
  readonly maximumSignificantDigits?: number;
  precisionModelType?: PrecisionModels;
  scale?: number;
}

/**
 * @nullable
 */
export type PointUserData = unknown | null;

export type Ordinates = typeof Ordinates[keyof typeof Ordinates];


// eslint-disable-next-line @typescript-eslint/no-redeclare
export const Ordinates = {
  NUMBER_0: 0,
  NUMBER_1: 1,
  NUMBER_2: 2,
  NUMBER_3: 3,
  NUMBER_4: 4,
  NUMBER_7: 7,
  NUMBER_8: 8,
  NUMBER_16: 16,
  NUMBER_32: 32,
  NUMBER_64: 64,
  NUMBER_128: 128,
  NUMBER_256: 256,
  NUMBER_512: 512,
  NUMBER_1024: 1024,
  NUMBER_2048: 2048,
  NUMBER_4096: 4096,
  NUMBER_8192: 8192,
  NUMBER_16384: 16384,
  NUMBER_32768: 32768,
  NUMBER_65535: 65535,
  NUMBER_65536: 65536,
  NUMBER_65539: 65539,
  NUMBER_65543: 65543,
  NUMBER_131072: 131072,
  NUMBER_262144: 262144,
  NUMBER_524288: 524288,
  NUMBER_1048576: 1048576,
  NUMBER_2097152: 2097152,
  NUMBER_4194304: 4194304,
  NUMBER_8388608: 8388608,
  NUMBER_16777216: 16777216,
  NUMBER_33554432: 33554432,
  NUMBER_67108864: 67108864,
  NUMBER_134217728: 134217728,
  NUMBER_268435456: 268435456,
  NUMBER_536870912: 536870912,
  NUMBER_1073741824: 1073741824,
  NUMBER_MINUS_2147483648: -2147483648,
  NUMBER_MINUS_65536: -65536,
  NUMBER_MINUS_1: -1,
} as const;

export type OgcGeometryType = typeof OgcGeometryType[keyof typeof OgcGeometryType];


// eslint-disable-next-line @typescript-eslint/no-redeclare
export const OgcGeometryType = {
  NUMBER_1: 1,
  NUMBER_2: 2,
  NUMBER_3: 3,
  NUMBER_4: 4,
  NUMBER_5: 5,
  NUMBER_6: 6,
  NUMBER_7: 7,
  NUMBER_8: 8,
  NUMBER_9: 9,
  NUMBER_10: 10,
  NUMBER_11: 11,
  NUMBER_12: 12,
  NUMBER_13: 13,
  NUMBER_14: 14,
  NUMBER_15: 15,
  NUMBER_16: 16,
} as const;

export interface Point {
  readonly area?: number;
  boundary?: Geometry;
  boundaryDimension?: Dimension;
  centroid?: Point;
  coordinate?: Coordinate;
  /** @nullable */
  readonly coordinates?: readonly Coordinate[] | null;
  coordinateSequence?: CoordinateSequence;
  dimension?: Dimension;
  envelope?: Geometry;
  envelopeInternal?: Envelope;
  factory?: GeometryFactory;
  /** @nullable */
  readonly geometryType?: string | null;
  interiorPoint?: Point;
  readonly isEmpty?: boolean;
  readonly isRectangle?: boolean;
  readonly isSimple?: boolean;
  readonly isValid?: boolean;
  readonly length?: number;
  m?: number;
  readonly numGeometries?: number;
  readonly numPoints?: number;
  ogcGeometryType?: OgcGeometryType;
  pointOnSurface?: Point;
  precisionModel?: PrecisionModel;
  srid?: number;
  /** @nullable */
  userData?: PointUserData;
  x?: number;
  y?: number;
  z?: number;
}

export interface NameAndIdDto {
  id?: string;
  /** @nullable */
  name?: string | null;
}

export interface MonthlyAggregation {
  /** @nullable */
  allDayAverageEightyFifthSpeed?: number | null;
  /** @nullable */
  allDayAverageSpeed?: number | null;
  /** @nullable */
  allDayAvgSpeedVsSpeedLimit?: number | null;
  /** @nullable */
  allDayEightyFifthSpeedVsSpeedLimit?: number | null;
  /** @nullable */
  allDayExtremeViolations?: number | null;
  /** @nullable */
  allDayFlow?: number | null;
  /** @nullable */
  allDayMaxSpeed?: number | null;
  /** @nullable */
  allDayMinSpeed?: number | null;
  /** @nullable */
  allDayPercentExtremeViolations?: number | null;
  /** @nullable */
  allDayPercentViolations?: number | null;
  /** @nullable */
  allDayVariability?: number | null;
  /** @nullable */
  allDayViolations?: number | null;
  /** @nullable */
  amPeakAverageEightyFifthSpeed?: number | null;
  /** @nullable */
  amPeakAverageSpeed?: number | null;
  /** @nullable */
  amPeakAvgSpeedVsSpeedLimit?: number | null;
  /** @nullable */
  amPeakEightyFifthSpeedVsSpeedLimit?: number | null;
  /** @nullable */
  amPeakExtremeViolations?: number | null;
  /** @nullable */
  amPeakFlow?: number | null;
  /** @nullable */
  amPeakMaxSpeed?: number | null;
  /** @nullable */
  amPeakMinSpeed?: number | null;
  /** @nullable */
  amPeakPercentExtremeViolations?: number | null;
  /** @nullable */
  amPeakPercentViolations?: number | null;
  /** @nullable */
  amPeakVariability?: number | null;
  /** @nullable */
  amPeakViolations?: number | null;
  binStartTime?: string;
  /** @nullable */
  createdDate?: string | null;
  /** @nullable */
  earlyMorningAverageEightyFifthSpeed?: number | null;
  /** @nullable */
  earlyMorningAverageSpeed?: number | null;
  /** @nullable */
  earlyMorningAvgSpeedVsSpeedLimit?: number | null;
  /** @nullable */
  earlyMorningEightyFifthSpeedVsSpeedLimit?: number | null;
  /** @nullable */
  earlyMorningExtremeViolations?: number | null;
  /** @nullable */
  earlyMorningFlow?: number | null;
  /** @nullable */
  earlyMorningMaxSpeed?: number | null;
  /** @nullable */
  earlyMorningMinSpeed?: number | null;
  /** @nullable */
  earlyMorningPercentExtremeViolations?: number | null;
  /** @nullable */
  earlyMorningPercentViolations?: number | null;
  /** @nullable */
  earlyMorningVariability?: number | null;
  /** @nullable */
  earlyMorningViolations?: number | null;
  /** @nullable */
  eveningAverageEightyFifthSpeed?: number | null;
  /** @nullable */
  eveningAverageSpeed?: number | null;
  /** @nullable */
  eveningAvgSpeedVsSpeedLimit?: number | null;
  /** @nullable */
  eveningEightyFifthSpeedVsSpeedLimit?: number | null;
  /** @nullable */
  eveningExtremeViolations?: number | null;
  /** @nullable */
  eveningFlow?: number | null;
  /** @nullable */
  eveningMaxSpeed?: number | null;
  /** @nullable */
  eveningMinSpeed?: number | null;
  /** @nullable */
  eveningPercentExtremeViolations?: number | null;
  /** @nullable */
  eveningPercentViolations?: number | null;
  /** @nullable */
  eveningVariability?: number | null;
  /** @nullable */
  eveningViolations?: number | null;
  /** @nullable */
  id?: string | null;
  /** @nullable */
  midDayAverageEightyFifthSpeed?: number | null;
  /** @nullable */
  midDayAverageSpeed?: number | null;
  /** @nullable */
  midDayAvgSpeedVsSpeedLimit?: number | null;
  /** @nullable */
  midDayEightyFifthSpeedVsSpeedLimit?: number | null;
  /** @nullable */
  midDayExtremeViolations?: number | null;
  /** @nullable */
  midDayFlow?: number | null;
  /** @nullable */
  midDayMaxSpeed?: number | null;
  /** @nullable */
  midDayMinSpeed?: number | null;
  /** @nullable */
  midDayPercentExtremeViolations?: number | null;
  /** @nullable */
  midDayPercentViolations?: number | null;
  /** @nullable */
  midDayVariability?: number | null;
  /** @nullable */
  midDayViolations?: number | null;
  /** @nullable */
  offPeakAverageEightyFifthSpeed?: number | null;
  /** @nullable */
  offPeakAverageSpeed?: number | null;
  /** @nullable */
  offPeakAvgSpeedVsSpeedLimit?: number | null;
  /** @nullable */
  offPeakEightyFifthSpeedVsSpeedLimit?: number | null;
  /** @nullable */
  offPeakExtremeViolations?: number | null;
  /** @nullable */
  offPeakFlow?: number | null;
  /** @nullable */
  offPeakMaxSpeed?: number | null;
  /** @nullable */
  offPeakMinSpeed?: number | null;
  /** @nullable */
  offPeakPercentExtremeViolations?: number | null;
  /** @nullable */
  offPeakPercentViolations?: number | null;
  /** @nullable */
  offPeakVariability?: number | null;
  /** @nullable */
  offPeakViolations?: number | null;
  /** @nullable */
  percentObserved?: number | null;
  /** @nullable */
  pmPeakAverageEightyFifthSpeed?: number | null;
  /** @nullable */
  pmPeakAverageSpeed?: number | null;
  /** @nullable */
  pmPeakAvgSpeedVsSpeedLimit?: number | null;
  /** @nullable */
  pmPeakEightyFifthSpeedVsSpeedLimit?: number | null;
  /** @nullable */
  pmPeakExtremeViolations?: number | null;
  /** @nullable */
  pmPeakFlow?: number | null;
  /** @nullable */
  pmPeakMaxSpeed?: number | null;
  /** @nullable */
  pmPeakMinSpeed?: number | null;
  /** @nullable */
  pmPeakPercentExtremeViolations?: number | null;
  /** @nullable */
  pmPeakPercentViolations?: number | null;
  /** @nullable */
  pmPeakVariability?: number | null;
  /** @nullable */
  pmPeakViolations?: number | null;
  segmentId?: string;
  sourceId?: number;
}

export interface Int64DataPoint {
  timestamp?: string;
  value?: number;
}

export interface ImpactType {
  /** @nullable */
  description?: string | null;
  /** @nullable */
  id?: string | null;
  /** @nullable */
  name?: string | null;
}

export interface Impact {
  /** @nullable */
  createdBy?: string | null;
  /** @nullable */
  createdOn?: string | null;
  /** @nullable */
  deletedBy?: string | null;
  /** @nullable */
  deletedOn?: string | null;
  /** @nullable */
  description?: string | null;
  /** @nullable */
  end?: string | null;
  endMile?: number;
  /** @nullable */
  id?: string | null;
  /** @nullable */
  impactTypeIds?: string[] | null;
  /** @nullable */
  impactTypes?: ImpactType[] | null;
  /** @nullable */
  segmentIds?: string[] | null;
  start?: string;
  startMile?: number;
  /** @nullable */
  updatedBy?: string | null;
  /** @nullable */
  updatedOn?: string | null;
}

export interface HourlySpeed {
  average?: number;
  binStartTime?: string;
  date?: string;
  /** @nullable */
  eightyFifthSpeed?: number | null;
  /** @nullable */
  extremeViolation?: number | null;
  /** @nullable */
  fifteenthSpeed?: number | null;
  /** @nullable */
  flow?: number | null;
  /** @nullable */
  maxSpeed?: number | null;
  /** @nullable */
  minSpeed?: number | null;
  /** @nullable */
  ninetyFifthSpeed?: number | null;
  /** @nullable */
  ninetyNinthSpeed?: number | null;
  /** @nullable */
  percentObserved?: number | null;
  segmentId?: string;
  /** @nullable */
  sourceDataAnalyzed?: boolean | null;
  sourceId?: number;
  /** @nullable */
  violation?: number | null;
}

export interface HistoricalSpeedOptions {
  /** @nullable */
  daysOfWeek?: DayOfWeek[] | null;
  endDate?: string;
  segmentId?: string;
  startDate?: string;
}

export interface GeometryOverlay { [key: string]: unknown }

export interface NtsGeometryServices {
  coordinateEqualityComparer?: CoordinateEqualityComparer;
  defaultCoordinateSequenceFactory?: CoordinateSequenceFactory;
  defaultPrecisionModel?: PrecisionModel;
  readonly defaultSRID?: number;
  geometryOverlay?: GeometryOverlay;
}

export interface GeometryFactory {
  coordinateSequenceFactory?: CoordinateSequenceFactory;
  geometryServices?: NtsGeometryServices;
  precisionModel?: PrecisionModel;
  readonly srid?: number;
}

/**
 * @nullable
 */
export type GeometryUserData = unknown | null;

export interface Envelope {
  readonly area?: number;
  centre?: Coordinate;
  readonly diameter?: number;
  readonly height?: number;
  readonly isNull?: boolean;
  readonly maxExtent?: number;
  readonly maxX?: number;
  readonly maxY?: number;
  readonly minExtent?: number;
  readonly minX?: number;
  readonly minY?: number;
  readonly width?: number;
}

export interface Geometry {
  readonly area?: number;
  boundary?: Geometry;
  boundaryDimension?: Dimension;
  centroid?: Point;
  coordinate?: Coordinate;
  /** @nullable */
  readonly coordinates?: readonly Coordinate[] | null;
  dimension?: Dimension;
  envelope?: Geometry;
  envelopeInternal?: Envelope;
  factory?: GeometryFactory;
  /** @nullable */
  readonly geometryType?: string | null;
  interiorPoint?: Point;
  readonly isEmpty?: boolean;
  readonly isRectangle?: boolean;
  readonly isSimple?: boolean;
  readonly isValid?: boolean;
  readonly length?: number;
  readonly numGeometries?: number;
  readonly numPoints?: number;
  ogcGeometryType?: OgcGeometryType;
  pointOnSurface?: Point;
  precisionModel?: PrecisionModel;
  srid?: number;
  /** @nullable */
  userData?: GeometryUserData;
}

export interface EffectivenessOfStrategiesOptions {
  /** @nullable */
  endTime?: string | null;
  /** @nullable */
  segmentIds?: string[] | null;
  /** @nullable */
  startTime?: string | null;
  strategyImplementedDate?: string;
}

export interface EffectivenessOfStrategiesDto {
  after?: TimeSegmentEffectiveness;
  before?: TimeSegmentEffectiveness;
  changeInAverageSpeed?: number;
  changeInEightyFifthPercentileSpeed?: number;
  changeInPercentExtremeViolations?: number;
  changeInPercentViolations?: number;
  changeInVariability?: number;
  segmentId?: string;
  /** @nullable */
  segmentName?: string | null;
  speedLimit?: number;
  /** @nullable */
  weeklyEffectiveness?: TimeSegmentEffectiveness[] | null;
}

export interface DoubleDataPoint {
  timestamp?: string;
  value?: number;
}

export type Dimension = typeof Dimension[keyof typeof Dimension];


// eslint-disable-next-line @typescript-eslint/no-redeclare
export const Dimension = {
  NUMBER_0: 0,
  NUMBER_1: 1,
  NUMBER_2: 2,
  NUMBER_3: 3,
  NUMBER_MINUS_3: -3,
  NUMBER_MINUS_2: -2,
  NUMBER_MINUS_1: -1,
} as const;

export type DayOfWeek = typeof DayOfWeek[keyof typeof DayOfWeek];


// eslint-disable-next-line @typescript-eslint/no-redeclare
export const DayOfWeek = {
  NUMBER_0: 0,
  NUMBER_1: 1,
  NUMBER_2: 2,
  NUMBER_3: 3,
  NUMBER_4: 4,
  NUMBER_5: 5,
  NUMBER_6: 6,
} as const;

export interface DataQualitySegment {
  /** @nullable */
  dataPoints?: DoubleDataPoint[] | null;
  endingMilePoint?: number;
  segmentId?: string;
  /** @nullable */
  segmentName?: string | null;
  startingMilePoint?: number;
}

export interface DataQualitySource {
  endDate?: string;
  /** @nullable */
  name?: string | null;
  /** @nullable */
  segments?: DataQualitySegment[] | null;
  sourceId?: number;
  startDate?: string;
}

export interface DataQualityOptions {
  endDate?: string;
  /** @nullable */
  segmentIds?: string[] | null;
  startDate?: string;
}

export interface DailySpeedViolationsDto {
  dailyExtremeViolationsCount?: number;
  dailyFlow?: number;
  dailyPercentExtremeViolations?: number;
  dailyPercentViolations?: number;
  dailyViolationsCount?: number;
  date?: string;
}

export interface CoordinateSequenceFactory {
  ordinates?: Ordinates;
}

export interface CoordinateSequence {
  readonly count?: number;
  readonly dimension?: number;
  first?: Coordinate;
  readonly hasM?: boolean;
  readonly hasZ?: boolean;
  last?: Coordinate;
  readonly measures?: number;
  readonly mOrdinateIndex?: number;
  ordinates?: Ordinates;
  readonly spatial?: number;
  readonly zOrdinateIndex?: number;
}

export interface CoordinateEqualityComparer { [key: string]: unknown }

export interface Coordinate {
  coordinateValue?: Coordinate;
  readonly isValid?: boolean;
  m?: number;
  x?: number;
  y?: number;
  z?: number;
}

export interface CongestionTrackingOptions {
  endDate?: string;
  /** @nullable */
  segmentId?: string | null;
  sourceId?: number;
  startDate?: string;
}

export interface CongestionTrackingDto {
  /** @nullable */
  data?: SpeedDataDto[] | null;
  endDate?: string;
  endingMilePoint?: number;
  segmentId?: string;
  /** @nullable */
  segmentName?: string | null;
  speedLimit?: number;
  startDate?: string;
  startingMilePoint?: number;
}

export interface AverageAndEightyFifthSeriesData {
  /** @nullable */
  average?: DoubleDataPoint[] | null;
  /** @nullable */
  eightyFifth?: Int64DataPoint[] | null;
}
