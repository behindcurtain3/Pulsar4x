using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulsar4X.ECSLib.ComponentFeatureSets.CargoStorage
{
    public interface ICargoDefinitionsLibrary
    {
        void LoadDefinitions(List<MineralSD> minerals,
            List<ProcessedMaterialSD> processedMaterials,
            List<ICargoable> otherCargo);

        void LoadMineralDefinitions(List<MineralSD> minerals);
        void LoadMaterialsDefinitions(List<ProcessedMaterialSD> materials);
        void LoadOtherDefinitions(List<ICargoable> otherCargo);

        Dictionary<StringIdentifier, ICargoable> GetAll();

        ICargoable GetAny(StringIdentifier id);

        bool IsOther(StringIdentifier id);
        ICargoable GetOther(string nameOfCargo);
        ICargoable GetOther(StringIdentifier guidOfCargo);

        bool IsMineral(StringIdentifier id);
        MineralSD GetMineral(string name);
        MineralSD GetMineral(StringIdentifier guid);
        Dictionary<StringIdentifier, MineralSD> GetMinerals();
        List<MineralSD> GetMineralsList();

        bool IsMaterial(StringIdentifier id);
        ProcessedMaterialSD GetMaterial(string name);
        ProcessedMaterialSD GetMaterial(StringIdentifier guid);
        Dictionary<StringIdentifier, ProcessedMaterialSD> GetMaterials();
        List<ProcessedMaterialSD> GetMaterialsList();
    }

    public class CargoDefinitionsLibrary : ICargoDefinitionsLibrary
    {
        private Dictionary<StringIdentifier, ICargoable> _definitions;
        private Dictionary<StringIdentifier, MineralSD> _minerals;
        private Dictionary<StringIdentifier, ProcessedMaterialSD> _processedMaterials;

        public CargoDefinitionsLibrary() : this(new List<MineralSD>(),
            new List<ProcessedMaterialSD>(),
            new List<ICargoable>())
        {
        }

        public CargoDefinitionsLibrary(List<MineralSD> minerals,
            List<ProcessedMaterialSD> processedMaterials,
            List<ICargoable> otherCargo)
        {
            _definitions = new Dictionary<StringIdentifier, ICargoable>();
            _minerals = new Dictionary<StringIdentifier, MineralSD>();
            _processedMaterials = new Dictionary<StringIdentifier, ProcessedMaterialSD>();

            LoadDefinitions(minerals, processedMaterials, otherCargo);
        }


        public void LoadDefinitions(List<MineralSD> minerals,
            List<ProcessedMaterialSD> processedMaterials,
            List<ICargoable> otherCargo)
        {
            LoadMineralDefinitions(minerals);
            LoadMaterialsDefinitions(processedMaterials);
            LoadOtherDefinitions(otherCargo);
        }

        public void LoadMineralDefinitions(List<MineralSD> minerals)
        {
            if (minerals != null)
            {
                foreach (var entry in minerals)
                {
                    _definitions[entry.ID] = entry;
                    _minerals[entry.ID] = entry;
                }
            }
        }

        public void LoadMaterialsDefinitions(List<ProcessedMaterialSD> materials)
        {
            if (materials != null)
            {
                foreach (var entry in materials)
                {
                    _definitions[entry.ID] = entry;
                    _processedMaterials[entry.ID] = entry;
                    entry.MineralsRequired?.ToList().ForEach(x => entry.ResourceCosts[x.Key] = x.Value);
                    entry.MaterialsRequired?.ToList().ForEach(x => entry.ResourceCosts[x.Key] = x.Value);
                }
            }
        }

        public void LoadOtherDefinitions(List<ICargoable> otherCargo)
        {
            if (otherCargo != null)
            {
                foreach (var entry in otherCargo)
                {
                    _definitions[entry.ID] = entry;
                }
            }
        }

        public ICargoable GetAny(StringIdentifier id)
        {
            if (_minerals.ContainsKey(id))
                return _minerals[id];

            if (_processedMaterials.ContainsKey(id))
                return _processedMaterials[id];

            if (_definitions.ContainsKey(id))
                return _definitions[id];

            return null;
        }

        public Dictionary<StringIdentifier, ICargoable> GetAll()
        {
            return _definitions;
        }


        public bool IsOther(StringIdentifier id)
        {
            return _definitions.ContainsKey(id) && (IsMineral(id) == false) && (IsMaterial(id) == false);
        }

        public ICargoable GetOther(string nameOfCargo)
        {
            if (_definitions.Values.Any(tg => tg.Name == nameOfCargo))
            {
                return _definitions.Values.Single(tg => tg.Name == nameOfCargo);
            }

            throw new Exception("Cargo item with the name " + nameOfCargo + " not found in TradeGoodLibrary. Was the trade good properly loaded?");
        }

        public ICargoable GetOther(StringIdentifier guidOfCargo)
        {
            return _definitions[guidOfCargo];
        }


        public bool IsMineral(StringIdentifier id)
        {
            return _minerals.ContainsKey(id);
        }

        public MineralSD GetMineral(string name)
        {
            var result = GetOther(name);
            return _minerals[result.ID];
        }

        public MineralSD GetMineral(StringIdentifier guid)
        {
            var result = GetOther(guid);
            return _minerals[result.ID];
        }

        public Dictionary<StringIdentifier, MineralSD> GetMinerals()
        {
            return _minerals;
        }

        public List<MineralSD> GetMineralsList()
        {
            return _minerals.Values.ToList();
        }


        public bool IsMaterial(StringIdentifier id)
        {
            return _processedMaterials.ContainsKey(id);
        }

        public ProcessedMaterialSD GetMaterial(string name)
        {
            var result = GetOther(name);
            return _processedMaterials[result.ID];
        }

        public ProcessedMaterialSD GetMaterial(StringIdentifier guid)
        {
            var result = GetOther(guid);
            return _processedMaterials[result.ID];
        }

        public Dictionary<StringIdentifier, ProcessedMaterialSD> GetMaterials()
        {
            return _processedMaterials;
        }

        public List<ProcessedMaterialSD> GetMaterialsList()
        {
            return _processedMaterials.Values.ToList();
        }
    }
}
