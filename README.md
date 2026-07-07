# DynaFlux

DynaFlux is a Dynamo ZeroTouch library for **Singapore BCA ETTV** calculations. It provides reusable node classes to model building envelope assemblies, surface orientations, and to compute ETTV per facade orientation and for the overall model.

> Target platform: Revit 2025 + Dynamo 3.3 (net8.0-windows)

---

## Features
- **BCA ETTV-aligned workflow** for opaque and fenestration assemblies
- Auto-derived **surface area** and **orientation** from geometry
- **Orientation correction factors** and solar gain factors
- **Per-orientation ETTV** breakdown and **overall average ETTV**

---

## Core Nodes
### Build
- **FluxMaterial** – Material layer with thickness and thermal conductivity
- **FluxConstruction** – Assembly definition with U-value, shading coefficients (`Sc1`, `Sc2`, `ScTot = Sc1 × Sc2`), and auto-inferred type: supplying `sc1` marks it as *Fenestration*, omitting it marks it as *Opaque*
- **FluxSurface** – Geometry + construction + orientation with auto-derived area
- **FluxOrientation** – Orientation name, angle, correction factor
- **FluxModel** – Aggregates surfaces, unique orientations, and constructions; includes `ProjectName` property

### Result
- **FluxOrientationEttvResult** – ETTV components per orientation (opaque conduction, fenestration conduction, fenestration radiation, gross heat gain, WWR, area breakdown, unique constructions)
- **FluxModelEttvResult** – ETTV per orientation + average ETTV

### Report
- **EttvReport.GenerateReport** – Generates a self-contained HTML ETTV report (with Chart.js charts) saved alongside the active `.dyn` script; pass `run = true` to execute

---

## ETTV Formula (BCA)

$$
	ext{ETTV} = \frac{12\sum(A_{w}U_{w})}{A_{w}} + \frac{3.4\sum(A_{f}U_{f})}{A_{o}} + \frac{211\sum(A_{f}SC_{f})\,CF}{A_{o}}
$$

Where:
- $A_{w}$ = opaque wall area
- $A_{f}$ = fenestration area
- $A_{o} = A_{w} + A_{f}$ = overall envelope area
- $U_{w}$ / $U_{f}$ = U-value of wall / fenestration
- $SC_{f}$ = shading coefficient for fenestration
- $CF$ = orientation correction factor

---

## Project Structure
- **DynaFlux/DynaFlux.csproj** – ZeroTouch library
- **DynaFlux/Nodes/** – Core classes and ETTV nodes
- **libs/** – Dynamo/ProtoGeometry references
- **scripts/** – Small inspection apps for local testing

---

## Build
1. Open **DynaFlux.sln** in Visual Studio 2022+
2. Ensure Dynamo assemblies are referenced:
   - `DynamoCore.dll`
   - `DynamoServices.dll`
   - `ProtoGeometry.dll`
3. Build the solution

The build outputs:
- `DynaFlux/bin/net8.0-windows/DynaFlux.dll`

---

## Usage (Dynamo)
1. Load `DynaFlux.dll` into Dynamo (package folder or Load Library)
2. Create materials and constructions
3. Create surfaces from geometry
4. Build a `FluxModel` (set `ProjectName` for report labelling)
5. Compute results with `FluxModelEttvResult`
6. Generate an HTML report with `EttvReport.GenerateReport` (set `run = true`)

---

## Reference
- Singapore BCA RETV Standard (ref: `DynaFlux/ref/retv.pdf`)

---

## License
Internal project use (update if a public license is required)
