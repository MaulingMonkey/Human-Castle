using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using SlimDX.Direct3D9;

namespace HumanCastle.Graphics {
	class DynamicVertexBuffer<Vertex> : List<Vertex>, IDisposable where Vertex : struct {
		static readonly VertexFormat VertexFVF  = (VertexFormat)typeof(Vertex).GetField("FVF",BindingFlags.Public|BindingFlags.Static).GetValue(null);
		static readonly int          VertexSize = Marshal.SizeOf(typeof(Vertex));

		public VertexFormat FVF { get { return VertexFVF; }}

		VertexBuffer RealVB = null;

		public VertexBuffer RenderVB( Device device ) {
			if ( this.Count == 0 ) return null;

			if ( RealVB!=null ) Debug.Assert( RealVB.Device == device );

			int size = (RealVB==null) ? 0 : RealVB.Description.SizeInBytes;
			int size_required = VertexSize * this.Count;

			if ( size < size_required ) {
				size = Math.Max(2*size,size_required);

				using ( RealVB ) {}
				RealVB = new VertexBuffer( device, size, Usage.None, VertexFVF, Pool.Managed );
			}

			var vb = RealVB.Lock(0,0,LockFlags.None);
			vb.WriteRange(this.ToArray()); // TOOD: Figure out if this, pinning, or a simple loop is fastest.
			RealVB.Unlock();

			return RealVB;
		}

		public void Dispose() {
			using ( RealVB ) {}
			RealVB = null;
		}
	}
}
