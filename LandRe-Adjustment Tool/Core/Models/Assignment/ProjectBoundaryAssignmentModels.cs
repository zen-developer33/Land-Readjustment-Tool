using NetTopologySuite.Geometries;

namespace Land_Readjustment_Tool.Core.Models.Assignment
{
    public sealed record ProjectBoundaryAssignmentCandidate(
        Guid CanvasObjectId,
        int CanvasLayerId,
        string LayerName,
        string LayerGroupName,
        string ObjectType,
        Envelope BoundingBox,
        string DisplayName);

    public sealed record ProjectBoundaryAssignmentResult(
        bool Success,
        string? ErrorMessage,
        int ObjectsCreated,
        int ObjectsRemoved,
        Envelope? BoundingBox);
}
