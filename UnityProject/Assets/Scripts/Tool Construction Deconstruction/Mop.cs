﻿using UnityEngine;

/// <summary>
/// Main component for Mop. Allows mopping to be done on tiles.
/// </summary>
[RequireComponent(typeof(Pickupable))]
public class Mop : Interactable<PositionalHandApply>
{
	//server-side only, tracks if mop is currently cleaning.
	private bool isCleaning;

	protected override bool WillInteract(PositionalHandApply interaction, NetworkSide side)
	{
		if (!base.WillInteract(interaction, side)) return false;
		//can only mop tiles
		if (!Validations.HasComponent<InteractableTiles>(interaction.TargetObject)) return false;
		return true;
	}

	protected override void ServerPerformInteraction(PositionalHandApply interaction)
	{
		if (!isCleaning)
		{
			//server is performing server-side logic for the interaction
			//do the mopping
			var progressFinishAction = new FinishProgressAction(
				reason =>
				{
					if (reason == FinishProgressAction.FinishReason.INTERRUPTED)
					{
						CancelCleanTile();
					}
					else if (reason == FinishProgressAction.FinishReason.COMPLETED)
					{
						CleanTile(interaction.WorldPositionTarget);
						isCleaning = false;
					}
				}
			);
			isCleaning = true;

			//Start the progress bar:
			UIManager.ProgressBar.StartProgress(interaction.WorldPositionTarget.RoundToInt(),
				5f, progressFinishAction, interaction.Performer);
		}
	}

	public void CleanTile(Vector3 worldPos)
	{
		var worldPosInt = worldPos.CutToInt();
		var matrix = MatrixManager.AtPoint(worldPosInt, true);
		var localPosInt = MatrixManager.WorldToLocalInt(worldPosInt, matrix);
		matrix.MetaDataLayer.Clean(worldPosInt, localPosInt, true);
	}


	private void CancelCleanTile()
	{
		//stop the in progress cleaning
		isCleaning = false;
	}

}