using NUnit.Framework;
using Pulsar4X.ECSLib;
using Pulsar4X.Orbital;
using System;
using System.Collections.Generic;

namespace Pulsar4X.Tests
{
    [TestFixture]
    [Description("Ship Entity Tests")]
    internal class ShipEntityTests
    {
        private Game _game;
        // private EntityManager _entityManager;
        private Entity _faction;
        private StarSystem _starSystem;
        private ShipDesign _shipDesign;
        private Entity _ship;
        private ComponentDesign _engineComponentDesign;
        // private ComponentTemplateSD _engineSD;
        private Entity _sol;

        [SetUp]
        public void Init()
        {
            _game = new Game(new NewGameSettings { GameName = "Unit Test Game", StartDateTime = DateTime.Now, MaxSystems = 1 });

            _faction = FactionFactory.CreateFaction(_game, "Terran");
            _faction.GetDataBlob<FactionTechDB>().ResearchedTechs.Add(new StringIdentifier("base.tech-engine-max-power"),3);
            _faction.GetDataBlob<FactionTechDB>().ResearchedTechs.Add(new StringIdentifier("base.tech-engine-min-power"), 3);
            _faction.GetDataBlob<FactionTechDB>().ResearchedTechs.Add(new StringIdentifier("base.tech-lox-fuel-consumption"), 5);
            _faction.GetDataBlob<FactionTechDB>().ResearchedTechs.Add(new StringIdentifier("base.tech-conventional-engine"), 1);
            _faction.GetDataBlob<FactionTechDB>().ResearchedTechs.Add(new StringIdentifier("base.tech-nuclear-thermal-engine"), 1);
            _faction.GetDataBlob<FactionTechDB>().ResearchedTechs.Add(new StringIdentifier("base.tech-nuclear-pulse-engine"), 1);
            _starSystem = new StarSystem(_game, "Sol", -1);
            _sol = TestingUtilities.BasicSol(_starSystem);
            /////Ship Class/////




        }


        [Test]
        public void TestShipCreation()
        {
            //_engineSD = NameLookup.GetTemplateSD(_game, "Alcubierre Warp Drive");
            //ComponentDesigner engineDesigner = DefaultStartFactory.DefaultEngineDesign(_game, _faction);
            //engineDesigner = new ComponentDesigner(_engineSD, _faction.GetDataBlob<FactionTechDB>());
            //engineDesigner.ComponentDesignAttributes["Size"].SetValueFromInput(5); //size = 25 power.
            //_engineComponentDesign = engineDesigner.CreateDesign(_faction);

            _engineComponentDesign = DefaultStartFactory.DefaultWarpDesign(_game, _faction);

            _shipDesign = DefaultStartFactory.DefaultShipDesign(_game, _faction);
            _ship = ShipFactory.CreateShip(_shipDesign, _faction, _sol, "Testship");

            ComponentInstancesDB instancesdb = _ship.GetDataBlob<ComponentInstancesDB>();
            instancesdb.TryGetComponentsByAttribute<WarpDriveAtb>(out var instances1);
            int originalEngineNumber = instances1.Count;

            WarpAbilityDB warpAbility = _ship.GetDataBlob<WarpAbilityDB>();

            MassVolumeDB mvdb = _ship.GetDataBlob<MassVolumeDB>();
            WarpDriveAtb warpAtb = _engineComponentDesign.GetAttribute<WarpDriveAtb>();
            double warpPower = warpAtb.WarpPower;
            Assert.AreEqual(warpPower * originalEngineNumber , warpAbility.TotalWarpPower, "Incorrect TotalEnginePower");
            double tonnage1 = mvdb.MassTotal;
            int expectedSpeed1 = ShipMovementProcessor.MaxSpeedCalc(warpAbility.TotalWarpPower, tonnage1);
            Assert.AreEqual(expectedSpeed1, warpAbility.MaxSpeed, "Incorrect Max Speed");


            EntityManipulation.AddComponentToEntity(_ship, _engineComponentDesign);
            instancesdb.TryGetComponentsByAttribute<WarpDriveAtb>(out var instances2);
            int add2engineNumber = instances2.Count;
            Assert.AreEqual(originalEngineNumber + 1, add2engineNumber);


            Assert.AreEqual(warpPower * add2engineNumber, warpAbility.TotalWarpPower, "Incorrect TotalEnginePower 2nd engine added");
            double tonnage2 = mvdb.MassTotal;
            int expectedSpeed2 = ShipMovementProcessor.MaxSpeedCalc(warpAbility.TotalWarpPower, tonnage2);
            Assert.AreEqual(expectedSpeed2, warpAbility.MaxSpeed, "Incorrect Max Speed 2nd engine");


            var energydb = _ship.GetDataBlob<EnergyGenAbilityDB>();
            var energyMax = energydb.EnergyStoreMax[energydb.EnergyType.ID];
            energydb.EnergyStored[energydb.EnergyType.ID] = energyMax;

            Assert.IsTrue(energyMax >= warpAbility.BubbleCreationCost, "Ship does not store enough energy for a succesfull warp bubble creation");

            Assert.AreEqual(warpAbility.CurrentVectorMS.Length(), 0);

            var posDB = _ship.GetDataBlob<PositionDB>();
            var ralpos = posDB.RelativePosition;
            var targetPos = new Vector3(ralpos.X , ralpos.Y, ralpos.Z);
            targetPos.X += expectedSpeed2 * 60 * 60; //distance for an hours travel.
            WarpMoveCommand.CreateCommand(
                _faction.Guid,
                _ship,
                _sol,
                targetPos,
                _ship.StarSysDateTime,
                new Vector3(0,0,0), tonnage1);

            Assert.AreEqual(warpAbility.CurrentVectorMS.Length(), expectedSpeed2, 1.0E-15);
            // _game.GamePulse.Ticklength = TimeSpan.FromSeconds(1);
            //_game.GamePulse.TimeStep();
            StaticRefLib.ProcessorManager.GetProcessor<WarpMovingDB>().ProcessEntity(_ship, 1);
            var ralposNow = posDB.RelativePosition;
            var distance = Math.Abs((ralpos - ralposNow).Length());

            Assert.AreEqual(distance, expectedSpeed2, 1.0E-15);

        }
  }
}
