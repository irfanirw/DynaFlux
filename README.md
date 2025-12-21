# DynaFlux ZeroTouch (Revit 2025)

This repo contains a minimal Dynamo ZeroTouch plugin scaffold targeting Revit 2025.

## Structure
- `DynaFlux/DynaFlux.csproj`
- `DynaFlux/Nodes/Arithmetic.cs` (first node: `Addition`)

## Next steps
1. Add references to Dynamo assemblies (from your Revit 2025 + Dynamo install):
   - `DynamoCore.dll`
   - `DynamoServices.dll`
2. Build the project.
3. Copy the resulting `DynaFluxCore.dll` to a Dynamo package folder or a location on Dynamo's search path.

If you want, tell me where your Revit/Dynamo install lives and I can wire the references directly.
