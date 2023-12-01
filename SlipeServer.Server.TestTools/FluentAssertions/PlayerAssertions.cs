using FluentAssertions.Execution;
using SlipeServer.Server.Elements;

namespace SlipeServer.Server.TestTools.FluentAssertions;

public class PlayerAssertions : ElementAssertionsBase<Player>
{
    public PlayerAssertions(Player player) : base(player)
    {
    }

    public override void BeEquivalentTo(Player player, string because = "", params object[] becauseArgs)
    {
        using var _ = new AssertionScope();

        base.BeEquivalentTo(player, because, becauseArgs);
        AssertPropertyEquality(e => e.Camera.Position, player.Camera.Position, "Camera.Position", because, becauseArgs);
        AssertPropertyEquality(e => e.Camera.Interior, player.Camera.Interior, "Camera.Interior", because, becauseArgs);
        AssertPropertyEquality(e => e.Camera.LookAt, player.Camera.LookAt, "Camera.LookAt", because, becauseArgs);
        AssertPropertyEquality(e => e.WantedLevel, player.WantedLevel, "WantedLevel", because, becauseArgs);
        AssertPropertyEquality(e => e.AimOrigin, player.AimOrigin, "AimOrigin", because, becauseArgs);
        AssertPropertyEquality(e => e.AimDirection, player.AimDirection, "AimDirection", because, becauseArgs);
        AssertPropertyEquality(e => e.CameraPosition, player.CameraPosition, "CameraPosition", because, becauseArgs);
        AssertPropertyEquality(e => e.CameraDirection, player.CameraDirection, "CameraDirection", because, becauseArgs);
        AssertPropertyEquality(e => e.CameraRotation, player.CameraRotation, "CameraRotation", because, becauseArgs);
        AssertPropertyEquality(e => e.IsOnGround, player.IsOnGround, "IsOnGround", because, becauseArgs);
        AssertPropertyEquality(e => e.IsDucked, player.IsDucked, "IsDucked", because, becauseArgs);
        AssertPropertyEquality(e => e.WearsGoggles, player.WearsGoggles, "WearsGoggles", because, becauseArgs);
        AssertPropertyEquality(e => e.HasContact, player.HasContact, "HasContact", because, becauseArgs);
        AssertPropertyEquality(e => e.IsChoking, player.IsChoking, "IsChoking", because, becauseArgs);
        AssertPropertyEquality(e => e.AkimboTargetUp, player.AkimboTargetUp, "AkimboTargetUp", because, becauseArgs);
        AssertPropertyEquality(e => e.IsSyncingVelocity, player.IsSyncingVelocity, "IsSyncingVelocity", because, becauseArgs);
        AssertPropertyEquality(e => e.IsStealthAiming, player.IsStealthAiming, "IsStealthAiming", because, becauseArgs);
        AssertPropertyEquality(e => e.IsVoiceMuted, player.IsVoiceMuted, "IsVoiceMuted", because, becauseArgs);
        AssertPropertyEquality(e => e.IsChatMuted, player.IsChatMuted, "IsChatMuted", because, becauseArgs);
        AssertPropertyEquality(e => e.Money, player.Money, "Money", because, becauseArgs);
        AssertPropertyEquality(e => e.NametagText, player.NametagText, "NametagText", because, becauseArgs);
        AssertPropertyEquality(e => e.IsNametagShowing, player.IsNametagShowing, "NametagText", because, becauseArgs);
        AssertPropertyEquality(e => e.NametagColor, player.NametagColor, "NametagText", because, becauseArgs);
    }
}
