using System;
using System.Collections.Generic;
using System.Drawing;
using HumanCastle.Graphics;
using HumanCastle.Model;
using SlimDX.Direct3D9;

namespace HumanCastle.View {
	enum TileType { Air, Grass, Dirt } // Temporary?
	enum EntityType { Guy, Gal } // Very temporary

	class Entity {
		public EntityType EntityType;
		public IVector3 Position;
	}

	class LocalMapView {
		public LocalMap LocalMap = new LocalMap(128,128,64);

		public IVector3 CameraFocusPosition = new IVector3(15,0,0);

		IEnumerable<Entity> GetVisibleEntities() {
			yield return new Entity() { EntityType = EntityType.Guy, Position = new IVector3(10,0,0) };
			yield return new Entity() { EntityType = EntityType.Guy, Position = new IVector3(20,0,0) };
		}

		TileType GetTileType( IVector3 xyz ) {
			return TileType.Grass;
		}

		static readonly List<uint> AtmosphericLayers = new List<uint>()
			{ 0x805A6D71 // premultiplied ARGB
			, 0xFFB4DBED
			};

		readonly AtmosphereRenderer2D AtmosphereRenderer2D = new AtmosphereRenderer2D();

		public void Setup( Device device ) {
			AtmosphereRenderer2D.Setup(device);
		}
		public void Teardown() {
			AtmosphereRenderer2D.Teardown();
		}

		Size TileSize = new Size(8,8);

		public void Render( ViewRenderArguments args ) {
			var device = args.Device;

			int z = CameraFocusPosition.Z;

			// TODO: Better view range calcs.
			// These are inclusve.
			int ymin = Math.Max(0,CameraFocusPosition.Y-100);
			int xmin = Math.Max(0,CameraFocusPosition.X-100);
			int xmax = Math.Min(LocalMap.Width -1,CameraFocusPosition.X+100);
			int ymax = Math.Min(LocalMap.Height-1,CameraFocusPosition.Y+100);

			for ( int realy=ymin ; realy<=ymax ; ++realy )
			for ( int realx=xmin ; realx<=xmax ; ++realx )
			{
				// relative coordinates (still in tiles):
				var ex = realx-CameraFocusPosition.X;
				var ey = realy-CameraFocusPosition.Y;

				var tt = GetTileType(new IVector3(realx,realy,z));

				int atmos_z = z;
				while ( atmos_z>=0 && GetTileType(new IVector3(realx,realy,atmos_z)) == TileType.Air ) --atmos_z;
				if ( atmos_z == -1 ) atmos_z -= 9001;

				if ( z!=atmos_z ) {
					int atmos_i = Math.Min(z-atmos_z-1,AtmosphericLayers.Count);
					AtmosphereRenderer2D.Add( new RectangleF(TileSize.Width*ex,TileSize.Height*ey,TileSize.Width,TileSize.Height), AtmosphericLayers[atmos_i] );
				}

				switch ( GetTileType(new IVector3(realx,realy,z)) ) {
				case TileType.Air:
					break;
				case TileType.Grass:
					break;
				case TileType.Dirt:
					break;
				default:
					// ???
					break;
				}
			}

			AtmosphereRenderer2D.Render(args);
		}
	}
}
