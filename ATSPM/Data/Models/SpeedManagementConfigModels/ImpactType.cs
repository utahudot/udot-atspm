namespace ATSPM.Data.Models.SpeedManagementConfigModels
{
    using System;

    public class ImpactType : IEquatable<ImpactType>
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ImpactType);
        }

        public bool Equals(ImpactType? other)
        {
            if (other == null)
                return false;

            return Id == other.Id &&
                   Name == other.Name &&
                   Description == other.Description;
        }
    }

}