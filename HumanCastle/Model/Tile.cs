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
		public StairType stairType = StairType.None;
	}


	//Flyweight to a tile declaration, which exposes more properties.
	struct Tile
	{
		public TileDeclaration decl;
	}
}
