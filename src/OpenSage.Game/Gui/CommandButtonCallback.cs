﻿using System;
using System.Linq;
using OpenSage.Gui.ControlBar;
using OpenSage.Logic.Object;
using OpenSage.Logic.Orders;

namespace OpenSage.Gui
{
    public static class CommandButtonCallback
    {
        public static void HandleCommand(Game game, CommandButton commandButton, ObjectDefinition objectDefinition)
        {
            var playerIndex = game.Scene3D.GetPlayerIndex(game.Scene3D.LocalPlayer);
            Order CreateOrder(OrderType type) => new Order(playerIndex, type);

            Order order = null;
            switch (commandButton.Command)
            {
                case CommandType.DozerConstruct:
                    game.OrderGenerator.StartConstructBuilding(objectDefinition);
                    break;

                case CommandType.ToggleOvercharge:
                    order = CreateOrder(OrderType.ToggleOvercharge);
                    break;

                case CommandType.Sell:
                    order = CreateOrder(OrderType.Sell);
                    break;

                case CommandType.UnitBuild:
                    order = CreateOrder(OrderType.CreateUnit);
                    order.AddIntegerArgument(objectDefinition.InternalId);
                    order.AddIntegerArgument(1);
                    break;

                case CommandType.SetRallyPoint:
                    game.OrderGenerator.SetRallyPoint();
                    break;

                case CommandType.PlayerUpgrade:
                case CommandType.ObjectUpgrade:
                    {
                        order = CreateOrder(OrderType.BeginUpgrade);
                        //TODO: figure this out correctly
                        var selection = game.Scene3D.LocalPlayer.SelectedUnits;
                        if (selection.Count == 0)
                        {
                            break;
                        }

                        var objId = game.Scene3D.GameObjects.GetObjectId(selection.First());
                        order.AddIntegerArgument(objId);
                        var upgrade = commandButton.Upgrade.Value;
                        order.AddIntegerArgument(upgrade.InternalId);
                    }
                    break;

                case CommandType.Stop:
                    // TODO: Also stop construction?
                    order = CreateOrder(OrderType.StopMoving);
                    break;

                case CommandType.SpecialPower:
                    {
                        var specialPower = commandButton.SpecialPower.Value;
                        if (commandButton.Options != null)
                        {
                            var needsPos = commandButton.Options.Get(CommandButtonOption.NeedTargetPos);
                            var needsObject = commandButton.Options.Get(CommandButtonOption.NeedTargetAllyObject)
                                             || commandButton.Options.Get(CommandButtonOption.NeedTargetEnemyObject)
                                             || commandButton.Options.Get(CommandButtonOption.NeedTargetNeutralObject);

                            if (needsPos)
                            {
                                game.OrderGenerator.StartSpecialPowerAtLocation(specialPower);
                            }
                            else if (needsObject)
                            {
                                game.OrderGenerator.StartSpecialPowerAtObject(specialPower);
                            }
                        }
                        else
                        {
                            order = CreateOrder(OrderType.SpecialPower);
                            order.AddIntegerArgument(specialPower.InternalId);
                            order.AddIntegerArgument(0);
                            order.AddObjectIdArgument(0);
                        }
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            if (order != null)
            {
                game.NetworkMessageBuffer.AddLocalOrder(order);
            }
        }
    }
}
