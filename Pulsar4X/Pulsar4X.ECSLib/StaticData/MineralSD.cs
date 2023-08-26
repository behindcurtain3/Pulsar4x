using System;
using System.Collections.Generic;

namespace Pulsar4X.ECSLib
{
    [StaticData(true, IDPropertyName = "ID")]
    public class MineralSD : ICargoable
    {
        public string Name { get; set; }
        public string Description;
        public StringIdentifier ID { get; set; }
        public StringIdentifier CargoTypeID { get; set; }
        public long MassPerUnit { get; set; }
        public double VolumePerUnit { get; set; }
        public Dictionary<BodyType, double> Abundance;
    }
}
