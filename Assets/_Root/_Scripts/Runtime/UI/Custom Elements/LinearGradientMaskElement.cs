using UnityEngine;
using UnityEngine.UIElements;

namespace PROJECTNAME.UI
{
[UxmlElement("LinearGradientMask", libraryPath = "Effects/Gradients",
			 visibility = LibraryVisibility.Visible)]
public partial class LinearGradientMaskElement : VisualElement
{
	[UxmlAttribute]
	public Texture2D Texture;
	[UxmlAttribute, ColorUsage(true, true),]
	public Color StartColour = Color.white;
	[UxmlAttribute, ColorUsage(true, true),]
	public Color EndColour = new(0f, 0f, 0f, 0f);
	[UxmlAttribute, Range(0f, 360f),]
	public float GradientAngle = 0f;
	[UxmlAttribute, Range(0f,1f),]
	public float GradientStartPoint = 0f;
	[UxmlAttribute, Range(0f,1f),]
	public float GradientEndPoint = 1f;
	[UxmlAttribute, Min(1f),]
	public int Segments = 32;


	public LinearGradientMaskElement()
	{
		style.flexGrow = 1f;
		generateVisualContent += OnGenerateVisualContent;
	}


	private void OnGenerateVisualContent(MeshGenerationContext mgc)
	{
		Rect rect = contentRect;
		if (rect.width < 0.01f || rect.height < 0.01f) return;

		const int INDICES_PER_QUAD = 6;
		int vertexCount = (Segments + 1) * 2;
		int indexCount = Segments * INDICES_PER_QUAD ;

		var vertices = new Vertex[vertexCount];
		var indices = new ushort[indexCount];

		for (var i = 0; i < Segments; i++)
		{
			int vertex = i * 2;
			int index = i * INDICES_PER_QUAD ;

			indices[index + 0] = (ushort)(vertex + 0);
			indices[index + 1] = (ushort)(vertex + 2);
			indices[index + 2] = (ushort)(vertex + 3);

			indices[index + 3] = (ushort)(vertex + 0);
			indices[index + 4] = (ushort)(vertex + 3);
			indices[index + 5] = (ushort)(vertex + 1);
		}

		for (var i = 0; i <= Segments; i++)
		{
			float position = (float)i / Segments;
			float x = rect.width * position;

			var vertexTopPosition = new Vector2(x, 0f);
			var vertexBottomPosition = new Vector2(x, rect.height);

			float gradientTTop = ComputeGradientT(vertexTopPosition, rect);
			float gradientTBottom = ComputeGradientT(vertexBottomPosition, rect);

			Color vertexTopTint = Color.Lerp(StartColour, EndColour, gradientTTop);
			Color vertexTopBottom = Color.Lerp(StartColour, EndColour, gradientTBottom);

			int top = i * 2;
			int bottom = top + 1;

			vertices[top] = new Vertex
			{
					position = new Vector3(x, 0f, Vertex.nearZ),
					tint = vertexTopTint,
					uv = new Vector2(position, 1f),
			};

			vertices[bottom] = new Vertex
			{
					position = new Vector3(x, rect.height, Vertex.nearZ),
					tint = vertexTopBottom,
					uv = new Vector2(position, 0f),
			};
		}

		MeshWriteData meshData = mgc.Allocate(vertexCount, indexCount, Texture);
		meshData.SetAllVertices(vertices);
		meshData.SetAllIndices(indices);
	}

	private float ComputeGradientT(Vector2 vertexPosition, Rect rect)
	{
		Vector2 start = GetGradientStart(rect);
		Vector2 end = GetGradientEnd(rect);

		Vector2 gradientDir = end - start;
		float lengthSq = gradientDir.sqrMagnitude;
		if (lengthSq < 0.0001f)
			return 0f;

		float t = Vector2.Dot(vertexPosition - start, gradientDir) / lengthSq;
		return Mathf.Clamp01(t);
	}

	private Vector2 GetGradientStart(Rect rect)
	{
		Vector2 center = rect.center;
		float rad = GradientAngle * Mathf.Deg2Rad;

		// Unit direction vector.
		Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
		float maxLength = Mathf.Max(rect.width, rect.height);

		Vector2 start = center + dir * (GradientStartPoint - 0.5f) * maxLength;
		return start;
	}

	private Vector2 GetGradientEnd(Rect rect)
	{
		Vector2 center = rect.center;
		float rad = GradientAngle * Mathf.Deg2Rad;

		Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
		float maxLength = Mathf.Max(rect.width, rect.height);

		Vector2 end = center + dir * (GradientEndPoint - 0.5f) * maxLength;
		return end;
	}
}
}
