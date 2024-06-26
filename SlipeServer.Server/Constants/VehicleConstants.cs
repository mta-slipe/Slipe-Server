using SlipeServer.Server.Elements;
using System.Collections.Generic;
using System.Drawing;

namespace SlipeServer.Server.Constants;

public class VehicleConstants
{
    public static HashSet<VehicleModel> TrailerModels { get; } = new()
    {
        VehicleModel.Trailer1,
        VehicleModel.Trailer2,
        VehicleModel.TrailerTankerCommando,
        VehicleModel.Trailer3,
        VehicleModel.BaggageTrailerCovered,
        VehicleModel.BaggageTrailerUncovered,
        VehicleModel.TrailerStairs,
        VehicleModel.FarmTrailer,
        VehicleModel.FarmTrailerTwo
    };

    public static HashSet<VehicleModel> WaterEntryVehicles { get; } = new()
    {
        VehicleModel.Leviathan,
        VehicleModel.Seasparrow,
        VehicleModel.Skimmer,
        VehicleModel.Vortex
    };

    public static HashSet<VehicleModel> VehiclesWithoutDoors { get; } = new()
    {
        VehicleModel.BFInjection,
        VehicleModel.RCBandit,
        VehicleModel.Caddy,
        VehicleModel.RCRaider,
        VehicleModel.Baggage,
        VehicleModel.Dozer,
        VehicleModel.Forklift,
        VehicleModel.Tractor,
        VehicleModel.RCTiger,
        VehicleModel.Bandito,
        VehicleModel.Kart,
        VehicleModel.Mower,
        VehicleModel.RCCam,
        VehicleModel.RCGoblin,
        VehicleModel.BloodringBanger,

        VehicleModel.Coastguard,
        VehicleModel.Dinghy,
        VehicleModel.Jetmax,
        VehicleModel.Launch,
        VehicleModel.Marquis,
        VehicleModel.Predator,
        VehicleModel.Reefer,
        VehicleModel.Speeder,
        VehicleModel.Squalo,
        VehicleModel.Tropic,

        VehicleModel.BF400,
        VehicleModel.Bike,
        VehicleModel.BMX,
        VehicleModel.Faggio,
        VehicleModel.FCR900,
        VehicleModel.Freeway,
        VehicleModel.MountainBike,
        VehicleModel.NRG500,
        VehicleModel.PCJ600,
        VehicleModel.Pizzaboy,
        VehicleModel.Sanchez,
        VehicleModel.Wayfarer,
        VehicleModel.HPV1000,

        VehicleModel.BoxFreight,
        VehicleModel.Streak,
        VehicleModel.StreakTrainTrailer,
        VehicleModel.FreightTrainFlatbed,
        VehicleModel.Freight,
        VehicleModel.Tram,
    };



