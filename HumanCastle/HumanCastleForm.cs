using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HumanCastle.Graphics;
using HumanCastle.View;
using SlimDX;
using SlimDX.Direct3D9;
using SlimDX.Windows;
using System.Diagnostics;

namespace HumanCastle {
	[System.ComponentModel.DesignerCategory("")] // Supresses unused form designer
	class HumanCastleForm : Form {
		Direct3D D3D;
		Device   Device;
		Assets   Assets = new Assets();
		LocalMapView RootView;

		bool                                       Postprocess = true;
		Texture                                    PostprocessTexture;
		readonly DynamicVertexBuffer<VertexXYZ_UV> PostprocessVB = new DynamicVertexBuffer<VertexXYZ_UV>();

		public HumanCastleForm() {
			ClientSize = new Size(800,600);
			Text = "Human Castle";

			D3D = new Direct3D();
			SetupDevice();
		}

		protected override void Dispose( bool disposing ) {
			if ( disposing ) {
				TeardownDevice();
				using ( D3D ) {}
			}
			base.Dispose(disposing);
		}

		PresentParameters PresentParameters { get {
			return new PresentParameters()
				{ BackBufferCount    = 1
				, BackBufferFormat   = Format.X8R8G8B8
				, BackBufferWidth    = ClientSize.Width
				, BackBufferHeight   = ClientSize.Height
				, DeviceWindowHandle = Handle
				, Windowed           = true
				};
		}}

		void SetupDevice() {
			Device = new Device(D3D,0,DeviceType.Hardware,Handle,CreateFlags.HardwareVertexProcessing,PresentParameters);
			Assets.Setup(Device);

			RootView = new LocalMapView(Assets);
			RootView.Setup(Device);
		}
		void TeardownDevice() {
			using ( PostprocessTexture ) {}
			using ( PostprocessVB ) {}

			RootView.Teardown();
			Assets.Teardown();
			using ( Device ) {}
		}

		HashSet<Keys> HeldKeys = new HashSet<Keys>();

		static uint CeilingPow2( uint i ) {
			if (i==0) return 0;
			--i;
			i |= i>>16;
			i |= i>> 8;
			i |= i>> 4;
			i |= i>> 2;
			i |= i>> 1;
			++i;
			return i;
		}
		static int CeilingPow2( int i ) {
			return unchecked((int)CeilingPow2((uint)i));
		}

