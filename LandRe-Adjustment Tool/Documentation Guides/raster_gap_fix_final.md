# RasterRenderLayer.cs — 3 Exact Changes to Remove Tile Gaps

---

## Change 1 — Line 1106 in `AlignDestinationToPixelGrid`

Find this exact line:
```csharp
return RectangleF.FromLTRB(left - 0.5f, top - 0.5f, right + 0.5f, bottom + 0.5f);
```

Replace with:
```csharp
return RectangleF.FromLTRB(left, top, right, bottom);
```

---

## Change 2 — Line 174 in `RenderVisible`

Find this exact line:
```csharp
graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
```

Replace with:
```csharp
graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
```

---

## Change 3 — Lines 757–770 — Replace the entire `DrawBitmap` method body

Find the entire method:
```csharp
private void DrawBitmap(
    Graphics graphics,
    Bitmap bitmap,
    RectangleF destination)
{
    if (_opacityImageAttributes == null)
    {
        graphics.DrawImage(bitmap, destination);
        return;
    }

    Rectangle destinationRectangle = Rectangle.Round(destination);
    graphics.DrawImage(
        bitmap,
        destinationRectangle,
        0,
        0,
        bitmap.Width,
        bitmap.Height,
        GraphicsUnit.Pixel,
        _opacityImageAttributes);
}
```

Replace with:
```csharp
private void DrawBitmap(
    Graphics graphics,
    Bitmap bitmap,
    RectangleF destination)
{
    Rectangle dest = Rectangle.FromLTRB(
        (int)Math.Floor(destination.Left),
        (int)Math.Floor(destination.Top),
        (int)Math.Ceiling(destination.Right),
        (int)Math.Ceiling(destination.Bottom));

    System.Drawing.Imaging.ImageAttributes attribs =
        _opacityImageAttributes ?? new System.Drawing.Imaging.ImageAttributes();
    bool ownedAttribs = _opacityImageAttributes == null;

    try
    {
        attribs.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
        graphics.DrawImage(
            bitmap,
            dest,
            0, 0, bitmap.Width, bitmap.Height,
            GraphicsUnit.Pixel,
            attribs);
    }
    finally
    {
        if (ownedAttribs)
            attribs.Dispose();
    }
}
```

---

## That is all. Do not change anything else.