    public static Dictionary<VehicleModel, byte> DoorsPerVehicle { get; } = new()
    {
        [VehicleModel.Landstalker] = 4,
        [VehicleModel.Bravura] = 4,
        [VehicleModel.Buffalo] = 4,
        [VehicleModel.Linerunner] = 4,
        [VehicleModel.Perennial] = 4,
        [VehicleModel.Sentinel] = 4,
        [VehicleModel.Dumper] = 1,
        [VehicleModel.FireTruck] = 2,
        [VehicleModel.Trashmaster] = 2,
        [VehicleModel.Stretch] = 4,
        [VehicleModel.Manana] = 2,
        [VehicleModel.Infernus] = 2,
        [VehicleModel.Voodoo] = 4,
        [VehicleModel.Pony] = 4,
        [VehicleModel.Mule] = 4,
        [VehicleModel.Cheetah] = 2,
        [VehicleModel.Ambulance] = 4,
        [VehicleModel.Leviathan] = 2,
        [VehicleModel.Moonbeam] = 4,
        [VehicleModel.Esperanto] = 4,
        [VehicleModel.Taxi] = 4,
        [VehicleModel.Washington] = 4,
        [VehicleModel.Bobcat] = 2,
        [VehicleModel.MrWhoopee] = 2,
        [VehicleModel.BFInjection] = 2,
        [VehicleModel.Hunter] = 1,
        [VehicleModel.Premier] = 4,
        [VehicleModel.Enforcer] = 4,
        [VehicleModel.Securicar] = 4,
        [VehicleModel.Banshee] = 2,
        [VehicleModel.Predator] = 0,
        [VehicleModel.Bus] = 1,
        [VehicleModel.Rhino] = 1,
        [VehicleModel.Barracks] = 2,
        [VehicleModel.Hotknife] = 2,
        [VehicleModel.Trailer1] = 2,
        [VehicleModel.Previon] = 4,
        [VehicleModel.Coach] = 1,
        [VehicleModel.Cabbie] = 4,
        [VehicleModel.Stallion] = 4,
        [VehicleModel.Rumpo] = 4,
        [VehicleModel.RCBandit] = 0,
        [VehicleModel.Romero] = 4,
        [VehicleModel.Packer] = 2,
        [VehicleModel.Monster] = 2,
        [VehicleModel.Admiral] = 4,
        [VehicleModel.Squalo] = 0,
        [VehicleModel.Seasparrow] = 2,
        [VehicleModel.Pizzaboy] = 0,
        [VehicleModel.Tram] = 0,
        [VehicleModel.Trailer2] = 2,
        [VehicleModel.Turismo] = 2,
        [VehicleModel.Speeder] = 0,
        [VehicleModel.Reefer] = 0,
        [VehicleModel.Tropic] = 0,
        [VehicleModel.Flatbed] = 2,
        [VehicleModel.Yankee] = 2,
        [VehicleModel.Caddy] = 0,
        [VehicleModel.Solair] = 4,
        [VehicleModel.BerkleysRCVan] = 4,
        [VehicleModel.Skimmer] = 2,
        [VehicleModel.PCJ600] = 0,
        [VehicleModel.Faggio] = 0,
        [VehicleModel.Freeway] = 0,
        [VehicleModel.RCBaron] = 0,
        [VehicleModel.RCRaider] = 0,
        [VehicleModel.Glendale] = 4,
        [VehicleModel.Oceanic] = 4,
        [VehicleModel.Sanchez] = 0,
        [VehicleModel.Sparrow] = 2,
        [VehicleModel.Patriot] = 4,
        [VehicleModel.Quadbike] = 0,
        [VehicleModel.Coastguard] = 0,
        [VehicleModel.Dinghy] = 0,
        [VehicleModel.Hermes] = 4,
        [VehicleModel.Sabre] = 4,
        [VehicleModel.Rustler] = 1,
        [VehicleModel.ZR350] = 2,
        [VehicleModel.Walton] = 4,
        [VehicleModel.Regina] = 4,
        [VehicleModel.Comet] = 2,
        [VehicleModel.BMX] = 0,
        [VehicleModel.Burrito] = 4,
        [VehicleModel.Camper] = 2,
        [VehicleModel.Marquis] = 0,
        [VehicleModel.Baggage] = 0,
        [VehicleModel.Dozer] = 0,
        [VehicleModel.Maverick] = 4,
        [VehicleModel.NewsChopper] = 4,
        [VehicleModel.Rancher] = 2,
        [VehicleModel.FBIRancher] = 4,
        [VehicleModel.Virgo] = 4,
        [VehicleModel.Greenwood] = 4,
        [VehicleModel.Jetmax] = 0,
        [VehicleModel.HotringRacer] = 4,
        [VehicleModel.Sandking] = 2,
        [VehicleModel.BlistaCompact] = 2,
        [VehicleModel.PoliceMaverick] = 4,
        [VehicleModel.Boxville] = 4,
        [VehicleModel.Benson] = 4,
        [VehicleModel.Mesa] = 4,
        [VehicleModel.RCGoblin] = 0,
        [VehicleModel.HotringRacer3] = 2,
        [VehicleModel.HotringRacer2] = 2,
        [VehicleModel.BloodringBanger] = 0,
        [VehicleModel.RancherLure] = 2,
        [VehicleModel.SuperGT] = 2,
        [VehicleModel.Elegant] = 4,
        [VehicleModel.Journey] = 4,
        [VehicleModel.Bike] = 0,
        [VehicleModel.MountainBike] = 0,
        [VehicleModel.Beagle] = 2,
        [VehicleModel.Cropduster] = 1,
        [VehicleModel.Stuntplane] = 1,
        [VehicleModel.Tanker] = 2,
        [VehicleModel.Roadtrain] = 2,
        [VehicleModel.Nebula] = 4,
        [VehicleModel.Majestic] = 4,
        [VehicleModel.Buccaneer] = 4,
        [VehicleModel.Shamal] = 1,
        [VehicleModel.Hydra] = 1,
        [VehicleModel.FCR900] = 0,
        [VehicleModel.NRG500] = 0,
        [VehicleModel.HPV1000] = 0,
        [VehicleModel.CementTruck] = 2,
        [VehicleModel.Towtruck] = 2,
        [VehicleModel.Fortune] = 4,
        [VehicleModel.Cadrona] = 4,
        [VehicleModel.FBITruck] = 4,
        [VehicleModel.Willard] = 4,
        [VehicleModel.Forklift] = 0,
        [VehicleModel.Tractor] = 0,
        [VehicleModel.CombineHarvester] = 1,
        [VehicleModel.Feltzer] = 2,
        [VehicleModel.Remington] = 4,
        [VehicleModel.Slamvan] = 2,
        [VehicleModel.Blade] = 2,
        [VehicleModel.Freight] = 0,
        [VehicleModel.Streak] = 0,
        [VehicleModel.Vortex] = 0,
        [VehicleModel.Vincent] = 4,
        [VehicleModel.Bullet] = 2,
        [VehicleModel.Clover] = 4,
        [VehicleModel.Sadler] = 2,
        [VehicleModel.FireTruckLadder] = 2,
        [VehicleModel.Hustler] = 2,
        [VehicleModel.Intruder] = 4,
        [VehicleModel.Primo] = 4,
        [VehicleModel.Cargobob] = 2,
        [VehicleModel.Tampa] = 2,
        [VehicleModel.Sunrise] = 4,
        [VehicleModel.Merit] = 4,
        [VehicleModel.UtilityVan] = 2,
        [VehicleModel.Nevada] = 4,
        [VehicleModel.Yosemite] = 4,
        [VehicleModel.Windsor] = 2,
        [VehicleModel.Monster2] = 2,
        [VehicleModel.Monster3] = 2,
        [VehicleModel.Uranus] = 4,
        [VehicleModel.Jester] = 4,
        [VehicleModel.Sultan] = 4,
        [VehicleModel.Stratum] = 4,
        [VehicleModel.Elegy] = 2,
        [VehicleModel.Raindance] = 2,
        [VehicleModel.RCTiger] = 0,
        [VehicleModel.Flash] = 4,
        [VehicleModel.Tahoma] = 4,
        [VehicleModel.Savanna] = 2,
        [VehicleModel.Bandito] = 0,
        [VehicleModel.FreightTrainFlatbed] = 0,
        [VehicleModel.StreakTrainTrailer] = 0,
        [VehicleModel.Kart] = 0,
        [VehicleModel.Mower] = 0,
        [VehicleModel.Dune] = 2,
        [VehicleModel.Sweeper] = 4,
        [VehicleModel.Broadway] = 2,
        [VehicleModel.Tornado] = 4,
        [VehicleModel.AT400] = 1,
        [VehicleModel.DFT30] = 2,
        [VehicleModel.Huntley] = 4,
        [VehicleModel.Stafford] = 4,
        [VehicleModel.BF400] = 0,
        [VehicleModel.Newsvan] = 4,
        [VehicleModel.Tug] = 1,
        [VehicleModel.TrailerTankerCommando] = 2,
        [VehicleModel.Emperor] = 4,
        [VehicleModel.Wayfarer] = 0,
        [VehicleModel.Euros] = 4,
        [VehicleModel.Hotdog] = 2,
        [VehicleModel.Club] = 2,
        [VehicleModel.BoxFreight] = 0,
        [VehicleModel.Trailer3] = 2,
        [VehicleModel.Andromada] = 1,
        [VehicleModel.Dodo] = 2,
        [VehicleModel.RCCam] = 1,
        [VehicleModel.Launch] = 1,
        [VehicleModel.PoliceLS] = 4,
        [VehicleModel.PoliceSf] = 4,
        [VehicleModel.PoliceLV] = 4,
        [VehicleModel.PoliceRanger] = 2,
        [VehicleModel.Picador] = 2,
        [VehicleModel.SwatTank] = 2,
        [VehicleModel.Alpha] = 2,
        [VehicleModel.Phoenix] = 2,
        [VehicleModel.GlendaleDamaged] = 4,
        [VehicleModel.SadlerDamaged] = 2,
        [VehicleModel.BaggageTrailerCovered] = 2,
        [VehicleModel.BaggageTrailerUncovered] = 2,
        [VehicleModel.TrailerStairs] = 2,
        [VehicleModel.FarmTrailer] = 2,
        [VehicleModel.FarmTrailerTwo] = 2
    };

