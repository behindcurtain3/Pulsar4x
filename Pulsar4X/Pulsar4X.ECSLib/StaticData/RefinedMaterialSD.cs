using System;
using System.Collections.Generic;
using Pulsar4X.ECSLib.Industry;

namespace Pulsar4X.ECSLib
{
    [StaticData(true, IDPropertyName = "ID")]
    public class ProcessedMaterialSD : ICargoable, IConstrucableDesign
    {
        public string Name { get; init; }
        public Dictionary<StringIdentifier, long> ResourceCosts { get; } = new ();

        public long IndustryPointCosts
        {
            get;
            init;
        }
        public StringIdentifier IndustryTypeID { get; set; }

        public void OnConstructionComplete(Entity industryEntity, VolumeStorageDB storage, StringIdentifier productionLine, IndustryJob batchJob, IConstrucableDesign designInfo)
        {
            var industryDB = industryEntity.GetDataBlob<IndustryAbilityDB>();
            ProcessedMaterialSD material = (ProcessedMaterialSD)designInfo;
            storage.AddCargoByUnit(material, OutputAmount);
            batchJob.ProductionPointsLeft = batchJob.ProductionPointsCost; //and reset the points left for the next job in the batch.
            batchJob.NumberCompleted += 1;
            if (batchJob.NumberCompleted == batchJob.NumberOrdered)
            {
                industryDB.ProductionLines[productionLine].Jobs.Remove(batchJob);
                if (batchJob.Auto)
                {
                    batchJob.NumberCompleted = 0;
                    industryDB.ProductionLines[productionLine].Jobs.Add(batchJob);
                }
            }
        }

        public string Description;
        public ConstructableGuiHints GuiHints { get; } = ConstructableGuiHints.None;
        public StringIdentifier ID { get; init; }

        public Dictionary<StringIdentifier, long> MineralsRequired;
        public Dictionary<StringIdentifier, long> MaterialsRequired;

        public ushort WealthCost;
        public ushort OutputAmount { get; init; }
        public StringIdentifier CargoTypeID { get; init; }
        public long MassPerUnit { get; init; }
        public double VolumePerUnit { get; init; }
    }
}