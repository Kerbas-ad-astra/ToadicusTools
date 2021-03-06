// ToadicusTools
//
// Tools.cs
//
// Copyright © 2014-2015, toadicus
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation and/or other
//    materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ToadicusTools
{
	public static partial class Tools
	{
		#if !HAS_SIFORMMATER
		public static readonly IFormatProvider SIFormatter = System.Globalization.CultureInfo.CurrentCulture;
		#endif

		#region LOGGING_TOOLS
		public static void PostLogMessage(LogChannel channel, string Msg)
		{
			switch (channel)
			{
				case LogChannel.Log:
					#if DEBUG
					Debug.Log(Msg);
					#else
					KSPLog.print(Msg);
					#endif
					break;
				case LogChannel.Warning:
					Debug.LogWarning(Msg);
					break;
				case LogChannel.Error:
					Debug.LogError(Msg);
					break;
				default:
					throw new NotImplementedException("Invalid channel, must pick one of Log, Warning, or Error.");
			}
		}

		public static void PostLogMessage(LogChannel channel, string Format, params object[] args)
		{
			string message = string.Format(Tools.SIFormatter, Format, args);

			PostLogMessage(message);
		}

		public static void PostLogMessage(string Msg)
		{
			PostLogMessage(LogChannel.Log, Msg);
		}

		public static void PostLogMessage(string Format, params object[] args)
		{
			PostLogMessage(LogChannel.Log, Format, args);
		}

		public static void PostWarningMessage(string Msg)
		{
			PostLogMessage(LogChannel.Warning, Msg);
		}

		public static void PostWarningMessage(string Format, params object[] args)
		{
			PostLogMessage(LogChannel.Warning, Format, args);
		}

		public static void PostErrorMessage(string Msg)
		{
			PostLogMessage(LogChannel.Error, Msg);
		}

		public static void PostErrorMessage(string Format, params object[] args)
		{
			PostLogMessage(LogChannel.Error, Format, args);
		}

		public static void Log(this Component component, LogChannel channel, string Msg)
		{
			Type componentType = component.GetType();
			string name;

			if (componentType == typeof(Vessel))
			{
				name = string.Format(Tools.SIFormatter, "{0} ({1})", componentType.Name, (component as Vessel).vesselName);
			}
			else if (componentType == typeof(Part))
			{
				name = string.Format(Tools.SIFormatter, "{0} ({1})", componentType.Name, (component as Part).partInfo.name);
			}
			else
			{
				name = componentType.Name;
			}

			string message = string.Format(Tools.SIFormatter, "[{0}] {1}", name, Msg);

			PostLogMessage(channel, message);
		}

		public static void Log(this Component component, string Msg)
		{
			component.Log(LogChannel.Log, Msg);
		}

		public static void Log(this Component component, string format, params object[] args)
		{
			string message = string.Format(Tools.SIFormatter, format, args);

			component.Log(message);
		}

		public static void LogWarning(this Component component, string Msg)
		{
			component.Log(LogChannel.Warning, Msg);
		}

		public static void LogWarning(this Component component, string format, params object[] args)
		{
			string message = string.Format(Tools.SIFormatter, format, args);

			component.LogWarning(message);
		}

		public static void LogError(this Component component, string Msg)
		{
			component.Log(LogChannel.Error, Msg);
		}

		public static void LogError(this Component component, string format, params object[] args)
		{
			string message = string.Format(Tools.SIFormatter, format, args);

			component.LogError(message);
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public static void LogDebug(this Component component, string Msg)
		{
			component.Log(LogChannel.Log, Msg);
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public static void LogDebug(this Component component, string format, params object[] args)
		{
			string message = string.Format(Tools.SIFormatter, format, args);

			component.Log(message);
		}

		public enum LogChannel
		{
			Log,
			Warning,
			Error
		}
		#endregion

		#region DEBUG_TOOLS
		private static ScreenMessage debugmsg = new ScreenMessage("", 4f, ScreenMessageStyle.UPPER_RIGHT);

		public static void PostMessageWithScreenMsg(string Msg)
		{
			if (HighLogic.LoadedScene > GameScenes.SPACECENTER)
			{

				debugmsg.message = Msg;
				ScreenMessages.PostScreenMessage(debugmsg, true);
			}

			PostLogMessage(Msg, LogChannel.Log);
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public static void PostDebugMessage(string Msg)
		{
			PostMessageWithScreenMsg(Msg);
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public static void PostDebugMessage(object Sender, params object[] args)
		{
			StringBuilder sb = GetStringBuilder();
			sb.AppendFormat("{0}:", Sender.GetType().Name);

			object arg;
			for (int idx = 0; idx < args.Length; idx++)
			{
				arg = args[idx];

				sb.AppendFormat("\n\t{0}", arg.ToString());
			}

			PostMessageWithScreenMsg(sb.ToString());

			PutStringBuilder(sb);
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public static void PostDebugMessage(object Sender, string Format, params object[] args)
		{
			StringBuilder sb = GetStringBuilder();

			if (Sender != null)
			{
				Type type = (Sender is Type) ? Sender as Type : Sender.GetType();
				sb.Append(type.Name);
				sb.Append(": ");
			}

			sb.AppendFormat(Format, args);

			PostMessageWithScreenMsg(sb.ToString());

			PutStringBuilder(sb);
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public static void DebugFieldsActivate(this PartModule partModule)
		{
			BaseField field;
			for (int idx = 0; idx < partModule.Fields.Count; idx++)
			{
				field = partModule.Fields[idx];
				field.guiActive = field.guiActiveEditor = true;
			}
		}

		public class DebugLogger : IDisposable
		{
			public static DebugLogger New(object caller)
			{
				return new DebugLogger(caller.GetType());
			}

			public static DebugLogger New(Type callingType)
			{
				return new DebugLogger(callingType);
			}

			private StringBuilder stringBuilder;

			private DebugLogger() {}

			private DebugLogger(Type caller)
			{
				this.stringBuilder = GetStringBuilder();

				this.stringBuilder.Append(caller.Name);
				this.stringBuilder.Append(": ");
			}

			[System.Diagnostics.Conditional("DEBUG")]
			public void Append(object value)
			{
				this.stringBuilder.Append(value);
			}

			[System.Diagnostics.Conditional("DEBUG")]
			public void AppendFormat(string format, params object[] args)
			{
				this.stringBuilder.AppendFormat(format, args);
			}

			[System.Diagnostics.Conditional("DEBUG")]
			public void AppendLine(string value)
			{
				this.stringBuilder.Append(value);
				this.stringBuilder.Append('\n');
			}

			[System.Diagnostics.Conditional("DEBUG")]
			public void Print(bool postToScreen)
			{
				if (postToScreen)
				{
					PostMessageWithScreenMsg(this.stringBuilder.ToString());
				}
				else
				{
					Debug.Log(this.stringBuilder.ToString());
				}

				this.stringBuilder.Length = 0;
			}

			[System.Diagnostics.Conditional("DEBUG")]
			public void Print()
			{
				PostMessageWithScreenMsg(this.stringBuilder.ToString());
			}
			[System.Diagnostics.Conditional("DEBUG")]
			public void Clear()
			{
				this.stringBuilder.Length = 0;
			}

			public void Dispose()
			{
				PutStringBuilder(this.stringBuilder);
			}

			~DebugLogger()
			{
				this.Dispose();
			}
		}
		#endregion

		private static Stack<StringBuilder> sbStack = new Stack<StringBuilder>();

		public static StringBuilder GetStringBuilder()
		{
			lock (sbStack)
			{
				if (sbStack.Count > 0)
				{
					StringBuilder sb = sbStack.Pop();

					if (sb == null)
					{
						sb = new StringBuilder();
					}
					else
					{
						sb.Length = 0;
					}
				
					return sb;
				}
				else
				{
					return new StringBuilder();
				}
			}
		}

		public static void PutStringBuilder(StringBuilder sb)
		{
			lock (sbStack)
			{
				sbStack.Push(sb);
			}
		}

		#region Array_Tools
		public static bool Contains(this GameScenes[] haystack, GameScenes needle)
		{
			GameScenes item;
			for (int idx = 0; idx < haystack.Length; idx++)
			{
				item = haystack[idx];
				if (item == needle)
				{
					return true;
				}
			}

			return false;
		}

		public static bool Contains(this CelestialBody[] haystack, CelestialBody needle)
		{
			CelestialBody item;
			for (int idx = 0; idx < haystack.Length; idx++)
			{
				item = haystack[idx];
				if (item == needle)
				{
					return true;
				}
			}
			return false;
		}

		public static bool Contains<T>(this T[] haystack, T needle)
		{
			T item;
			for (int idx = 0; idx < haystack.Length; idx++)
			{
				item = haystack[idx];
				if (object.Equals(item, needle))
				{
					return true;
				}
			}
			return false;
		}
		#endregion

		#region Enum_Tools
		public static bool TryParse<enumType>(string value, out enumType result)
			where enumType : struct, IConvertible, IComparable, IFormattable
		{
			try
			{
				if (!typeof(enumType).IsEnum)
				{
					throw new ArgumentException("result must be of an enum type");
				}

				result = (enumType)Enum.Parse(typeof(enumType), value);
				return true;
			}
			catch (Exception e)
			{
				Debug.LogWarning(string.Format(Tools.SIFormatter, "[{0}] failed to parse value '{1}': {2}",
					typeof(enumType).Name,
					value,
					e.Message
				));

				result = (enumType)Enum.GetValues(typeof(enumType)).GetValue(0);
				return false;
			}
		}
		#endregion

		#region IComparable Extensions
		public static T Min<T>(params T[] values) where T : IComparable<T>
		{
			if (values.Length < 2)
			{
				throw new ArgumentException("Min must be called with at least two arguments.");
			}

			IComparable<T> minValue = values[0];

			for (long i = 1; i < values.LongLength; i++)
			{
				IComparable<T> value = values[i];

				if (value.CompareTo((T)minValue) < 0)
				{
					minValue = value;
				}
			}

			return (T)minValue;
		}
		#endregion

		#region Stopwatch Extensions
		public static void Restart(this System.Diagnostics.Stopwatch stopwatch)
		{
			stopwatch.Reset();
			stopwatch.Start();
		}
		#endregion

		#region UI_Control Extensions
		public static UI_Control uiControlCurrent(this BaseField field)
		{
			if (HighLogic.LoadedSceneIsFlight)
			{
				return field.uiControlFlight;
			}
			else if (HighLogic.LoadedSceneIsEditor)
			{
				return field.uiControlEditor;
			}
			else
			{
				return null;
			}
		}
		#endregion

		public static Part GetSceneRootPart()
		{
			Part rootPart;
			switch (HighLogic.LoadedScene)
			{
				case GameScenes.EDITOR:
					rootPart = EditorLogic.RootPart;
					break;
				case GameScenes.FLIGHT:
					rootPart = FlightGlobals.ActiveVessel != null ? FlightGlobals.ActiveVessel.rootPart : null;
					break;
				default:
					rootPart = null;
					break;
			}

			return rootPart;
		}

		public static bool SetIfDefault<T>(this T o, T val)
		{
			if (System.Object.Equals(o, default(T)))
			{
				o = val;
				return true;
			}

			return false;
		}
	}
}