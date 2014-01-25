using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Drawing
{
	[ComVisible (true)]
	[Serializable]
	public struct PointD
	{
		//
		// Static Fields
		//
		public static readonly PointD Empty;

		//
		// Properties
		//
		[Browsable (false)]
		public bool IsEmpty
		{
			get
			{
				return (double)this.x == 0.0 && (double)this.y == 0.0;
			}
		}

		private double x;
		public double X
		{
			get
			{
				return this.x;
			}
			set
			{
				this.x = value;
			}
		}

		private double y;
		public double Y
		{
			get
			{
				return this.y;
			}
			set
			{
				this.y = value;
			}
		}

		//
		// Constructors
		//
		public PointD (double x, double y)
		{
			this.x = x;
			this.y = y;
		}

		//
		// Static Methods
		//
		public static PointD Add (PointD pt, SizeF sz)
		{
			return new PointD (pt.X + (double)sz.Width, pt.Y + (double)sz.Height);
		}

		public static PointD Add (PointD pt, Size sz)
		{
			return new PointD (pt.X + (double)sz.Width, pt.Y + (double)sz.Height);
		}

		public static PointD Subtract (PointD pt, Size sz)
		{
			return new PointD (pt.X - (double)sz.Width, pt.Y - (double)sz.Height);
		}

		public static PointD Subtract (PointD pt, SizeF sz)
		{
			return new PointD (pt.X - (double)sz.Width, pt.Y - (double)sz.Height);
		}

		//
		// Methods
		//
		public override bool Equals (object obj)
		{
			return obj is PointD && this == (PointD)obj;
		}

		public override int GetHashCode ()
		{
			return (int)this.x ^ (int)this.y;
		}

		public override string ToString ()
		{
			return string.Format ("{{X={0}, Y={1}}}", this.x.ToString (CultureInfo.CurrentCulture), this.y.ToString (CultureInfo.CurrentCulture));
		}

		//
		// Operators
		//
		public static PointD operator + (PointD pt, SizeF sz)
		{
			return new PointD (pt.X + (double)sz.Width, pt.Y + (double)sz.Height);
		}

		public static PointD operator + (PointD pt, Size sz)
		{
			return new PointD (pt.X + (double)sz.Width, pt.Y + (double)sz.Height);
		}

		public static bool operator == (PointD left, PointD right)
		{
			return left.X == right.X && left.Y == right.Y;
		}

		public static bool operator != (PointD left, PointD right)
		{
			return left.X != right.X || left.Y != right.Y;
		}

		public static PointD operator - (PointD pt, Size sz)
		{
			return new PointD (pt.X - (double)sz.Width, pt.Y - (double)sz.Height);
		}

		public static PointD operator - (PointD pt, SizeF sz)
		{
			return new PointD (pt.X - (double)sz.Width, pt.Y - (double)sz.Height);
		}
	}
}
