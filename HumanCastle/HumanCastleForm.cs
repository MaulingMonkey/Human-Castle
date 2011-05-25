using System;
using System.Drawing;
using System.Windows.Forms;
using HumanCastle.Graphics;
using HumanCastle.View;
using SlimDX;
using SlimDX.Direct3D9;
using SlimDX.Windows;

namespace HumanCastle {
	[System.ComponentModel.DesignerCategory("")] // Supresses unused form designer
	class HumanCastleForm : Form {
		Direct3D D3D;
		Device   Device;
		Assets   Assets = new Assets();

		LocalMapView RootView = new LocalMapView();

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

		void SetupDevice() {
			Device = new Device(D3D,0,DeviceType.Hardware,Handle,CreateFlags.HardwareVertexProcessing,new PresentParameters()
				{ BackBufferCount    = 1
				, BackBufferFormat   = Format.X8R8G8B8
				, BackBufferWidth    = ClientSize.Width
				, BackBufferHeight   = ClientSize.Height
				, DeviceWindowHandle = Handle
				, Windowed           = true
				});
			Assets.Setup(Device);
			RootView.Setup(Device);
		}
		void TeardownDevice() {
			RootView.Teardown();
			Assets.Teardown();
			using ( Device ) {}
		}

		void MainLoop() {
			Device.Clear( ClearFlags.Target | ClearFlags.ZBuffer, unchecked((int)0xFF000000u), -1.0f, 0 );
			Device.SetTransform( TransformState.Projection, Matrix.OrthoOffCenterLH( 0.5f, ClientSize.Width + 0.5f, ClientSize.Height + 0.5f, 0.5f, +1.0f, -1.0f ) );
			Device.SetRenderState( RenderState.AlphaBlendEnable , true      );
			Device.SetRenderState( RenderState.Lighting         , false     );
			Device.SetRenderState( RenderState.CullMode         , Cull.None                );
			Device.SetRenderState( RenderState.ZFunc            , Compare.GreaterEqual     );
			Device.SetRenderState( RenderState.ZEnable          , false );
			Device.SetRenderState( RenderState.DestinationBlend , Blend.InverseSourceAlpha );

			Device.SetSamplerState( 0, SamplerState.MinFilter, TextureFilter.Linear );
			Device.SetSamplerState( 0, SamplerState.MagFilter, TextureFilter.Linear );

			Device.BeginScene();
			RootView.Render( new ViewRenderArguments()
				{ Assets = Assets
				, Device = Device
				, Form   = this
				});
			Device.EndScene();
			Device.Present();
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			switch ( e.KeyCode ) {
			case Keys.PageUp:
				RootView.CameraFocusPosition.Z++;
				break;
			case Keys.PageDown:
				RootView.CameraFocusPosition.Z--;
				break;
			}
			base.OnKeyDown(e);
		}

		[STAThread] static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			var form = new HumanCastleForm();
			MessagePump.Run( form, form.MainLoop );
		}
	}
}
