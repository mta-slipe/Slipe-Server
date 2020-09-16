using System;
using System.Numerics;
using System.Text.RegularExpressions;

namespace MtaServer.Server.Elements
{
    public class Player: Ped
    {
        public override ElementType ElementType => ElementType.Player;

        public Client Client { get; }

        public PlayerWeapon CurrentWeapon { get; set; }
        public Element? ContactElement { get; set; }

        public Vector3 AimOrigin { get; set; }
        public Vector3 AimDirection { get; set; }

        public Vector3 CameraPosition { get; set; }
        public Vector3 CameraDirection { get; set; }
        public float CameraRotation { get; set; }

        public bool IsInWater { get; set; }
        public bool IsOnGround { get; set; }
        public bool IsDucked { get; set; }
        public bool WearsGoggles { get; set; }
        public bool HasContact { get; set; }
        public bool IsChoking { get; set; }
        public bool AkimboTargetUp { get; set; }
        public bool IsOnFire { get; set; }
        public bool IsSyncingVelocity { get; set; }
        public bool IsStealthAiming { get; set; }

        internal Player(Client client) : base(0, Vector3.Zero)
        {
            this.Client = client;
        }

        public new Player AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }

        public void HandleCommand(string command, string[] arguments) => OnCommand?.Invoke(command, arguments);
        public event Action<string, string[]>? OnCommand;
    }
}
