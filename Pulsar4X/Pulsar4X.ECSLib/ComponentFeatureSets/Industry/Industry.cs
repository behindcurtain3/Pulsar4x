using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Pulsar4X.ECSLib.Industry
{
    [Flags]
    public enum ConstructionType
    {
        None            = 1 << 1,
        Installations   = 1 << 2,
        ShipComponents  = 1 << 3,
        Fighters        = 1 << 4,
        Ordinance        = 1 << 5,
    }

    public interface IIndustryDB
    {
        int ConstructionPoints { get; }

        List<JobBase> JobBatchList { get; }

        List<IConstrucableDesign> GetJobItems(FactionInfoDB factionInfoDB);

    }

    public class IndustryAtb : IComponentDesignAttribute
    {
        [JsonProperty]
        public Dictionary<StringIdentifier, int> IndustryPoints { get; private set; } = new ();

        [JsonProperty]
        private double MaxProductionVolume;

        public IndustryAtb(Dictionary<StringIdentifier, double> industryRates)
        {
            MaxProductionVolume = double.PositiveInfinity;

            int i = 0;
            foreach (var kvp in industryRates)
            {
                IndustryPoints[kvp.Key] = (int)kvp.Value;
                i++;
            }
        }

        public IndustryAtb(Dictionary<StringIdentifier, double> industryRates, double maxProductionVolume)
        {
            MaxProductionVolume = maxProductionVolume;

            int i = 0;
            foreach (var kvp in industryRates)
            {
                IndustryPoints[kvp.Key] = (int)kvp.Value;
                i++;
            }
        }

        public void OnComponentInstallation(Entity parentEntity, ComponentInstance componentInstance)
        {
            var db = parentEntity.GetDataBlob<IndustryAbilityDB>();
            IndustryAbilityDB.ProductionLine newline = new();
            newline.MaxVolume = MaxProductionVolume;
            newline.IndustryTypeRates = IndustryPoints;
            newline.Name = componentInstance.Name;

            if (db == null)
            {
                db = new IndustryAbilityDB(componentInstance.ID, newline);
                parentEntity.SetDataBlob(db);
            }
            else
            {
                db.ProductionLines.Add(componentInstance.ID, newline);
            }
        }

        public string AtbName()
        {
            return "Industry";
        }

        public string AtbDescription()
        {
            string industryTypesAndPoints = "";
            foreach (var kvp in IndustryPoints)
            {
                var name = StaticRefLib.StaticData.IndustryTypes[kvp.Key].Name;
                var amount = kvp.Value;

                industryTypesAndPoints += name + "\t" + amount + "\n";
            }
            return industryTypesAndPoints;
        }

    }
    public enum ConstructableGuiHints
    {
        None,
        CanBeLaunched,
        CanBeInstalled,
        IsOrdinance
    }
    public interface IConstrucableDesign
    {
        ConstructableGuiHints GuiHints { get; }

        StringIdentifier ID { get;  }
        string Name { get;  } //player defined name. ie "5t 2kn Thruster".

        Dictionary<StringIdentifier, long> ResourceCosts { get; }

        long IndustryPointCosts { get; }
        StringIdentifier IndustryTypeID { get; }
        ushort OutputAmount { get; }
        void OnConstructionComplete(Entity industryEntity, VolumeStorageDB storage, StringIdentifier productionLine, IndustryJob batchJob, IConstrucableDesign designInfo);

    }

    public abstract class JobBase
    {
        public virtual string Name { get; internal set; }
        public StringIdentifier JobID = new StringIdentifier("player", Guid.NewGuid().ToString());
        public StringIdentifier ItemGuid { get; protected set; }
        public ushort NumberOrdered { get; set; }
        public ushort NumberCompleted { get; internal set; }

        /// <summary>
        /// for single item under construction.
        /// </summary>
        public long ProductionPointsLeft
        {
            get;
            internal set;
        }

        /// <summary>
        /// Per Item
        /// </summary>
        public long ProductionPointsCost { get; protected set; }
        public bool Auto { get; internal set; }

        public Dictionary<StringIdentifier, long> ResourcesRequiredRemaining { get; internal set; } = new ();
        public Dictionary<StringIdentifier, long> ResourcesCosts { get; internal set; } = new ();

        public JobBase()
        {
        }

        public JobBase(StringIdentifier guid, ushort numberOrderd, int jobPoints, bool auto)
        {
            ItemGuid = guid;
            NumberOrdered = numberOrderd;
            NumberCompleted = 0;
            ProductionPointsLeft = jobPoints;
            ProductionPointsCost = jobPoints;
            Auto = auto;
        }

        public abstract void InitialiseJob(ushort numberOrderd, bool auto);

    }

    public class IndustryJob : JobBase
    {
        internal StringIdentifier TypeID;

        public IndustryJob(FactionInfoDB factionInfo, StringIdentifier itemID)
        {
            ItemGuid = itemID;
            var design = factionInfo.IndustryDesigns[itemID];
            TypeID = design.IndustryTypeID;
            Name = design.Name;
            ResourcesRequiredRemaining = new Dictionary<StringIdentifier, long>(design.ResourceCosts);
            ResourcesCosts = design.ResourceCosts;
            ProductionPointsLeft = design.IndustryPointCosts;
            ProductionPointsCost = design.IndustryPointCosts;
            NumberOrdered = 1;
        }

        public Entity InstallOn { get; set; } = null;

        public override void InitialiseJob(ushort numberOrderd, bool auto)
        {
            NumberOrdered = numberOrderd;
            NumberCompleted = 0;
            Auto = auto;
        }
    }

    public class IndustryOrder2 : EntityCommand
    {

        public override string Name
        {
            get
            {
                return "Industry: " + OrderType.ToString();
            }
        }


        public override string Details { get; } = "Instant";

        public enum OrderTypeEnum
        {
            NewJob,
            CancelJob,
            ChangePriority,
            EditJob
        }
        public OrderTypeEnum OrderType;

        public StringIdentifier ItemID { get; set; }
        public ushort NumberOrderd { get; set; }
        public bool RepeatJob { get; set; } = false;
        public bool AutoInstall { get; set; } = false;

        public short Delta { get; set; }

        public override ActionLaneTypes ActionLanes => ActionLaneTypes.IneteractWithSelf;
        public override bool IsBlocking => true; //?why block?


        private Entity _entityCommanding;

        private StringIdentifier productionLineID;
        internal override Entity EntityCommanding{get{return _entityCommanding;}}


        private Entity _factionEntity;
        // private ComponentDesign _design;
        private IndustryJob _job;



        public static IndustryOrder2 CreateNewJobOrder(
            StringIdentifier factionGuid, Entity thisEntity, StringIdentifier productionLineID,
            IndustryJob jobItem
        )
        {

            IndustryOrder2 order = new IndustryOrder2(factionGuid, thisEntity);
            order._job = jobItem;
            order.OrderType = OrderTypeEnum.NewJob;
            order.ItemID = jobItem.ItemGuid;
            order.productionLineID = productionLineID;
            return order;
        }



        public static IndustryOrder2 CreateCancelJobOrder(
            StringIdentifier factionGuid, Entity thisEntity, StringIdentifier productionLineID,
            StringIdentifier OrderID
        )
        {
            IndustryOrder2 order = new IndustryOrder2(factionGuid, thisEntity);
            order.OrderType = OrderTypeEnum.CancelJob;
            order.ItemID = OrderID;
            order.productionLineID = productionLineID;
            return order;
        }

        public static IndustryOrder2 CreateChangePriorityOrder(
            StringIdentifier factionGuid, Entity thisEntity,
            StringIdentifier productionLineID,
            StringIdentifier OrderID, short delta
        )
        {
            IndustryOrder2 order = new IndustryOrder2(factionGuid, thisEntity);
            order.OrderType = OrderTypeEnum.ChangePriority;
            order.ItemID = OrderID;
            order.Delta = delta;
            order.productionLineID = productionLineID;
            return order;
        }

        public static IndustryOrder2 CreateEditJobOrder(
            StringIdentifier factionGuid, Entity thisEntity, StringIdentifier productionLineID,
            StringIdentifier OrderID, ushort quantity = 1, bool repeatJob = false, bool autoInstall = false
        )
        {
            IndustryOrder2 order = new IndustryOrder2(factionGuid, thisEntity);
            order.OrderType = OrderTypeEnum.EditJob;
            order.ItemID = OrderID;
            order.NumberOrderd = quantity;
            order.RepeatJob = repeatJob;
            order.AutoInstall = autoInstall;
            order.productionLineID = productionLineID;
            return order;
        }


        private IndustryOrder2(StringIdentifier factionGuid, Entity thisEntity)
        {
            RequestingFactionGuid = factionGuid;
            EntityCommandingGuid = thisEntity.Guid;
            CreatedDate = thisEntity.StarSysDateTime;
            UseActionLanes = false;
        }


        internal override void ActionCommand(DateTime atDateTime)
        {
            if (!IsRunning)
            {
                switch (OrderType)
                {
                    case OrderTypeEnum.NewJob:
                        IndustryTools.AddJob( _entityCommanding, productionLineID, _job);
                        break;
                    case OrderTypeEnum.CancelJob:
                        IndustryTools.CancelExsistingJob(_entityCommanding, productionLineID, ItemID);
                        break;
                    case OrderTypeEnum.EditJob:
                        IndustryTools.EditExsistingJob(_entityCommanding, productionLineID, ItemID, RepeatJob, NumberOrderd, AutoInstall);
                        break;
                    case OrderTypeEnum.ChangePriority:
                        IndustryTools.ChangeJobPriority(_entityCommanding, productionLineID, ItemID, Delta);
                        break;
                }


                IsRunning = true;
            }
        }

        internal override bool IsValidCommand(Game game)
        {

            if (CommandHelpers.IsCommandValid(game.GlobalManager, RequestingFactionGuid, EntityCommandingGuid, out _factionEntity, out _entityCommanding))
            {
                var factionInfo = _factionEntity.GetDataBlob<FactionInfoDB>();
                //_job.InitialiseJob(factionInfo, _entityCommanding, ItemID, NumberOrderd, RepeatJob);

                /* TODO: should we check this? do we need to?
                if (factionInfo.ComponentDesigns.ContainsKey(ItemID))
                {
                    _design = factionInfo.ComponentDesigns[ItemID];
                    _job = new ConstructJob(_design, NumberOrderd, RepeatJob);
                    if (_design.IndustryTypeID.HasFlag(IndustryTypeID.Installations))
                        _job.InstallOn = _entityCommanding;
                    return true;

                }
                */
                return true;
            }
            return false;
        }

        public override bool IsFinished()
        {
            if (_job.Auto == false && _job.NumberCompleted == _job.NumberOrdered)
            {
                return true;
            }
            return false;
        }

    }
}