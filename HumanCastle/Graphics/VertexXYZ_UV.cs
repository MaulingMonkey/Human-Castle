using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.Direct3D9;

namespace HumanCastle.Graphics {
	struct VertexXYZ_UV {
		public Vector3 Position;
		public float X { get { return Position.X; } set { Position.X = value; }}
		public float Y { get { return Position.Y; } set { Position.Y = value; }}
		public float Z { get { return Position.Z; } set { Position.Z = value; }}

		public Vector2 Texture0;
		public float U { get { return Texture0.X; } set { Texture0.X = value; }}
		public float V { get { return Texture0.Y; } set { Texture0.Y = value; }}

		public VertexXYZ_UV( Vector3 position, Vector2 texture0 ) {
			Position = position;
			Texture0 = texture0;
		}

		public VertexXYZ_UV( float x, float y, float z, float u, float v ) {
			Position = new Vector3(x,y,z);
			Texture0 = new Vector2(u,v);
		}

		public static readonly VertexFormat FVF  = VertexFormat.Position | VertexFormat.Texture1;
		public static readonly int          Size = Marshal.SizeOf(typeof(VertexXYZ_UV));
	}
}
