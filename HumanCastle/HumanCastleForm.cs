using System;
using System.Drawing;
using System.Windows.Forms;
using HumanCastle.Graphics;
using HumanCastle.View;
using SlimDX;
using SlimDX.Direct3D9;
using SlimDX.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

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
			RootView.Setup(Device);
		}
		void TeardownDevice() {
			RootView.Teardown();
			Assets.Teardown();
			using ( Device ) {}
		}

		HashSet<Keys> HeldKeys = new HashSet<Keys>();


		DateTime PreviousFrame = DateTime.Now;
		float CameraPanCooldown = 0;
		void MainLoop() {
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

		protected override void OnResize(EventArgs e) {
			try {
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
