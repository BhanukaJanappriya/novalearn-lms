namespace NovaLearn.Domain.Content;

/// <summary>The kind of material a lesson delivers, which decides how it is rendered.</summary>
public enum LessonType
{
    /// <summary>A hosted or embedded video, addressed by <c>ContentUrl</c>.</summary>
    Video,

    /// <summary>A downloadable PDF, addressed by <c>ContentUrl</c>.</summary>
    Pdf,

    /// <summary>Inline rich text held in <c>TextContent</c>.</summary>
    Text,

    /// <summary>An external resource, addressed by <c>ContentUrl</c>.</summary>
    Link
}
