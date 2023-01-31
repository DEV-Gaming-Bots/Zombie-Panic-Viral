namespace ZPViral.Player;

public partial class PlayerPawn
{
	public Entity Using { get; protected set; }

	protected virtual void TickUse()
	{
		// This is serverside only
		if ( !Game.IsServer ) return;

		// Turn prediction off
		using ( Prediction.Off() )
		{
			if ( Input.Pressed( InputButton.Use ) )
			{
				Using = FindUsable();

				if ( Using == null )
				{
					UseFail();
					return;
				}
			}

			if ( !Input.Down( InputButton.Use ) )
			{
				StopUsing();
				return;
			}

			if ( !Using.IsValid() )
				return;

			// If we move too far away or something we should probably ClearUse()?

			//
			// If use returns true then we can keep using it
			//
			if ( Using is IUse use && use.OnUse( this ) )
				return;

			StopUsing();
		}
	}

	/// <summary>
	/// Player tried to use something but there was nothing there.
	/// Tradition is to give a disappointed boop.
	/// </summary>
	protected virtual void UseFail()
	{
		PlaySound( "player_use_fail" );
	}

	/// <summary>
	/// If we're using an entity, stop using it
	/// </summary>
	protected virtual void StopUsing()
	{
		Using = null;
	}

	/// <summary>
	/// Returns if the entity is a valid usable entity
	/// </summary>
	protected bool IsValidUseEntity( Entity e )
	{
		if ( e == null ) return false;

		//if ( e is IInteractable interactable )
		//	if ( !interactable.IsUsable( this ) ) return false;

		if ( e is IUse usable )
			if ( !usable.IsUsable( this ) ) return false;

		return true;
	}

	/// <summary>
	/// Find a usable entity for this player to use
	/// </summary>
	protected virtual Entity FindUsable()
	{
		// First try a direct 0 width line
		var tr = GetEyeTraceResult( 85.0f );

		// See if any of the parent entities are usable if we ain't.
		var ent = tr.Entity;
		while ( ent.IsValid() && !IsValidUseEntity( ent ) )
		{
			ent = ent.Parent;
		}

		// Nothing found, try a wider search
		if ( !IsValidUseEntity( ent ) )
		{
			tr = GetEyeTraceResult( 85.0f, 2.0f );

			// See if any of the parent entities are usable if we ain't.
			ent = tr.Entity;
			while ( ent.IsValid() && !IsValidUseEntity( ent ) )
			{
				ent = ent.Parent;
			}
		}

		if ( !IsValidUseEntity( ent ) ) return null;

		return ent;
	}
}
