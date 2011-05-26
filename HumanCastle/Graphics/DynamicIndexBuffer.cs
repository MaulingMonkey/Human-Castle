using System;
using System.Collections.Generic;
using System.Diagnostics;
using SlimDX.Direct3D9;

namespace HumanCastle.Graphics {
	class DynamicIndexBuffer : List<uint>, IDisposable {
		IndexBuffer RealIB = null;

		public void Add( int index ) {
			Add(unchecked((uint)index));
		}

		public IndexBuffer RenderIB( Device device ) {
			if ( this.Count == 0 ) return null;

			if ( RealIB!=null ) Debug.Assert( RealIB.Device == device );

			int size = (RealIB==null) ? 0 : RealIB.Description.SizeInBytes;
			int size_required = this.Count * sizeof(uint);

			if ( size < size_required ) {
				size = Math.Max(2*size,size_required);

				using ( RealIB ) {}
				RealIB = new IndexBuffer( device, size, Usage.None, Pool.Managed, false );
			}

			var ib = RealIB.Lock(0,0,LockFlags.None);
			ib.WriteRange(this.ToArray()); // TOOD: Figure out if this, pinning, or a simple loop is fastest.
			RealIB.Unlock();

			return RealIB;
		}

		public void Dispose() {
			using ( RealIB ) {}
			RealIB = null;
		}
	}
}
