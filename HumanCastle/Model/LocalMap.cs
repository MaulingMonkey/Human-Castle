using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using SlimDX;
using System.IO;
using HumanCastle.Graphics;

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
		private TileDeclaration basicTile = new TileDeclaration();
		private TileDeclaration upStair = new TileDeclaration();
		private TileDeclaration downStair = new TileDeclaration();
		private TileDeclaration airTile = new TileDeclaration();

		void createTiles(Assets assets)
		{
			basicTile.Material.Texture = assets.MMGrass;

			upStair.StairType = StairType.Up;
			upStair.Material.Texture = assets.MMGrass;

			downStair.StairType = StairType.Down;
			downStair.Material.Texture = assets.MMGrass;
		}

		public LocalMap(int w, int h, int d, Assets assets)
		{
			Width = w; 
			Height = h;
			Depth = d;

			tiles = new Tile[w, h, d];

			createTiles(assets);

			for (int z = 0; z < d; ++z)
			{
				TileDeclaration decl = (z == 0 ? basicTile : airTile);
				for (int y = 0; y < h; ++y)
				{
					for (int x = 0; x < w; ++x)
					{
						tiles[x, y, z].IsPassable = true;
						tiles[x, y, z].Declaration = decl;
					}
				}
			}
		}

		//This entire constructor is lol.
		public LocalMap(Stream file, Assets assets)
		{
			StreamReader reader = new StreamReader(file);
			string line = reader.ReadLine();
			
			//Read the dimensions.
			string[] dims = line.Split(' ');
			Width = Convert.ToInt32(dims[0]);
			Height = Convert.ToInt32(dims[1]);
			Depth = Convert.ToInt32(dims[2]);

			tiles = new Tile[Width, Height, Depth];
			createTiles(assets);

			for (int z = 0; z < Depth; ++z)
			{
				if (z != 0) 
				{ 
					reader.ReadLine(); 
				}

				for (int y = 0; y < Height; ++y)
				{
					string[] tileLine = reader.ReadLine().Split(' ');

					for (int x = 0; x < Width; ++x)
					{
						switch (Convert.ToInt32(tileLine[x]))
						{
							case 0:
								tiles[x, y, z].Declaration = basicTile;
								tiles[x, y, z].IsPassable = false;
								break;
							case 1:
								tiles[x, y, z].Declaration = basicTile;
								tiles[x, y, z].IsPassable = true;
								break;
							case 2:
								tiles[x, y, z].Declaration = upStair;
								tiles[x, y, z].IsPassable = true;
								break;
							case 3:
								tiles[x, y, z].Declaration = downStair;
								tiles[x, y, z].IsPassable = true;
								break;
						}
					}
				}
			}
		}

		public Tile this[IVector3 i]
		{
			get { return this[i.X, i.Y, i.Z]; }
			set { this[i.X, i.Y, i.Z] = value; }
		}

		public Tile this[int x, int y, int z]
		{
			get { return tiles[x, y, z]; }
			set { tiles[x, y, z] = value; }
		}

		private double Cost(IVector3 node, IVector3 end)
		{
			return IVector3.Dot(node, end);
		}

		private List<IVector3> PassableNodes(IVector3 node)
		{
			var result = new List<IVector3>();

			Tile currentTile = this[node];
			TileDeclaration decl = currentTile.Declaration;

			if (decl.StairType != StairType.None)
			{
				if ((decl.StairType & StairType.Up) != 0)
				{
					IVector3 target = node;
					target.Z += 1;
					if (this[target].IsPassable)
					{
						result.Add(target);
					}
				}

				if ((decl.StairType & StairType.Down) != 0)
				{
					IVector3 target = node;
					target.Z -= 1;
					if (this[target].IsPassable)
					{
						result.Add(target);
					}
				}
			}

			for (int x = -1; x <= 1; ++x)
			{
				for (int y = -1; y <= 1; ++y)
				{
					if (y == 0 && x == 0)
					{
						continue;
					}

					IVector3 target = node;
					target.X += x;
					target.Y += y;

					
					if (target.BoundedBy(IVector3.Zero, Dimensions))
					{
						Tile tile = this[target];
						if (tile.IsPassable)
						{
							result.Add(target);
						}
					}
				}
			}
			return result;
		}

		private void Reconstruct(Dictionary<IVector3, IVector3> camefrom, IVector3 current, List<IVector3> result)
		{
			if (camefrom.ContainsKey(current))
			{
				Reconstruct(camefrom, camefrom[current], result);
			}
			result.Add(current);
		}

		// Basic A* implementation.
		public List<IVector3> Path(IVector3 start, IVector3 end)
		{
			Debug.Assert(start.BoundedBy(IVector3.Zero, Dimensions));
			Debug.Assert(end.BoundedBy(IVector3.Zero, Dimensions));

			List<IVector3> result = new List<IVector3>();

			HashSet<IVector3> closed = new HashSet<IVector3>();
			HashSet<IVector3> open = new HashSet<IVector3>();
			open.Add(start);

			var cameFrom = new Dictionary<IVector3, IVector3>();

			var goalScore = new Dictionary<IVector3, double>();
			goalScore.Add(start, 0);

			var guessScore = new Dictionary<IVector3, double>();
			guessScore.Add(start, Cost(start, end));

			var finalScore = new Dictionary<IVector3, double>();
			finalScore.Add(start, guessScore[start]);

			while(open.Count != 0)
			{
				double minscore = finalScore[open.First()];
				IVector3 node = open.First();
				foreach (var n in open)
				{
					if (finalScore[n] < minscore)
					{
						minscore = finalScore[n];
						node = n;
					}
				}

				if (node == end)
				{
					Reconstruct(cameFrom, cameFrom[end], result);
					result.Add(end);
					return result;
				}

				open.Remove(node);
				closed.Add(node);

				foreach (var t in PassableNodes(node))
				{

					if (closed.Contains(t))
					{
						continue;
					}

					bool isBetter = false;
					double score = goalScore[node] + IVector3.Distance(node, t);
					if (!open.Contains(t))
					{
						open.Add(t);
						isBetter = true;
					}
					else if (score < goalScore[t])
					{
						isBetter = true;
					}
					else
					{
						isBetter = false;
					}

					if (isBetter)
					{
						cameFrom[t] = node;
						goalScore[t] = score;
						guessScore[t] = Cost(t, end);
						finalScore[t] = score + Cost(t, end);
					}
				}
			}

			//No path! 
			return null;
		}
	}
}
