using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZPViral.Player;

public partial class SurvivorPawn : PlayerPawn
{
	[Net] public bool IsInfected { get; set; }
	TimeUntil timeInfection;

	public enum InfectionEnum
	{
		None,
		Window,
		Symptom,
		Transform,
	}

	InfectionEnum infectStatus;

	public void Infect()
	{
		IsInfected = true;
		infectStatus = InfectionEnum.Window;
		timeInfection = 20.0f;
	}

	public override void CreateHull()
	{
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
	
		base.CreateHull();
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		if(IsInfected)
		{
			if ( timeInfection > 0.0f ) return;

			switch( infectStatus )
			{
				case InfectionEnum.Window:
					infectStatus = InfectionEnum.Symptom;
					break;
				case InfectionEnum.Symptom:
					infectStatus = InfectionEnum.Transform;
					break;
				case InfectionEnum.Transform:

					break;
			}
		}
	}

	public override void Spawn()
	{
		infectStatus = InfectionEnum.None;

		RenderColor = Color.White;

		SetModel( "models/citizen/citizen.vmdl" );
		Tags.Add( "survivor" );

		LifeState = LifeState.Alive;

		Components.Create<Controller>();
		Components.Create<Inventory>();
		Components.Create<PlayerAnimator>();

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		SetSpawnPosition();
		CreateHull();
	}

	public override void OnKilled()
	{
		base.OnKilled();
	}
}

