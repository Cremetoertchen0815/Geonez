using JetBrains.Annotations;

namespace LocaliSaatana
{
	[UsedImplicitly]
	public class Literal
	{
		[UsedImplicitly]
		public string Name { get; init; } = null!;
		
		[UsedImplicitly]
		public List<string> Translations { get; init; } = [];
	}
}
