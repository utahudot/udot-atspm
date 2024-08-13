
export interface Impact {
    id?: string | null;
    description?: string | null;
    start?: Date | null;
    end?: Date | null;
    startMile: number;
    endMile: number;
    impactTypeIds?: string[] | null;
    impactTypes?: ImpactType[] | null;
    createdOn?: Date | null;
    createdBy?: string | null;
    updatedOn?: Date | null;
    updatedBy?: string | null;
    deletedOn?: Date | null;
    deletedBy?: string | null;
    segmentIds?: string[] | null;
}

export interface ImpactType {
    id?: string | null;
    name: string;
    description?: string | null;
}