    public static Dictionary<VehicleModel, byte> SeatsPerVehicle { get; } = new()
    {
        [VehicleModel.Landstalker] = 4,
        [VehicleModel.Bravura] = 4,
        [VehicleModel.Buffalo] = 4,
        [VehicleModel.Linerunner] = 4,
        [VehicleModel.Perennial] = 4,
        [VehicleModel.Sentinel] = 4,
        [VehicleModel.Dumper] = 1,
        [VehicleModel.FireTruck] = 2,
        [VehicleModel.Trashmaster] = 2,
        [VehicleModel.Stretch] = 4,
        [VehicleModel.Manana] = 2,
        [VehicleModel.Infernus] = 2,
        [VehicleModel.Voodoo] = 4,
        [VehicleModel.Pony] = 4,
        [VehicleModel.Mule] = 4,
        [VehicleModel.Cheetah] = 2,
        [VehicleModel.Ambulance] = 4,
        [VehicleModel.Leviathan] = 2,
        [VehicleModel.Moonbeam] = 4,
        [VehicleModel.Esperanto] = 4,
        [VehicleModel.Taxi] = 4,
        [VehicleModel.Washington] = 4,
        [VehicleModel.Bobcat] = 2,
        [VehicleModel.MrWhoopee] = 2,
        [VehicleModel.BFInjection] = 2,
        [VehicleModel.Hunter] = 1,
        [VehicleModel.Premier] = 4,
        [VehicleModel.Enforcer] = 4,
        [VehicleModel.Securicar] = 4,
        [VehicleModel.Banshee] = 2,
        [VehicleModel.Predator] = 1,
        [VehicleModel.Bus] = 1,
        [VehicleModel.Rhino] = 1,
        [VehicleModel.Barracks] = 2,
        [VehicleModel.Hotknife] = 2,
        [VehicleModel.Trailer1] = 0,
        [VehicleModel.Previon] = 4,
        [VehicleModel.Coach] = 1,
        [VehicleModel.Cabbie] = 4,
        [VehicleModel.Stallion] = 4,
        [VehicleModel.Rumpo] = 4,
        [VehicleModel.RCBandit] = 1,
        [VehicleModel.Romero] = 4,
        [VehicleModel.Packer] = 2,
        [VehicleModel.Monster] = 2,
        [VehicleModel.Admiral] = 4,
        [VehicleModel.Squalo] = 1,
        [VehicleModel.Seasparrow] = 2,
        [VehicleModel.Pizzaboy] = 2,
        [VehicleModel.Tram] = 0,
        [VehicleModel.Trailer2] = 0,
        [VehicleModel.Turismo] = 2,
        [VehicleModel.Speeder] = 1,
        [VehicleModel.Reefer] = 1,
        [VehicleModel.Tropic] = 1,
        [VehicleModel.Flatbed] = 2,
        [VehicleModel.Yankee] = 2,
        [VehicleModel.Caddy] = 2,
        [VehicleModel.Solair] = 4,
        [VehicleModel.BerkleysRCVan] = 4,
        [VehicleModel.Skimmer] = 2,
        [VehicleModel.PCJ600] = 2,
        [VehicleModel.Faggio] = 2,
        [VehicleModel.Freeway] = 2,
        [VehicleModel.RCBaron] = 1,
        [VehicleModel.RCRaider] = 0,
        [VehicleModel.Glendale] = 4,
        [VehicleModel.Oceanic] = 4,
        [VehicleModel.Sanchez] = 1,
        [VehicleModel.Sparrow] = 2,
        [VehicleModel.Patriot] = 4,
        [VehicleModel.Quadbike] = 1,
        [VehicleModel.Coastguard] = 1,
        [VehicleModel.Dinghy] = 0,
        [VehicleModel.Hermes] = 4,
        [VehicleModel.Sabre] = 4,
        [VehicleModel.Rustler] = 1,
        [VehicleModel.ZR350] = 2,
        [VehicleModel.Walton] = 4,
        [VehicleModel.Regina] = 4,
        [VehicleModel.Comet] = 2,
        [VehicleModel.BMX] = 1,
        [VehicleModel.Burrito] = 4,
        [VehicleModel.Camper] = 2,
        [VehicleModel.Marquis] = 1,
        [VehicleModel.Baggage] = 1,
        [VehicleModel.Dozer] = 1,
        [VehicleModel.Maverick] = 4,
        [VehicleModel.NewsChopper] = 4,
        [VehicleModel.Rancher] = 2,
        [VehicleModel.FBIRancher] = 4,
        [VehicleModel.Virgo] = 4,
        [VehicleModel.Greenwood] = 4,
        [VehicleModel.Jetmax] = 1,
        [VehicleModel.HotringRacer] = 4,
        [VehicleModel.Sandking] = 2,
        [VehicleModel.BlistaCompact] = 2,
        [VehicleModel.PoliceMaverick] = 4,
        [VehicleModel.Boxville] = 4,
        [VehicleModel.Benson] = 4,
        [VehicleModel.Mesa] = 4,
        [VehicleModel.RCGoblin] = 1,
        [VehicleModel.HotringRacer3] = 2,
        [VehicleModel.HotringRacer2] = 2,
        [VehicleModel.BloodringBanger] = 2,
        [VehicleModel.RancherLure] = 2,
        [VehicleModel.SuperGT] = 2,
        [VehicleModel.Elegant] = 4,
        [VehicleModel.Journey] = 4,
        [VehicleModel.Bike] = 1,
        [VehicleModel.MountainBike] = 1,
        [VehicleModel.Beagle] = 2,
        [VehicleModel.Cropduster] = 1,
        [VehicleModel.Stuntplane] = 1,
        [VehicleModel.Tanker] = 2,
        [VehicleModel.Roadtrain] = 2,
        [VehicleModel.Nebula] = 4,
        [VehicleModel.Majestic] = 4,
        [VehicleModel.Buccaneer] = 4,
        [VehicleModel.Shamal] = 1,
        [VehicleModel.Hydra] = 1,
        [VehicleModel.FCR900] = 2,
        [VehicleModel.NRG500] = 2,
        [VehicleModel.HPV1000] = 2,
        [VehicleModel.CementTruck] = 2,
        [VehicleModel.Towtruck] = 2,
        [VehicleModel.Fortune] = 4,
        [VehicleModel.Cadrona] = 4,
        [VehicleModel.FBITruck] = 4,
        [VehicleModel.Willard] = 4,
        [VehicleModel.Forklift] = 1,
        [VehicleModel.Tractor] = 1,
        [VehicleModel.CombineHarvester] = 1,
        [VehicleModel.Feltzer] = 2,
        [VehicleModel.Remington] = 4,
        [VehicleModel.Slamvan] = 2,
        [VehicleModel.Blade] = 2,
        [VehicleModel.Freight] = 2,
        [VehicleModel.Streak] = 2,
        [VehicleModel.Vortex] = 1,
        [VehicleModel.Vincent] = 4,
        [VehicleModel.Bullet] = 2,
        [VehicleModel.Clover] = 4,
        [VehicleModel.Sadler] = 2,
        [VehicleModel.FireTruckLadder] = 2,
        [VehicleModel.Hustler] = 2,
        [VehicleModel.Intruder] = 4,
        [VehicleModel.Primo] = 4,
        [VehicleModel.Cargobob] = 2,
        [VehicleModel.Tampa] = 2,
        [VehicleModel.Sunrise] = 4,
        [VehicleModel.Merit] = 4,
        [VehicleModel.UtilityVan] = 2,
        [VehicleModel.Nevada] = 4,
        [VehicleModel.Yosemite] = 4,
        [VehicleModel.Windsor] = 2,
        [VehicleModel.Monster2] = 2,
        [VehicleModel.Monster3] = 2,
        [VehicleModel.Uranus] = 4,
        [VehicleModel.Jester] = 4,
        [VehicleModel.Sultan] = 4,
        [VehicleModel.Stratum] = 4,
        [VehicleModel.Elegy] = 2,
        [VehicleModel.Raindance] = 2,
        [VehicleModel.RCTiger] = 1,
        [VehicleModel.Flash] = 4,
        [VehicleModel.Tahoma] = 4,
        [VehicleModel.Savanna] = 2,
        [VehicleModel.Bandito] = 1,
        [VehicleModel.FreightTrainFlatbed] = 2,
        [VehicleModel.StreakTrainTrailer] = 2,
        [VehicleModel.Kart] = 1,
        [VehicleModel.Mower] = 1,
        [VehicleModel.Dune] = 2,
        [VehicleModel.Sweeper] = 4,
        [VehicleModel.Broadway] = 2,
        [VehicleModel.Tornado] = 4,
        [VehicleModel.AT400] = 1,
        [VehicleModel.DFT30] = 2,
        [VehicleModel.Huntley] = 4,
        [VehicleModel.Stafford] = 4,
        [VehicleModel.BF400] = 2,
        [VehicleModel.Newsvan] = 4,
        [VehicleModel.Tug] = 1,
        [VehicleModel.TrailerTankerCommando] = 0,
        [VehicleModel.Emperor] = 4,
        [VehicleModel.Wayfarer] = 2,
        [VehicleModel.Euros] = 4,
        [VehicleModel.Hotdog] = 2,
        [VehicleModel.Club] = 2,
        [VehicleModel.BoxFreight] = 2,
        [VehicleModel.Trailer3] = 0,
        [VehicleModel.Andromada] = 1,
        [VehicleModel.Dodo] = 2,
        [VehicleModel.RCCam] = 1,
        [VehicleModel.Launch] = 1,
        [VehicleModel.PoliceLS] = 4,
        [VehicleModel.PoliceSf] = 4,
        [VehicleModel.PoliceLV] = 4,
        [VehicleModel.PoliceRanger] = 2,
        [VehicleModel.Picador] = 2,
        [VehicleModel.SwatTank] = 2,
        [VehicleModel.Alpha] = 2,
        [VehicleModel.Phoenix] = 2,
        [VehicleModel.GlendaleDamaged] = 4,
        [VehicleModel.SadlerDamaged] = 2,
        [VehicleModel.BaggageTrailerCovered] = 0,
        [VehicleModel.BaggageTrailerUncovered] = 0,
        [VehicleModel.TrailerStairs] = 0,
        [VehicleModel.FarmTrailer] = 0,
        [VehicleModel.FarmTrailerTwo] = 0
    };

