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
                }
        };

        public static Dictionary<Clothes, int> ValidClothes = new Dictionary<Clothes, int>
        {
            [Clothes.Shirt] = 67,
            [Clothes.Head] = 32,
            [Clothes.Trousers] = 44,
            [Clothes.Shoes] = 37,
            [Clothes.TattoosLeftUpperArm] = 2, //
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
