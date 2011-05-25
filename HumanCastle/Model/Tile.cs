using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HumanCastle.Model
{
	enum StairType
	{
		None = 0, 
		Down = 1 << 0,
		Up = 1 << 1,
		UpDown = Down | Up
	}


	class TileDeclaration
	{
		public bool passable = false;

		//+1 if up stair; -1 if down stair
		//More deltas might be possible; dunno.
		public StairType stairType = StairType.None;
	}


	//Flyweight to a tile declaration, which exposes more properties.
	struct Tile
	{
		public TileDeclaration decl;
	}
}
