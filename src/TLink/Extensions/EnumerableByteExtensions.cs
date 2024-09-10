namespace DSC.TLink.Extensions
{
	internal static class EnumerableByteExtensions
	{
		public static IEnumerable<byte> Concat(this IEnumerable<byte> byteEnumerable, byte appendByte)
		{
			foreach (byte b in byteEnumerable)
			{
				yield return b;
			}
			yield return appendByte;
		}
	}
}
