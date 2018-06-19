﻿using System;
using ImGuiNET;
using Pulsar4X.ECSLib;

namespace Pulsar4X.SDL2UI
{
    public class OrbitOrderWindow : PulsarGuiWindow// IOrderWindow
    {
       
        EntityState OrderingEntity;

        EntityState TargetEntity;
        Vector4 InsertionPoint;
        Vector4 PeriapsisPoint;
        double _semiMajorKm;
        double _semiMinorKm;

        string _displayText;
        string _tooltipText = "";
        OrbitOrderWiget _orbitWidget;
        bool _smMode;

        enum States: byte { NeedsEntity, NeedsTarget, NeedsApoapsis, NeedsPeriapsis, NeedsActioning }
        States CurrentState;
        enum Events: byte { SelectedEntity, SelectedPosition, ClickedAction, AltClicked}
        Action[,] fsm;


        private OrbitOrderWindow(EntityState entity, bool smMode = false)
        {
            

            OrderingEntity = entity;
            _smMode = smMode;
            IsActive = true;

            _displayText = "Orbit Order: " + OrderingEntity.Name;
            _tooltipText = "Select target to orbit";
            CurrentState = States.NeedsTarget;

            if (OrderingEntity.Entity.HasDataBlob<OrbitDB>())
            {
                _orbitWidget = new OrbitOrderWiget(OrderingEntity.Entity.GetDataBlob<OrbitDB>());
                _state.MapRendering.UIWidgets.Add(_orbitWidget);
            }

            fsm = new Action[5, 4]
            {
                //selectEntity      selectPos               clickAction     altClick
                {DoNothing,         DoNothing,              DoNothing,      AbortOrder,  },     //needsEntity
                {TargetSelected,    DoNothing,              DoNothing,      GoBackState, }, //needsTarget
                {DoNothing,         InsertionPntSelected,   DoNothing,      GoBackState, }, //needsApopapsis
                {DoNothing,         PeriapsisPntSelected,   DoNothing,      GoBackState, }, //needsPeriapsis
                {DoNothing,         DoNothing,              ActionCmd,      GoBackState, }  //needsActoning
            };

        }

        internal static OrbitOrderWindow GetInstance(EntityState entity, bool SMMode = false)
        {
            if (!_state.LoadedWindows.ContainsKey(typeof(OrbitOrderWindow)))
            {
                return new OrbitOrderWindow(entity, SMMode);
            }
            var instance = (OrbitOrderWindow)_state.LoadedWindows[typeof(OrbitOrderWindow)];
            instance.OrderingEntity = entity;
            instance.CurrentState = States.NeedsTarget;
            return instance;
        }


        void DoNothing() { return; }
        void EntitySelected() { 
            OrderingEntity = _state.LastClickedEntity;
            CurrentState = States.NeedsTarget;
        }
        void TargetSelected() { 
            TargetEntity = _state.LastClickedEntity;
            if (_orbitWidget != null)
            {
                int index = _state.MapRendering.UIWidgets.IndexOf(_orbitWidget);
                _orbitWidget = new OrbitOrderWiget(TargetEntity.Entity);
                _state.MapRendering.UIWidgets[index] = _orbitWidget;
            }
            else
            {
                _orbitWidget = new OrbitOrderWiget(TargetEntity.Entity);
                _state.MapRendering.UIWidgets.Add(_orbitWidget);
            }
            _tooltipText = "Select Apoapsis point";
            CurrentState = States.NeedsApoapsis;
        }
        void InsertionPntSelected() { 
            InsertionPoint = _state.LastWorldPointClicked;
            _semiMajorKm = (GetTargetPosition() - InsertionPoint).Length();
            _tooltipText = "Select Periapsis point";
            CurrentState = States.NeedsPeriapsis;
        }
        void PeriapsisPntSelected() { 
            PeriapsisPoint = _state.LastWorldPointClicked;
            _semiMinorKm = (GetTargetPosition() - PeriapsisPoint).Length();
            _tooltipText = "Action to give order";
            CurrentState = States.NeedsActioning;
        }
        void ActionCmd() 
        {
            OrbitBodyCommand.CreateOrbitBodyCommand(
                _state.Game, 
                _state.Faction, 
                OrderingEntity.Entity, 
                TargetEntity.Entity, 
                PointDFunctions.Length(_orbitWidget.Apoapsis), 
                PointDFunctions.Length(_orbitWidget.Periapsis));
            CloseWindow();
        }
        void ActionAddDB()
        {
            _state.SpaceMasterVM.SMSetOrbitToEntity(OrderingEntity.Entity, TargetEntity.Entity, PointDFunctions.Length(_orbitWidget.Periapsis), _state.CurrentSystemDateTime);
            CloseWindow();
        }

