﻿using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XanBotCore.Permissions;
using XanBotCore.ServerRepresentation;
using XanBotCore.UserObjects;

namespace XanBotCore.CommandSystem {

	/// <summary>
	/// Represents a runnable command for this bot.
	/// </summary>
	public abstract class Command : IComparable<Command> {
		/// <summary>
		/// The name of this command. This is also used to execute it. Should not include spaces or special characters. Should be all lowercase.
		/// </summary>
		public abstract string Name { get; }

		/// <summary>
		/// The description of this command returned by the stock help command.
		/// </summary>
		public abstract string Description { get; }

		/// <summary>
		/// The usage syntax of this command returned by the stock help command.
		/// </summary>
		public abstract string Syntax { get; }

		/// <summary>
		/// The permission level required to use this command.
		/// </summary>
		public abstract byte RequiredPermissionLevel { get; }

		/// <summary>
		/// Returns true if the specified member can use the command.
		/// </summary>
		/// <param name="member">The member using this command.</param>
		/// <returns>true if the specified member can use the command, false if they can not.</returns>
		public virtual bool CanUseCommand(XanBotMember member) {
			// Command would be ICommand if this wasn't going to be common across literally every command.
			// There *could* be edge cases in which this method is different, so that's why I have it here anyway.
			return member.PermissionLevel >= RequiredPermissionLevel;
		}

		/// <summary>
		/// Executes the command. This does not check if the user can run it. Check if the user is authorized before using this method.
		/// </summary>
		/// <param name="context">The <see cref="BotContext"/> that this command is running in.</param>
		/// <param name="executingMember">The member executing this command.</param>
		/// <param name="originalMessage">The DiscordMessage that was responsible for invoking this command.</param>
		/// <param name="args">The arguments of the command split via shell32.DLL's handling system.</param>
		/// <param name="allArgs">Every argument passed into this command as its raw string. This is used to preserve quotes and other characters stripped by shell32.</param>
		public abstract void ExecuteCommand(BotContext context, XanBotMember executingMember, DiscordMessage originalMessage, string[] args, string allArgs);

		public int CompareTo(Command other) {
			if (other == null) return 1;
			int permComparison = RequiredPermissionLevel.CompareTo(other.RequiredPermissionLevel);
			if (permComparison != 0) return permComparison;
			return Name.CompareTo(other.Name);
		}
	}
}