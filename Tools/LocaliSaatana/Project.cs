using System.Text;
using JetBrains.Annotations;

namespace LocaliSaatana
{
	internal class Project
	{
		[UsedImplicitly]
		public string ContentPath { get; init; } = null!;
		
		[UsedImplicitly]
		public string MapPath { get; init; } = null!;
		
		[UsedImplicitly]
		public string Namespace { get; init; } = null!;
		
		[UsedImplicitly]
		public List<(string Literal, string Name)> Languages { get; init; } = [];
		
		[UsedImplicitly]
		public List<Literal> Literals { get; init; } = [];

		public async Task Build()
		{
			//Create map
			var sb = new StringBuilder();
			sb.AppendLine($"namespace {Namespace};\n");
			sb.AppendLine("/// <summary>\n/// Provides constants for reading language-variable text literals from the localisation dictionary.\n/// </summary>");
			sb.AppendLine("public enum TextLiterals");
			sb.AppendLine("{");
			for (var i = 0; i < Literals.Count; i++)
			{
				var literal = Literals[i];
				sb.Append($"\t{literal.Name} = {i}");
				sb.Append(i + 1 < Literals.Count ? ",\n" : "\n");
			}
			sb.AppendLine("}\n\n");
			sb.AppendLine("/// <summary>\n/// Lists the available languages for the localisation dictionary.\n/// </summary>");
			sb.AppendLine("public enum Languages");
			sb.AppendLine("{");
			for (var i = 0; i < Languages.Count; i++)
			{
				var lang = Languages[i].Literal;
				sb.Append($"\t{lang} = {i}");
				sb.Append(i + 1 < Languages.Count ? ",\n" : "\n");
			}
			sb.AppendLine("}");
			await File.WriteAllTextAsync(MapPath, sb.ToString());

			//Create content
			for (var i = 0; i < Languages.Count; i++)
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
				await File.WriteAllTextAsync(ContentPath + "\\lang_" + lang.Literal + ".xml", sb.ToString());
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
			await File.WriteAllTextAsync(ContentPath + "\\languages.xml", sb.ToString());
		}
	}
}
