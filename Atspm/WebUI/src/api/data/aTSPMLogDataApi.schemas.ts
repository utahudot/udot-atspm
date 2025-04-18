/**
 * Generated by orval v6.23.0 🍺
 * Do not edit manually.
 * ATSPM Log Data Api
 * ATSPM Log Data with OpenAPI, Swashbuckle, and API versioning.
 * OpenAPI spec version: 1.0
 */
export type GetEventLogDaysWithEventLogsFromLocationIdentifierParams = {
/**
 * Type that inherits from Utah.Udot.Atspm.Data.Models.EventLogModels.EventLogModelBase
 */
dataType?: string;
start?: string;
end?: string;
};

export type GetEventLogArchivedEventsFromLocationIdentifierAndDeviceIdAndDataTypeParams = {
/**
 * Archive date of event to start with
 */
start?: string;
/**
 * Archive date of event to end with
 */
end?: string;
};

export type GetEventLogArchivedEventsFromLocationIdentifierAndDataTypeParams = {
/**
 * Archive date of event to start with
 */
start?: string;
/**
 * Archive date of event to end with
 */
end?: string;
};

export type GetEventLogArchivedEventsFromLocationIdentifierAndDeviceIdParams = {
/**
 * Archive date of event to start with
 */
start?: string;
/**
 * Archive date of event to end with
 */
end?: string;
};

export type GetEventLogArchivedEventsFromLocationIdentifierParams = {
/**
 * Archive date of event to start with
 */
start?: string;
/**
 * Archive date of event to end with
 */
end?: string;
};

export type GetAggregationArchivedAggregationsFromLocationIdentifierAndDataTypeParams = {
/**
 * Archive date of aggregation to start with
 */
start?: string;
/**
 * Archive date of aggregation to end with
 */
end?: string;
};

export type GetAggregationArchivedAggregationsFromLocationIdentifierParams = {
/**
 * Archive date of aggregation to start with
 */
start?: string;
/**
 * Archive date of aggregation to end with
 */
end?: string;
};

export interface SpeedEvent {
  detectorId?: string | null;
  kph?: number;
  locationIdentifier?: string | null;
  mph?: number;
  timestamp?: string;
}

export interface SignalPlanAggregation {
  /** @deprecated */
  binStartTime?: string;
  end?: string;
  locationIdentifier?: string | null;
  planNumber?: number;
  start?: string;
}

export interface SignalEventCountAggregation {
  /** @deprecated */
  binStartTime?: string;
  end?: string;
  eventCount?: number;
  locationIdentifier?: string | null;
  start?: string;
}

export interface ProblemDetails {
  detail?: string | null;
  instance?: string | null;
  status?: number | null;
  title?: string | null;
  type?: string | null;
  [key: string]: unknown;
}

export interface PriorityAggregation {
  /** @deprecated */
  binStartTime?: string;
  end?: string;
  locationIdentifier?: string | null;
  priorityNumber?: number;
  priorityRequests?: number;
  priorityServiceEarlyGreen?: number;
  priorityServiceExtendedGreen?: number;
  start?: string;
}

export interface PreemptionAggregation {
  /** @deprecated */
  binStartTime?: string;
  end?: string;
  locationIdentifier?: string | null;
  preemptNumber?: number;
  preemptRequests?: number;
  preemptServices?: number;
  start?: string;
}

export interface PhaseTerminationAggregation {
  /** @deprecated */
  binStartTime?: string;
  end?: string;
  forceOffs?: number;
  gapOuts?: number;
  locationIdentifier?: string | null;
  maxOuts?: number;
  phaseNumber?: number;
  start?: string;
  unknown?: number;
}

export interface PhaseSplitMonitorAggregation {
  /** @deprecated */
  binStartTime?: string;
  eightyFifthPercentileSplit?: number;
  end?: string;
  locationIdentifier?: string | null;
  phaseNumber?: number;
  skippedCount?: number;
  start?: string;
}

export interface PhasePedAggregation {
  /** @deprecated */
  binStartTime?: string;
  end?: string;
  imputedPedCallsRegistered?: number;
  locationIdentifier?: string | null;
  maxPedDelay?: number;
  minPedDelay?: number;
  pedBeginWalkCount?: number;
  pedCallsRegisteredCount?: number;
  pedCycles?: number;
  pedDelay?: number;
  pedRequests?: number;
  phaseNumber?: number;
  start?: string;
  uniquePedDetections?: number;
}

export interface PhaseLeftTurnGapAggregation {
  approachId?: number;
  /** @deprecated */
  binStartTime?: string;
  end?: string;
  gapCount1?: number;
  gapCount10?: number;
  gapCount11?: number;
  gapCount2?: number;
  gapCount3?: number;
  gapCount4?: number;
  gapCount5?: number;
  gapCount6?: number;
  gapCount7?: number;
  gapCount8?: number;
  gapCount9?: number;
  locationIdentifier?: string | null;
  phaseNumber?: number;
  start?: string;
  sumGapDuration1?: number;
  sumGapDuration2?: number;
  sumGapDuration3?: number;
  sumGreenTime?: number;
}

export interface PhaseCycleAggregation {
  approachId?: number;
  /** @deprecated */
  binStartTime?: string;
  end?: string;
  greenTime?: number;
  locationIdentifier?: string | null;
  phaseNumber?: number;
  redTime?: number;
  start?: string;
  totalGreenToGreenCycles?: number;
  totalRedToRedCycles?: number;
  yellowTime?: number;
}

export interface PedestrianCounter {
  in?: number;
  locationIdentifier?: string | null;
  out?: number;
  timestamp?: string;
}

export interface IndianaEvent {
  eventCode?: number;
  eventParam?: number;
  locationIdentifier?: string | null;
  timestamp?: string;
}

export interface DetectorEventCountAggregation {
  approachId?: number;
  /** @deprecated */
  binStartTime?: string;
  detectorPrimaryId?: number;
  end?: string;
  eventCount?: number;
  locationIdentifier?: string | null;
  start?: string;
}

export interface ApproachYellowRedActivationAggregation {
  approachId?: number;
  /** @deprecated */
  binStartTime?: string;
  cycles?: number;
  end?: string;
  isProtectedPhase?: boolean;
  locationIdentifier?: string | null;
  phaseNumber?: number;
  severeRedLightViolations?: number;
  start?: string;
  totalRedLightViolations?: number;
  violationTime?: number;
  yellowActivations?: number;
}

export interface ApproachSplitFailAggregation {
  approachId?: number;
  /** @deprecated */
  binStartTime?: string;
  cycles?: number;
  end?: string;
  greenOccupancySum?: number;
  greenTimeSum?: number;
  isProtectedPhase?: boolean;
  locationIdentifier?: string | null;
  phaseNumber?: number;
  redOccupancySum?: number;
  redTimeSum?: number;
  splitFailures?: number;
  start?: string;
}

export interface ApproachSpeedAggregation {
  approachId?: number;
  /** @deprecated */
  binStartTime?: string;
  end?: string;
  locationIdentifier?: string | null;
  speed15th?: number;
  speed85th?: number;
  speedVolume?: number;
  start?: string;
  summedSpeed?: number;
}

export interface ApproachPcdAggregation {
  approachId?: number;
  arrivalsOnGreen?: number;
  arrivalsOnRed?: number;
  arrivalsOnYellow?: number;
  /** @deprecated */
  binStartTime?: string;
  end?: string;
  isProtectedPhase?: boolean;
  locationIdentifier?: string | null;
  phaseNumber?: number;
  start?: string;
  totalDelay?: number;
  volume?: number;
}

