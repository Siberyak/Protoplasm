using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.Serialization;

namespace ColorsMixer
{
	[DataContract]
	[DebuggerDisplay("({A}, {R}, {G}, {B})")]
	public struct Color : IEquatable<Color>
	{
		public static readonly Color Transparent = FromArgb(0, 255, 255, 255);

		[DataMember]
		public byte A { get; private set; }
		[DataMember]
		public byte R { get; private set; }
		[DataMember]
		public byte G { get; private set; }
		[DataMember]
		public byte B { get; private set; }

		public static Color FromArgb(byte a, byte r, byte g, byte b)
		{
			return new Color { A = a, R = r, G = g, B = b };
		}

		public static Color FromArgb(byte a, Color color)
		{
			return new Color { A = a, R = color.R, G = color.G, B = color.B };
		}

		public static implicit operator System.Drawing.Color(Color color)
		{
			return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
		}

		public static implicit operator Color(System.Drawing.Color color)
		{
			return FromArgb(color.A, color.R, color.G, color.B);
		}

		bool IEquatable<Color>.Equals(Color other)
		{
			return Equals(this, other);
		}

		public static bool operator ==(Color a, Color b)
		{
			return Equals(a, b);
		}

		public static bool operator !=(Color a, Color b)
		{
			return !(a == b);
		}
	}

	public static class AlfaColorsMixer
	{
		public static Color Mix(this Color upper, Color lower)
		{
			var a = (byte)(upper.A + lower.A - (upper.A * lower.A) / 256 - 1);
			var c1 = lower.ChangeToNoAlfa();
			var c2 = c1.MixNoAlfaWithAlfa(upper);
			var c3 = c2.ChangeAlfa(a);

			return Color.FromArgb(a, c3.R, c3.G, c3.B);
		}

		public static System.Drawing.Color Mix(this System.Drawing.Color upper, System.Drawing.Color lower)
		{
			return Mix((Color)upper, (Color)lower);
		}

		static Color MixNoAlfaWithAlfa(this Color lower, Color upper)
		{
			var k = 1 - (upper.A + 1) / 256.0;
			return Color.FromArgb
				(
					255,
					MixColorComponents(lower, upper, x => x.R, k),
					MixColorComponents(lower, upper, x => x.G, k),
					MixColorComponents(lower, upper, x => x.B, k)
				);
		}

		static byte MixColorComponents(Color lower, Color upper, Func<Color, double> gv, double k)
		{
			var l = gv(lower) + 1;
			var u = gv(upper) + 1;

			return (byte)Math.Round(u + (l - u) * k - 1);
		}

		/// <summary>
		/// возвращает Color.FromArgb(alfa, color)
		/// </summary>
		/// <param name="color">исходный цвет</param>
		/// <param name="alfa">уровень прозрачности</param>
		/// <returns></returns>
		public static Color ApplyAlfa(this Color color, byte alfa)
		{
			return Color.FromArgb(alfa, color);
		}


		/// <summary>
		/// возвращает Color.FromArgb(alfa, color)
		/// </summary>
		/// <param name="color">исходный цвет</param>
		/// <param name="alfa">уровень прозрачности</param>
		/// <returns></returns>
		public static System.Drawing.Color ApplyAlfa(this System.Drawing.Color color, byte alfa)
		{
			return ApplyAlfa((Color)color, alfa);
		}

		/// <summary>
		/// ¬ычисл€ет цвет, у которого альфа соответствует заданной, и такой, что при рисовании им по "асолютно белому" получаетс€ исходный 
		/// </summary>
		/// <param name="color">исходный цвет</param>
		/// <param name="alfa">уровень прозрачности</param>
		/// <returns></returns>
		public static Color ChangeAlfa(this Color color, byte alfa)
		{
			var c = ChangeToNoAlfa(color);
			var ret = c.CalcByAlfa(alfa);
			return ret;
		}

		/// <summary>
		/// ¬ычисл€ет цвет, у которого альфа соответствует заданной, и такой, что при рисовании им по "асолютно белому" получаетс€ исходный 
		/// </summary>
		/// <param name="color">исходный цвет</param>
		/// <param name="alfa">уровень прозрачности</param>
		/// <returns></returns>
		public static System.Drawing.Color ChangeAlfa(this System.Drawing.Color color, byte alfa)
		{
			return ChangeAlfa((Color)color, alfa);
		}

		static Color ChangeToNoAlfa(this Color color)
		{
			if (color.A == 255)
				return color;

			var a = (byte)(color.A + 1);
			var da = (byte)(256 - a);

			var r = CalcNoAlfaComponent(a, da, color.R);
			var g = CalcNoAlfaComponent(a, da, color.G);
			var b = CalcNoAlfaComponent(a, da, color.B);
			return Color.FromArgb(255, r, g, b);
		}

		static byte CalcNoAlfaComponent(byte a, byte da, byte value)
		{
			var k = (value + 1) / 256.0;
			return (byte)Math.Round(da + a * k - 1);
		}

		static Color CalcByAlfa(this Color color, byte alfa)
		{
			if (alfa == color.A)
				return color;

			var a = (byte)(alfa + 1);
			var da = (byte)(256 - a);

			return Color.FromArgb
				(
					alfa,
					CalcByAlfaComponent(a, da, color.R),
					CalcByAlfaComponent(a, da, color.G),
					CalcByAlfaComponent(a, da, color.B)
				);
		}

		private static byte CalcByAlfaComponent(byte alfa, byte da, byte value)
		{
			var k = (value + 1 - da) / (double)alfa;

			return (byte)Math.Round(256 * k - 1);
		}
	}
}