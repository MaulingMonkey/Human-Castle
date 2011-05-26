using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.Direct3D9;

namespace HumanCastle.Graphics {
	struct VertexXYZ_Diffuse {
		public Vector3 Position;
		public float X { get { return Position.X; } set { Position.X = value; }}
		public float Y { get { return Position.Y; } set { Position.Y = value; }}
		public float Z { get { return Position.Z; } set { Position.Z = value; }}

		public uint    Diffuse;

		public VertexXYZ_Diffuse( Vector3 position, uint diffuse ) {
			Position = position;
			Diffuse  = diffuse;
		}

		public VertexXYZ_Diffuse( float x, float y, float z, uint diffuse ) {
			Position = new Vector3(x,y,z);
			Diffuse  = diffuse;
		}

		public static readonly VertexFormat FVF  = VertexFormat.Position | VertexFormat.Diffuse;
		public static readonly int          Size = Marshal.SizeOf(typeof(VertexXYZ_Diffuse));
	}
}
