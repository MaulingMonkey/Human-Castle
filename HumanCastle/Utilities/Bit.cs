using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HumanCastle.Utilities
{
	class Bit
	{
		public static bool IsSet(uint value, uint mask)
		{
			return (value & mask) == mask;
		}

		public static uint Set(uint original, uint mask, bool value)
		{
			if (value)
			{
				return original | mask;
			}
			else
			{
				return original & ~mask;
			}
		}
	}
}
