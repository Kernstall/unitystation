using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

/// <summary>
/// Message a client (or server player) sends to the server to request the server to validate
/// and perform a HandApply interaction (if validation succeeds).
///
/// The server-side validation and interaction processing is delegated to a gameObject and component of the
/// client's choice. When the message is sent, they specify the gameobject and component that should
/// process the request on the server side.
/// </summary>
public class RequestHandApplyMessage : ClientMessage
{
	public static short MessageType = (short) MessageTypes.RequestHandApplyMessage;

	//object that will process the interaction
	public uint ProcessorObject;
	//netid of the object being targeted
	public uint TargetObject;
	//targeted body part
	public BodyPartType TargetBodyPart;
	public string TargetComponent;

	public override IEnumerator Process()
	{
		//look up item in active hand slot
		var clientPNA = SentByPlayer.Script.playerNetworkActions;
		var usedSlot = HandSlot.ForName(clientPNA.activeHand);
		var usedObject = clientPNA.Inventory[usedSlot.equipSlot].Item;
		yield return WaitFor(TargetObject, ProcessorObject);
		var targetObj = NetworkObjects[0];
		var processorObj = NetworkObjects[1];
		var performerObj = SentByPlayer.GameObject;

		ProcessHandApply(usedObject, targetObj, processorObj, performerObj, usedSlot, TargetComponent);
	}


	private void ProcessHandApply(GameObject handObject, GameObject targetObj, GameObject processorObj, GameObject performerObj, HandSlot usedSlot, string specificComponent)
	{
		//try to look up the components on the processor that can handle this interaction
		var processorComponents = InteractionMessageUtils.TryGetProcessors<HandApply>(processorObj, specificComponent);
		//invoke each component that can handle this interaction
		var handApply = HandApply.ByClient(performerObj, handObject, targetObj, TargetBodyPart, usedSlot);
		foreach (var processorComponent in processorComponents)
		{
			if (processorComponent.ServerProcessInteraction(handApply))
			{
				//something happened, don't check further components
				return;
			}
		}
	}

	/// <summary>
	/// For most cases you should use InteractionMessageUtils.SendRequest() instead of this.
	///
	/// Sends a request to the server to validate + perform the interaction.
	/// </summary>
	/// <param name="handApply">info on the interaction being performed. Each object involved in the interaction
	/// must have a networkidentity.</param>
	/// <param name="processorObject">object who has a component implementing IInteractionProcessor<HandApply> which
	/// will process the interaction on the server-side. This object must have a NetworkIdentity and there must only be one instance
	/// of this component on the object. For organization, we suggest that the component which is sending this message
	/// should be on the processorObject, as such this parameter should almost always be passed using "this.gameObject", and
	/// should almost always be either a component on the target object or a component on the used object</param>
	/// <param name="specificComponent">Name of the specific component to be targeted by the interaction</param>
	public static void Send(HandApply handApply, GameObject processorObject, string specificComponent = null)
	{
		var msg = new RequestHandApplyMessage
		{
			TargetObject = handApply.TargetObject.GetComponent<NetworkIdentity>().netId,
			ProcessorObject = processorObject.GetComponent<NetworkIdentity>().netId,
			TargetBodyPart = handApply.TargetBodyPart,
			TargetComponent = specificComponent
		};
		msg.Send();
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		ProcessorObject = reader.ReadUInt32();
		TargetObject = reader.ReadUInt32();
		TargetBodyPart = (BodyPartType) reader.ReadUInt32();
		TargetComponent = reader.ReadString();
	}

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.WriteUInt32(ProcessorObject);
		writer.WriteUInt32(TargetObject);
		writer.WriteInt32((int) TargetBodyPart);
		writer.WriteString(TargetComponent);
	}

}
