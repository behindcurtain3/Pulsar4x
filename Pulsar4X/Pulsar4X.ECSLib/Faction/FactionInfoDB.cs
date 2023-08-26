using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Pulsar4X.ECSLib.ComponentFeatureSets.Missiles;
using Pulsar4X.ECSLib.Industry;

namespace Pulsar4X.ECSLib
{
    public class FactionInfoDB : BaseDataBlob, IGetValuesHash
    {

        [JsonProperty]
        public List<Entity> Species { get; internal set; } = new List<Entity>();


        [JsonProperty]
        public List<StringIdentifier> KnownSystems { get; internal set; } = new List<StringIdentifier>();


        public ReadOnlyDictionary<StringIdentifier, List<Entity>> KnownJumpPoints => new ReadOnlyDictionary<StringIdentifier, List<Entity>>(InternalKnownJumpPoints);
        [JsonProperty]
        internal Dictionary<StringIdentifier, List<Entity>> InternalKnownJumpPoints = new Dictionary<StringIdentifier, List<Entity>>();


        [JsonProperty]
        public List<Entity> KnownFactions { get; internal set; } = new List<Entity>();


        [PublicAPI]
        [JsonProperty]
        public List<Entity> Colonies { get; internal set; } = new List<Entity>();

        [JsonProperty]
        public Dictionary<StringIdentifier, ShipDesign> ShipDesigns = new ();

        [JsonProperty]
        public Dictionary<StringIdentifier, OrdnanceDesign> MissileDesigns = new ();

        public ReadOnlyDictionary<StringIdentifier, ComponentDesign> ComponentDesigns => new ReadOnlyDictionary<StringIdentifier, ComponentDesign>(InternalComponentDesigns);
        [JsonProperty]
        internal Dictionary<StringIdentifier, ComponentDesign> InternalComponentDesigns = new Dictionary<StringIdentifier, ComponentDesign>();


        public Dictionary<StringIdentifier, IConstrucableDesign> IndustryDesigns = new ();



        [JsonProperty]
        /// <summary>
        /// stores sensor contacts for the entire faction, when a contact is created it gets added here.
        /// </summary>
        internal Dictionary<StringIdentifier, SensorContact> SensorContacts = new Dictionary<StringIdentifier, SensorContact>();

        public Dictionary<EventType, bool> HaltsOnEvent { get; } = new Dictionary<EventType, bool>();

        [JsonProperty]
        private Dictionary<Entity, uint> FactionAccessRoles { get; set; } = new Dictionary<Entity, uint>();
        internal ReadOnlyDictionary<Entity, AccessRole> AccessRoles => new ReadOnlyDictionary<Entity, AccessRole>(FactionAccessRoles.ToDictionary(kvp => kvp.Key, kvp => (AccessRole)kvp.Value));



        public FactionInfoDB()
        {
            var componentDesigns = new Dictionary<StringIdentifier, ComponentDesign>();
            var shipClasses = new Dictionary<StringIdentifier, ShipDesign>();
            SetIndustryDesigns(componentDesigns, shipClasses);
            HaltsOnEvent.Add(EventType.OrdersHalt, true);
        }

        public FactionInfoDB(
            List<Entity> species,
            List<StringIdentifier> knownSystems,
            List<Entity> colonies,
            Dictionary<StringIdentifier, ComponentDesign> componentDesigns,
            Dictionary<StringIdentifier, ShipDesign> shipClasses)
        {
            Species = species;
            KnownSystems = knownSystems;
            Colonies = colonies;
            InternalComponentDesigns = componentDesigns;
            ShipDesigns = shipClasses;
            KnownFactions = new List<Entity>();
            SetIndustryDesigns(componentDesigns, shipClasses);
            HaltsOnEvent.Add(EventType.OrdersHalt, true);
        }


        public FactionInfoDB(FactionInfoDB factionDB)
        {
            Species = new List<Entity>(factionDB.Species);
            KnownSystems = new List<StringIdentifier>(factionDB.KnownSystems);
            KnownFactions = new List<Entity>(factionDB.KnownFactions);
            Colonies = new List<Entity>(factionDB.Colonies);
            InternalKnownJumpPoints = new Dictionary<StringIdentifier, List<Entity>>(factionDB.KnownJumpPoints);

            ShipDesigns = new Dictionary<StringIdentifier, ShipDesign>(factionDB.ShipDesigns);
            InternalComponentDesigns = new Dictionary<StringIdentifier, ComponentDesign>(factionDB.ComponentDesigns);
            IndustryDesigns = new Dictionary<StringIdentifier, IConstrucableDesign>(factionDB.IndustryDesigns);
            HaltsOnEvent.Add(EventType.OrdersHalt, true);

        }

        public override object Clone()
        {
            return new FactionInfoDB(this);
        }

        void SetIndustryDesigns(
            Dictionary<StringIdentifier, ComponentDesign> componentDesigns,
            Dictionary<StringIdentifier, ShipDesign> shipClasses)
        {
            foreach (var mat in StaticRefLib.StaticData.CargoGoods.GetMaterialsList())
            {
                IndustryDesigns[mat.ID] = mat;
            }
            foreach (var design in componentDesigns)
            {
                IndustryDesigns[design.Key] = design.Value;
            }
            foreach (var design in shipClasses)
            {
                IndustryDesigns[design.Key] = design.Value;
            }
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            ((Game)context.Context).PostLoad += (sender, args) => { };
        }

        public int GetValueCompareHash(int hash = 17)
        {
            foreach (var item in Species)
            {
                hash = Misc.ValueHash(item.Guid, hash);
            }
            foreach (var item in KnownSystems)
            {
                hash = Misc.ValueHash(item, hash);
            }
            foreach (var item in KnownFactions)
            {
                hash = Misc.ValueHash(item.Guid, hash);
            }
            foreach (var item in Colonies)
            {
                hash = Misc.ValueHash(item.Guid, hash);
            }
            foreach (var item in ShipDesigns.Keys)
            {
                hash = Misc.ValueHash(item, hash);
            }
            foreach (var item in InternalComponentDesigns)
            {
                hash = Misc.ValueHash(item.Key, hash);
                hash = Misc.ValueHash(item.Value.ID, hash);
            }
            foreach (var system in InternalKnownJumpPoints)
            {
                hash = Misc.ValueHash(system.Key, hash);
                foreach (var jp in system.Value)
                {
                    hash = Misc.ValueHash(jp.Guid, hash);
                }

            }

            return hash;
        }
    }
}