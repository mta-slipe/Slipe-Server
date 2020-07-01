using System;
using MtaServer.Packets;
using MTAServerWrapper.Server;
using System.Net;
using System.Numerics;
using System.Text.RegularExpressions;

namespace MtaServer.Server.Elements
{
    public class Player: Ped
    {
        public override ElementType ElementType => ElementType.Player;

        public Client Client { get; }

        public string? Name { get; set; }
        public string? NameNoColor
        {
            get
            {
                if (Name == null)
                    return null;
                string tempName = Name;
                while (true)
                {
                    if (Regex.Replace(tempName, "[#][0-9a-fA-F][0-9a-fA-F][0-9a-fA-F][0-9a-fA-F][0-9a-fA-F][0-9a-fA-F]", "") == tempName)
                        return tempName;

                        tempName = Regex.Replace(tempName, "[#][0-9a-fA-F][0-9a-fA-F][0-9a-fA-F][0-9a-fA-F][0-9a-fA-F][0-9a-fA-F]", "");
                }
            }
        }
        public float Health { get; set; }
        public float Armor { get; set; }
        public PlayerWeapon CurrentWeapon { get; set; }

        public Element? ContactElement { get; set; }

        public Vector3 AimOrigin { get; set; }
        public Vector3 AimDirection { get; set; }

        public Vector3 CameraPosition { get; set; }
        public Vector3 CameraDirection { get; set; }
        public float CameraRotation { get; set; }

        public bool IsInWater { get; set; }
        public bool IsOnGround { get; set; }
        public bool HasJetpack { get; set; }
        public bool IsDucked { get; set; }
        public bool WearsGoggles { get; set; }
        public bool HasContact { get; set; }
        public bool IsChoking { get; set; }
        public bool AkimboTargetUp { get; set; }
        public bool IsOnFire { get; set; }
        public bool IsSyncingVelocity { get; set; }
        public bool IsStealthAiming { get; set; }

        internal Player(Client client): base()
        {
            this.Client = client;
        }

        public void HandleCommand(string command, string[] arguments) => OnCommand?.Invoke(command, arguments);
        public void HandleJoin() => OnJoin?.Invoke(this);

        public static event Action<Player>? OnJoin;
        public event Action<string, string[]>? OnCommand;
    }
}
