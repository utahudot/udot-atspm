using ATSPM.Data.Enums;

namespace ATSPM.ConfigApi.Models
{
    public class RouteDto
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public ICollection<RouteLocationDto> RouteLocations { get; set; }
    }

    public class RouteLocationDto
    {
        public int Order { get; set; }

        /// <summary>
        /// Primary phase number
        /// </summary>
        public int PrimaryPhase { get; set; }

        /// <summary>
        /// Opposing phase number
        /// </summary>
        public int OpposingPhase { get; set; }

        /// <summary>
        /// Related primary direction
        /// </summary>
        public DirectionTypes PrimaryDirectionId { get; set; }

        /// <summary>
        /// Related opposing direction
        /// </summary>
        public DirectionTypes OpposingDirectionId { get; set; }

        /// <summary>
        /// Is primary overlap
        /// </summary>
        public bool IsPrimaryOverlap { get; set; }

        /// <summary>
        /// Is opposing overlap
        /// </summary>
        public bool IsOpposingOverlap { get; set; }

        /// <summary>
        /// Related previous location distance
        /// </summary>
        public int? PreviousLocationDistanceId { get; set; }
        public RouteDistanceDto PreviousLocationDistance { get; set; }

        /// <summary>
        /// Related next location distance
        /// </summary>
        public int? NextLocationDistanceId { get; set; }
        public RouteDistanceDto NextLocationDistance { get; set; }

        /// <inheritdoc/>
        public string LocationIdentifier { get; set; }


        /// <inheritdoc/>
        public int RouteId { get; set; }
    }

    public class RouteDistanceDto
    {
        public int Id { get; set; }
        public double Distance { get; set; }
        public string LocationIdentifierA { get; set; }
        public string LocationIdentifierB { get; set; }
    }

}