		DateTime PreviousFrame = DateTime.Now;
		float CameraPanCooldown = 0;
		void MainLoop() 
		{
			var now = DateTime.Now;

			var dt = (float)(now-PreviousFrame).TotalSeconds;
			if ( dt<0 ) dt=0;
			if ( dt>1 ) dt=1;
			PreviousFrame = now;

			CameraPanCooldown -= dt;
			if ( new[]{Keys.Left,Keys.Right,Keys.Up,Keys.Down,Keys.PageUp,Keys.PageDown}.Any(k=>HeldKeys.Contains(k)) )
			while ( CameraPanCooldown < 0 )
			{
				if ( HeldKeys.Contains(Keys.Left    ) ) --RootView.CameraFocusPosition.X;
				if ( HeldKeys.Contains(Keys.Right   ) ) ++RootView.CameraFocusPosition.X;
				if ( HeldKeys.Contains(Keys.Up      ) ) --RootView.CameraFocusPosition.Y;
				if ( HeldKeys.Contains(Keys.Down    ) ) ++RootView.CameraFocusPosition.Y;
				if ( HeldKeys.Contains(Keys.PageUp  ) ) ++RootView.CameraFocusPosition.Z;
				if ( HeldKeys.Contains(Keys.PageDown) ) --RootView.CameraFocusPosition.Z;
				CameraPanCooldown += 0.1f;
			} else if ( CameraPanCooldown<0 ) {
				CameraPanCooldown = 0f;
			}

			Device.SetRenderState( RenderState.AlphaBlendEnable , true      );
			Device.SetRenderState( RenderState.Lighting         , false     );
			Device.SetRenderState( RenderState.CullMode         , Cull.None      );
			Device.SetRenderState( RenderState.ZFunc            , Compare.Always );
			Device.SetRenderState( RenderState.ZEnable          , false );
			Device.SetRenderState( RenderState.DestinationBlend , Blend.InverseSourceAlpha );

			Device.SetSamplerState( 0, SamplerState.MinFilter, TextureFilter.Point );
			Device.SetSamplerState( 0, SamplerState.MagFilter, TextureFilter.Point );

			bool really_postprocess = Postprocess && Device.Capabilities.MaxTextureWidth>=ClientSize.Width && Device.Capabilities.MaxTextureHeight>+ClientSize.Height;

			if ( really_postprocess ) {
				if ( PostprocessTexture!=null ) {
					var desc = PostprocessTexture.GetLevelDescription(0);
					if ( desc.Width < ClientSize.Width || desc.Height < ClientSize.Height ) {
						PostprocessTexture.Dispose();
						PostprocessTexture = null;
					}
				}

				if ( PostprocessTexture==null ) try {
					PostprocessTexture = new Texture( Device, CeilingPow2(ClientSize.Width), CeilingPow2(ClientSize.Height), 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default );
					//if (!D3D.CheckDepthStencilMatch(0,DeviceType.Hardware,Form.
				} catch ( Direct3D9Exception ) {
					really_postprocess = false;
				}
			}

			if ( really_postprocess ) {
				Device.SetRenderTarget( 0, PostprocessTexture.GetSurfaceLevel(0) );
				Device.Viewport = new Viewport(0,0,ClientSize.Width,ClientSize.Height,0,1);
				var desc = PostprocessTexture.GetLevelDescription(0);
				Device.SetTransform( TransformState.Projection, Matrix.OrthoOffCenterLH( 0.5f, desc.Width + 0.5f, desc.Height + 0.5f, 0.5f, +1.0f, -1.0f ) );
			} else {
				Device.SetTransform( TransformState.Projection, Matrix.OrthoOffCenterLH( 0.5f, ClientSize.Width + 0.5f, ClientSize.Height + 0.5f, 0.5f, +1.0f, -1.0f ) );
			}

			Device.BeginScene();
			Device.Clear( ClearFlags.Target, unchecked((int)0xFF112233u), 0f, 0 );
#if true
			RootView.Render( new ViewRenderArguments()
				{ Assets = Assets
				, Device = Device
				, Form   = this
				});
#else
			var VB = new VertexBuffer( Device, 4 * (sizeof(float)*4 + sizeof(uint)), Usage.None, VertexFormat.PositionRhw, Pool.Managed );
			var vb = VB.Lock(0,0,LockFlags.None);
			vb.Write( new Vector4(25f, 25f, 0.5f, 1f) ); vb.Write( 0xFF00FF00u );
			vb.Write( new Vector4(75f, 25f, 0.5f, 1f) ); vb.Write( 0xFF00FF00u );
			vb.Write( new Vector4(75f, 75f, 0.5f, 1f) ); vb.Write( 0xFF00FF00u );
			vb.Write( new Vector4(25f, 75f, 0.5f, 1f) ); vb.Write( 0xFF00FF00u );
			VB.Unlock();
			Device.SetTexture(0,null);
			Device.SetStreamSource( 0, VB, 0, sizeof(float)*4 + sizeof(uint) );
			Device.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;
			Device.DrawPrimitives( PrimitiveType.TriangleFan, 0, 2 );
			VB.Dispose();
#endif
			Device.EndScene();

			if ( really_postprocess ) {
				var w = ClientSize.Width;
				var h = ClientSize.Height;

				Device.SetRenderTarget( 0, Device.GetBackBuffer(0,0) );
				Device.Clear( ClearFlags.Target, unchecked((int)0xFF332211u), 1f, 0 );
				Device.SetTransform( TransformState.Projection, Matrix.OrthoOffCenterLH( 0.5f, w+0.5f, h+0.5f, 0.5f, +1.0f, -1.0f ) );
				Device.SetTransform( TransformState.View      , Matrix.Identity );
				Device.SetTransform( TransformState.World     , Matrix.Identity );


				var desc = PostprocessTexture.GetLevelDescription(0);
				var u = w*1f/desc.Width;
				var v = h*1f/desc.Height;

				PostprocessVB.Clear();
				PostprocessVB.Add( new VertexXYZ_UV( 0, 0, 0, 0, 0 ) );
				PostprocessVB.Add( new VertexXYZ_UV( w, 0, 0, u, 0 ) );
				PostprocessVB.Add( new VertexXYZ_UV( w, h, 0, u, v ) );
				PostprocessVB.Add( new VertexXYZ_UV( 0, h, 0, 0, v ) );

				Device.BeginScene();
				Device.SetTexture( 0, PostprocessTexture );
				Device.VertexFormat = VertexXYZ_UV.FVF;
				Device.SetStreamSource( 0, PostprocessVB.RenderVB(Device), 0, VertexXYZ_UV.Size );
				Device.DrawPrimitives( PrimitiveType.TriangleFan, 0, 2 );
				Device.EndScene();
			}

			Device.Present();
		}

		protected override void OnResize(EventArgs e) {
			try {
				// default pool must always be nuked:
				using ( PostprocessTexture ) {}
				PostprocessTexture = null;

				if ( Device != null ) Device.Reset(PresentParameters);
			} catch ( Direct3D9Exception ) {
				TeardownDevice();
				SetupDevice();
			}

			base.OnResize(e);
		}
		protected override void OnKeyDown(KeyEventArgs e) {
			HeldKeys.Add(e.KeyCode);
			base.OnKeyDown(e);
		}

		protected override void OnKeyUp(KeyEventArgs e) {
			HeldKeys.Remove(e.KeyCode);
			base.OnKeyUp(e);
		}

		[STAThread] static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			var form = new HumanCastleForm();
			MessagePump.Run( form, form.MainLoop );
		}
	}
}
