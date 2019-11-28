﻿using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XanBotCore.Exceptions;
using XanBotCore.Permissions;
using XanBotCore.ServerRepresentation;
using XanBotCore.UserObjects;
using XanBotCore.Utility;

namespace XanBotCore.CommandSystem.Commands {
	class CommandHelp : Command {
		public override string Name { get; } = "help";

		public override string Description { get; } = "Lists every command or returns information on a command.\n\nSome commands may show something called \"Arguments\" as part of their documentation. This is the text like `<someArg>` or `[someArg]`.\n"
						+ "Any text shown in greater than/less than (these: `<>`) is a **required argument.** This means that the command will not work unless you include text in place of it. For instance, say there's a command: `>> say <message>` --  `message` is a required argument, so you need to have something after `>> say`, like `>> say Hello!`, in order for it to work. Do not include the `< >` in your text. (So `>> say <Hello!>` is wrong.)\n\n"
						+ "Any text shown in square brackets (these: `[]`) is an **optional argument.** This means that the command will still work if you don't include text there. A great example is this command -- `>> help` can **optionally** take in the name of a specific command to get more information on that command. If you don't do that, it shows the list of every command. Do not include the `[ ]` in your text. (Same as above)\n\n"
						+ "Command arguments are split by spaces. The way this is handled is identical to that of the Windows Command Line. For example: The command `>> cmd abc Cool Text! 123` will be interpreted with four arguments: `abc`, `Cool`, `Text!`, and `123`. You can put quotes around arguments to join them, like `>> cmd abc \"Cool Text!\" 123` which will evaluate into *three* arguments: `abc`, `Cool Text!`, and `123`";

		public override string Syntax {
			get {
				return Name + " [commandName]";
			}
		}

		public override byte RequiredPermissionLevel { get; } = PermissionRegistry.PERMISSION_LEVEL_STANDARD_USER;

		public override void ExecuteCommand(BotContext context, XanBotMember executingMember, DiscordMessage originalMessage, string[] args, string allArgs) {
			if (args.Length == 0) {
				string text = "Commands in yellow with a `+` before them are commands you can use. Commands in red with a `-` before them are commands you cannot use. ";
				text += "\nSay **`>> help command_name_here`** to get more documentation on a specific command. Say **`>> help help`** to get information on how commands work.";
				text += "```diff\n";
				foreach (Command cmd in CommandMarshaller.Commands) {
					int spaces = 34;
					string usagePrefix = "+ ";
					if (executingMember != null) {
						usagePrefix = cmd.CanUseCommand(executingMember) ? "+ " : "- ";
					}
					text += usagePrefix + cmd.Name;
					spaces -= (cmd.Name.Length + 2);
					for (int i = 0; i < spaces; i++) {
						text += " ";
					}
					text += $"Requires Permission Level {cmd.RequiredPermissionLevel} (or higher).";
					text += "\n";
				}

				if (context.ContextSpecificCommands.Length > 0) {
					text += "\nCommands specific to this server:\n\n";
					foreach (Command cmd in context.ContextSpecificCommands) {
						int spaces = 34;
						string usagePrefix = "+";
						if (executingMember != null) {
							usagePrefix = cmd.CanUseCommand(executingMember) ? "+ " : "- ";
						}
						text += usagePrefix + cmd.Name;
						spaces -= (cmd.Name.Length + 2);
						for (int i = 0; i < spaces; i++) {
							text += " ";
						}
						text += $"Requires Permission Level {cmd.RequiredPermissionLevel} (or higher).";
						text += "\n";
					}
				}

				text += "```\n";
				
				ResponseUtil.RespondTo(originalMessage, text);
			}
			else if (args.Length == 1) {
				string command = args[0];
				foreach (Command cmd in CommandMarshaller.Commands) {
					if (cmd.Name.ToLower() == command.ToLower()) {
						int locatedGraves = 0;
						foreach (char c in cmd.Syntax.ToCharArray()) {
							if (c == '`') locatedGraves++;
						}

						string text;
						if (locatedGraves % 2 != 0) {
							text = string.Format("**Command:** `{0}` \n{1}\n\n**Usage:** `{2}", cmd.Name, cmd.Description, cmd.Syntax);
						} else {
							text = string.Format("**Command:** `{0}` \n{1}\n\n**Usage:** `{2}`", cmd.Name, cmd.Description, cmd.Syntax);
						}
						ResponseUtil.RespondTo(originalMessage, text);
						return;
					}
				}
				throw new CommandException(this, "Command `" + command + "` does not exist.");
			}
			else {
				throw new CommandException(this, "Invalid argument count. Expected no arguments, or one argument which is the name of the command you wish to get details on.");
			}
		}
	}
}