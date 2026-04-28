# Copilot Instructions

## Project Guidelines
- The project uses a layered architecture with ProjectSettings entity storing canvas configuration (CanvasBackgroundColor, CanvasGridColor as hex strings, CanvasGridVisible). MapCanvasRenderSettings should integrate with ProjectSettings through a dedicated factory method CreateFromProjectSettings() that safely converts hex colors with fallback defaults.