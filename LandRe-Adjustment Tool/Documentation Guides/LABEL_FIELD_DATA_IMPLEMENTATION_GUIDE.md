# Label Field Data Implementation Guide

This guide records the rule for every layer label field in RePlot:

Every field shown in the layer label UI must be backed by real data in both the preview editor and the map renderer. Do not add a field to the UI unless the sample-record builder and renderer can resolve it from the same object data path.

## Required Data Flow

Layer labels are configured from the layer property manager and edited in `frmLabelExpressionEditor`.

Live preview data is built by `frmMain.GetLayerSampleRecordsAsync`. It must load the canvas object plus all domain links needed by labels:

- `CanvasLayer`
- `BaselineParcel`, `LandOwner`, `MalpotReference`
- `Road`
- `Block`
- `ReplottedParcel`, `ReplottedParcel.Block`, `ReplottedParcel.PlotType`
- source metadata attributes from `CadastralCanvasMetadata.AttributesJson`

Map rendering resolves label text in `CanvasVectorRenderer.ResolveLabelFieldValue`. Whenever a new field is added to the editor list, add the matching resolver branch there too.

Shared canvas object repository queries must hydrate the same navigation data for rendering and property refresh paths.

Important: road, block, baseline parcel, and replotted parcel assignments may be present on the scalar canvas object fields (`RoadId`, `BlockId`, `BaselineParcelId`, `ReplottedParcelId`) even when EF one-to-one navigation includes are null. Label sample and render paths must explicitly hydrate from these scalar IDs. Do not rely only on `.Include(item => item.Road)` or `.Include(item => item.Block)` for assigned layer labels.

## Field Groups

Original/cadastral parcel labels:

- `ParcelNo`, `MapSheetNo`, `FullUniqueParcelCode`
- `OwnerName`, `OwnerFatherSpouse`, `OwnershipType`, `HasTenant`, `TenantName`
- `AreaSqm`, `AreaRAPD`, `AreaBKD`, `FieldMeasuredAreaSqm`, `EffectiveAreaSqm`, `CalculatedAreaSqm`, `Perimeter`
- `Province`, `District`, `Municipality`, `WardNo`, `LandUse`, `MothNo`, `PaanaNo`, `AssignmentStatus`

Road labels:

- `RoadName`, `RoadCode`, `RoadStatus`, `RoadType`, `SurfaceType`
- `RoadWidth`, `RightOfWayWidth`, `Length`, `RoadDescription`

Block labels:

- `BlockName`, `BlockCode`, `BlockLandUse`, `BlockDepth`, `BlockDepthGeometry`
- `BlockAreaSqm`, `BlockAreaRAPD`, `BlockAreaBKD`, `Perimeter`, `BlockDescription`

Replotted parcel labels:

- `ReplottedParcelNo`, `SystemGeneratedNumber`, `DerivedNumber`, `BlockSequenceNumber`
- `PlotTypeName`, `PlotBlockName`
- `PlotAreaSqm`, `PlotAreaRAPD`, `PlotAreaBKD`, `Perimeter`, `PlotNotes`

Generic, drawing, markup, and external layer labels:

- `LabelText`, `ObjectDescription`, `ObjectType`, `LayerName`
- `CalculatedAreaSqm`, `AreaRAPD`, `AreaBKD`, `Perimeter`, `Length`, `X`, `Y`
- `SourceLayer`, `SourceFileName`, `SourceFormat`, `MatchedText`, `Id`
- any non-empty source attribute keys found in the sampled layer objects

## Implementation Rule

When a new layer/domain label field is needed:

1. Add it to `frmLabelExpressionEditor.GetAvailableFields`.
2. Add preview sample data in `frmMain.GetLayerSampleRecordsAsync`.
3. Add map render resolution in `CanvasVectorRenderer.ResolveLabelFieldValue`.
4. Ensure the relevant repository/query includes the needed navigation properties.
5. Build the project and verify the preview does not show `NO DATA AVAILABLE` when any sampled layer object has the requested data.

The label editor should only show `NO DATA AVAILABLE` after checking sampled layer objects for a record that can satisfy the expression. A single unassigned or blank object must not make the whole layer preview fail.
