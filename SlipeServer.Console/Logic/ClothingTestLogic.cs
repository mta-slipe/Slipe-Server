using SlipeServer.Server.Constants;
using SlipeServer.Server.Services;

namespace SlipeServer.Console.Logic;

public class ClothingTestLogic
{
    private readonly ICommandService commandService;

    public ClothingTestLogic(
        ICommandService commandService
    )
    {
        this.commandService = commandService;
        this.SetupTestCommands();
    }

    private void SetupTestCommands()
    {
        this.commandService.AddCommand("resetclothing").Triggered += (source, args) =>
        {
            args.Player.Clothing.Reset();
        };

        this.commandService.AddCommand("personalFashion").Triggered += (source, args) =>
        {
            long i = (new Random()).NextInt64();
            args.Player.Clothing.Shirt = (byte)(i % ClothingConstants.ShirtsCount);
            args.Player.Clothing.Head = (byte)(i % ClothingConstants.HeadsCount);
            args.Player.Clothing.Trousers = (byte)(i % ClothingConstants.TrousersCount);
            args.Player.Clothing.Shoes = (byte)(i % ClothingConstants.ShoesCount);
            args.Player.Clothing.TattoosLeftUpperArm = (byte)(i % ClothingConstants.TattoosLeftUpperArmCount);
            args.Player.Clothing.TattoosLeftLowerArm = (byte)(i % ClothingConstants.TattoosLeftLowerArmCount);
            args.Player.Clothing.TattoosRightUpperArm = (byte)(i % ClothingConstants.TattoosRightUpperArmCount);
            args.Player.Clothing.TattoosRightLowerArm = (byte)(i % ClothingConstants.TattoosRightLowerArmCount);
            args.Player.Clothing.TattoosBack = (byte)(i % ClothingConstants.TattoosBackCount);
            args.Player.Clothing.TattoosLeftChest = (byte)(i % ClothingConstants.TattoosLeftChestCount);
            args.Player.Clothing.TattoosRightChest = (byte)(i % ClothingConstants.TattoosRightChestCount);
            args.Player.Clothing.TattoosStomach = (byte)(i % ClothingConstants.TattoosStomachCount);
            args.Player.Clothing.TattoosLowerBack = (byte)(i % ClothingConstants.TattoosLowerBackCount);
            args.Player.Clothing.Necklace = (byte)(i % ClothingConstants.NecklaceCount);
            args.Player.Clothing.Watch = (byte)(i % ClothingConstants.WatchesCount);
            args.Player.Clothing.Glasses = (byte)(i % ClothingConstants.GlassesCount);
            args.Player.Clothing.Hat = (byte)(i % ClothingConstants.HatsCount);
        };
    }
}
