using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HumanCastle.Model
{
	struct IVector3
	{
		public static IVector3 Zero = new IVector3(0, 0, 0);
		
		public static int Dot(IVector3 a, IVector3 b)
		{
			return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
		}

		public static double Distance(IVector3 a, IVector3 b)
		{
			double d = Dot(a, b);
			return Math.Sqrt(d);
		}

		public int X, Y, Z;
		public IVector3(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		//Some basic operations.
		public bool BoundedBy(IVector3 min, IVector3 max)
		{
			if (X >= min.X && Y >= min.Y && Z >= min.Z &&
				X < max.X && Y < max.Y && Z < max.Z)
			{
				return true;
			}
			return false;
		}

		public static bool operator ==(IVector3 a, IVector3 b)
		{
			return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
		}

		public static bool operator !=(IVector3 a, IVector3 b)
		{
			return a.X != b.X || a.Y != b.Y || a.Z != b.Z;
		}

		public int GetHashCode()
		{
			return X ^ (Y * 263) + Z * 773;
		}
	}
}
