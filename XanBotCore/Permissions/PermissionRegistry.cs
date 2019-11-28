﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XanBotCore.DataPersistence;
using XanBotCore.ServerRepresentation;
using XanBotCore.Exceptions;
using XanBotCore.UserObjects;
using XanBotCore.Logging;

namespace XanBotCore.Permissions {

	/// <summary>
	/// Represents permission levels.
	/// </summary>
	public class PermissionRegistry {

		private static readonly Dictionary<BotContext, Dictionary<ulong, byte>> PermissionsInContext = new Dictionary<BotContext, Dictionary<ulong, byte>>();


		private static bool AllowXanInternal = true;

		/// <summary>
		/// If this is true (which it is by default), the bot's creator will have the maximum permission level, 255.<para/>
		/// Since this is your bot and you should be able to dictate security, this is under a boolean so that you may set it to false if needed.<para/>
		/// Throws an <see cref="InvalidOperationException"/> if this is set after the bot has been initialized, as the member object may have already been created with the permission level.
		/// </summary>
		public static bool AllowXanMaxPermissionLevel {
			get {
				return AllowXanInternal;
			}
			set {
				if (XanBotCoreSystem.Created) throw new InvalidOperationException("Cannot set this value after bot initialization (it does not ensure that the permission value hasn't been set already). Set this value before calling XanBotCoreSystem.InitializeBotAsync()");
				AllowXanInternal = false;
			}
		}

		/// <summary>
		/// The permission level that represents a user who is not a member of the current guild.<para/>
		/// Usage of this permission constant is optional. It is simply here as a provided standard, not as a mandated value.
		/// </summary>
		public const byte PERMISSION_LEVEL_NONMEMBER = 0;

		/// <summary>
		/// The permission level that represents someone who is blacklisted from using all commands.<para/>
		/// Usage of this permission constant is optional. It is simply here as a provided standard, not as a mandated value.
		/// </summary>
		public const byte PERMISSION_LEVEL_BLACKLISTED = 1;

		/// <summary>
		/// The default permission level of users.<para/>
		/// Usage of this permission constant is optional. It is simply here as a provided standard, not as a mandated value.
		/// </summary>
		public const byte PERMISSION_LEVEL_STANDARD_USER = 2;

		/// <summary>
		/// The permission level of standard users who are trusted with slightly more power than average users.<para/>
		/// Usage of this permission constant is optional. It is simply here as a provided standard, not as a mandated value.
		/// </summary>
		public const byte PERMISSION_LEVEL_TRUSTED_USER = 3;

		/// <summary>
		/// The permission level of operators, which represents users are allowed to control basic bot functions. In general setups, moderators will receive this level.<para/>
		/// Usage of this permission constant is optional. It is simply here as a provided standard, not as a mandated value.
		/// </summary>
		public const byte PERMISSION_LEVEL_OPERATOR = 63;

		/// <summary>
		/// The permission level of administrators, which represents users who are allowed to control advanced bot functions (like shutting down the bot). In general setups, administrators will receive this level.<para/>
		/// Usage of this permission constant is optional. It is simply here as a provided standard, not as a mandated value.
		/// </summary>
		public const byte PERMISSION_LEVEL_ADMINISTRATOR = 127;

		/// <summary>
		/// The permission level of the server owner. It is intended that all commands usable by <see cref="PERMISSION_LEVEL_BACKEND_CONSOLE"/> are also usable by this level.<para/>
		/// Usage of this permission constant is optional. It is simply here as a provided standard, not as a mandated value.
		/// </summary>
		public const byte PERMISSION_LEVEL_SERVER_OWNER = 254;

		/// <summary>
		/// The permission level used by the backend console.<para/>
		/// Usage of this permission constant is optional. It is simply here as a provided standard, not as a mandated value.
		/// </summary>
		public const byte PERMISSION_LEVEL_BACKEND_CONSOLE = 255;

		/// <summary>
		/// The default permission level of all member objects.<para/>
		/// Any members that have their permission level set to this value will not be stored in data persistence by this code.
		/// </summary>
		public static byte DefaultPermissionLevel { get; set; } = PERMISSION_LEVEL_STANDARD_USER;

		/// <summary>
		/// References the data store for user permissions and gets the user's associated permission level. Returns <see cref="DefaultPermissionLevel"/> if the value does not exist.
		/// </summary>
		/// <param name="userId">The ID of the user to get permissions of.</param>
		/// <param name="context">The bot context to grab the information from.</param>
		/// <returns></returns>
		public static byte GetPermissionLevelOfUser(ulong userId, BotContext context) {
			XConfiguration cfg = XConfiguration.GetConfigurationUtility(context, "userPerms.permissions");
			string permLvl = cfg.GetConfigurationValue(userId.ToString(), DefaultPermissionLevel.ToString(), reloadConfigFile: true);
			if (byte.TryParse(permLvl, out byte perms)) {
				return perms;
			}
			throw new MalformedConfigDataException("The data stored for the permission level of user [" + userId + "] is malformed. Reason: Could not cast " + permLvl + " into a byte.");
		}

		/// <summary>
		/// Stores the set permission level of this user for the sake of data persistence. This is internal since it is called whenever the property <see cref="XanBotMember.PermissionLevel"/> is set, and should not be called by users.
		/// </summary>
		/// <param name="member">The member to set in the internal cache.</param>
		/// <param name="saveToFileNow">If true, the config file for this context's user permissions will be set.</param>
		internal static void UpdatePermissionLevelOfMember(XanBotMember member, bool saveToFileNow = false) {
			if (!PermissionsInContext.ContainsKey(member.Context))
				PermissionsInContext[member.Context] = new Dictionary<ulong, byte>();

			PermissionsInContext[member.Context][member.Id] = member.PermissionLevel;
			if (saveToFileNow) {
				SaveContextPermissionsToFile(member.Context);
			}
		}

		internal static void SaveAllUserPermissionsToFile() {
			foreach (BotContext context in BotContextRegistry.AllContexts) {
				SaveContextPermissionsToFile(context);
			}
		}

		public static void SaveContextPermissionsToFile(BotContext context) {
			if (!PermissionsInContext.ContainsKey(context)) return;
			XConfiguration cfg = XConfiguration.GetConfigurationUtility(context, "userPerms.permissions");
			foreach (ulong id in PermissionsInContext[context].Keys) {
				cfg.SetConfigurationValue(id.ToString(), PermissionsInContext[context][id].ToString(), true);
			}
			cfg.SaveConfigurationFile();
		}
	}
}