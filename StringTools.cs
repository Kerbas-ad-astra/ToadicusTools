﻿// ToadicusTools
//
// StringTools.cs
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

using KSP;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ToadicusTools
{
	public static partial class Tools
	{
		public static readonly SIFormatProvider SIFormatter = new SIFormatProvider();

		/// <summary>
		/// <para>Replaces the format items in a specified string with the string representation of corresponding objects in a
		/// specified array.</para>
		/// <para>&#160;</para>
		/// <para>Uses the custom SIFormatter format provider, to facilitate SI formats for double and double-like numbers, as
		/// MuMech_ToSI.</para>
		/// </summary>
		/// <param name="format">A composite format string.</param>
		/// <param name="args">An object array that contains zero or more objects to format.</param>
		public static string Format(string format, params object[] args)
		{
			return string.Format(SIFormatter, format, args);
		}

		public static string ToMD5Hash(this string input, int outLength = 32)
		{
			var alg = System.Security.Cryptography.MD5.Create();
			var hash = alg.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));

			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			int byteLength = outLength / 2;

			for (int idx = 0; idx < byteLength; idx++)
			{
				var b = hash[idx];
				sb.Append(b.ToString("x2"));
			}

			return sb.ToString();
		}

		public static StringBuilder Print(this StringBuilder sb)
		{
			Tools.PostLogMessage(sb.ToString());

			return sb;
		}

		public static StringBuilder AddIntendedLine(this StringBuilder sb, string line, int indent = 0)
		{
			if (indent > 0) {
				sb.Append(' ', indent * 4);
			}

			sb.AppendLine(line);

			return sb;
		}

		public static string SPrint<T>(this T[] array, string delimiter, Func<T, string> stringFunc)
		{
			StringBuilder sb = GetStringBuilder();
			T item;
			string s;

			for (int idx = 0; idx < array.Length; idx++)
			{
				if (idx > 0)
				{
					sb.Append(delimiter);
				}

				item = array[idx];

				sb.Append(item == null ? "null" : stringFunc == null ? item.ToString() : stringFunc(item));
			}

			s = sb.ToString();

			PutStringBuilder(sb);

			return s;
		}

		public static string SPrint<T>(this T[] array, Func<T, string> stringFunc, string delimiter = ", ")
		{
			return SPrint(array, delimiter, stringFunc);
		}

		public static string SPrint<T>(this T[] array, string delimiter = ", ")
		{
			return array.SPrint(delimiter, null);
		}

		public static string SPrint<T>(this IList<T> list, string delimiter, Func<T, string> stringFunc)
		{
			StringBuilder sb = GetStringBuilder();
			T item;
			string s;

			for (int idx = 0; idx < list.Count; idx++)
			{
				if (idx > 0)
				{
					sb.Append(delimiter);
				}

				item = list[idx];

				sb.Append(item == null ? "null" : stringFunc == null ? item.ToString() : stringFunc(item));
			}

			s = sb.ToString();

			PutStringBuilder(sb);

			return s;
		}

		public static string SPrint<T>(this List<T> list, string delimiter, Func<T, string> stringFunc)
		{
			return SPrint<T>(list as IList<T>, delimiter, stringFunc);
		}

		public static string SPrint<T>(this List<T> list, Func<T, string> stringFunc, string delimiter = ", ")
		{
			return SPrint(list, delimiter, stringFunc);
		}

		public static string SPrint<T>(this IList<T> list, Func<T, string> stringFunc, string delimiter = ", ")
		{
			return SPrint(list, delimiter, stringFunc);
		}

		public static string SPrint<T>(this List<T> list, string delimiter = ", ")
		{
			return list.SPrint(delimiter, null);
		}

		public static string SPrint<T>(this IList<T> list, string delimiter = ", ")
		{
			return list.SPrint(delimiter, null);
		}
	}

	/// <summary>
	/// <para>Facilitates a new "SI prefixed" string format for doubles and double-like numbers, "S[x[,y[,z]]]" where:</para>
	/// <list type="bullet">
	/// <item><description>x:  Number of digits to display after the decimal point,</description></item>
	/// <item><description>y:  Minimum magnitude to be used for an SI prefix, e.g. -3 for "milli", and</description></item>
	/// <item><description>z:  Maximum magnitude to be used for an SI prefix, e.g. 9 for "giga".</description></item>
	/// </list>
	/// <para>When used thus:</para>
	/// <example>string.Format("{0:x,y,z}", d"),</example>
	/// <para>the formatter will invoke Tools.MuMech_ToSI(d, x, y, z).</para>
	/// </summary>
	public class SIFormatProvider : IFormatProvider, ICustomFormatter
	{
		public static string ToSI(double value, int sigFigs)
		{
			if (value == 0)
			{
				return "0.0";
			}

			if (double.IsNaN(value))
			{
				return "NaN";
			}

			if (double.IsInfinity(value))
			{
				if (double.IsNegativeInfinity(value))
				{
					return "-∞";
				}
				else
				{
					return "∞";
				}
			}

			string format;

			double absValue = Math.Abs(value);

			int magnitude = (int)Math.Log10(absValue);
			int significance;
			int divisorExp;
			int decimalPlaces;

			string prefix = string.Empty;

			if (magnitude < 0 || absValue < 1)
			{
				decimalPlaces = 1;

				significance = 1;
			}
			else
			{
				decimalPlaces = 0;

				significance = (sigFigs / 3) * 3;
			}

			if (Math.Abs(magnitude) >= significance)
			{
				divisorExp = magnitude - significance;
			}
			else
			{
				divisorExp = 0;
			}

			switch (divisorExp)
			{
				case 0:
					break;
				case 1:
				case 2:
				case 3:
					value /= 1e3;
					magnitude -= 3;
					prefix = "k";
					break;
				case 4:
				case 5:
				case 6:
					value /= 1e6;
					magnitude -= 6;
					prefix = "M";
					break;
				case 7:
				case 8:
				case 9:
					value /= 1e9;
					magnitude -= 9;
					prefix = "G";
					break;
				case 10:
				case 11:
				case 12:
					value /= 1e12;
					magnitude -= 12;
					prefix = "T";
					break;
				case 13:
				case 14:
				case 15:
					value /= 1e15;
					magnitude -= 15;
					prefix = "P";
					break;
				case 16:
				case 17:
				case 18:
					value /= 1e18;
					magnitude -= 18;
					prefix = "E";
					break;
				case 19:
				case 20:
				case 21:
					value /= 1e21;
					magnitude -= 21;
					prefix = "Z";
					break;
				case 22:
				case 23:
				case 24:
					value /= 1e24;
					magnitude -= 24;
					prefix = "Y";
					break;
				case -1:
				case -2:
				case -3:
					value *= 1e3;
					magnitude += 3;
					prefix = "m";
					break;
				case -4:
				case -5:
				case -6:
					value *= 1e6;
					magnitude += 6;
					prefix = "µ";
					break;
				case -7:
				case -8:
				case -9:
					value *= 1e9;
					magnitude += 9;
					prefix = "n";
					break;
				case -10:
				case -11:
				case -12:
					value *= 1e12;
					magnitude += 12;
					prefix = "p";
					break;
				case -13:
				case -14:
				case -15:
					value *= 1e15;
					magnitude += 15;
					prefix = "f";
					break;
				case -16:
				case -17:
				case -18:
					value *= 1e18;
					magnitude += 18;
					prefix = "a";
					break;
				case -19:
				case -20:
				case -21:
					value *= 1e21;
					magnitude += 21;
					prefix = "z";
					break;
				case -22:
				case -23:
				case -24:
					value *= 1e24;
					magnitude += 24;
					prefix = "y";
					break;
				default:
					if (divisorExp > 0)
					{
						value /= 1e24;
						magnitude -= 24;
						prefix = "Y";
					}
					else
					{
						value *= 1e24;
						magnitude += 24;
						prefix = "y";
					}

					format = string.Format("{{0:g{0}}}{1}", sigFigs < 5 ? 2 : sigFigs - 3, prefix);

					return string.Format(format, value, prefix);
			}

			decimalPlaces += sigFigs - magnitude - 1;

			if (decimalPlaces < 0)
			{
				double divisor = Tools.Pow(10d, -decimalPlaces);
				value = ((int)value / divisor) * divisor;
				decimalPlaces = 0;
			}

			format = string.Format("{{0:f{0}}}{1}", decimalPlaces, prefix);

			return string.Format(format, value, prefix);
		}

		public object GetFormat(Type type)
		{
			if (type == typeof(ICustomFormatter))
			{
				return this;
			}
			else
			{
				return null;
			}
		}

		public string Format(string format, object arg, IFormatProvider formatProvider)
		{
			if (format == null)
			{
				return arg == null ? "null" : arg.ToString();
			}

			if (arg == null)
			{
				return "null";
			}

			if (formatProvider == null)
			{
				throw new ArgumentNullException("formatProvider");
			}

			if (!this.Equals(formatProvider))
			{
				return null;
			}

			if (format.Length == 0)
			{
				throw new FormatException("Format string is empty.");
			}

			if (arg is IFormattable && arg is IConvertible)
			{
				switch (format[0])
				{
					case 'S':
					case 's':
						string[] args = format.Substring(1).Split(new char[] { ',' }, 3);

						double d = Convert.ToDouble(arg);

						int digits = 3;
						int MinMagnitude = 0;
						int MaxMagnitude = int.MaxValue;

						if (args.Length > 0)
						{
							digits = int.Parse(args[0]);
						}

						if (args.Length == 1)
						{
							return ToSI(d, digits);
						}

						if (args.Length > 1)
						{
							MinMagnitude = int.Parse(args[1]);
						}
						if (args.Length > 2)
						{
							MaxMagnitude = int.Parse(args[2]);
						}

						return Tools.MuMech_ToSI(d, digits, MinMagnitude, MaxMagnitude);
					default:
						return ((IFormattable)arg).ToString(format, System.Globalization.CultureInfo.CurrentCulture);
				}
			}
			else if (arg != null)
			{
				return arg.ToString();
			}
			else
			{
				return "NULL";
			}
		}
	}

}