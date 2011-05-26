using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using SlimDX.Direct3D9;

using Vertex = HumanCastle.Graphics.VertexXYZ_UV;

namespace HumanCastle.Graphics {
	class BatchSpriteRenderer {
		Device Device;
		AutoDictionary<TextureKey,PerTextureInfo> Texture = new AutoDictionary<TextureKey,PerTextureInfo>();

		public BatchSpriteRenderer() {
		}

		public void Add( Texture tile, RectangleF where ) { Add(tile,where,0.0f); }
		public void Add( Texture tile, RectangleF where, float z ) {
			var info  = Texture[ new TextureKey() { Texture = tile } ];

			uint i = (uint)info.VB.Count;

			info.VB.AddRange( new Vertex[]
				{ new Vertex( where.Left , where.Top   , z, 0, 0 )
				, new Vertex( where.Right, where.Top   , z, 1, 0 )
				, new Vertex( where.Right, where.Bottom, z, 1, 1 )
				, new Vertex( where.Left , where.Bottom, z, 0, 1 )
				});
			info.IB.AddRange( new uint[] { i+0, i+1, i+2, i+0, i+2, i+3 } );
		}

		public void Render( ViewRenderArguments args ) {
			var device = args.Device;

			foreach ( var entry in Texture ) {
				if ( entry.Value.IB.Count == 0 ) return;

				device.SetTexture( 0, entry.Key.Texture );
				device.VertexFormat = entry.Value.VB.FVF;
				device.SetStreamSource( 0, entry.Value.VB.RenderVB(device), 0, Vertex.Size );
				device.Indices = entry.Value.IB.RenderIB(device);
				device.DrawIndexedPrimitives( PrimitiveType.TriangleList, 0, 0, entry.Value.VB.Count, 0, entry.Value.VB.Count/2 );

				entry.Value.IB.Clear();
				entry.Value.VB.Clear();
			}
		}

		public void Setup( Device device ) {
			Device = device;
		}
		public void Teardown() {
			foreach ( var entry in Texture ) entry.Value.Dispose();
		}

		struct TextureKey {
			public Texture Texture;
		}

		class PerTextureInfo : IDisposable {
			public readonly DynamicVertexBuffer<Vertex> VB = new DynamicVertexBuffer<Vertex>();
			public readonly DynamicIndexBuffer          IB = new DynamicIndexBuffer();

			public void Dispose() {
				using ( VB ) {}
				using ( IB ) {}
			}
		}
	}
}
