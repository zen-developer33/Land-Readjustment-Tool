namespace Land_Readjustment_Tool.Core.Entities.Roads
{
    public enum DonutValidationStatus
    {
        NotChecked,
        Valid,
        InvalidGeometry,
        WrongWindingDirection,
        HoleOutsideExterior,
        HolesOverlap,
        HoleAreaExceedsParcel
    }
}
