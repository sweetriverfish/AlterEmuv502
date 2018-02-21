using Game.Enums;
using Game.Entities;
using Core.Networking;

namespace Game.Handlers
{
    class Equipment : PacketHandler<User> {
        public override void Handle(User sender, InPacket packetReader)
        {
            if (sender.Authenticated)
            {
                bool equipItem = !packetReader.ReadBool();
                byte bTargetClass = packetReader.ReadByte();
                if (bTargetClass < Objects.Inventory.Equipment.MAX_CLASSES)
                {
                    Classes targetClass = (Classes)bTargetClass;
                    string weaponCode = packetReader.ReadString(4).ToUpper();
                    byte targetSlot = 0;

                    if (weaponCode.Length == 4)
                    {
                        if (targetSlot < 8)
                        {
                            if (equipItem)
                            {
                                targetSlot = packetReader.ReadByte(5);
                                if (targetSlot < Objects.Inventory.Equipment.MAX_SLOTS)
                                {
                                    if (Managers.ItemManager.Instance.Items.ContainsKey(weaponCode))
                                    {
                                        Objects.Items.ItemData item = null;
                                        if (Managers.ItemManager.Instance.Items.TryGetValue(weaponCode, out item))
                                        {
                                            if (item.IsWeapon)
                                            {
                                                Objects.Items.Weapon weapon = (Objects.Items.Weapon)item;
                                                if (weapon != null)
                                                {
                                                    if (weapon.Active && weapon.CanEquip[(byte)targetClass, targetSlot])
                                                    {
                                                        Objects.Inventory.Item equipmentItem = sender.Inventory.Get(weapon.Code);
                                                        if (equipmentItem != null && equipmentItem.Slot >= 0)
                                                        { // Does the user have the item.
                                                            Objects.Inventory.Item equipedItem = sender.Inventory.Equipment.Get(targetClass, targetSlot);
                                                            if (equipedItem == null || equipmentItem.Slot != equipedItem.Slot)
                                                            {
                                                                // string Type = getBlock(2);
                                                                if (equipmentItem.Equiped[(byte)targetClass] >= 0)
                                                                    sender.Inventory.Equipment.Remove(targetClass, (byte)equipmentItem.Equiped[(byte)targetClass]);

                                                                sender.Inventory.Equipment.Add(targetClass, targetSlot, equipmentItem);
                                                                sender.Inventory.Equipment.Build();
                                                                sender.Inventory.Equipment.BuildInternal();
                                                                sender.Send(new Packets.Equipment(targetClass, sender.Inventory.Equipment.ListsInternal[(byte)targetClass]));
                                                            }
                                                            else
                                                            {
                                                                sender.Send(new Packets.Equipment(Packets.Equipment.ErrorCode.AlreadyEquipped)); // Already equiped.
                                                            }
                                                        }
                                                        else
                                                        {
                                                            bool isFound = false; // ATTAMPT TO CHECK IF THE ITEM IS A DEFAULT ITEM.
                                                            Objects.Inventory.Item equipedItem = sender.Inventory.Equipment.Get(targetClass, targetSlot);
                                                            for (byte j = 0; j < Objects.Inventory.Inventory.DEFAULT_ITEMS.Length; j++)
                                                            {
                                                                if (weaponCode == Objects.Inventory.Inventory.DEFAULT_ITEMS[j])
                                                                {
                                                                    isFound = true;
                                                                    if (equipedItem == null || equipedItem.Slot != -1)
                                                                    {
                                                                        sender.Inventory.Equipment.Add(targetClass, targetSlot, new Objects.Inventory.Item(-1, 0, Objects.Inventory.Inventory.DEFAULT_ITEMS[j], 0));
                                                                        sender.Inventory.Equipment.Build();
                                                                        sender.Inventory.Equipment.BuildInternal();
                                                                        sender.Send(new Packets.Equipment(targetClass, sender.Inventory.Equipment.ListsInternal[(byte)targetClass]));
                                                                    }
                                                                    else
                                                                    {
                                                                        sender.Send(new Packets.Equipment(Packets.Equipment.ErrorCode.AlreadyEquipped)); // Already equiped.
                                                                    }
                                                                    break;
                                                                }
                                                            }
                                                            if (!isFound)
                                                            {
                                                                sender.Disconnect(); // potentiality scripting or packet changing.. TODO: LOG
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        sender.Disconnect(); // potentiality scripting or packet changing.. TODO: LOG
                                                    }
                                                }
                                                else
                                                {
                                                    sender.Disconnect(); // Server error?
                                                }
                                            }
                                        }
                                        else
                                        {
                                            sender.Disconnect(); // Server error?
                                        }
                                    }
                                    else
                                    {
                                        sender.Disconnect(); // potentiality scripting or packet changing.. TODO: LOG
                                    }

                                }
                                else
                                {
                                    sender.Disconnect(); // potentiality scripting or packet changing.. TODO: LOG
                                }
                            }
                            else
                            {
                                targetSlot = packetReader.ReadByte(3);
                                Objects.Inventory.Item equipedItem = sender.Inventory.Equipment.Get(targetClass, targetSlot);
                                if (equipedItem != null)
                                {
                                    sender.Inventory.Equipment.Remove(targetClass, targetSlot);
                                    sender.Inventory.Equipment.Build();
                                    sender.Send(new Packets.Equipment(targetClass, sender.Inventory.Equipment.Lists[(byte)targetClass]));
                                }
                            }
                        }
                    }
                }
                else
                {
                    sender.Disconnect();
                }
            }
        }
    }
}
