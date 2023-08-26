using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Pulsar4X.ECSLib
{

    public class VolumeStorageDB : BaseDataBlob, IAbilityDescription
    {
        public Dictionary<StringIdentifier, TypeStore> TypeStores = new();
        public double TotalStoredMass { get; internal set; } = 0;

        public int TransferRateInKgHr { get; set; } = 500;

        public double TransferRangeDv_mps { get; set; } = 100;

        [JsonConstructor]
        internal VolumeStorageDB()
        {
        }

        public VolumeStorageDB(StringIdentifier type, double maxVolume)
        {
            TypeStores.Add(type, new TypeStore(maxVolume));
        }


        public VolumeStorageDB(VolumeStorageDB db)
        {
            TypeStores = new Dictionary<StringIdentifier, TypeStore>();
            foreach (var kvp in db.TypeStores)
            {
                TypeStores.Add(kvp.Key, kvp.Value.Clone());
            }
            TotalStoredMass = db.TotalStoredMass;
            TransferRangeDv_mps = db.TransferRangeDv_mps;
            TransferRateInKgHr = db.TransferRateInKgHr;
        }

        public override object Clone()
        {
            return new VolumeStorageDB(this);
        }

        public string AbilityName()
        {
            return "Cargo Volume";
        }

        public string AbilityDescription()
        {
            string desc = "Total Volume storage\n";
            foreach (var kvp in TypeStores)
            {
                string name = StaticRefLib.StaticData.CargoTypes[kvp.Key].Name;
                desc += name + "\t" + kvp.Value.MaxVolume + "\n";
            }

            return desc;
        }
    }

    public class TypeStore
    {
        public double MaxVolume;
        internal double FreeVolume;
        public Dictionary<StringIdentifier, long> CurrentStoreInUnits = new Dictionary<StringIdentifier, long>();
        public Dictionary<StringIdentifier, ICargoable> Cargoables =  new Dictionary<StringIdentifier, ICargoable>();
        public TypeStore(double maxVolume)
        {
            MaxVolume = maxVolume;
            FreeVolume = maxVolume;
        }



        public TypeStore Clone()
        {
            TypeStore clone = new TypeStore(MaxVolume);
            clone.FreeVolume = FreeVolume;
            clone.CurrentStoreInUnits = new Dictionary<StringIdentifier, long>(CurrentStoreInUnits);
            clone.Cargoables = new Dictionary<StringIdentifier, ICargoable>(Cargoables);
            return clone;
        }

    }

    public class VolumeStorageAtb : IComponentDesignAttribute
    {
        public StringIdentifier StoreTypeID;
        public double MaxVolume;

        public VolumeStorageAtb(string storageTypeID, double maxVolume)
        {
            StoreTypeID = new StringIdentifier(storageTypeID);
            MaxVolume = maxVolume;
        }

        public VolumeStorageAtb(StringIdentifier storeTypeID, double maxVolume)
        {
            StoreTypeID = storeTypeID;
            MaxVolume = maxVolume;
        }

        public void OnComponentInstallation(Entity parentEntity, ComponentInstance componentInstance)
        {
            if (!parentEntity.HasDataBlob<VolumeStorageDB>())
            {
                var newdb = new VolumeStorageDB(StoreTypeID, MaxVolume);
                parentEntity.SetDataBlob(newdb);
            }
            else
            {
                var db = parentEntity.GetDataBlob<VolumeStorageDB>();
                if (db.TypeStores.ContainsKey(StoreTypeID))
                {
                    db.TypeStores[StoreTypeID].MaxVolume += MaxVolume;
                    db.TypeStores[StoreTypeID].FreeVolume += MaxVolume;
                }
                else
                {
                    db.TypeStores.Add(StoreTypeID, new TypeStore(MaxVolume));
                }
            }
        }

        public string AtbName()
        {
            return "Cargo Volume";
        }

        public string AtbDescription()
        {
            return "Adds " + MaxVolume + " m^3 Volume to parent cargo storage";
        }
    }


}
