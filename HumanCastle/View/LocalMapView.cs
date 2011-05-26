using System;
using System.Collections.Generic;
using System.Drawing;
using HumanCastle.Graphics;
using HumanCastle.Model;
using SlimDX.Direct3D9;
using SlimDX;

namespace HumanCastle.View {
	enum TileType { Air, Grass, Dirt } // Temporary?
	enum EntityType { Guy, Gal } // Very temporary

	class Entity {
		public EntityType EntityType;
		public IVector3 Position;
	}

	class LocalMapView {
		public LocalMap LocalMap;

		public IVector3 CameraFocusPosition = new IVector3(15,0,0);

		IEnumerable<Entity> GetVisibleEntities() {
			yield return new Entity() { EntityType = EntityType.Guy, Position = new IVector3(10,0,0) };
			yield return new Entity() { EntityType = EntityType.Guy, Position = new IVector3(20,0,0) };
		}

		TileType GetTileType( IVector3 xyz ) {
			if ( xyz.Z == 0 ) return TileType.Grass;
			return TileType.Air;
		}

		static readonly List<uint> AtmosphericLayers = new List<uint>()
			{ 0x805A6D71 // premultiplied ARGB
			, 0xFFB4DBED
			};

		readonly BatchAtmosphereRenderer2D AtmosphereRenderer2D = new BatchAtmosphereRenderer2D(); // TODO: also use for higlighting to-mine areas?
		readonly BatchSpriteRenderer       BatchSpriteRenderer  = new BatchSpriteRenderer();

		public LocalMapView(Assets assets)
		{
			LocalMap = new LocalMap(128, 128, 32, assets);
		}

		public void Setup( Device device ) {
			AtmosphereRenderer2D.Setup(device);
			BatchSpriteRenderer.Setup(device);
		}

		public void Teardown() {
			AtmosphereRenderer2D.Teardown();
			BatchSpriteRenderer.Teardown();
		}

		Size TileSize = new Size(8,8);
		int  Zoom = 2;

		public void Render( ViewRenderArguments args ) 
		{
			var device = args.Device;
			device.SetTransform( TransformState.View, Matrix.Scaling(Zoom,Zoom,1) * Matrix.Translation( args.Form.ClientSize.Width/2, args.Form.ClientSize.Height/2, 0 ) );

			int z = CameraFocusPosition.Z;

			// TODO: Better view range calcs.
			// These are inclusve.
			int ymin = Math.Max(0,CameraFocusPosition.Y-50);
			int xmin = Math.Max(0,CameraFocusPosition.X-50);
			int xmax = Math.Min(LocalMap.Width -1,CameraFocusPosition.X+50);
			int ymax = Math.Min(LocalMap.Height-1,CameraFocusPosition.Y+50);

			for ( int tile_y=ymin ; tile_y<=ymax ; ++tile_y )
			{
				for ( int tile_x=xmin ; tile_x<=xmax ; ++tile_x )
				{
					// relative coordinates (still in tiles):
					var ex = tile_x-CameraFocusPosition.X;
					var ey = tile_y-CameraFocusPosition.Y;

					// render location:
					var rect = new Rectangle(TileSize.Width*ex,TileSize.Height*ey,TileSize.Width,TileSize.Height);

					var tt = GetTileType(new IVector3(tile_x,tile_y,z));

					int ground_z = z;
					while ( ground_z>=0 && GetTileType(new IVector3(tile_x,tile_y,ground_z)) == TileType.Air ) --ground_z;
					if ( ground_z < 0 ) ground_z -= 9001;

					if ( z!=ground_z ) {
						int atmos_i = Math.Min(z-ground_z-1,AtmosphericLayers.Count-1);
						AtmosphereRenderer2D.Add( rect, AtmosphericLayers[atmos_i] );
					}

					Assets assets = args.Assets;
					if (ground_z >= 0)
					{
						Tile tile = LocalMap[tile_x, tile_y, ground_z];
						if (assets.TextureForMaterial(tile.Declaration.Material) != null)
						{
							BatchSpriteRenderer.Add(assets.TextureForMaterial(tile.Declaration.Material), rect);
						}
					}
				}
			}

			foreach ( var entity in GetVisibleEntities() ) {
				// relative coordinates (still in tiles):
				var ex = entity.Position.X-CameraFocusPosition.X;
				var ey = entity.Position.Y-CameraFocusPosition.Y;

				// render location:
				var rect = new Rectangle(TileSize.Width*ex,TileSize.Height*ey,TileSize.Width,TileSize.Height);

				if ( entity.Position.Z <= CameraFocusPosition.Z ) switch ( entity.EntityType ) {
				case EntityType.Guy:
					BatchSpriteRenderer.Add( args.Assets.OrxyCaveMan  , rect );
					break;
				case EntityType.Gal:
					BatchSpriteRenderer.Add( args.Assets.OrxyCaveWoman, rect );
					break;
				}
			}

			BatchSpriteRenderer.Render(args);
			AtmosphereRenderer2D.Render(args);
		}
	}
}