    public static HashSet<VehicleModel> AdjustablePropertyModels { get; } = new()
    {
        VehicleModel.Dumper,
        VehicleModel.Packer,
        VehicleModel.Dozer,
        VehicleModel.Hydra,
        VehicleModel.Towtruck,
        VehicleModel.Forklift,
        VehicleModel.Tractor,
        VehicleModel.Andromada,
        VehicleModel.CementTruck,
    };

    public static HashSet<VehicleModel> TurretModels { get; } = new()
    {
        VehicleModel.FireTruck,
        VehicleModel.Rhino,
        VehicleModel.SwatTank,
    };

    public static readonly Dictionary<byte, Color> ColorsPerId = new()
    {
        [0] = Color.FromArgb(255, 0, 0, 0),
        [1] = Color.FromArgb(255, 245, 245, 245),
        [2] = Color.FromArgb(255, 42, 119, 161),
        [3] = Color.FromArgb(255, 132, 4, 16),
        [4] = Color.FromArgb(255, 38, 55, 57),
        [5] = Color.FromArgb(255, 134, 68, 110),
        [6] = Color.FromArgb(255, 215, 142, 16),
        [7] = Color.FromArgb(255, 76, 117, 183),
        [8] = Color.FromArgb(255, 189, 190, 198),
        [9] = Color.FromArgb(255, 94, 112, 114),
        [10] = Color.FromArgb(255, 70, 89, 122),
        [11] = Color.FromArgb(255, 101, 106, 121),
        [12] = Color.FromArgb(255, 93, 126, 141),
        [13] = Color.FromArgb(255, 88, 89, 90),
        [14] = Color.FromArgb(255, 214, 218, 214),
        [15] = Color.FromArgb(255, 156, 161, 163),
        [16] = Color.FromArgb(255, 51, 95, 63),
        [17] = Color.FromArgb(255, 115, 14, 26),
        [18] = Color.FromArgb(255, 123, 10, 42),
        [19] = Color.FromArgb(255, 159, 157, 148),
        [20] = Color.FromArgb(255, 59, 78, 120),
        [21] = Color.FromArgb(255, 115, 46, 62),
        [22] = Color.FromArgb(255, 105, 30, 59),
        [23] = Color.FromArgb(255, 150, 145, 140),
        [24] = Color.FromArgb(255, 81, 84, 89),
        [25] = Color.FromArgb(255, 63, 62, 69),
        [26] = Color.FromArgb(255, 165, 169, 167),
        [27] = Color.FromArgb(255, 99, 92, 90),
        [28] = Color.FromArgb(255, 61, 74, 104),
        [29] = Color.FromArgb(255, 151, 149, 146),
        [30] = Color.FromArgb(255, 66, 31, 33),
        [31] = Color.FromArgb(255, 95, 39, 43),
        [32] = Color.FromArgb(255, 132, 148, 171),
        [33] = Color.FromArgb(255, 118, 123, 124),
        [34] = Color.FromArgb(255, 100, 100, 100),
        [35] = Color.FromArgb(255, 90, 87, 82),
        [36] = Color.FromArgb(255, 37, 37, 39),
        [37] = Color.FromArgb(255, 45, 58, 53),
        [38] = Color.FromArgb(255, 147, 163, 150),
        [39] = Color.FromArgb(255, 109, 122, 136),
        [40] = Color.FromArgb(255, 34, 25, 24),
        [41] = Color.FromArgb(255, 111, 103, 95),
        [42] = Color.FromArgb(255, 124, 28, 42),
        [43] = Color.FromArgb(255, 95, 10, 21),
        [44] = Color.FromArgb(255, 25, 56, 38),
        [45] = Color.FromArgb(255, 93, 27, 32),
        [46] = Color.FromArgb(255, 157, 152, 114),
        [47] = Color.FromArgb(255, 122, 117, 96),
        [48] = Color.FromArgb(255, 152, 149, 134),
        [49] = Color.FromArgb(255, 173, 176, 176),
        [50] = Color.FromArgb(255, 132, 137, 136),
        [51] = Color.FromArgb(255, 48, 79, 69),
        [52] = Color.FromArgb(255, 77, 98, 104),
        [53] = Color.FromArgb(255, 22, 34, 72),
        [54] = Color.FromArgb(255, 39, 47, 75),
        [55] = Color.FromArgb(255, 125, 98, 86),
        [56] = Color.FromArgb(255, 158, 164, 171),
        [57] = Color.FromArgb(255, 156, 141, 113),
        [58] = Color.FromArgb(255, 109, 24, 34),
        [59] = Color.FromArgb(255, 78, 104, 129),
        [60] = Color.FromArgb(255, 156, 156, 152),
        [61] = Color.FromArgb(255, 145, 115, 71),
        [62] = Color.FromArgb(255, 102, 28, 38),
        [63] = Color.FromArgb(255, 148, 157, 159),
        [64] = Color.FromArgb(255, 164, 167, 165),
        [65] = Color.FromArgb(255, 142, 140, 70),
        [66] = Color.FromArgb(255, 52, 26, 30),
        [67] = Color.FromArgb(255, 106, 122, 140),
        [68] = Color.FromArgb(255, 170, 173, 142),
        [69] = Color.FromArgb(255, 171, 152, 143),
        [70] = Color.FromArgb(255, 133, 31, 46),
        [71] = Color.FromArgb(255, 111, 130, 151),
        [72] = Color.FromArgb(255, 88, 88, 83),
        [73] = Color.FromArgb(255, 154, 167, 144),
        [74] = Color.FromArgb(255, 96, 26, 35),
        [75] = Color.FromArgb(255, 32, 32, 44),
        [76] = Color.FromArgb(255, 164, 160, 150),
        [77] = Color.FromArgb(255, 170, 157, 132),
        [78] = Color.FromArgb(255, 120, 34, 43),
        [79] = Color.FromArgb(255, 14, 49, 109),
        [80] = Color.FromArgb(255, 114, 42, 63),
        [81] = Color.FromArgb(255, 123, 113, 94),
        [82] = Color.FromArgb(255, 116, 29, 40),
        [83] = Color.FromArgb(255, 30, 46, 50),
        [84] = Color.FromArgb(255, 77, 50, 47),
        [85] = Color.FromArgb(255, 124, 27, 68),
        [86] = Color.FromArgb(255, 46, 91, 32),
        [87] = Color.FromArgb(255, 57, 90, 131),
        [88] = Color.FromArgb(255, 109, 40, 55),
        [89] = Color.FromArgb(255, 167, 162, 143),
        [90] = Color.FromArgb(255, 175, 177, 177),
        [91] = Color.FromArgb(255, 54, 65, 85),
        [92] = Color.FromArgb(255, 109, 108, 110),
        [93] = Color.FromArgb(255, 15, 106, 137),
        [94] = Color.FromArgb(255, 32, 75, 107),
        [95] = Color.FromArgb(255, 43, 62, 87),
        [96] = Color.FromArgb(255, 155, 159, 157),
        [97] = Color.FromArgb(255, 108, 132, 149),
        [98] = Color.FromArgb(255, 78, 96),
        [99] = Color.FromArgb(255, 174, 155, 127),
        [100] = Color.FromArgb(255, 64, 108, 143),
        [101] = Color.FromArgb(255, 31, 37, 59),
        [102] = Color.FromArgb(255, 171, 146, 118),
        [103] = Color.FromArgb(255, 19, 69, 115),
        [104] = Color.FromArgb(255, 150, 129, 108),
        [105] = Color.FromArgb(255, 100, 104, 106),
        [106] = Color.FromArgb(255, 16, 80, 130),
        [107] = Color.FromArgb(255, 161, 153, 131),
        [108] = Color.FromArgb(255, 56, 86, 148),
        [109] = Color.FromArgb(255, 82, 86, 97),
        [110] = Color.FromArgb(255, 127, 105, 86),
        [111] = Color.FromArgb(255, 140, 146, 154),
        [112] = Color.FromArgb(255, 89, 110, 135),
        [113] = Color.FromArgb(255, 71, 53, 50),
        [114] = Color.FromArgb(255, 68, 98, 79),
        [115] = Color.FromArgb(255, 115, 10, 39),
        [116] = Color.FromArgb(255, 34, 52, 87),
        [117] = Color.FromArgb(255, 100, 13, 27),
        [118] = Color.FromArgb(255, 163, 173, 198),
        [119] = Color.FromArgb(255, 105, 88, 83),
        [120] = Color.FromArgb(255, 155, 139, 128),
        [121] = Color.FromArgb(255, 98, 11, 28),
        [122] = Color.FromArgb(255, 91, 93, 94),
        [123] = Color.FromArgb(255, 98, 68, 40),
        [124] = Color.FromArgb(255, 115, 24, 39),
        [125] = Color.FromArgb(255, 27, 55, 109),
        [126] = Color.FromArgb(255, 236, 106, 174),
    };

