﻿using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// A set of flags to show active chat channels. Be aware this can contain multiple active chat channels at a time!
/// </summary>
[Flags]
public enum ChatChannel
{
	[Description("")] 	None 		= 0,
	[Description("")] 	Examine 	= 1 << 0,
	[Description("")] 	Local 		= 1 << 1,
	[Description("")] 	OOC 		= 1 << 2,
	[Description(";")] 	Common 		= 1 << 3,
	[Description(":b")] Binary 		= 1 << 4,
	[Description(":u")] Supply 		= 1 << 5,
	[Description(":y")] CentComm 	= 1 << 6,
	[Description(":c")] Command 	= 1 << 7,
	[Description(":e")] Engineering = 1 << 8,
	[Description(":m")] Medical 	= 1 << 9,
	[Description(":n")] Science 	= 1 << 10,
	[Description(":s")] Security 	= 1 << 11,
	[Description(":v")] Service 	= 1 << 12,
	[Description(":t")] Syndicate 	= 1 << 13,
	[Description("")] 	System 		= 1 << 14,
	[Description(":g")] Ghost 		= 1 << 15,
	[Description("")] 	Combat 		= 1 << 16,
	[Description("")]	Warning		= 1 << 17
}

/// <summary>
/// A set of flags to show active chat modifiers. Be aware this can contain multiple active chat modifiers at once!
/// </summary>
[Flags]
public enum ChatModifier
{
	None 	= 0,
	Drunk 	= 1 << 0,
	Stutter = 1 << 1,
	Mute 	= 1 << 2,
	Hiss 	= 1 << 3,
	Clown 	= 1 << 4,
	Whisper = 1 << 5,
}

public class ChatEvent
{
	public ChatChannel channels;
	public string message;
	public ChatModifier modifiers = ChatModifier.None;
	public string speaker;
	public double timestamp;
	public Vector2 position;
	public float radius;
	public float sizeMod = 1f;
	/// <summary>
	/// Send chat message only to those on this matrix
	/// </summary>
	public MatrixInfo matrix = MatrixInfo.Invalid;

	public ChatEvent() {
		timestamp = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
	}

	public ChatEvent(string message, ConnectedPlayer speaker, ChatChannel channels)
	{
		var player = speaker.Script;
		this.channels = channels;
		this.modifiers = (player == null) ? ChatModifier.None : player.GetCurrentChatModifiers();
		this.speaker = ((channels & ChatChannel.OOC) == ChatChannel.OOC) ? speaker.Username : player.name;
		this.position = ((player == null) ? Vector2.zero : (Vector2) player.gameObject.transform.position);
		this.message = ProcessMessage(message, this.speaker, this.channels, modifiers);
	}

	public ChatEvent(string message, ChatChannel channels, bool skipProcessing = false)
	{
		this.channels = channels;
		speaker = "";
		if (skipProcessing)
		{
			this.message = message;
		}
		else
		{
			this.message = ProcessMessage(message, speaker, this.channels, modifiers);
		}
	}

	public static ChatChannel GetNonNetworkedChannels()
	{
		return ChatChannel.Examine | ChatChannel.System;
	}

	private string ProcessMessage(string message, string speaker, ChatChannel channels, ChatModifier modifiers)
	{
		message = StripTags(message);

		//Skip everything if system message
		if ((channels & ChatChannel.System) == ChatChannel.System)
		{
			this.channels = ChatChannel.System;
			this.modifiers = ChatModifier.None;
			return $"<b><i>{message}</i></b>";
		}

		//Skip everything in case of combat channel
		if ((channels & ChatChannel.Combat) == ChatChannel.Combat)
		{
			this.channels = ChatChannel.Combat;
			this.modifiers = ChatModifier.None;
			return $"<b>{message}</b>"; //POC
		}

		//Skip everything if examining something
		if ((channels & ChatChannel.Examine) == ChatChannel.Examine)
		{
			this.channels = ChatChannel.Examine;
			this.modifiers = ChatModifier.None;
			return $"<b><i>{message}</i></b>";
		}

		// Skip everything if the message is a local warning
		if ((channels & ChatChannel.Warning) == ChatChannel.Warning)
		{
			this.channels = ChatChannel.Warning;
			this.modifiers = ChatModifier.None;
			return $"<i>{message}</i>";
		}

		//Check for emote. If found skip chat modifiers, make sure emote is only in Local channel
		Regex rx = new Regex("^(/me )");
		if (rx.IsMatch(message))
		{
			// /me message
			this.channels = ChatChannel.Local;
			message = rx.Replace(message, " ");
			message = $"<i><b>{speaker}</b> {message}</i>";
			return message;
		}

		//Check for OOC. If selected, remove all other channels and modifiers (could happen if UI fucks up or someone tampers with it)
		if ((channels & ChatChannel.OOC) == ChatChannel.OOC)
		{
			this.channels = ChatChannel.OOC;
			this.modifiers = ChatModifier.None;

			message = $"<b>{speaker}: {message}</b>";
			return message;
		}

		//Ghosts don't get modifiers
		if ((channels & ChatChannel.Ghost) == ChatChannel.Ghost)
		{
			this.channels = ChatChannel.Ghost;
			this.modifiers = ChatModifier.None;
			return $"<b>{speaker}: {message}</b>";
		}

		message = ApplyModifiers(message, modifiers);
		if (message.Length < 1)
		{
			// if message + modifiers leads to no text, do not display
			this.channels = ChatChannel.None;
		}
		message = "<b>" + speaker + "</b> says: \"" + message + "\"";

		return message;
	}

