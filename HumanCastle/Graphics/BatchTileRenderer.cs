using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using SlimDX.Direct3D9;

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

				if ( entry.Value.RealVB==null || entry.Value.RealVB.Description.SizeInBytes < Vertex.Size * entry.Value.VB.Count ) {
					using ( entry.Value.RealVB ) {}
					using ( entry.Value.RealIB ) {}
					entry.Value.RealVB = new VertexBuffer( device, Vertex.Size * entry.Value.VB.Count, Usage.None, Vertex.FVF, Pool.Managed );
					entry.Value.RealIB = new IndexBuffer(  device, sizeof(uint)* entry.Value.IB.Count, Usage.None, Pool.Managed, false );
				}

				var vb = entry.Value.RealVB.Lock(0,0,LockFlags.None);
				vb.WriteRange(entry.Value.VB.ToArray());
				entry.Value.RealVB.Unlock();

				var ib = entry.Value.RealIB.Lock(0,0,LockFlags.None);
				ib.WriteRange(entry.Value.IB.ToArray());
				entry.Value.RealIB.Unlock();

				device.SetTexture( 0, entry.Key.Texture );
				device.VertexFormat = entry.Value.RealVB.Description.FVF;
				device.SetStreamSource( 0, entry.Value.RealVB, 0, Vertex.Size );
				device.Indices = entry.Value.RealIB;
				device.DrawIndexedPrimitives( PrimitiveType.TriangleList, 0, 0, entry.Value.VB.Count, 0, entry.Value.VB.Count/4 );
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
			public readonly List<Vertex> VB = new List<Vertex>();
			public readonly List<uint  > IB = new List<uint>();

			public IndexBuffer  RealIB;
			public VertexBuffer RealVB;

			public void Dispose() {
				if ( RealIB != null ) RealIB.Dispose(); RealIB = null;
				if ( RealVB != null ) RealVB.Dispose(); RealVB = null;
			}
		}

		struct Vertex {
			public float X,Y,Z;
			public float U,V;

			public Vertex( float x, float y, float z, float u, float v ) { X=x; Y=y; Z=z; U=u; V=v; }

			public static readonly int Size = Marshal.SizeOf(typeof(Vertex));
			public static readonly VertexFormat FVF = VertexFormat.Position | VertexFormat.Texture1;
		}
	}
}