    public static Dictionary<VehicleModel, VehicleType> VehicleTypesPerModel { get; } = new()
    {
        [VehicleModel.Landstalker] = VehicleType.Automobile,
        [VehicleModel.Bravura] = VehicleType.Automobile,
        [VehicleModel.Buffalo] = VehicleType.Automobile,
        [VehicleModel.Linerunner] = VehicleType.Automobile,
        [VehicleModel.Perennial] = VehicleType.Automobile,
        [VehicleModel.Sentinel] = VehicleType.Automobile,
        [VehicleModel.Dumper] = VehicleType.MonsterTruck,
        [VehicleModel.FireTruck] = VehicleType.Automobile,
        [VehicleModel.Trashmaster] = VehicleType.Automobile,
        [VehicleModel.Stretch] = VehicleType.Automobile,
        [VehicleModel.Manana] = VehicleType.Automobile,
        [VehicleModel.Infernus] = VehicleType.Automobile,
        [VehicleModel.Voodoo] = VehicleType.Automobile,
        [VehicleModel.Pony] = VehicleType.Automobile,
        [VehicleModel.Mule] = VehicleType.Automobile,
        [VehicleModel.Cheetah] = VehicleType.Automobile,
        [VehicleModel.Ambulance] = VehicleType.Automobile,
        [VehicleModel.Leviathan] = VehicleType.Helicopter,
        [VehicleModel.Moonbeam] = VehicleType.Automobile,
        [VehicleModel.Esperanto] = VehicleType.Automobile,
        [VehicleModel.Taxi] = VehicleType.Automobile,
        [VehicleModel.Washington] = VehicleType.Automobile,
        [VehicleModel.Bobcat] = VehicleType.Automobile,
        [VehicleModel.MrWhoopee] = VehicleType.Automobile,
        [VehicleModel.BFInjection] = VehicleType.Automobile,
        [VehicleModel.Hunter] = VehicleType.Helicopter,
        [VehicleModel.Premier] = VehicleType.Automobile,
        [VehicleModel.Enforcer] = VehicleType.Automobile,
        [VehicleModel.Securicar] = VehicleType.Automobile,
        [VehicleModel.Banshee] = VehicleType.Automobile,
        [VehicleModel.Predator] = VehicleType.Boat,
        [VehicleModel.Bus] = VehicleType.Automobile,
        [VehicleModel.Rhino] = VehicleType.Automobile,
        [VehicleModel.Barracks] = VehicleType.Automobile,
        [VehicleModel.Hotknife] = VehicleType.Automobile,
        [VehicleModel.Trailer1] = VehicleType.Trailer,
        [VehicleModel.Previon] = VehicleType.Automobile,
        [VehicleModel.Coach] = VehicleType.Automobile,
        [VehicleModel.Cabbie] = VehicleType.Automobile,
        [VehicleModel.Stallion] = VehicleType.Automobile,
        [VehicleModel.Rumpo] = VehicleType.Automobile,
        [VehicleModel.RCBandit] = VehicleType.Automobile,
        [VehicleModel.Romero] = VehicleType.Automobile,
        [VehicleModel.Packer] = VehicleType.Automobile,
        [VehicleModel.Monster] = VehicleType.MonsterTruck,
        [VehicleModel.Admiral] = VehicleType.Automobile,
        [VehicleModel.Squalo] = VehicleType.Boat,
        [VehicleModel.Seasparrow] = VehicleType.Helicopter,
        [VehicleModel.Pizzaboy] = VehicleType.Motorcycle,
        [VehicleModel.Tram] = VehicleType.Train,
        [VehicleModel.Trailer2] = VehicleType.Trailer,
        [VehicleModel.Turismo] = VehicleType.Automobile,
        [VehicleModel.Speeder] = VehicleType.Boat,
        [VehicleModel.Reefer] = VehicleType.Boat,
        [VehicleModel.Tropic] = VehicleType.Boat,
        [VehicleModel.Flatbed] = VehicleType.Automobile,
        [VehicleModel.Yankee] = VehicleType.Automobile,
        [VehicleModel.Caddy] = VehicleType.Automobile,
        [VehicleModel.Solair] = VehicleType.Automobile,
        [VehicleModel.BerkleysRCVan] = VehicleType.Automobile,
        [VehicleModel.Skimmer] = VehicleType.Plane,
        [VehicleModel.PCJ600] = VehicleType.Motorcycle,
        [VehicleModel.Faggio] = VehicleType.Motorcycle,
        [VehicleModel.Freeway] = VehicleType.Motorcycle,
        [VehicleModel.RCBaron] = VehicleType.Plane,
        [VehicleModel.RCRaider] = VehicleType.Helicopter,
        [VehicleModel.Glendale] = VehicleType.Automobile,
        [VehicleModel.Oceanic] = VehicleType.Automobile,
        [VehicleModel.Sanchez] = VehicleType.Motorcycle,
        [VehicleModel.Sparrow] = VehicleType.Helicopter,
        [VehicleModel.Patriot] = VehicleType.Automobile,
        [VehicleModel.Quadbike] = VehicleType.QuadBike,
        [VehicleModel.Coastguard] = VehicleType.Boat,
        [VehicleModel.Dinghy] = VehicleType.Boat,
        [VehicleModel.Hermes] = VehicleType.Automobile,
        [VehicleModel.Sabre] = VehicleType.Automobile,
        [VehicleModel.Rustler] = VehicleType.Plane,
        [VehicleModel.ZR350] = VehicleType.Automobile,
        [VehicleModel.Walton] = VehicleType.Automobile,
        [VehicleModel.Regina] = VehicleType.Automobile,
        [VehicleModel.Comet] = VehicleType.Automobile,
        [VehicleModel.BMX] = VehicleType.Bmx,
        [VehicleModel.Burrito] = VehicleType.Automobile,
        [VehicleModel.Camper] = VehicleType.Automobile,
        [VehicleModel.Marquis] = VehicleType.Boat,
        [VehicleModel.Baggage] = VehicleType.Automobile,
        [VehicleModel.Dozer] = VehicleType.Automobile,
        [VehicleModel.Maverick] = VehicleType.Helicopter,
        [VehicleModel.NewsChopper] = VehicleType.Helicopter,
        [VehicleModel.Rancher] = VehicleType.Automobile,
        [VehicleModel.FBIRancher] = VehicleType.Automobile,
        [VehicleModel.Virgo] = VehicleType.Automobile,
        [VehicleModel.Greenwood] = VehicleType.Automobile,
        [VehicleModel.Jetmax] = VehicleType.Boat,
        [VehicleModel.HotringRacer] = VehicleType.Automobile,
        [VehicleModel.Sandking] = VehicleType.Automobile,
        [VehicleModel.BlistaCompact] = VehicleType.Automobile,
        [VehicleModel.PoliceMaverick] = VehicleType.Helicopter,
        [VehicleModel.Boxville] = VehicleType.Automobile,
        [VehicleModel.Benson] = VehicleType.Automobile,
        [VehicleModel.Mesa] = VehicleType.Automobile,
        [VehicleModel.RCGoblin] = VehicleType.Helicopter,
        [VehicleModel.HotringRacer3] = VehicleType.Automobile,
        [VehicleModel.HotringRacer2] = VehicleType.Automobile,
        [VehicleModel.BloodringBanger] = VehicleType.Automobile,
        [VehicleModel.RancherLure] = VehicleType.Automobile,
        [VehicleModel.SuperGT] = VehicleType.Automobile,
        [VehicleModel.Elegant] = VehicleType.Automobile,
        [VehicleModel.Journey] = VehicleType.Automobile,
        [VehicleModel.Bike] = VehicleType.Bmx,
        [VehicleModel.MountainBike] = VehicleType.Bmx,
        [VehicleModel.Beagle] = VehicleType.Plane,
        [VehicleModel.Cropduster] = VehicleType.Plane,
        [VehicleModel.Stuntplane] = VehicleType.Plane,
        [VehicleModel.Tanker] = VehicleType.Automobile,
        [VehicleModel.Roadtrain] = VehicleType.Automobile,
        [VehicleModel.Nebula] = VehicleType.Automobile,
        [VehicleModel.Majestic] = VehicleType.Automobile,
        [VehicleModel.Buccaneer] = VehicleType.Automobile,
        [VehicleModel.Shamal] = VehicleType.Plane,
        [VehicleModel.Hydra] = VehicleType.Plane,
        [VehicleModel.FCR900] = VehicleType.Motorcycle,
        [VehicleModel.NRG500] = VehicleType.Motorcycle,
        [VehicleModel.HPV1000] = VehicleType.Motorcycle,
        [VehicleModel.CementTruck] = VehicleType.Automobile,
        [VehicleModel.Towtruck] = VehicleType.Automobile,
        [VehicleModel.Fortune] = VehicleType.Automobile,
        [VehicleModel.Cadrona] = VehicleType.Automobile,
        [VehicleModel.FBITruck] = VehicleType.Automobile,
        [VehicleModel.Willard] = VehicleType.Automobile,
        [VehicleModel.Forklift] = VehicleType.Automobile,
        [VehicleModel.Tractor] = VehicleType.Automobile,
        [VehicleModel.CombineHarvester] = VehicleType.Automobile,
        [VehicleModel.Feltzer] = VehicleType.Automobile,
        [VehicleModel.Remington] = VehicleType.Automobile,
        [VehicleModel.Slamvan] = VehicleType.Automobile,
        [VehicleModel.Blade] = VehicleType.Automobile,
        [VehicleModel.Freight] = VehicleType.Train,
        [VehicleModel.Streak] = VehicleType.Train,
        [VehicleModel.Vortex] = VehicleType.Plane,
        [VehicleModel.Vincent] = VehicleType.Automobile,
        [VehicleModel.Bullet] = VehicleType.Automobile,
        [VehicleModel.Clover] = VehicleType.Automobile,
        [VehicleModel.Sadler] = VehicleType.Automobile,
        [VehicleModel.FireTruckLadder] = VehicleType.Automobile,
        [VehicleModel.Hustler] = VehicleType.Automobile,
        [VehicleModel.Intruder] = VehicleType.Automobile,
        [VehicleModel.Primo] = VehicleType.Automobile,
        [VehicleModel.Cargobob] = VehicleType.Helicopter,
        [VehicleModel.Tampa] = VehicleType.Automobile,
        [VehicleModel.Sunrise] = VehicleType.Automobile,
        [VehicleModel.Merit] = VehicleType.Automobile,
        [VehicleModel.UtilityVan] = VehicleType.Automobile,
        [VehicleModel.Nevada] = VehicleType.Plane,
        [VehicleModel.Yosemite] = VehicleType.Automobile,
        [VehicleModel.Windsor] = VehicleType.Automobile,
        [VehicleModel.Monster2] = VehicleType.MonsterTruck,
        [VehicleModel.Monster3] = VehicleType.MonsterTruck,
        [VehicleModel.Uranus] = VehicleType.Automobile,
        [VehicleModel.Jester] = VehicleType.Automobile,
        [VehicleModel.Sultan] = VehicleType.Automobile,
        [VehicleModel.Stratum] = VehicleType.Automobile,
        [VehicleModel.Elegy] = VehicleType.Automobile,
        [VehicleModel.Raindance] = VehicleType.Helicopter,
        [VehicleModel.RCTiger] = VehicleType.Automobile,
        [VehicleModel.Flash] = VehicleType.Automobile,
        [VehicleModel.Tahoma] = VehicleType.Automobile,
        [VehicleModel.Savanna] = VehicleType.Automobile,
        [VehicleModel.Bandito] = VehicleType.Automobile,
        [VehicleModel.FreightTrainFlatbed] = VehicleType.Train,
        [VehicleModel.StreakTrainTrailer] = VehicleType.Train,
        [VehicleModel.Kart] = VehicleType.Automobile,
        [VehicleModel.Mower] = VehicleType.Automobile,
        [VehicleModel.Dune] = VehicleType.MonsterTruck,
        [VehicleModel.Sweeper] = VehicleType.Automobile,
        [VehicleModel.Broadway] = VehicleType.Automobile,
        [VehicleModel.Tornado] = VehicleType.Automobile,
        [VehicleModel.AT400] = VehicleType.Plane,
        [VehicleModel.DFT30] = VehicleType.Automobile,
        [VehicleModel.Huntley] = VehicleType.Automobile,
        [VehicleModel.Stafford] = VehicleType.Automobile,
        [VehicleModel.BF400] = VehicleType.Motorcycle,
        [VehicleModel.Newsvan] = VehicleType.Automobile,
        [VehicleModel.Tug] = VehicleType.Automobile,
        [VehicleModel.TrailerTankerCommando] = VehicleType.Trailer,
        [VehicleModel.Emperor] = VehicleType.Automobile,
        [VehicleModel.Wayfarer] = VehicleType.Motorcycle,
        [VehicleModel.Euros] = VehicleType.Automobile,
        [VehicleModel.Hotdog] = VehicleType.Automobile,
        [VehicleModel.Club] = VehicleType.Automobile,
        [VehicleModel.BoxFreight] = VehicleType.Train,
        [VehicleModel.Trailer3] = VehicleType.Trailer,
        [VehicleModel.Andromada] = VehicleType.Plane,
        [VehicleModel.Dodo] = VehicleType.Plane,
        [VehicleModel.RCCam] = VehicleType.Automobile,
        [VehicleModel.Launch] = VehicleType.Boat,
        [VehicleModel.PoliceLS] = VehicleType.Automobile,
        [VehicleModel.PoliceSf] = VehicleType.Automobile,
        [VehicleModel.PoliceLV] = VehicleType.Automobile,
        [VehicleModel.PoliceRanger] = VehicleType.Automobile,
        [VehicleModel.Picador] = VehicleType.Automobile,
        [VehicleModel.SwatTank] = VehicleType.Automobile,
        [VehicleModel.Alpha] = VehicleType.Automobile,
        [VehicleModel.Phoenix] = VehicleType.Automobile,
        [VehicleModel.GlendaleDamaged] = VehicleType.Automobile,
        [VehicleModel.SadlerDamaged] = VehicleType.Automobile,
        [VehicleModel.BaggageTrailerCovered] = VehicleType.Trailer,
        [VehicleModel.BaggageTrailerUncovered] = VehicleType.Trailer,
        [VehicleModel.TrailerStairs] = VehicleType.Trailer,
        [VehicleModel.FarmTrailer] = VehicleType.Trailer,
        [VehicleModel.FarmTrailerTwo] = VehicleType.Trailer,
    };
}
