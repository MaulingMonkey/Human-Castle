using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using SlimDX;

namespace HumanCastle.Model
{
	/*
	 * This is the game map in which most of the gameplay is supposed to take place.
	 * Acts more or less like a collection of tiles, and exposes some basic pathfinding primitives.
	 */
	class LocalMap
	{
		private uint width;
		public uint Width { get { return width; } }

		private uint height;
		public uint Height { get { return height; } }

		private uint depth;
		public uint Depth { get { return depth; } }
		

		private Tile[, ,] tiles;
		//LOLO, hack.
		private TileDeclaration passableTile;
		private TileDeclaration impassibleTile;

		public LocalMap(uint w, uint h, uint d)
		{
			width = w; 
			height = h;
			depth = d;

			tiles = new Tile[w, h, d];
			passableTile = new TileDeclaration();
			passableTile.passable = true;

			impassibleTile = new TileDeclaration();
			impassibleTile.passable = false;


		}
	}
}
