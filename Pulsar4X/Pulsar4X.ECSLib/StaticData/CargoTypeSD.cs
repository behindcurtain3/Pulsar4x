using System;

namespace Pulsar4X.ECSLib
{
    [StaticData(true, IDPropertyName = "ID")]
    public class CargoTypeSD
    {
        public string Name;
        public string Description;
        public StringIdentifier ID;
    }

    public interface ICargoable
    {
        Guid ID { get; }
        string Name { get; }
        StringIdentifier CargoTypeID { get;  }

        /// <summary>
        /// The smallest unit mass. 1 for most minerals etc.
        /// </summary>
        long MassPerUnit { get; }

        double VolumePerUnit { get; }
    }

    [StaticData(true, IDPropertyName = "ID")]
    public class IndustryTypeSD
    {
        public string Name;
        public StringIdentifier ID;
    }
}