        void AbortOrder() { CloseWindow(); }
        void GoBackState() { CurrentState -= 1; }

        Vector4 GetTargetPosition()
        {
            return TargetEntity.Entity.GetDataBlob<PositionDB>().AbsolutePosition;
        }

        internal override void Display()
        {
            if (IsActive)
            {
                ImVec2 size = new ImVec2(200, 100);
                ImVec2 pos = new ImVec2(_state.MainWinSize.x / 2 - size.x / 2, _state.MainWinSize.y / 2 - size.y / 2);

                ImGui.SetNextWindowSize(size, ImGuiCond.FirstUseEver);
                ImGui.SetNextWindowPos(pos, ImGuiCond.FirstUseEver);

                if (ImGui.Begin(_displayText, ref IsActive, _flags))
                {

                    ImGui.SetTooltip(_tooltipText);
                    ImGui.Text("Target: ");
                    ImGui.SameLine();
                    ImGui.Text(TargetEntity.Name);

                    ImGui.Text("Apoapsis: ");
                    ImGui.SameLine();
                    ImGui.Text(_semiMajorKm.ToString());

                    ImGui.Text("Periapsis: ");
                    ImGui.SameLine();
                    ImGui.Text(_semiMinorKm.ToString());

                    if (ImGui.Button("Action Order"))
                        fsm[(byte)CurrentState, (byte)Events.ClickedAction].Invoke();

                    if (_smMode)
                    {
                        ImGui.SameLine();
                        if (ImGui.Button("Add OrbitDB"))
                        {
                            ActionAddDB();
                        }
                    }

                    if (_orbitWidget != null)
                    {

                        switch (CurrentState)
                        {
                            case States.NeedsEntity:

                                break;
                            case States.NeedsTarget:

                                break;
                            case States.NeedsApoapsis:
                                {
                                    var mousePos = ImGui.GetMousePos();

                                    var mouseWorldPos = _state.Camera.MouseWorldCoordinate();
                                    var ralitivePos = (GetTargetPosition() - mouseWorldPos);
                                    _orbitWidget.SetApoapsis(ralitivePos.X, ralitivePos.Y);

                                    //_orbitWidget.OrbitEllipseSemiMaj = (float)_semiMajorKm;
                                    break;
                                }
                            case States.NeedsPeriapsis:
                                {
                                    var mousePos = ImGui.GetMousePos();

                                    var mouseWorldPos = _state.Camera.MouseWorldCoordinate();
                                    var ralitivePos = (GetTargetPosition() - mouseWorldPos);
                                    _orbitWidget.SetPeriapsis(ralitivePos.X, ralitivePos.Y);
                                    break;
                                }
                            case States.NeedsActioning:
                                break;
                            default:
                                break;
                        }
                    }



                    ImGui.End();
                }
            }
        }

        internal override void EntityClicked(EntityState entity, MouseButtons button)
        {
            if(button == MouseButtons.Primary)
                fsm[(byte)CurrentState, (byte)Events.SelectedEntity].Invoke();
        }
        internal override void MapClicked(Vector4 worldPos, MouseButtons button)
        {
            if (button == MouseButtons.Primary)
            {
                fsm[(byte)CurrentState, (byte)Events.SelectedPosition].Invoke();
            }
            if (button == MouseButtons.Alt)
            {
                fsm[(byte)CurrentState, (byte)Events.AltClicked].Invoke();
            }
        }

        void CloseWindow()
        {
            IsActive = false;
            CurrentState = States.NeedsEntity;


            if(_orbitWidget != null)
                _state.MapRendering.UIWidgets.Remove(_orbitWidget);
        }
    }
}
