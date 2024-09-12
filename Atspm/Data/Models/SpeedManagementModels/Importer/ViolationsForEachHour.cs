namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Importer
{
    public class ViolationsForEachHour
    {
        public DateTime Date { get; set; }
        public int SpeedLimit { get; set; }

        public int ViolationsHour0 { get; set; }
        public int ExtremeViolationsHour0 { get; set; }
        public int DataQualityHour0 { get; set; }

        public int ViolationsHour1 { get; set; }
        public int ExtremeViolationsHour1 { get; set; }
        public int DataQualityHour1 { get; set; }

        public int ViolationsHour2 { get; set; }
        public int ExtremeViolationsHour2 { get; set; }
        public int DataQualityHour2 { get; set; }

        public int ViolationsHour3 { get; set; }
        public int ExtremeViolationsHour3 { get; set; }
        public int DataQualityHour3 { get; set; }

        public int ViolationsHour4 { get; set; }
        public int ExtremeViolationsHour4 { get; set; }
        public int DataQualityHour4 { get; set; }

        public int ViolationsHour5 { get; set; }
        public int ExtremeViolationsHour5 { get; set; }
        public int DataQualityHour5 { get; set; }

        public int ViolationsHour6 { get; set; }
        public int ExtremeViolationsHour6 { get; set; }
        public int DataQualityHour6 { get; set; }

        public int ViolationsHour7 { get; set; }
        public int ExtremeViolationsHour7 { get; set; }
        public int DataQualityHour7 { get; set; }

        public int ViolationsHour8 { get; set; }
        public int ExtremeViolationsHour8 { get; set; }
        public int DataQualityHour8 { get; set; }

        public int ViolationsHour9 { get; set; }
        public int ExtremeViolationsHour9 { get; set; }
        public int DataQualityHour9 { get; set; }

        public int ViolationsHour10 { get; set; }
        public int ExtremeViolationsHour10 { get; set; }
        public int DataQualityHour10 { get; set; }

        public int ViolationsHour11 { get; set; }
        public int ExtremeViolationsHour11 { get; set; }
        public int DataQualityHour11 { get; set; }

        public int ViolationsHour12 { get; set; }
        public int ExtremeViolationsHour12 { get; set; }
        public int DataQualityHour12 { get; set; }

        public int ViolationsHour13 { get; set; }
        public int ExtremeViolationsHour13 { get; set; }
        public int DataQualityHour13 { get; set; }

        public int ViolationsHour14 { get; set; }
        public int ExtremeViolationsHour14 { get; set; }
        public int DataQualityHour14 { get; set; }

        public int ViolationsHour15 { get; set; }
        public int ExtremeViolationsHour15 { get; set; }
        public int DataQualityHour15 { get; set; }

        public int ViolationsHour16 { get; set; }
        public int ExtremeViolationsHour16 { get; set; }
        public int DataQualityHour16 { get; set; }

        public int ViolationsHour17 { get; set; }
        public int ExtremeViolationsHour17 { get; set; }
        public int DataQualityHour17 { get; set; }

        public int ViolationsHour18 { get; set; }
        public int ExtremeViolationsHour18 { get; set; }
        public int DataQualityHour18 { get; set; }

        public int ViolationsHour19 { get; set; }
        public int ExtremeViolationsHour19 { get; set; }
        public int DataQualityHour19 { get; set; }

        public int ViolationsHour20 { get; set; }
        public int ExtremeViolationsHour20 { get; set; }
        public int DataQualityHour20 { get; set; }

        public int ViolationsHour21 { get; set; }
        public int ExtremeViolationsHour21 { get; set; }
        public int DataQualityHour21 { get; set; }

        public int ViolationsHour22 { get; set; }
        public int ExtremeViolationsHour22 { get; set; }
        public int DataQualityHour22 { get; set; }

        public int ViolationsHour23 { get; set; }
        public int ExtremeViolationsHour23 { get; set; }
        public int DataQualityHour23 { get; set; }


        // Function to get the violation count for a specific hour
        public int GetViolation(int hour)
        {
            switch (hour)
            {
                case 0:
                    return ViolationsHour0;
                case 1:
                    return ViolationsHour1;
                case 2:
                    return ViolationsHour2;
                case 3:
                    return ViolationsHour3;
                case 4:
                    return ViolationsHour4;
                case 5:
                    return ViolationsHour5;
                case 6:
                    return ViolationsHour6;
                case 7:
                    return ViolationsHour7;
                case 8:
                    return ViolationsHour8;
                case 9:
                    return ViolationsHour9;
                case 10:
                    return ViolationsHour10;
                case 11:
                    return ViolationsHour11;
                case 12:
                    return ViolationsHour12;
                case 13:
                    return ViolationsHour13;
                case 14:
                    return ViolationsHour14;
                case 15:
                    return ViolationsHour15;
                case 16:
                    return ViolationsHour16;
                case 17:
                    return ViolationsHour17;
                case 18:
                    return ViolationsHour18;
                case 19:
                    return ViolationsHour19;
                case 20:
                    return ViolationsHour20;
                case 21:
                    return ViolationsHour21;
                case 22:
                    return ViolationsHour22;
                case 23:
                    return ViolationsHour23;
                default:
                    return 0;
            }
        }

        // Function to get the extreme violation count for a specific hour
        public int GetExtremeViolation(int hour)
        {
            switch (hour)
            {
                case 0:
                    return ExtremeViolationsHour0;
                case 1:
                    return ExtremeViolationsHour1;
                case 2:
                    return ExtremeViolationsHour2;
                case 3:
                    return ExtremeViolationsHour3;
                case 4:
                    return ExtremeViolationsHour4;
                case 5:
                    return ExtremeViolationsHour5;
                case 6:
                    return ExtremeViolationsHour6;
                case 7:
                    return ExtremeViolationsHour7;
                case 8:
                    return ExtremeViolationsHour8;
                case 9:
                    return ExtremeViolationsHour9;
                case 10:
                    return ExtremeViolationsHour10;
                case 11:
                    return ExtremeViolationsHour11;
                case 12:
                    return ExtremeViolationsHour12;
                case 13:
                    return ExtremeViolationsHour13;
                case 14:
                    return ExtremeViolationsHour14;
                case 15:
                    return ExtremeViolationsHour15;
                case 16:
                    return ExtremeViolationsHour16;
                case 17:
                    return ExtremeViolationsHour17;
                case 18:
                    return ExtremeViolationsHour18;
                case 19:
                    return ExtremeViolationsHour19;
                case 20:
                    return ExtremeViolationsHour20;
                case 21:
                    return ExtremeViolationsHour21;
                case 22:
                    return ExtremeViolationsHour22;
                case 23:
                    return ExtremeViolationsHour23;
                default:
                    return 0;
            }
        }
        public int GetDataQuality(int hour)
        {
            switch (hour)
            {
                case 0:
                    return DataQualityHour0;
                case 1:
                    return DataQualityHour1;
                case 2:
                    return DataQualityHour2;
                case 3:
                    return DataQualityHour3;
                case 4:
                    return DataQualityHour4;
                case 5:
                    return DataQualityHour5;
                case 6:
                    return DataQualityHour6;
                case 7:
                    return DataQualityHour7;
                case 8:
                    return DataQualityHour8;
                case 9:
                    return DataQualityHour9;
                case 10:
                    return DataQualityHour10;
                case 11:
                    return DataQualityHour11;
                case 12:
                    return DataQualityHour12;
                case 13:
                    return DataQualityHour13;
                case 14:
                    return DataQualityHour14;
                case 15:
                    return DataQualityHour15;
                case 16:
                    return DataQualityHour16;
                case 17:
                    return DataQualityHour17;
                case 18:
                    return DataQualityHour18;
                case 19:
                    return DataQualityHour19;
                case 20:
                    return DataQualityHour20;
                case 21:
                    return DataQualityHour21;
                case 22:
                    return DataQualityHour22;
                case 23:
                    return DataQualityHour23;
                default:
                    return 0;
            }
        }

        public ViolationsForEachHour PopulateViolationsForEachHour(int hour, int violation, int extremeViolation, int dataQuality)
        {
            switch (hour)
            {
                case 0:
                    this.ViolationsHour0 = violation;
                    this.ExtremeViolationsHour0 = extremeViolation;
                    this.DataQualityHour0 = dataQuality;
                    return this;
                case 1:
                    this.ViolationsHour1 = violation;
                    this.ExtremeViolationsHour1 = extremeViolation;
                    this.DataQualityHour1 = dataQuality;
                    return this;
                case 2:
                    this.ViolationsHour2 = violation;
                    this.ExtremeViolationsHour2 = extremeViolation;
                    this.DataQualityHour2 = dataQuality;
                    return this;
                case 3:
                    this.ViolationsHour3 = violation;
                    this.ExtremeViolationsHour3 = extremeViolation;
                    this.DataQualityHour3 = dataQuality;
                    return this;
                case 4:
                    this.ViolationsHour4 = violation;
                    this.ExtremeViolationsHour4 = extremeViolation;
                    this.DataQualityHour4 = dataQuality;
                    return this;
                case 5:
                    this.ViolationsHour5 = violation;
                    this.ExtremeViolationsHour5 = extremeViolation;
                    this.DataQualityHour5 = dataQuality;
                    return this;
                case 6:
                    this.ViolationsHour6 = violation;
                    this.ExtremeViolationsHour6 = extremeViolation;
                    this.DataQualityHour6 = dataQuality;
                    return this;
                case 7:
                    this.ViolationsHour7 = violation;
                    this.ExtremeViolationsHour7 = extremeViolation;
                    this.DataQualityHour7 = dataQuality;
                    return this;
                case 8:
                    this.ViolationsHour8 = violation;
                    this.ExtremeViolationsHour8 = extremeViolation;
                    this.DataQualityHour8 = dataQuality;
                    return this;
                case 9:
                    this.ViolationsHour9 = violation;
                    this.ExtremeViolationsHour9 = extremeViolation;
                    this.DataQualityHour9 = dataQuality;
                    return this;
                case 10:
                    this.ViolationsHour10 = violation;
                    this.ExtremeViolationsHour10 = extremeViolation;
                    this.DataQualityHour10 = dataQuality;
                    return this;
                case 11:
                    this.ViolationsHour11 = violation;
                    this.ExtremeViolationsHour11 = extremeViolation;
                    this.DataQualityHour11 = dataQuality;
                    return this;
                case 12:
                    this.ViolationsHour12 = violation;
                    this.ExtremeViolationsHour12 = extremeViolation;
                    this.DataQualityHour12 = dataQuality;
                    return this;
                case 13:
                    this.ViolationsHour13 = violation;
                    this.ExtremeViolationsHour13 = extremeViolation;
                    this.DataQualityHour13 = dataQuality;
                    return this;
                case 14:
                    this.ViolationsHour14 = violation;
                    this.ExtremeViolationsHour14 = extremeViolation;
                    this.DataQualityHour14 = dataQuality;
                    return this;
                case 15:
                    this.ViolationsHour15 = violation;
                    this.ExtremeViolationsHour15 = extremeViolation;
                    this.DataQualityHour15 = dataQuality;
                    return this;
                case 16:
                    this.ViolationsHour16 = violation;
                    this.ExtremeViolationsHour16 = extremeViolation;
                    this.DataQualityHour16 = dataQuality;
                    return this;
                case 17:
                    this.ViolationsHour17 = violation;
                    this.ExtremeViolationsHour17 = extremeViolation;
                    this.DataQualityHour17 = dataQuality;
                    return this;
                case 18:
                    this.ViolationsHour18 = violation;
                    this.ExtremeViolationsHour18 = extremeViolation;
                    this.DataQualityHour18 = dataQuality;
                    return this;
                case 19:
                    this.ViolationsHour19 = violation;
                    this.ExtremeViolationsHour19 = extremeViolation;
                    this.DataQualityHour19 = dataQuality;
                    return this;
                case 20:
                    this.ViolationsHour20 = violation;
                    this.ExtremeViolationsHour20 = extremeViolation;
                    this.DataQualityHour20 = dataQuality;
                    return this;
                case 21:
                    this.ViolationsHour21 = violation;
                    this.ExtremeViolationsHour21 = extremeViolation;
                    this.DataQualityHour21 = dataQuality;
                    return this;
                case 22:
                    this.ViolationsHour22 = violation;
                    this.ExtremeViolationsHour22 = extremeViolation;
                    this.DataQualityHour22 = dataQuality;
                    return this;
                case 23:
                    this.ViolationsHour23 = violation;
                    this.ExtremeViolationsHour23 = extremeViolation;
                    this.DataQualityHour23 = dataQuality;
                    return this;
                default:
                    return this;
            }

        }



    }
}