using System.Collections.Generic;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

using Vertex = HumanCastle.Graphics.VertexXYZ_Diffuse;

namespace HumanCastle.Graphics {
	class BatchAtmosphereRenderer2D {
		readonly List<Vertex> Verticies = new List<Vertex>();
		readonly List<int   > Indicies  = new List<int>();

		public void Add( RectangleF where, uint argb ) {
			var i = Verticies.Count;

			Indicies.Add(i+0);
			Indicies.Add(i+1);
			Indicies.Add(i+2);
			Indicies.Add(i+0);
			Indicies.Add(i+2);
			Indicies.Add(i+3);

			Verticies.Add( new Vertex() { Position = new Vector3(where.Left ,where.Top   ,0), Diffuse=argb } );
			Verticies.Add( new Vertex() { Position = new Vector3(where.Right,where.Top   ,0), Diffuse=argb } );
			Verticies.Add( new Vertex() { Position = new Vector3(where.Right,where.Bottom,0), Diffuse=argb } );
			Verticies.Add( new Vertex() { Position = new Vector3(where.Left ,where.Bottom,0), Diffuse=argb } );
		}

		VertexBuffer VB;
		IndexBuffer  IB;

		public void Render( ViewRenderArguments args ) {
			if ( Indicies.Count == 0 ) return;

			var device = args.Device;

			if ( VB==null || VB.Description.SizeInBytes < Vertex.Size * Verticies.Count ) {
				using ( VB ) {}
				using ( IB ) {}
				VB = new VertexBuffer( device, Vertex.Size * Verticies.Count, Usage.None, Vertex.FVF, Pool.Managed );
				IB = new IndexBuffer(  device, sizeof(uint)* Indicies .Count, Usage.None, Pool.Managed, false );
			}

			var vb = VB.Lock(0,0,LockFlags.None);
			vb.WriteRange(Verticies.ToArray());
			VB.Unlock();

			var ib = IB.Lock(0,0,LockFlags.None);
			ib.WriteRange(Indicies.ToArray());
			IB.Unlock();

			device.SetTexture(0,null);
			device.Indices = IB;
			device.VertexFormat = Vertex.FVF;
			device.SetStreamSource(0,VB,0,Vertex.Size);
			device.DrawIndexedPrimitives(PrimitiveType.TriangleList,0,0,Verticies.Count,0,Verticies.Count/2);

			Indicies.Clear();
			Verticies.Clear();
		}
		public void Setup( Device device ) {
		}
		public void Teardown() {
			using ( VB ) {} VB = null;
			using ( IB ) {} IB = null;
		}
	}
}
