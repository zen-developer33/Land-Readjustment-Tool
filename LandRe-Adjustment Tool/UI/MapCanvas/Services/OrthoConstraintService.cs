using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;

namespace Land_Readjustment_Tool.UI.MapCanvas.Services
{
    public enum OrthoConstraintAxis
    {
        Horizontal,
        Vertical
    }

    public static class OrthoConstraintService
    {
        public static PointD ConstrainToDominantAxis(PointD anchor, PointD candidate)
        {
            return Constrain(anchor, candidate, ResolveDominantAxis(anchor, candidate));
        }

        public static PointD Constrain(PointD anchor, PointD candidate, OrthoConstraintAxis axis)
        {
            return axis == OrthoConstraintAxis.Horizontal
                ? new PointD(candidate.X, anchor.Y)
                : new PointD(anchor.X, candidate.Y);
        }

        public static OrthoConstraintAxis ResolveDominantAxis(PointD anchor, PointD candidate)
        {
            double dx = candidate.X - anchor.X;
            double dy = candidate.Y - anchor.Y;

            return Math.Abs(dx) >= Math.Abs(dy)
                ? OrthoConstraintAxis.Horizontal
                : OrthoConstraintAxis.Vertical;
        }
    }
}
