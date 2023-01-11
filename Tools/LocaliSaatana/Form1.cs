using System;
using System.Linq;
using System.Windows.Forms;

namespace Nez.LocaliSaatana
{
	public partial class Form1 : Form
	{
		private Project _project;
		private Literal _currentLit;
		public Form1()
		{
			InitializeComponent();
		}

		private void btnAddLiteral_Click(object sender, EventArgs e)
		{
			if (_project == null) return;
			if (Prompt.ShowDialog("Enter literal name", "Add literal", out string literalName) == DialogResult.OK)
			{
				_project.Literals.Add(new Literal() { Name = literalName });
				var literal = _project.Literals.Last();
				for (int i = 0; i < _project.Languages.Count; i++)
				{
					literal.Translations.Add("");
				}
				RefreshLiteralList();
			}
		}

		private void RefreshAll()
		{
			this.Text = "LocaliSaatana - " + (_project.FileName ?? "New Project");
			RefreshLiteralList();
			RefreshLanguages();
		}

		private void RefreshLiteralList()
		{
			lstLiterals.Items.Clear();
			foreach (var literal in _project.Literals)
			{
				lstLiterals.Items.Add(literal.Name);
			}
			RefreshLiteral();
		}

		private void RefreshLiteral()
		{
			_currentLit = lstLiterals.SelectedIndex < 0 || lstLanguages.SelectedIndex < 0 ? null : _project.Literals[lstLiterals.SelectedIndex];
			txtLiteral.Text = _currentLit?.Translations[lstLanguages.SelectedIndex] ?? string.Empty;
		}

		private void RefreshLanguages()
		{
			lstLanguages.Items.Clear();
			if (_project == null) return;
			foreach (var language in _project.Languages) lstLanguages.Items.Add($"{language.Name} ({language.Literal})");
			RefreshLiteral();
		}


		private void lstLiterals_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (lstLiterals.SelectedIndex == -1)
			{
				_currentLit = null;
				return;
			}
			RefreshLiteral();
		}

		private void lstLanguages_SelectedIndexChanged(object sender, EventArgs e) => RefreshLiteral();

		private void newToolStripMenuItem_Click(object sender, EventArgs e)
		{
			_project = new Project();
			RefreshAll();
		}

		private void addToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (_project == null) return;
			if (Prompt.ShowDialog("Please enter Language name:", "Add Language", out string name) == DialogResult.OK)
			{
				if (Prompt.ShowDialog("Please enter Language literal(enum value):", "Add Language", out string literal) == DialogResult.OK)
				{
					_project.Languages.Add((literal, name));
					for (int i = 0; i < _project.Literals.Count; i++) _project.Literals[i].Translations.Add("");
				}
			}
			RefreshLanguages();
		}

		private void removeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			int idx;
			if (_project == null || (idx = lstLanguages.SelectedIndex) < 0) return;
			_project.Languages.RemoveAt(idx);
			for (int i = 0; i < _project.Literals.Count; i++) _project.Literals[i].Translations.RemoveAt(idx);
			RefreshLanguages();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			_project = new Project();
			RefreshAll();
		}

		private void txtLiteral_TextChanged(object sender, EventArgs e)
		{
			if (_currentLit == null) return;
			_currentLit.Translations[lstLanguages.SelectedIndex] = txtLiteral.Text;
		}

		private void btnRemLiteral_Click(object sender, EventArgs e)
		{
			if (_project == null || lstLiterals.SelectedIndex < 0) return;
			_project.Literals.RemoveAt(lstLiterals.SelectedIndex);
			RefreshLiteralList();
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (_project == null) return;
			if (_project.FileName == null)
			{
				if (saveDialogProject.ShowDialog() == DialogResult.OK)
				{
					_project.FileName = saveDialogProject.FileName;
				}
				else return;
			}
			var data = Newtonsoft.Json.JsonConvert.SerializeObject(_project);
			System.IO.File.WriteAllText(_project.FileName, data);
		}

		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (saveDialogProject.ShowDialog() == DialogResult.OK)
			{
				_project.FileName = saveDialogProject.FileName;
			}
			else return;
			var data = Newtonsoft.Json.JsonConvert.SerializeObject(_project);
			System.IO.File.WriteAllText(_project.FileName, data);
			RefreshAll();
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				if (openDialogProject.ShowDialog() == DialogResult.OK)
				{
					var data = System.IO.File.ReadAllText(openDialogProject.FileName);
					_project = Newtonsoft.Json.JsonConvert.DeserializeObject<Project>(data);
					_project.FileName = openDialogProject.FileName;
				}
			}
			catch (Exception)
			{
				MessageBox.Show("Invalid project file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			RefreshAll();
		}

		private void buildToolStripMenuItem_Click(object sender, EventArgs e) => _project?.Build();

		private void setContentPathToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (_project == null) return;
			folderDialogContent.SelectedPath = _project.ContentPath;
			if (folderDialogContent.ShowDialog() == DialogResult.OK)
			{
				_project.ContentPath = folderDialogContent.SelectedPath;
			}
		}

		private void setMapPathToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (_project == null) return;
			saveDialogMap.FileName = _project.MapPath;
			if (saveDialogMap.ShowDialog() == DialogResult.OK)
			{
				_project.MapPath = saveDialogMap.FileName;
			}

			if (Prompt.ShowDialog("Please enter the desired Namespace:", "Set Map", out var s, _project.Namespace) == DialogResult.OK)
			{
				_project.Namespace = s;
			}
		}
	}
}
