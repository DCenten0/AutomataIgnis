<h1 align="center">Fire Cellular Automata for Real-World Fire Spread Simulation</h1>

<p align="center"> <img src="https://github.com/user-attachments/assets/d08483a4-de6a-448d-b550-5eab52a7f729" alt="Fire Spread Simulation" /> </p>

This project introduces a cellular automata-based fire propagation model that simulates wildfire dynamics using image-based terrain input. Developed as part of the "Advanced Simulation and Modeling" course at Rhein-Waal University, the tool integrates environmental factors like wind, humidity, and vegetation flammability to predict fire behavior. The simulation framework enables users to upload terrain images, adjust parameters in real time, and visualize fire spread under diverse conditions.

<p>The full research paper is available <a href="https://github.com/DCenten0/AutomataIgnis/blob/main/Fire_Automata_Paper.pdf" title="Fire Automata Paper">here</a>.</p>

<h2>Key Features</h2>

* **Image-Based Terrain Mapping:** Processes uploaded images to detect vegetation and assign flammability values using HSL color thresholds.
  
* **Dynamic Environmental Factors:**
  * Wind direction and strength bias fire spread probabilities.
  * Humidity reduce the spread probability.
  * Vegetation type and brightness influence flammability.

* **Interactive GUI:** Built with WPF (Windows Presentation Foundation) for real-time parameter adjustment and visualization.

<h2>How It Works</h2>

<h3>Cellular Automata Model</h3> 

The simulation uses a grid-based approach where each cell represents terrain with properties like flammability, vegetation status, and fire state. 

Key code components include:

**Fire_Cell Class (Visual Basic):**

```
Public Class Fire_Cell
    Public Property IsOnFire As Boolean
    Public Property IsVegetation As Boolean
    Public Property Flammability As Double
    Public Property RecSize As Integer
    Public ReadOnly Property CellColor As Brush
End Class
```

**Fire Spread Probability Calculation:**

```
Private Function CalculateSpreadProbability(fromCell As Fire_Cell, toCell As Fire_Cell) As Double
    Dim baseProbability = toCell.Flammability
    Dim windFactor = GetWindFactor(fromCell, toCell)
    Dim humidityEffect = (100 - s1Humidity.Value) / 100
    Return Math.Max(0.0, Math.Min(1.0, baseProbability * windFactor * humidityEffect))
End Function
```

<h2>Application Interface</h2>

<p align="center"> <img src="https://github.com/user-attachments/assets/c89aba5c-c546-4e95-bf27-35c64b62acee" alt="Application Interface" /> </p>

* **User Controls:** Adjust wind direction, humidity, flammability type and cell size.
* **Terrain Image Upload:** Map vegetation using HSL thresholds.
* **Real-Time Visualization:** Watch fire propagate across the grid.

<h2>Results & Validation</h2>

* **Wind Impact:** Fire spread 5x faster in wind-aligned cells.
* **Humidity Dampening:** High humidity reduced ignition rates by 40–60%.
* **Vegetation Sensitivity:** Grassland cells burned 3x faster than urban areas.

**Limitations:**

* Assumes flat terrain (no elevation modeling).
* Simplified wind patterns (no turbulence).

<h2>Getting Started</h2>

<h3>Prerequisites</h3>

* .NET Framework 4.7+
* Visual Studio (for code modification)

<h3>Installation</h3>

1. Clone the repository:
```
git clone https://github.com/DCenten0/AutomataIgnis.git
```

2. Open FireSimulation.sln in Visual Studio.
3. Build and run the project.

<h3>Usage</h3>

1. Upload a terrain image (e.g., PNG/JPG).
2. Adjust wind, humidity, and flammability parameters.
3. Click on "Start" button to start the simulation.

<h2>Future Improvements</h2>

* Integrate elevation data for 3D terrain modeling.
* Add multispectral image support for precise vegetation detection.
