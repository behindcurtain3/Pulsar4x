using Newtonsoft.Json;
using Pulsar4X.ECSLib.ComponentFeatureSets.CargoStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Pulsar4X.ECSLib
{
    /// <summary>
    /// This class acts as a simple store of all the games static data.
    /// It is saved alongside the rest of the game data.
    /// This class is generaly managed by the StaticDataManager.
    /// </summary>
    public class StaticDataStore
    {
        private static readonly Dictionary<StringIdentifier, object> Data = new();

        internal void Store(StringIdentifier id, object data)
        {
            // Check for bad data or bad id
            if(data == null || id.ToString().IsNullOrEmpty() || id.Name.IsNullOrEmpty()) return;

            if(Data.ContainsKey(id))
            {
                // override existing data
                Data[id] = data;
            }
            else
            {
                Data.Add(id, data);
            }
        }

        /// <summary>
        /// Easily convert string to Type
        /// </summary>
        private static readonly Dictionary<string, Type> StringsToTypes = InitializeStringsToTypes();

        /// <summary>
        /// Reverse dictionary of the above.
        /// </summary>
        private static readonly Dictionary<Type, string> TypesToStrings = InitializeTypesToStrings();

        /// <summary>
        /// List which stores all the atmospheric gases.
        /// </summary>
        [JsonIgnore]
        public WeightedList<AtmosphericGasSD> AtmosphericGases = new WeightedList<AtmosphericGasSD>();

        public AtmosphericGasSD GetAtmosGasByName(string name)
        {
            foreach (var gas in AtmosphericGases)
            {
                if (gas.Value.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return gas.Value;
                }
            }

            throw new Exception("Atmospheric Gas " + name + " Not Found.");
        }

        public AtmosphericGasSD GetAtmosGasBySymbol(string chemicalSymbol)
        {
            foreach (var gas in AtmosphericGases)
            {
                if (gas.Value.ChemicalSymbol.Equals(chemicalSymbol, StringComparison.InvariantCultureIgnoreCase))
                {
                    return gas.Value;
                }
            }

            throw new Exception("Atmospheric Gas with symbol " + chemicalSymbol + " Not Found.");
        }

        /// <summary>
        /// List which stores all the Commander Name themes.
        /// </summary>
        [JsonIgnore]
        public List<CommanderNameThemeSD> CommanderNameThemes = new List<CommanderNameThemeSD>();

        /// <summary>
        /// Dictionary which stores all the Minerals.
        /// </summary>
        [JsonIgnore]
        public ICargoDefinitionsLibrary CargoGoods = new CargoDefinitionsLibrary();

        /// <summary>
        /// Dictionary which stores all the Technologies.
        /// stored in a dictionary to allow fast lookup of a specific Technology based on its guid.
        /// </summary>
        [JsonIgnore]
        public Dictionary<StringIdentifier, TechSD> Techs = new ();

        /// <summary>
        /// Dictionary which stores all Components.
        /// </summary>
        [JsonIgnore]
        public Dictionary<Guid, ComponentTemplateSD> ComponentTemplates = new Dictionary<Guid, ComponentTemplateSD>();

        /// <summary>
        /// Stores ComponentTemplates by the Attribute Type Name.
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, List<ComponentTemplateSD>> ComponentTemplatesByAttribute = new Dictionary<string, List<ComponentTemplateSD>>();

        /// <summary>
        /// Dictionary to store CargoTypes
        /// </summary>
        [JsonIgnore]
        public Dictionary<StringIdentifier, CargoTypeSD> CargoTypes = new ();

        [JsonIgnore]
        public Dictionary<StringIdentifier, IndustryTypeSD> IndustryTypes = new ();

        public Dictionary<StringIdentifier, ArmorSD> ArmorTypes = new ();

        /// <summary>
        /// Settings used by system generation.
        /// @todo make Galaxy gen use this instead of default data (DO NOT DELETE THE HARD CODED DATA THO, that should be a fall back).
        /// </summary>
        public SystemGenSettingsSD SystemGenSettings;

        /// <summary>
        /// This list holds the version info of all the loaded data sets.
        /// </summary>
        [PublicAPI]
        [JsonIgnore]
        public List<DataVersionInfo> LoadedDataSets => _loadedDataSets;

        [JsonProperty]
        private List<DataVersionInfo> _loadedDataSets;

        public StaticDataStore()
        {
            _loadedDataSets = new List<DataVersionInfo>();
        }

        #region Static field initializers

        /// <summary>
        /// Initializes the StringsToTypes static dictionary.
        /// </summary>
        private static Dictionary<string, Type> InitializeStringsToTypes()
        {
            return new Dictionary<string, Type>
            {
                {
                    "AtmosphericGases", typeof(WeightedList<AtmosphericGasSD>)
                },
                {
                    "CommanderNameThemes", typeof(List<CommanderNameThemeSD>)
                },
                {
                    "Minerals", typeof(Dictionary<Guid, MineralSD>)
                },
                {
                    "Techs", typeof(Dictionary<StringIdentifier, TechSD>)
                },
                {
                    "ProcessedMaterials", typeof(Dictionary<Guid, ProcessedMaterialSD>)
                },
                {
                    "ComponentTemplates", typeof(Dictionary<Guid, ComponentTemplateSD>)
                },
                {
                    "CargoTypes", typeof(Dictionary<StringIdentifier, CargoTypeSD>)
                },
                {
                    "IndustryTypes", typeof(Dictionary<StringIdentifier, IndustryTypeSD>)
                },
                {
                    "ArmorTypes", typeof(Dictionary<StringIdentifier, ArmorSD>)
                },
                {
                    "SystemGenSettings", typeof(SystemGenSettingsSD)
                },
                {
                    "VersionInfo", typeof(VersionInfo)
                }
            };
        }

        /// <summary>
        /// Initializes the TypesToStrings static dictionary.
        /// //todo work out if this is safe? doe .Net gurentee that InitializeStringsToTypes() is called first somehow??
        /// </summary>
        /// <returns></returns>
        private static Dictionary<Type, string> InitializeTypesToStrings()
        {
            return new Dictionary<Type, string>
            {
                {
                    typeof(WeightedList<AtmosphericGasSD>), "AtmosphericGases"
                },
                {
                    typeof(List<CommanderNameThemeSD>), "CommanderNameThemes"
                },
                {
                    typeof(List<MineralSD>), "Minerals"
                },
                {
                    typeof(Dictionary<StringIdentifier, TechSD>), "Techs"
                },
                {
                    typeof(Dictionary<Guid, ProcessedMaterialSD>), "RefinedMaterials"
                },
                {
                    typeof(Dictionary<Guid, ComponentTemplateSD>), "Components"
                },
                {
                    typeof(Dictionary<StringIdentifier, CargoTypeSD>), "CargoTypes"
                },
                {
                    typeof(Dictionary<StringIdentifier, IndustryTypeSD>), "IndustryTypes"
                },
                {
                    typeof(Dictionary<StringIdentifier, ArmorSD>), "ArmorTypes"
                },
                {
                    typeof(SystemGenSettingsSD), "SystemGenSettings"
                },
                {
                    typeof(VersionInfo), "VersionInfo"
                }
            };
        }

        #endregion

        #region Public API

        /// <summary>
        /// This functin goes through each Static Data type in the store looking for one that has an ID that
        /// matches the one provided.
        /// Returns null if the id is not found.
        /// </summary>
        [PublicAPI]
        [CanBeNull]
        public object FindDataObjectUsingID(Guid id)
        {
            var cargoGood = CargoGoods.GetAny(id);
            if (cargoGood != null)
                return cargoGood;

            if (ComponentTemplates.ContainsKey(id))
                return ComponentTemplates[id];

            return null;
        }

        public object FindDataObjectUsingID(StringIdentifier id)
        {
            if (Techs.ContainsKey(id))
                return Techs[id];

            if (CargoTypes.ContainsKey(id))
                return CargoTypes[id];
            return null;
        }

        public Dictionary<Guid, StringIdentifier> StorageTypeMap = new ();
        internal void SetStorageTypeMap()
        {
            StorageTypeMap.Clear();
            var allCargoDefs = CargoGoods.GetAll();
            foreach (var item in allCargoDefs)
                StorageTypeMap.Add(item.Key, item.Value.CargoTypeID);
            foreach (var item in ComponentTemplates)
                StorageTypeMap.Add(item.Key, item.Value.CargoTypeID);
        }


        public ICargoable GetICargoable(Guid id)
        {
            return (ICargoable)CargoGoods.GetAny(id);
        }

        #endregion

        #region Private functions

        #region private void Store(dynamic object) overloads.

        /// <summary>
        /// Stores Atmospheric Gas Static Data.
        /// </summary>
        internal void Store(WeightedList<AtmosphericGasSD> atmosphericGases)
        {
            if (atmosphericGases != null)
            {
                foreach (WeightedValue<AtmosphericGasSD> atmosphericGas in atmosphericGases)
                {
                    if (AtmosphericGases.ContainsValue(atmosphericGas.Value))
                    {
                        // Update existing value
                        int index = AtmosphericGases.IndexOf(atmosphericGas.Value);
                        AtmosphericGases[index] = atmosphericGas;
                    }
                    else
                    {
                        // Add new value
                        AtmosphericGases.Add(atmosphericGas);
                    }
                }
            }
        }

        /// <summary>
        /// Stores Commander Name Themes.
        /// </summary>
        internal void Store(List<CommanderNameThemeSD> commanderNameThemes)
        {
            if (commanderNameThemes != null)
            {
                foreach (CommanderNameThemeSD commanderNameThemeSD in commanderNameThemes)
                {
                    if (CommanderNameThemes.Contains(commanderNameThemeSD))
                    {
                        // Update existing value.
                        int index = CommanderNameThemes.IndexOf(commanderNameThemeSD);
                        CommanderNameThemes[index] = commanderNameThemeSD;
                    }
                    else
                    {
                        // Add new value.
                        CommanderNameThemes.Add(commanderNameThemeSD);
                    }
                }
            }
        }

        /// <summary>
        /// Stores Mineral Static Data. Will overwrite an existing mineral if the IDs match.
        /// </summary>
        internal void Store(Dictionary<Guid, MineralSD> minerals)
        {
            if (minerals != null)
            {
                CargoGoods.LoadMineralDefinitions(minerals.Values.ToList());
            }
        }

        /// <summary>
        /// Stores Technology Static Data. Will overwrite any existing Techs with the same ID.
        /// </summary>
        internal void Store(Dictionary<StringIdentifier, TechSD> techs)
        {
            if (techs != null)
            {
                foreach (var (key, tech) in techs)
                {
                    tech.ID = key;
                    Techs[key] = tech;
                    Store(key, tech);
                }
            }
        }


        /// <summary>
        /// Stores ConstructableObj Static Data. Will overwrite any existing ConstructableObjs with the same ID.
        /// </summary>
        internal void Store(Dictionary<Guid, ProcessedMaterialSD> recipes)
        {
            if (recipes != null)
            {
                CargoGoods.LoadMaterialsDefinitions(recipes.Values.ToList());
            }
        }

        /// <summary>
        /// Stores Component Static Data. Will overwrite any existing Component with the same ID.
        /// </summary>
        internal void Store(Dictionary<Guid, ComponentTemplateSD> components)
        {
            if (components != null)
            {
                //ComponentTemplatesByAttribute = new Dictionary<string, List<ComponentTemplateSD>>();
                foreach (KeyValuePair<Guid, ComponentTemplateSD> component in components)
                {
                    ComponentTemplates[component.Key] = component.Value;
                    foreach (ComponentTemplateAttributeSD attrbSD in component.Value.ComponentAtbSDs)
                    {
                        if(attrbSD.AttributeType != null)
                        {
                            if (!ComponentTemplatesByAttribute.TryGetValue(attrbSD.AttributeType, out List<ComponentTemplateSD> lst))
                            {
                                lst = new List<ComponentTemplateSD>();
                                ComponentTemplatesByAttribute.Add(attrbSD.AttributeType, lst);
                            }

                            lst.Add(component.Value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Stores cargoType Static Data. Will overwrite any existing Component with the same ID.
        /// </summary>
        internal void Store(Dictionary<StringIdentifier, CargoTypeSD> cargoTypes)
        {
            if (cargoTypes != null)
            {
                foreach (var (key, cargoType) in cargoTypes)
                {
                    cargoType.ID = key;
                    Store(key, cargoType);
                    CargoTypes[key] = cargoType;
                }
            }
        }

        internal void Store(Dictionary<StringIdentifier, IndustryTypeSD> industryTypes)
        {
            if (industryTypes != null)
            {
                foreach (var (key, industryType) in industryTypes)
                {
                    industryType.ID = key;
                    IndustryTypes[key] = industryType;
                    Store(key, industryType);
                }
            }
        }

        internal void Store(Dictionary<StringIdentifier, ArmorSD> armorTypes)
        {
            if (armorTypes != null)
            {
                foreach (var (key, armorType) in armorTypes)
                {
                    armorType.ResourceID = key;
                    Store(key, armorType);
                    ArmorTypes[key] = armorType;
                }
            }
        }

        internal void Store(SystemGenSettingsSD settings)
        {
            SystemGenSettings = settings;
        }

        #endregion

        /// <summary>
        /// Returns a type custom string for a type of static data. This string is used to tell
        /// what type of static data is being imported (and is thus exported as well).
        /// </summary>
        public static string GetTypeString(Type type)
        {
            string s;
            TypesToStrings.TryGetValue(type, out s);
            return s;
        }

        /// <summary>
        /// Gets the matching type for a type string. Used when importing previously exported
        /// static data to know what type to import it as.
        /// </summary>
        public static Type GetType(string typeString)
        {
            return StringsToTypes[typeString];
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            foreach (string dataSet in _loadedDataSets.Select(dataVersionInfo => dataVersionInfo.Directory).ToList())
            {
                StaticDataManager.LoadData(dataSet, (Game)context.Context);
            }
            SetStorageTypeMap();
        }
        #endregion

    }


}