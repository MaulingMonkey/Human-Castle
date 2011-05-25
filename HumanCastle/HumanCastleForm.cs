using System;
using System.Drawing;
using System.Windows.Forms;
using HumanCastle.Graphics;
using SlimDX.Direct3D9;
using SlimDX.Windows;

namespace HumanCastle {
	[System.ComponentModel.DesignerCategory("")] // Supresses unused form designer
	class HumanCastleForm : Form {
		Direct3D D3D;
		Device   Device;
		Assets   Assets = new Assets();

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
		}
		void TeardownDevice() {
			Assets.Teardown();
			using ( Device ) {}
		}

		void MainLoop() {
			Device.Clear( ClearFlags.Target, unchecked((int)0xFF112233u), 0f, 0 );
			Device.Present();
		}

		[STAThread] static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			var form = new HumanCastleForm();
			MessagePump.Run( form, form.MainLoop );
		}
	}
}
