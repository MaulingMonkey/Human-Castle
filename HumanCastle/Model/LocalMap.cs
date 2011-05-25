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
		private double Cost(IVector3 node, IVector3 end)
		{
			return IVector3.Dot(node, end);
		}

		private TileDeclaration TileAt(IVector3 target)
		{
			Debug.Assert(target.BoundedBy(IVector3.Zero, Dimensions));
			return tiles[target.X, target.Y, target.Z].decl;
		}

		private List<IVector3> passableNodes(IVector3 node)
		{
			var result = new List<IVector3>();

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

					if (target.BoundedBy(IVector3.Zero, Dimensions) && 
						TileAt(target).passable)
					{
						result.Add(target);
					}
				}
			}
			return result;
		}

		public void reconstruct(Dictionary<IVector3, IVector3> camefrom, IVector3 current, List<IVector3> result)
		{
			if (camefrom.ContainsKey(current))
			{
				reconstruct(camefrom, camefrom[current], result);
			}
			result.Add(current);
		}

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
					//Uhhh.
					reconstruct(cameFrom, cameFrom[end], result);
					result.Add(end);
					return result;
				}

				open.Remove(node);
				closed.Add(node);

				foreach (var t in passableNodes(node))
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
						//Ummm.
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
