using System.Collections.Generic;
using System.Text;

namespace Nez.LocaliSaatana
{
	internal class Project
	{
		public string FileName { get; set; }
		public string ContentPath { get; set; } = "..\\Nez\\Content";
		public string MapPath { get; set; } = "..\\Nez\\";
		public string Namespace { get; set; } = "Nez.LocaliSaatana";
		public List<(string Literal, string Name)> Languages { get; set; } = new List<(string Literal, string Name)>();
		public List<Literal> Literals { get; set; } = new List<Literal>();

		public void Build()
		{
			//Create map
			var sb = new StringBuilder();
			sb.AppendLine($"namespace {Namespace};\n");
			sb.AppendLine("/// <summary>\n/// Provides constants for reading language-variable text literals from the localisation dictionary.\n/// </summary>");
			sb.AppendLine("public enum TextLiterals");
			sb.AppendLine("{");
			for (int i = 0; i < Literals.Count; i++)
			{
				var literal = Literals[i];
				sb.Append($"\t{literal.Name} = {i}");
				sb.Append(i + 1 < Literals.Count ? ",\n" : "\n");
			}
			sb.AppendLine("}\n\n");
			sb.AppendLine("/// <summary>\n/// Lists the available languages for the localisation dictionary.\n/// </summary>");
			sb.AppendLine("public enum Languages");
			sb.AppendLine("{");
			for (int i = 0; i < Languages.Count; i++)
			{
				var lang = Languages[i].Literal;
				sb.Append($"\t{lang} = {i}");
				sb.Append(i + 1 < Languages.Count ? ",\n" : "\n");
			}
			sb.AppendLine("}");
			System.IO.File.WriteAllText(MapPath, sb.ToString());

			//Create content
			for (int i = 0; i < Languages.Count; i++)
			{
				var lang = Languages[i];
				sb = new StringBuilder();
				sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
				sb.AppendLine("<XnaContent xmlns:ns=\"Microsoft.Xna.Framework\">");
				sb.AppendLine("\t<Asset Type=\"string[]\">");
				foreach (var item in Literals)
				{
					sb.AppendLine($"\t\t<Item>{item.Translations[i]}</Item>");
				}
				sb.AppendLine("\t</Asset>");
				sb.AppendLine("</XnaContent>");
				System.IO.File.WriteAllText(ContentPath + "\\lang_" + lang.Literal + ".xml", sb.ToString());
			}

			//Create language description list
			sb = new StringBuilder();
			sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
			sb.AppendLine("<XnaContent xmlns:ns=\"Microsoft.Xna.Framework\">");
			sb.AppendLine("\t<Asset Type=\"string[]\">");
			foreach (var item in Languages)
			{
				sb.AppendLine($"\t\t<Item>{item.Name}</Item>");
			}
			sb.AppendLine("\t</Asset>");
			sb.AppendLine("</XnaContent>");
			System.IO.File.WriteAllText(ContentPath + "\\languages.xml", sb.ToString());
		}
	}
}
