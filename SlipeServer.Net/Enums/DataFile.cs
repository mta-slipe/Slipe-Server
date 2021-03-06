using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Net.Enums
{
    [Flags]
    public enum DataFile
    {
        None = 0,
        carmodsDat = 0x01,
        animgrpDat = 0x02,
        handlingCfg = 0x04,
        ar_statsDat = 0x08,
        meleeDat = 0x10,
        clothesDat = 0x20,
        objectDat = 0x40,
        defaultDat = 0x80,
        surfaceDat = 0x100,
        defaultIde = 0x200,
        gtaDat = 0x400,
        surfinfoDat = 0x800,
        pedsIde = 0x1000,
        vehiclesIde = 0x2000,
        pedstatsDat = 0x4000,
        waterDat = 0x8000,
        txdcutIde = 0x10000,
        water1Dat = 0x20000,
        weaponsCol = 0x40000,
        weaponDat = 0x80000,
        plantsDat = 0x100000,
        pedIfp = 0x200000,
        furniturDat = 0x400000,
        procobjDat = 0x800000,
        maps = 0x1000000,
        timecycDat = 0x2000000,
    }
}
