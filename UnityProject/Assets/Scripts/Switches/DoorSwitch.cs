﻿using System.Collections;
using UnityEngine;
using Mirror;

/// <summary>
/// Allows object to function as a door switch - opening / closing door when clicked.
/// </summary>
public class DoorSwitch : NBHandApplyInteractable
{
	private Animator animator;
	private SpriteRenderer spriteRenderer;

	public DoorController[] doorControllers;

	private void Start()
	{
		//This is needed because you can no longer apply shutterSwitch prefabs (it will move all of the child sprite positions)
		gameObject.layer = LayerMask.NameToLayer("WallMounts");
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		animator = GetComponent<Animator>();
	}

	protected override bool WillInteract(HandApply interaction, NetworkSide side)
	{
		if (!base.WillInteract(interaction, side)) return false;
		//this validation is only done client side for their convenience - they can't
		//press button while it's animating.
		if (side == NetworkSide.Client)
		{
			//if the button is idle and not animating it can be pressed
			return animator.GetCurrentAnimatorStateInfo(0).IsName("Idle");
		}

		return true;
	}

	protected override void ServerPerformInteraction(HandApply interaction)
	{
		for (int i = 0; i < doorControllers.Length; i++)
		{
			if (!doorControllers[i].IsOpened)
			{
				doorControllers[i].Open();
			}
		}
		RpcPlayButtonAnim();
	}

	[ClientRpc]
	public void RpcPlayButtonAnim()
	{
		animator.SetTrigger("activated");
	}
}