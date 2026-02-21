using System.IO;
using Hebron.Runtime;

namespace StbImageSharp;

#if !STBSHARP_INTERNAL
public
#else
internal
#endif
class ImageResultFloat
{
	public int Width { get; set; }
	public int Height { get; set; }
	public ColorComponents SourceComp { get; set; }
	public ColorComponents Comp { get; set; }
	public float[] Data { get; set; } = [];

	internal static unsafe ImageResultFloat FromResult(float* result, int width, int height, ColorComponents comp,
		ColorComponents req_comp)
	{
		if (result == null)
			throw new InvalidOperationException(StbImage.stbi__g_failure_reason);

		var image = new ImageResultFloat
		{
			Width = width,
			Height = height,
			SourceComp = comp,
			Comp = req_comp == ColorComponents.Default ? comp : req_comp
		};

		image.Data = new float[width * height * (int)image.Comp];
		new ReadOnlySpan<float>(result, image.Data.Length).CopyTo(image.Data);

		return image;
	}

	public static unsafe ImageResultFloat FromStream(Stream stream,
		ColorComponents requiredComponents = ColorComponents.Default)
	{
		float* result = null;

		try
		{
			int x, y, comp;

			var context = new StbImage.stbi__context(stream);

			result = StbImage.stbi__loadf_main(context, &x, &y, &comp, (int)requiredComponents);

			return FromResult(result, x, y, (ColorComponents)comp, requiredComponents);
		}
		finally
		{
			if (result != null)
				CRuntime.free(result);
		}
	}

	public static ImageResultFloat FromMemory(byte[] data,
		ColorComponents requiredComponents = ColorComponents.Default)
	{
		using var stream = new MemoryStream(data);
		return FromStream(stream, requiredComponents);
	}
}
