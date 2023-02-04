﻿using System;
using System.Collections.Generic;
using System.Linq;
namespace ZPViral.Player;

public partial class PlayerAnimator : EntityComponent<PlayerPawn>, ISingletonComponent
{
	public virtual void Simulate(IClient cl)
	{
		var player = Entity;
		var controller = player.Controller;
		CitizenAnimationHelper helper = new CitizenAnimationHelper( player );

		if ( cl.Components.Get<DevCamera>() == null || !cl.Components.Get<DevCamera>().Enabled )
			helper.WithLookAt( player.AimRay.Position + player.AimRay.Forward * 200 );

		helper.WithVelocity( player.Velocity );
		helper.WithWishVelocity( controller.GetWishVelocity() );

		helper.DuckLevel = Input.Down( InputButton.Duck ) ? 0.75f : 0.0f;

		helper.IsGrounded = controller.GroundEntity != null;

		Rotation rotation = player.ViewAngles.ToRotation();
		var idealRotation = Rotation.LookAt( rotation.Forward.WithZ( 0 ), Vector3.Up );
		player.Rotation = Rotation.Slerp( player.Rotation, idealRotation, controller.GetWishVelocity().Length * Time.Delta * 0.05f );

		/*if ( LastActiveWeapon is WeaponBase wep )
			wep.SimulateAnimator( helper );
		else
			helper.HoldType = CitizenAnimationHelper.HoldTypes.None;*/

	}

	/*public virtual void Simulate( IClient cl )
	{
		var player = Entity;
		var controller = player.Controller;
		CitizenAnimationHelper animHelper = new CitizenAnimationHelper( player );

		animHelper.WithWishVelocity( controller.GetWishVelocity() );
		animHelper.WithVelocity( controller.Velocity );
		animHelper.WithLookAt( player.EyePosition + player.EyeRotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f );
		animHelper.AimAngle = player.EyeRotation;
		animHelper.FootShuffle = 0f;
		animHelper.DuckLevel = MathX.Lerp( animHelper.DuckLevel, 1 - controller.CurrentEyeHeight.Remap( 30, 72, 0, 1 ).Clamp( 0, 1 ), Time.Delta * 10.0f );
		//animHelper.VoiceLevel = (Game.IsClient && cl.IsValid()) ? cl.Voice.LastHeard < 0.5f ? cl.Voice.CurrentLevel : 0.0f : 0.0f;
		animHelper.IsGrounded = controller.GroundEntity != null;
		animHelper.IsSwimming = player.GetWaterLevel() >= 0.5f;
		animHelper.IsWeaponLowered = false;

		var weapon = player.ActiveWeapon;
		if ( weapon.IsValid() )
		{
			//player.SetAnimParameter( "holdtype", (int)weapon.HoldType );
			//player.SetAnimParameter( "holdtype_handedness", (int)weapon.Handedness );
		}
	}*/

	public virtual void FrameSimulate( IClient cl )
	{
		//
	}
}
