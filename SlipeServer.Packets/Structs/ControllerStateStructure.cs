namespace SlipeServer.Packets.Structs;

public class ControllerState
{
    public short LeftStickX { get; set; }
    public short LeftStickY { get; set; }
    public short RightStickX { get; set; }
    public short RightStickY { get; set; }
    public short LeftSould2 { get; set; }
    public short RightShoulder2 { get; set; }
    public short DPadUp { get; set; }
    public short DPadDown { get; set; }
    public short DPadLeft { get; set; }
    public short DPadRight { get; set; }
    public short Start { get; set; }
    public short Select { get; set; }
    public short ShockButtonR { get; set; }
    public short ChatIndicated { get; set; }
    public short RadioTrackSkip { get; set; }
    public bool LeftShoulder1 { get; set; }
    public bool RightShoulder1 { get; set; }
    public bool ButtonSquare { get; set; }
    public bool ButtonCross { get; set; }
    public bool ButtonCircle { get; set; }
    public bool ButtonTriangle { get; set; }
    public bool ShockButtonL { get; set; }
    public bool PedWalk { get; set; }


    public ControllerState()
    {

    }
}
