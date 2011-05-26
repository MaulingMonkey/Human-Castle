using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HumanCastle.Utilities;
namespace HumanCastle.Model
{
	[Flags]
	enum StairType : uint
	{
		None = 0, 
		Down = 1 << 0,
		Up = 1 << 1,
		Ramp = 1 << 2,
		UpDown = Down | Up
	}


	class TileDeclaration
	{
		public StairType StairType { get; set; }
		public Material Material { get; set; }


		public TileDeclaration()
		{
			StairType = StairType.None;
			Material = new Material();
		}
	}


	//Flyweight to a tile declaration, which exposes more properties.
	struct Tile
	{
		[Flags]
		private enum TileFlags : uint
		{
			Passable = 1 << 0,

		}

		public TileDeclaration Declaration { get; set; }

		public bool IsPassable
		{
			get { return Bit.IsSet(flags, (uint)TileFlags.Passable); }
			set { flags = Bit.Set(flags, (uint)TileFlags.Passable, value); }
		}

		private uint flags;
	}
}
