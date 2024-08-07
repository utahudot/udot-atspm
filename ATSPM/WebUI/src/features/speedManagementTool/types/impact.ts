
export interface ImpactType {
    id?: string | null;
    name: string;
    description?: string | null;
  }

export interface Impact {
    id?: string | null;
    description?: string | null;
    start: Date;
    end?: Date | null;
    startMile: number;
    endMile: number;
    impactTypeId: string;
    impactType?: ImpactType | null;
  }