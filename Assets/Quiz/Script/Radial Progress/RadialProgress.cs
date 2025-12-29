using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

// An element that displays progress inside a partially filled circle
[UxmlElement]
public partial class RadialProgress : VisualElement
{
    // USS class names
    public static readonly string ussClassName = "radial-progress";
    public static readonly string ussLabelClassName = "radial-progress__label";

    // Custom USS properties
    static readonly CustomStyleProperty<Color> s_TrackColor =
        new CustomStyleProperty<Color>("--track-color");

    static readonly CustomStyleProperty<Color> s_ProgressColor =
        new CustomStyleProperty<Color>("--progress-color");

    // Meshes
    EllipseMesh m_TrackMesh;
    EllipseMesh m_ProgressMesh;

    // Label
    Label m_Label;

    const int k_NumSteps = 200;

    float m_Progress;
    float m_MaximumValue = 10f;

    // ---- UXML-exposed attributes ----

    [UxmlAttribute("progress")]
    public float progress
    {
        get => m_Progress;
        set
        {
            m_Progress = value;
            MarkDirtyRepaint();
        }
    }

    [UxmlAttribute("max-value")]
    public float maximumValue
    {
        get => m_MaximumValue;
        set
        {
            m_MaximumValue = value;
            MarkDirtyRepaint();
        }
    }

    // ---- Public API ----

    public void SetText(string text)
    {
        m_Label.text = text;
    }

    // ---- Constructor ----

    public RadialProgress()
    {
        m_Label = new Label();
        m_Label.AddToClassList(ussLabelClassName);
        hierarchy.Add(m_Label);

        m_ProgressMesh = new EllipseMesh(k_NumSteps);
        m_TrackMesh = new EllipseMesh(k_NumSteps);

        AddToClassList(ussClassName);

        RegisterCallback<CustomStyleResolvedEvent>(OnCustomStylesResolved);
        generateVisualContent += GenerateVisualContent;

        progress = 0f;
    }

    // ---- Styling ----

    void OnCustomStylesResolved(CustomStyleResolvedEvent evt)
    {
        UpdateCustomStyles();
    }

    void UpdateCustomStyles()
    {
        if (customStyle.TryGetValue(s_ProgressColor, out var progressColor))
            m_ProgressMesh.color = progressColor;

        if (customStyle.TryGetValue(s_TrackColor, out var trackColor))
            m_TrackMesh.color = trackColor;

        if (m_ProgressMesh.isDirty || m_TrackMesh.isDirty)
            MarkDirtyRepaint();
    }

    // ---- Rendering ----

    void GenerateVisualContent(MeshGenerationContext context)
    {
        DrawMeshes(context);
    }

    void DrawMeshes(MeshGenerationContext context)
    {
        float halfWidth = contentRect.width * 0.5f;
        float halfHeight = contentRect.height * 0.5f;

        if (halfWidth < 2f || halfHeight < 2f)
            return;

        m_ProgressMesh.width = halfWidth;
        m_ProgressMesh.height = halfHeight;
        m_ProgressMesh.borderSize = 10;
        m_ProgressMesh.UpdateMesh();

        m_TrackMesh.width = halfWidth;
        m_TrackMesh.height = halfHeight;
        m_TrackMesh.borderSize = 10;
        m_TrackMesh.UpdateMesh();

        // Track
        var trackData =
            context.Allocate(m_TrackMesh.vertices.Length, m_TrackMesh.indices.Length);
        trackData.SetAllVertices(m_TrackMesh.vertices);
        trackData.SetAllIndices(m_TrackMesh.indices);

        float clampedProgress = Mathf.Clamp(m_Progress, 0f, m_MaximumValue);
        int sliceSize = Mathf.FloorToInt(k_NumSteps * clampedProgress / m_MaximumValue);

        if (sliceSize == 0)
            return;

        sliceSize *= 6;

        var progressData =
            context.Allocate(m_ProgressMesh.vertices.Length, sliceSize);
        progressData.SetAllVertices(m_ProgressMesh.vertices);

        using var tempIndices =
            new NativeArray<ushort>(m_ProgressMesh.indices, Allocator.Temp);
        progressData.SetAllIndices(tempIndices.Slice(0, sliceSize));
    }
}
