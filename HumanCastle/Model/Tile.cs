using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HumanCastle.Model
{
	class TileDeclaration
	{
		public bool passable;
	}


	//Flyweight to a tile declaration, which exposes more properties.
	struct Tile
	{
		TileDeclaration decl;
	}
}
