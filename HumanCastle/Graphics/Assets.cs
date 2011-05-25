using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using HumanCastle.Properties;
using SlimDX.Direct3D9;
using System.Diagnostics;

namespace HumanCastle.Graphics {
	class Assets {
		// Properties here should match a Resources ID
		// Reflection magic will automatically deal with the rest
		public Texture MMGrass       { get; private set; }
		public Texture OrxyCaveMan   { get; private set; }
		public Texture OrxyCaveWoman { get; private set; }

		Texture NewTextureFromBitmap( Device device, Bitmap bitmap ) {
			int w = bitmap.Width;
			int h = bitmap.Height;

			var t = new Texture( device, w, h, 1, Usage.None, Format.A8R8G8B8, Pool.Managed );

			var dest = t.LockRectangle(0,LockFlags.None);
			var src  = bitmap.LockBits( new Rectangle(0,0,w,h), ImageLockMode.ReadOnly,PixelFormat.Format32bppPArgb );
			try {
				for ( int y=0 ; y<h ; ++y ) {
					dest.Data.Seek( y * dest.Pitch, SeekOrigin.Begin );
					dest.Data.WriteRange( new IntPtr( src.Scan0.ToInt64() + src.Stride*y ), 4*w );
				}
			} finally {
				bitmap.UnlockBits(src);
				t.UnlockRectangle(0);
			}

			return t;
		}

		public void Setup( Device device ) {
			foreach ( var p in typeof(Assets).GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) ) {
				var bitmap = typeof(Resources).GetProperty(p.Name,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static).GetValue(null,null) as Bitmap;
				if ( bitmap == null ) Debug.Fail("Missing resource: "+p.Name);
				var texture = NewTextureFromBitmap( device, bitmap );
				p.SetValue( this, texture, null );
			}
		}
		public void Teardown() {
			foreach ( var p in typeof(Assets).GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) ) {
				using ( p.GetValue(this,null) as Texture ) {} // dispose of 'em all!
				p.SetValue(this,null,null); // and null, why not
			}
		}
	}
}
