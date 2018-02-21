using System;
using System.Linq;

using Game.Objects.Items;
using Game.Entities;
using Core.Networking;

namespace Game.Handlers {
    class Itemshop : PacketHandler<User> {

        private int[] days = { 3, 7, 15, 30 };

        public override void Handle(User sender, InPacket packetReader)
        {
            ushort actionType = packetReader.ReadUshort();

            if (actionType >= (ushort)Enums.ItemAction.BuyItem && actionType <= (ushort)Enums.ItemAction.UseItem)
            {

                if (actionType == (ushort)Enums.ItemAction.BuyItem)
                {
                    string itemCode = packetReader.ReadString().ToUpper();

                    if (itemCode.Length == 4)
                    {
                        if (Managers.ItemManager.Instance.Items.ContainsKey(itemCode))
                        {
                            ItemData item = Managers.ItemManager.Instance.Items[itemCode];
                            if (item != null)
                            {
                                uint dbId = packetReader.ReadUint(2);
                                //if (item.dbId == dbId) {
                                byte length = packetReader.ReadByte(4);
                                if (length < 4)
                                {
                                    if (item.Shop.IsBuyable)
                                    {
                                        //TODO: Level check.
                                        //TODO: Add add dinar function.

                                        if (sender.Inventory.Items.Count < Objects.Inventory.Inventory.MAX_ITEMS)
                                        {
                                            if (!item.Shop.RequiresPremium || (item.Shop.RequiresPremium && sender.Premium != Enums.Premium.Free2Play))
                                            {
                                                int price = item.Shop.Cost[length];
                                                if (price >= 0)
                                                {
                                                    int moneyCalc = (int)sender.Money - price;
                                                    if (moneyCalc >= 0)
                                                    {

                                                        var invItem = (Objects.Inventory.Item)null;
                                                        try
                                                        {
                                                            invItem = sender.Inventory.Items.Select(n => n.Value).Where(n => n.ItemCode == item.Code).First();
                                                        }
                                                        catch { invItem = null; }

                                                        uint utcTime = (uint)DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
                                                        uint itemLength = (uint)(86400 * days[length]);
                                                        sender.Money = (uint)moneyCalc;

                                                        if (invItem != null)
                                                        { // Has item in inventory.
                                                          // Extend & insert into db :)
                                                            Databases.Game.Query(string.Concat("INSERT INTO user_inventory (`id`, `owner`, `code`, `startdate`, `length`, `price`, `expired`, `deleted`) VALUES (NULL, '", sender.ID, "', '", item.Code.ToUpper(), "', '", utcTime, "', '", itemLength, "', '", price, "', '0', '0'); UPDATE user_details SET money='", sender.Money, "' WHERE id = ", sender.ID, ";"));
                                                            invItem.ExpireDate = invItem.ExpireDate.AddSeconds(itemLength);
                                                            sender.Inventory.Rebuild();
                                                            sender.Send(new Packets.Itemshop(sender));
                                                        }
                                                        else
                                                        { // No item in invetory
                                                          // Insert & fetch id
                                                            uint itemdbId = 0;
                                                            MySql.Data.MySqlClient.MySqlCommand cmd;

                                                            try
                                                            {
                                                                cmd = new MySql.Data.MySqlClient.MySqlCommand(string.Concat("INSERT INTO user_inventory (`id`, `owner`, `code`, `startdate`, `length`, `price`, `expired`, `deleted`) VALUES (NULL, '", sender.ID, "', '", item.Code.ToUpper(), "', '", utcTime, "', '", itemLength, "', '", price, "', '0', '0');"), Databases.Game.connection);
                                                                cmd.ExecuteNonQuery();
                                                                itemdbId = (uint)cmd.LastInsertedId;
                                                            }
                                                            catch { itemdbId = 0; }

                                                            if (itemdbId > 0)
                                                            {
                                                                Databases.Game.Query(string.Concat("UPDATE user_details SET money='", sender.Money, "' WHERE id = ", sender.ID, ";"));
                                                                sender.Inventory.Add(new Objects.Inventory.Item(-1, itemdbId, item.Code, (utcTime + itemLength)));
                                                                sender.Inventory.Rebuild();
                                                                sender.Send(new Packets.Itemshop(sender));
                                                            }
                                                            else
                                                            {
                                                                sender.Send(new Packets.Itemshop(Packets.Itemshop.ErrorCodes.CannotBeBougth));
                                                            }
                                                        }

                                                    }
                                                    else
                                                    {
                                                        sender.Send(new Packets.Itemshop(Packets.Itemshop.ErrorCodes.NotEnoughDinar));
                                                    }
                                                }
                                                else
                                                {
                                                    sender.Disconnect(); // Item can't be bought for this period. - Cheating?
                                                }
                                            }
                                            else
                                            {
                                                sender.Send(new Packets.Itemshop(Packets.Itemshop.ErrorCodes.PremiumOnly));
                                            }
                                        }
                                        else
                                        {
                                            sender.Send(new Packets.Itemshop(Packets.Itemshop.ErrorCodes.InventoryFull)); // Inventory is full.
                                        }

                                    }
                                    else
                                    {
                                        sender.Send(new Packets.Itemshop(Packets.Itemshop.ErrorCodes.CannotBeBougth)); // Buying an item that isn't for sale? - Cheating?
                                    }
                                }
                                else
                                {
                                    sender.Disconnect(); // Cheating?
                                }
                            }
                            else
                            {
                                sender.Disconnect(); // Invalid id for the item - Cheating?
                            }
                        }
                        else
                        {
                            sender.Disconnect(); // Server error.
                        }
                    }
                    else
                    {
                        sender.Disconnect(); // Code doesn't exist - Cheating?
                    }
                }
                else
                {
                    sender.Disconnect(); // Wrong Code - Cheating?
                }
            }
            /*else if (actionType == (ushort)Enums.ItemAction.UseItem)
            {
                sender.Send(new Packets.Itemshop(Packets.Itemshop.ErrorCodes.InvalidItem));
            }*/
            else
            {
                sender.Send(new Packets.Itemshop(Packets.Itemshop.ErrorCodes.InvalidItem));
            }
            /*} else {
                sender.Disconnect(); // Invalid Action type - Cheating?
            }*/
        }
    }
}