	private string StripTags(string input)
	{
		//Regex - find "<" followed by any number of not ">" and ending in ">". Matches any HTML tags.
		Regex rx = new Regex("[<][^>]+[>]");
		string output = rx.Replace(input, "");

		return output;
	}

	private string ApplyModifiers(string input, ChatModifier modifiers)
	{
		string output = input;

		//Clowns say a random number (1-3) HONK!'s after every message
		if ((modifiers & ChatModifier.Clown) == ChatModifier.Clown)
		{
			int intensity = Random.Range(1, 4);
			for (int i = 0; i < intensity; i++)
			{
				if (i == 0)
				{
					output = output + " HONK!";
				}
				else
				{
					output = output + "HONK!";
				}
			}
		}

		//Sneks say extra S's
		if ((modifiers & ChatModifier.Hiss) == ChatModifier.Hiss)
		{
			//Regex - find 1 or more "s"
			Regex rx = new Regex("s+|S+");
			output = rx.Replace(output, Hiss);
		}

		//Stuttering people randomly repeat beginnings of words
		if ((modifiers & ChatModifier.Stutter) == ChatModifier.Stutter)
		{
			//Regex - find word boundary followed by non digit, non special symbol, non end of word letter. Basically find the start of words.
			Regex rx = new Regex(@"(\b)+([^\d\W])\B");
			output = rx.Replace(output, Stutter);
		}

		//Drunk people slur all "s" into "sh", randomly ...hic!... between words and have high % to ...hic!... after a sentance
		if ((modifiers & ChatModifier.Drunk) == ChatModifier.Drunk)
		{
			//Regex - find 1 or more "s"
			Regex rx = new Regex("s+|S+");
			output = rx.Replace(output, Slur);
			//Regex - find 1 or more whitespace
			rx = new Regex(@"\s+");
			output = rx.Replace(output, Hic);
			//50% chance to ...hic!... at end of sentance
			if (Random.Range(1, 3) == 1)
			{
				output = output + " ...hic!...";
			}
		}
		if ((modifiers & ChatModifier.Whisper) == ChatModifier.Whisper)
		{
			//If user is in barely conscious state, make text italic
			//todo: decrease range and modify text somehow
			//This can be changed later to other status effects
			output = "<i>"+output+"</i>";
		}
		if ((modifiers & ChatModifier.Mute) == ChatModifier.Mute)
		{
			//If user is in unconscious state remove text
			//This can be changed later to other status effects
			output = "";
		}

		return output;
	}

	#region Match Evaluators - contains the methods for string replacement magic

	private static string Slur(Match m)
	{
		string x = m.ToString();
		if (char.IsLower(x[0]))
		{
			x = x + "h";
		}
		else
		{
			x = x + "H";
		}

		return x;
	}

	private static string Hic(Match m)
	{
		string x = m.ToString();
		//10% chance to hic at any given space
		if (Random.Range(1, 11) == 1)
		{
			x = " ...hic!... ";
		}

		return x;
	}

	private static string Hiss(Match m)
	{
		string x = m.ToString();
		if (char.IsLower(x[0]))
		{
			x = x + "ss";
		}
		else
		{
			x = x + "SS";
		}

		return x;
	}

	private static string Stutter(Match m)
	{
		string x = m.ToString();
		string stutter = "";
		//20% chance to stutter at any given consonant
		if (Random.Range(1, 6) == 1)
		{
			//Randomly pick how bad is the stutter
			int intensity = Random.Range(1, 4);
			for (int i = 0; i < intensity; i++)
			{
				stutter = stutter + x + "... "; //h... h... h...
			}

			stutter = stutter + x; //h... h... h... h[ello]
		}
		else
		{
			stutter = x;
		}
		return stutter;
	}

	#endregion

	/// <summary>
    /// Convenient static factory for creating a ChatChannel.Local message.
    /// </summary>
    public static ChatEvent Local(string message, Vector2 atWorldPosition, float range = 9f)
    {
    	return new ChatEvent
    	{
    		message = message,
    		channels = ChatChannel.Local,
    		position = atWorldPosition,
    		radius = range
    	};
    }
}