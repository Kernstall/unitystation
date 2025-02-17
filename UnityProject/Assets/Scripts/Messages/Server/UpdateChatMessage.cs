﻿using System.Collections;
using UnityEngine;
using Mirror;

/// <summary>
///     Message that tells client to add a ChatEvent to their chat
/// </summary>
public class UpdateChatMessage : ServerMessage
{
	public static short MessageType = (short) MessageTypes.UpdateChatMessage;
	public ChatChannel Channels;
	public string ChatMessageText;
	public uint Recipient;//fixme: Recipient is redundant! Can be safely removed

	public override IEnumerator Process()
	{
		yield return WaitFor(Recipient);

		ChatRelay.Instance.AddToChatLogClient(ChatMessageText, Channels);
	}

	public static UpdateChatMessage Send(GameObject recipient, ChatChannel channels, string message)
	{
		UpdateChatMessage msg =
			new UpdateChatMessage {Recipient = recipient.GetComponent<NetworkIdentity>().netId, Channels = channels, ChatMessageText = message};

		msg.SendTo(recipient);
		return msg;
	}

	public override string ToString()
	{
		return string.Format("[UpdateChatMessage Recipient={0} Channels={1} ChatMessageText={2}]", Recipient, Channels, ChatMessageText);
	}
}