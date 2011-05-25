using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using SlimDX;
using System.IO;

namespace HumanCastle.Model
{
	/*
	 * This is the game map in which most of the gameplay is supposed to take place.
	 * Acts more or less like a collection of tiles, and exposes some basic pathfinding primitives.
	 */
	class LocalMap
	{
		public int Width { get; private set; }
		public int Height { get; private set; }
		public int Depth { get; private set; }
		public IVector3 Dimensions { get { return new IVector3(Width, Height, Depth); } }

		private Tile[, ,] tiles;
		//LOLO, hack.
		private TileDeclaration passableTile;
		private TileDeclaration impassibleTile;

		public LocalMap(int w, int h, int d)
		{
			Width = w; 
			Height = h;
			Depth = d;

			tiles = new Tile[w, h, d];
			passableTile = new TileDeclaration();
			passableTile.passable = true;

			impassibleTile = new TileDeclaration();
			impassibleTile.passable = false;
		}

		//This entire constructor is lol.
		public LocalMap(Stream file)
		{
			StreamReader reader = new StreamReader(file);
			string line = reader.ReadLine();
			
			//Read the dimensions.
			string[] dims = line.Split(' ');
			Width = Convert.ToInt32(dims[0]);
			Height = Convert.ToInt32(dims[1]);
			Depth = Convert.ToInt32(dims[2]);

			tiles = new Tile[Width, Height, Depth];
			passableTile = new TileDeclaration();
			passableTile.passable = true;

			impassibleTile = new TileDeclaration();
			impassibleTile.passable = false;

			for (int z = 0; z < Depth; ++z)
			{
				for (int y = 0; y < Height; ++y)
				{
					string[] tileLine = reader.ReadLine().Split(' ');

					for (int x = 0; x < Width; ++x)
					{
						if (tileLine[x] == "1")
						{
							tiles[x, y, z].decl = passableTile;
						}
						else
						{
							tiles[x, y, z].decl = impassibleTile;
						}
					}
				}
			}
		}

		//Distance estimator <_<
		private int Cost(IVector3 node, IVector3 end)
		{
			return IVector3.Dot(node, end);
		}

		public List<IVector3> Path(IVector3 start, IVector3 end)
		{
			Debug.Assert(Dimensions.BoundedBy(start, end));

			List<IVector3> result = new List<IVector3>();
			
			List<IVector3> closed = new List<IVector3>();
			List<IVector3> open = new List<IVector3>();
			open.Add(start);

			var goalScore = new Dictionary<IVector3, int>();
			goalScore.Add(start, 0);

			var guessScore = new Dictionary<IVector3, int>();
			guessScore.Add(start, Cost(start, end));

			var finalScore = new Dictionary<IVector3, int>();
			finalScore.Add(start, guessScore[start]);

			while(open.Count != 0)
			{
				var lowestScore = goalScore.Min(x => x.Value);
				var node = goalScore.Where(x => x.Value == lowestScore).First().Key;

				if (node == end)
				{
					//Uhhh.
					return result;
				}

				open.Remove(node);
				closed.Add(node);

				
			}
		}
	}
}
