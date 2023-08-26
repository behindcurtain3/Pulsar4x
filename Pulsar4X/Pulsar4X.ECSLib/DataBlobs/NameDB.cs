using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace Pulsar4X.ECSLib
{
    [DebuggerDisplay("{" + nameof(OwnersName) + "}")]
    public class NameDB : BaseDataBlob, ISensorCloneMethod, IGetValuesHash
    {

        /// <summary>
        /// Each faction can have a different name for whatever entity has this blob.
        /// </summary>
        [JsonProperty]
        private readonly Dictionary<StringIdentifier, string> _names = new ();

        [PublicAPI]
        public string DefaultName => _names[new StringIdentifier("player", "default")];

        public string OwnersName
        {
            get
            {
                if (_names.ContainsKey(OwningEntity.FactionOwnerID))
                    return _names[OwningEntity.FactionOwnerID];
                else return DefaultName;
            }
        }

        public NameDB() { _names.Add(new StringIdentifier("player", "default"), "Un-Named");}

        public NameDB(string defaultName)
        {
            _names.Add(new StringIdentifier("player", "default"), defaultName);
        }

        public NameDB(string defaultName, StringIdentifier factionID, string factionsName)
        {
            _names.Add(new StringIdentifier("player", "default"), defaultName);
            _names.Add(factionID, factionsName);
        }

        #region Cloning Interface.

        public NameDB(NameDB nameDB)
        {
            _names = new Dictionary<StringIdentifier, string>(nameDB._names);
        }

        public override object Clone()
        {
            return new NameDB(this);
        }

        #endregion

        [PublicAPI]
        public string GetName(StringIdentifier requestingFaction)
        {
            string name;
            if (!_names.TryGetValue(requestingFaction, out name))
            {
                // Entry not found for the specific entity.
                // Return guid instead. TODO: call an automatic naming function
                if (StaticRefLib.Game.GameMasterFaction.Guid == requestingFaction)
                {
                    name = OwnersName;
                    SetName(requestingFaction, OwnersName);
                }
                else
                {
                    name = OwningEntity.Guid.ToString();
                    SetName(requestingFaction, name);
                }

            }
            return name;
        }

        [PublicAPI]
        public string GetName(Entity requestingFaction)
        {
            return GetName(requestingFaction.Guid);
        }


        [PublicAPI]
        public void SetName(StringIdentifier requestingFaction, string specifiedName)
        {
            _names[requestingFaction] = specifiedName;
            if (requestingFaction == OwningEntity.FactionOwnerID)
            {
                _names[StaticRefLib.SpaceMaster.Guid] = specifiedName;
            }
        }

        public BaseDataBlob SensorClone(SensorInfoDB sensorInfo)
        {
            return new NameDB(this, sensorInfo);
        }

        public int GetValueCompareHash(int hash = 17)
        {
            foreach (var item in _names)
            {
                hash = Misc.ValueHash(item.Key, hash);
                hash = Misc.ValueHash(item.Value, hash);
            }
            return hash;
        }

        public void SensorUpdate(SensorInfoDB sensorInfo)
        {
            //do nothing for this.
        }

        NameDB(NameDB db, SensorInfoDB sensorInfo)
        {
            _names.Add(new StringIdentifier("player", "default"), db.DefaultName);
            _names[sensorInfo.Faction.Guid] = db.GetName(sensorInfo.Faction.Guid);
        }
    }
}