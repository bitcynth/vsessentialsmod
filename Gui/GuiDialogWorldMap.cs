﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Vintagestory.GameContent
{
    public delegate void OnViewChangedDelegatae(List<Vec2i> nowVisibleChunks, List<Vec2i> nowHiddenChunks);


    public class GuiDialogWorldMap : GuiDialogGeneric
    {
        public override bool DisableWorldInteract()
        {
            return false;
        }

        EnumDialogType dialogType = EnumDialogType.HUD;
        public override EnumDialogType DialogType => dialogType; 
        public List<MapComponent> mapComponents = new List<MapComponent>();


        public GuiElementMap mapElem;
        OnViewChangedDelegatae viewChanged;
        long listenerId;
        GuiElementHoverText hoverTextElem;
        bool requireRecompose = false;

        public GuiDialogWorldMap(OnViewChangedDelegatae viewChanged, ICoreClientAPI capi) : base("", capi)
        {
            this.viewChanged = viewChanged;
            ComposeDialog();

            
        }

        public override bool TryOpen()
        {
            ComposeDialog();
            return base.TryOpen();
        }

        private void ComposeDialog()
        {
            ElementBounds mapBounds = ElementBounds.Fixed(0, 28, 1200, 800);
            ElementBounds layerList = mapBounds.RightCopy().WithFixedSize(10, 350);

            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(3);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(mapBounds, layerList);

            ElementBounds dialogBounds =
                ElementStdBounds.AutosizedMainDialog
                .WithAlignment(EnumDialogArea.CenterMiddle)
                .WithFixedAlignmentOffset(-ElementGeometrics.DialogToScreenPadding, 0);

            if (dialogType == EnumDialogType.HUD)
            {
                mapBounds = ElementBounds.Fixed(0, 0, 250, 250);

                bgBounds = ElementBounds.Fill.WithFixedPadding(2);
                bgBounds.BothSizing = ElementSizing.FitToChildren;
                bgBounds.WithChildren(mapBounds);

                dialogBounds =
                    ElementStdBounds.AutosizedMainDialog
                    .WithAlignment(EnumDialogArea.RightTop)
                    .WithFixedAlignmentOffset(-ElementGeometrics.DialogToScreenPadding, ElementGeometrics.DialogToScreenPadding);
            }

            Vec3d centerPos = capi.World.Player.Entity.Pos.XYZ;

            if (SingleComposer != null) SingleComposer.Dispose();

            SingleComposer = capi.Gui
                .CreateCompo("worldmap", dialogBounds, false)
                .AddDialogBG(bgBounds)
                .AddIf(dialogType == EnumDialogType.Dialog)
                    .AddDialogTitleBar("World Map", OnTitleBarClose)
                    .AddInset(mapBounds, 2)
                .EndIf()
                .BeginChildElements(bgBounds)
                    .AddHoverText("", CairoFont.WhiteDetailText(), 350, mapBounds.FlatCopy(), "hoverText")
                    .AddInteractiveElement(mapElem = new GuiElementMap(mapComponents, centerPos, capi, mapBounds))
                .EndChildElements()
                .Compose()
            ;
            SingleComposer.OnRecomposed += SingleComposer_OnRecomposed;

            mapElem.viewChanged = viewChanged;
            mapElem.ZoomAdd(1, 0.5f, 0.5f);
            

            hoverTextElem = SingleComposer.GetHoverText("hoverText");
            hoverTextElem.SetAutoWidth(true);

            if (listenerId != 0)
            {
                capi.Event.UnregisterGameTickListener(listenerId);
            }
            listenerId = capi.Event.RegisterGameTickListener(
                (dt) => {
                    mapElem.EnsureMapFullyLoaded();
                    if (requireRecompose)
                    {
                        TryClose();
                        TryOpen();
                        requireRecompose = false;
                    }
                }
                , 100);
        }

        private void SingleComposer_OnRecomposed()
        {
            requireRecompose = true;
        }

        public override void OnGuiOpened()
        {
            base.OnGuiOpened();
            if (mapElem != null) mapElem.worldBoundsBefore = new Cuboidi();
            mapComponents.Clear();
            mapElem.EnsureMapFullyLoaded();

            OnMouseMove(new MouseEvent() { X = capi.Input.MouseX, Y = capi.Input.MouseY });
        }


        private void OnTitleBarClose()
        {
            TryClose();
        }

        public override bool TryClose()
        {
            if (DialogType == EnumDialogType.Dialog && capi.Settings.Bool["hudOpened"])
            {
                Open(EnumDialogType.HUD);
                return false;
            }

            return base.TryClose();
        }

        public void Open(EnumDialogType type)
        {
            this.dialogType = type;
            opened = false;
            TryOpen();
        }
        
        
        
        public override void OnGuiClosed()
        {
            base.OnGuiClosed();

            capi.Event.UnregisterGameTickListener(listenerId);
            listenerId = 0;

            foreach (MapComponent cmp in mapComponents)
            {
                cmp.Dispose();
            }

            mapComponents.Clear();
        }

        Vec3d hoveredWorldPos = new Vec3d();
        public override void OnMouseMove(MouseEvent args)
        {
            base.OnMouseMove(args);

            if (SingleComposer != null && SingleComposer.Bounds.PointInside(args.X, args.Y))
            {
                double x = args.X - SingleComposer.Bounds.absX;
                double y = args.Y - SingleComposer.Bounds.absY - GuiElement.scaled(30); // no idea why the 30 :/
                //Console.WriteLine("{0}/{1}", args.X, args.Y);

                StringBuilder hoverText = new StringBuilder();
                mapElem.TranslateViewPosToWorldPos(new Vec2f((float)x, (float)y), ref hoveredWorldPos);
                hoveredWorldPos.Y++;

                //BlockPos pos = capi.World.Player.Entity.Pos.AsBlockPos;
                double yAbs = hoveredWorldPos.Y;
                hoveredWorldPos.Sub(capi.World.DefaultSpawnPosition.AsBlockPos);
                hoveredWorldPos.Y = yAbs;

                hoverText.AppendLine(string.Format("{0}, {1}, {2}", (int)hoveredWorldPos.X, (int)hoveredWorldPos.Y, (int)hoveredWorldPos.Z));

                foreach (MapComponent cmp in mapComponents)
                {
                    cmp.OnMouseMove(args, mapElem, hoverText);
                }

                string text = hoverText.ToString().TrimEnd();

                hoverTextElem.SetNewText(text);
            }

        }

        public override void OnMouseDown(MouseEvent args)
        {
            base.OnMouseDown(args);
        }
    }
}
