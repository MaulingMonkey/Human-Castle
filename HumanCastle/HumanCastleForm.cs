using System;
using System.Drawing;
using System.Windows.Forms;
using SlimDX.Windows;

namespace HumanCastle {
	[System.ComponentModel.DesignerCategory("")]
	class HumanCastleForm : Form {
		public HumanCastleForm() {
			ClientSize = new Size(800,600);
			Text = "Human Castle";
		}

		protected override void Dispose( bool disposing ) {
			if ( disposing ) {
			}
			base.Dispose(disposing);
		}

		void MainLoop() {
		}

		[STAThread] static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			var form = new HumanCastleForm();
			MessagePump.Run( form, form.MainLoop );
		}
	}
}
