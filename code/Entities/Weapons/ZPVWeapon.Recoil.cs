using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZPViral.Weapons;

public partial class Weapon : AnimatedEntity
{
	[Net, Predicted] public Vector2 CurrentRecoil { get; set; }
	[Net, Predicted] public TimeUntil TimeUntilRemove { get; set; }

	public void AddRecoil()
	{
		CurrentRecoil += new Vector2( Recoil.x, Recoil.y ) * Time.Delta;
		TimeUntilRemove = TimeRecoilRecovery;
	}

	public void SimulateRecoil( IClient cl, PlayerPawn player )
	{
		var pitchOffset = Input.AnalogLook.pitch;

		if ( TimeUntilRemove )
			CurrentRecoil -= DecayFactor * Time.Delta;

		if ( pitchOffset > 0f )
		{
			// Figure this magic number out later, it's shit
			pitchOffset *= 8f;
			var newPitch = (CurrentRecoil.y - pitchOffset).Clamp( 0f, MaxRecoil );
			CurrentRecoil = CurrentRecoil.WithY( newPitch );
		}

		CurrentRecoil = CurrentRecoil.Clamp( 0, MaxRecoil );
	}
}
