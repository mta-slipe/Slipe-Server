using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Net.Wrappers.Enums
{
    public enum SpecialDetection
    {
        None = 0,
        DisallowD3d9 = 12,
        DisallowVM = 14,
        DisallowedDisabledDriverSign = 15,
        DisasllowDisableAntiCheat = 16,
        DisallowNonStandardGta3 = 20,
        DisallowResourceScriptDownloadErrors = 22,
        DisallowResourceFileDownloadErrors = 23,
        DisallowWine = 28,
        IgnoreInjectedKeyboardInput = 31,
        IgnoreInjectedMouseInput = 32,
        DisallowNetLimiters = 33,
        DisallowInternetCafe = 34,
        DisallowFpsLockers = 35,
        DisallowAutoHotKey = 36,
    }
}
