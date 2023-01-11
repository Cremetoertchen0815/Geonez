using System.Windows.Forms;

namespace Nez.LocaliSaatana
{
	public static class Prompt
	{
		public static DialogResult ShowDialog(string text, string caption, out string result, string defVal = null)
		{
			Form prompt = new Form()
			{
				Width = 500,
				Height = 150,
				FormBorderStyle = FormBorderStyle.FixedDialog,
				Text = caption,
				StartPosition = FormStartPosition.CenterScreen
			};
			Label textLabel = new Label() { Left = 50, Top = 20, Text = text, Width = 400 };
			TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400, Text = defVal ?? string.Empty };
			Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
			confirmation.Click += (sender, e) => { prompt.Close(); };
			prompt.Controls.Add(textBox);
			prompt.Controls.Add(confirmation);
			prompt.Controls.Add(textLabel);
			prompt.AcceptButton = confirmation;

			var res = prompt.ShowDialog();
			result = res == DialogResult.OK ? textBox.Text : "";
			return res;
		}
	}
}
