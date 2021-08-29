using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Server.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Constants
{
    public class ClothesConstants
    {
        public static Dictionary<Clothes, PedClothing[]> ClothesTextureModel = new Dictionary<Clothes, PedClothing[]>
        {
            [Clothes.Shirt] = new PedClothing[] {
                    new PedClothing("player_torso", "torso", (byte)Clothes.Shirt),
                    new PedClothing("vestblack", "vest", (byte)Clothes.Shirt),
                    new PedClothing("vest", "vest", (byte)Clothes.Shirt),
                    new PedClothing("tshirt2horiz", "tshirt2", (byte)Clothes.Shirt),
                    new PedClothing("tshirtwhite", "tshirt", (byte)Clothes.Shirt),
                    new PedClothing("tshirtilovels", "tshirt", (byte)Clothes.Shirt),
                    new PedClothing("tshirtblunts", "tshirt", (byte)Clothes.Shirt),
                    new PedClothing("shirtbplaid", "shirtb", (byte)Clothes.Shirt),
                    new PedClothing("shirtbcheck", "shirtb", (byte)Clothes.Shirt),
                    new PedClothing("field", "field", (byte)Clothes.Shirt),
                    new PedClothing("tshirterisyell", "tshirt", (byte)Clothes.Shirt),
                    new PedClothing("tshirterisorn", "tshirt", (byte)Clothes.Shirt),
                    new PedClothing("trackytop2eris", "trackytop1", (byte)Clothes.Shirt),
                    new PedClothing("bbjackrim", "bbjack", (byte)Clothes.Shirt),
                    new PedClothing("bballjackrstar", "bbjack", (byte)Clothes.Shirt),
                    new PedClothing("baskballdrib", "baskball", (byte)Clothes.Shirt),
                    new PedClothing("baskballrim", "baskball", (byte)Clothes.Shirt),
                    new PedClothing("sixtyniners", "tshirt", (byte)Clothes.Shirt),
                    new PedClothing("bandits", "baseball", (byte)Clothes.Shirt),
                    new PedClothing("tshirtprored", "tshirt", (byte)Clothes.Shirt),
                    new PedClothing("tshirtproblk", "tshirt", (byte)Clothes.Shirt),
                    new PedClothing("trackytop1pro", "trackytop1", (byte)Clothes.Shirt),
                    new PedClothing("hockeytop", "sweat", (byte)Clothes.Shirt),
                    new PedClothing("bbjersey", "sleevt", (byte)Clothes.Shirt),
                    new PedClothing("shellsuit", "trackytop1", (byte)Clothes.Shirt),
                    new PedClothing("tshirtheatwht", "tshirt", (byte)Clothes.Shirt),
                    new PedClothing("tshirtbobomonk", "tshirt", (byte)Clothes.Shirt),
                    new PedClothing("tshirtbobored", "tshirt", (byte)Clothes.Shirt),
                    new PedClothing("tshirtbase5", "tshirt", (byte)Clothes.Shirt),
                    new PedClothing("tshirtsuburb", "tshirt", (byte)Clothes.Shirt),
                    new PedClothing("hoodyamerc", "hoodya", (byte)Clothes.Shirt),
                    new PedClothing("hoodyabase5", "hoodya", (byte)Clothes.Shirt),
                    new PedClothing("hoodyarockstar", "hoodya", (byte)Clothes.Shirt),
                    new PedClothing("wcoatblue", "wcoat", (byte)Clothes.Shirt),
                    new PedClothing("coach", "coach", (byte)Clothes.Shirt),
                    new PedClothing("coachsemi", "coach", (byte)Clothes.Shirt),
                    new PedClothing("sweatrstar", "sweat", (byte)Clothes.Shirt),
                    new PedClothing("hoodyAblue", "hoodyA", (byte)Clothes.Shirt),
                    new PedClothing("hoodyAblack", "hoodyA", (byte)Clothes.Shirt),
                    new PedClothing("hoodyAgreen", "hoodyA", (byte)Clothes.Shirt),
                    new PedClothing("sleevtbrown", "sleevt", (byte)Clothes.Shirt),
                    new PedClothing("shirtablue", "shirta", (byte)Clothes.Shirt),
                    new PedClothing("shirtayellow", "shirta", (byte)Clothes.Shirt),
                    new PedClothing("shirtagrey", "shirta", (byte)Clothes.Shirt),
                    new PedClothing("shirtbgang", "shirtb", (byte)Clothes.Shirt),
                    new PedClothing("tshirtzipcrm", "tshirt", (byte)Clothes.Shirt),
                    new PedClothing("tshirtzipgry", "tshirt", (byte)Clothes.Shirt),
                    new PedClothing("denimfade", "denim", (byte)Clothes.Shirt),
                    new PedClothing("bowling", "hawaii", (byte)Clothes.Shirt),
                    new PedClothing("hoodjackbeige", "hoodjack", (byte)Clothes.Shirt),
                    new PedClothing("baskballloc", "baskball", (byte)Clothes.Shirt),
                    new PedClothing("tshirtlocgrey", "tshirt", (byte)Clothes.Shirt),
                    new PedClothing("tshirtmaddgrey", "tshirt", (byte)Clothes.Shirt),
                    new PedClothing("tshirtmaddgrn", "tshirt", (byte)Clothes.Shirt),
                    new PedClothing("suit1grey", "suit1", (byte)Clothes.Shirt),
                    new PedClothing("suit1blk", "suit1", (byte)Clothes.Shirt),
                    new PedClothing("leather", "leather", (byte)Clothes.Shirt),
                    new PedClothing("painter", "painter", (byte)Clothes.Shirt),
                    new PedClothing("hawaiiwht", "hawaii", (byte)Clothes.Shirt),
                    new PedClothing("hawaiired", "hawaii", (byte)Clothes.Shirt),
                    new PedClothing("sportjack", "trackytop1", (byte)Clothes.Shirt),
                    new PedClothing("suit1red", "suit1", (byte)Clothes.Shirt),
                    new PedClothing("suit1blue", "suit1", (byte)Clothes.Shirt),
                    new PedClothing("suit1yellow", "suit1", (byte)Clothes.Shirt),
                    new PedClothing("suit2grn", "suit2", (byte)Clothes.Shirt),
                    new PedClothing("tuxedo", "suit2", (byte)Clothes.Shirt),
                    new PedClothing("suit1gang", "suit1", (byte)Clothes.Shirt),
                    new PedClothing("letter", "sleevt", (byte)Clothes.Shirt),
                }
        };

        public static Dictionary<Clothes, int> ValidClothes = new Dictionary<Clothes, int>
        {
            [Clothes.Shirt] = 67,
            [Clothes.Head] = 32,
            [Clothes.Trousers] = 44,
            [Clothes.Shoes] = 37,
            [Clothes.TattoosLeftUpperArm] = 2,
            [Clothes.TattoosLeftLowerArm] = 3,
            [Clothes.TattoosRightUpperArm] = 3,
            [Clothes.TattoosRightLowerArm] = 3,
            [Clothes.TattoosBack] = 6,
            [Clothes.TattoosLeftChest] = 6,
            [Clothes.TattoosRightChest] = 6,
            [Clothes.TattoosStomach] = 6,
            [Clothes.TattoosLowerBack] = 5,
            [Clothes.Necklace] = 11,
            [Clothes.Watches] = 11,
            [Clothes.Glasses] = 16,
            [Clothes.Hats] = 56,
            [Clothes.Extra] = 8,
        };
    }
}
