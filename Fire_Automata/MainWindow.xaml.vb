Imports System.Windows.Threading
Imports Microsoft.Win32
Imports System.Drawing

Class MainWindow

    Private ReadOnly Property RecSize As Integer
        Get
            Return slResize.Value
        End Get
    End Property

    Private LastRecSize As Integer

    Const MaxRecSize As Integer = 30
    Const MinRecSize As Integer = 4

    Private WidthBoard As Integer
    Private HeightBoard As Integer
    Private SimuBoard As Shapes.Rectangle(,)

    Private ImagePath As String

    Private IsLeftMouseDown As Boolean = False
    Private IsRightMouseDown As Boolean = False

    Private ReadOnly CardinalPoints As Dictionary(Of String, (Integer, Integer)) = New Dictionary(Of String, (Integer, Integer)) From {
        {"N", (-1, 0)},
        {"NE", (-1, 1)},
        {"E", (0, 1)},
        {"SE", (1, 1)},
        {"S", (1, 0)},
        {"SW", (1, -1)},
        {"W", (0, -1)},
        {"NW", (-1, -1)}
    }

    Private ReadOnly Flammabilitype As Dictionary(Of String, Double) = New Dictionary(Of String, Double) From {
        {"Grassland - 0.7", 0.7},
        {"Shrub - 0.6", 0.6},
        {"Coniferous Forest - 0.5", 0.5},
        {"Deciduous Forest - 0.4", 0.4},
        {"Urban - 0.2", 0.2}
    }

    Private IsSimulationStarted As Boolean = False
    Private SimulationDelay As Integer = 64
    Private SimulationTimer As DispatcherTimer
    Private IsEditing As Boolean = False


    Private SimuBoardCopy As Shapes.Rectangle(,)
    Private CanvasCopy As Canvas

    Private ActiveFires As New List(Of Fire_Cell)


#Region "Properties"

#End Region

#Region "Events"

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        cbFlammabilityType.ItemsSource = Flammabilitype
        cbFlammabilityType.SelectedIndex = 0
        cbFlammabilityType.DisplayMemberPath = "Key"

        cbWindDir.ItemsSource = CardinalPoints
        cbWindDir.SelectedIndex = 0
        cbWindDir.DisplayMemberPath = "Key"

        slResize.Maximum = MaxRecSize
        slResize.TickFrequency = 2
        slResize.Minimum = MinRecSize
        slResize.Value = 10
        LastRecSize = RecSize
        CreateBoard()
    End Sub

    Private Sub btnOpenFile_Click(sender As Object, e As RoutedEventArgs) Handles btnOpenFile.Click
        Dim openFileDialog As New OpenFileDialog With {
           .Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff"
        }

        StopSimulation()

        If openFileDialog.ShowDialog() = True Then
            ImagePath = openFileDialog.FileName
            LoadImageOnBoard()
            ActiveFires.Clear()
        End If
    End Sub

    Private Sub btnApplySettings_Click(sender As Object, e As RoutedEventArgs) Handles btnApplySettings.Click
        If LastRecSize <> RecSize Then
            StopSimulation()
            LoadImageOnBoard()
            LastRecSize = RecSize
            ActiveFires.Clear()
        End If
    End Sub

    Private Sub Left_MouseDown(sender As Object, e As MouseButtonEventArgs)
        Dim cell = TryCast(TryCast(sender, Shapes.Rectangle).Tag, Fire_Cell)
        If cell IsNot Nothing AndAlso Not cell.IsOnFire Then
            cell.IsOnFire = True
            ActiveFires.Add(cell)
        End If
    End Sub

    Private Sub Right_MouseDown(sender As Object, e As MouseButtonEventArgs)
        Dim cell = TryCast(TryCast(sender, Shapes.Rectangle).Tag, Fire_Cell)
        If cell IsNot Nothing AndAlso cell.IsOnFire Then
            cell.IsOnFire = False
            ActiveFires.Remove(cell)
        End If
    End Sub

    Private Sub btnStart_Click(sender As Object, e As RoutedEventArgs) Handles btnStart.Click
        StartSimulation()
    End Sub

    Private Sub btnStop_Click(sender As Object, e As RoutedEventArgs) Handles btnStop.Click
        StopSimulation()
    End Sub

    Private Sub btnReset_Click(sender As Object, e As RoutedEventArgs) Handles btnReset.Click
        ActiveFires.Clear
        StopSimulation()
        LoadImageOnBoard()
    End Sub

    Private Sub btnApplyFam_Click(sender As Object, e As RoutedEventArgs) Handles btnApplyFam.Click
        StopSimulation()
        ApplyFlamability()
    End Sub

#End Region

#Region "Methods"

    Sub GetCanvasSize()
        WidthBoard = CInt(Math.Floor(cvFire_Automata.ActualWidth))
        HeightBoard = CInt(Math.Floor(cvFire_Automata.ActualHeight))
    End Sub

    Sub CreateBoard()
        cvFire_Automata.Children.Clear()
        GetCanvasSize()
        SimuBoard = New Shapes.Rectangle(HeightBoard, WidthBoard) {}
    End Sub

    Private Sub LoadImageOnBoard()

        If ImagePath Is Nothing Or ImagePath = "" Then Return

        If cvFire_Automata.Visibility = Visibility.Hidden Then cvFire_Automata.Visibility = Visibility.Visible
        If tbopenfile.Visibility = Visibility.Visible Then tbopenfile.Visibility = Visibility.Collapsed

        Dim bitmap As New Bitmap(ImagePath)

        cvFire_Automata.Children.Clear()

        Try

            Dim myBitmapImage As New BitmapImage()

            ' BitmapImage.UriSource must be in a BeginInit/EndInit block
            myBitmapImage.BeginInit()
            myBitmapImage.UriSource = New Uri(ImagePath)

            ' Resize the image to match the grid dimensions
            Dim resizedBitmap As New Bitmap(bitmap, New Size(WidthBoard \ RecSize, HeightBoard \ RecSize))

            ' Map image pixels to grid cells
            For i = 0 To resizedBitmap.Height - 1
                For j = 0 To resizedBitmap.Width - 1
                    Dim pixelColor = resizedBitmap.GetPixel(j, i)

                    ' Use pixel color to set cell properties
                    Dim cell As Fire_Cell = New Fire_Cell With {
                        .RecSize = RecSize,
                        .IsVegetation = False,
                        .Col = j,
                        .Row = i
                    }

                    If IsVegetation(pixelColor) Then
                        cell.IsVegetation = True
                        cell.IsOnFire = False
                        cell.Flammability = GetFlammability(pixelColor)
                    End If

                    AddHandler cell.Rec.MouseLeftButtonDown, AddressOf Left_MouseDown
                    AddHandler cell.Rec.MouseRightButtonDown, AddressOf Right_MouseDown

                    SimuBoard(i, j) = cell.Rec
                    Canvas.SetTop(cell.Rec, i * RecSize)
                    Canvas.SetLeft(cell.Rec, j * RecSize)
                    cvFire_Automata.Children.Add(cell.Rec)

                Next
            Next

            myBitmapImage.EndInit()
            imgContainer.Source = myBitmapImage

        Catch ex As Exception
            MessageBox.Show("Error Opening file: " + ex.ToString, "Error", MessageBoxButton.OK)
            cvFire_Automata.Visibility = Visibility.Hidden
            tbopenfile.Visibility = Visibility.Visible
        End Try

    End Sub

    Private Function GetFlammability(pixelColor As Color) As Double
        Dim brightness As Double = pixelColor.GetBrightness()
        Dim baseFlammability As Double = CType(cbFlammabilityType.SelectedItem, KeyValuePair(Of String, Double)).Value
        Return Math.Max(0.0, Math.Min(1.0, brightness * baseFlammability))
    End Function

    Private Function GetWindDir() As (Integer, Integer)
        Return CType(cbWindDir.SelectedItem, KeyValuePair(Of String, (Integer, Integer))).Value
    End Function

    Private Function IsVegetation(color As Color) As Boolean
        Dim hue As Double = color.GetHue()
        Dim saturation As Double = color.GetSaturation()
        Dim brightness As Double = color.GetBrightness()

        'Vegetation: Green hues are between 60-215, with sufficient saturation And brightness saturation = 0.09 And 0.6 brigthness 0.05 And 0.59
        Return hue >= 60 AndAlso hue <= 210 AndAlso saturation >= 0.09 AndAlso saturation <= 0.6 AndAlso brightness >= 0.05 AndAlso brightness <= 0.59

    End Function

    Private Sub SpreadFire()
        Dim newFires As New List(Of Fire_Cell)

        ' Process only active fire cells
        For Each currentCell In ActiveFires
            Dim neighbors = GetNeighbors(currentCell)

            For Each neighbor In neighbors
                If neighbor Is Nothing OrElse Not neighbor.IsVegetation OrElse neighbor.IsOnFire Then Continue For
                Dim spreadProbability = CalculateSpreadProbability(currentCell, neighbor)
                If Rnd() < spreadProbability Then newFires.Add(neighbor)
            Next
        Next

        ' Update fire status and active fire list
        For Each cell In newFires
            cell.IsOnFire = True
            ActiveFires.Add(cell)
        Next

    End Sub

    Private Function GetNeighbors(cell As Fire_Cell) As List(Of Fire_Cell)
        Dim neighbors As New List(Of Fire_Cell)

        For Each offset In CardinalPoints.Values
            Dim newRow = cell.Row + offset.Item1
            Dim newCol = cell.Col + offset.Item2

            If newRow >= 0 AndAlso newRow < SimuBoard.GetLength(0) AndAlso
           newCol >= 0 AndAlso newCol < SimuBoard.GetLength(1) Then

                Dim neighbor = SimuBoard(newRow, newCol)
                If neighbor IsNot Nothing AndAlso neighbor.Tag IsNot Nothing Then
                    neighbors.Add(TryCast(neighbor.Tag, Fire_Cell))
                End If
            End If
        Next

        Return neighbors
    End Function

    Private Function CalculateSpreadProbability(fromCell As Fire_Cell, toCell As Fire_Cell) As Double
        Dim baseProbability = toCell.Flammability

        ' Adjust for wind direction
        Dim windFactor = GetWindFactor(fromCell, toCell)
        baseProbability *= windFactor

        ' Adjust for humidity
        Dim humidityEffect = (100 - slHumidity.Value) / 100 ' Lower humidity increases probability
        baseProbability *= humidityEffect

        Return Math.Max(0.0, Math.Min(1.0, baseProbability))
    End Function

    Private Function GetWindFactor(fromCell As Fire_Cell, toCell As Fire_Cell) As Double
        Dim windOffset = GetWindDir()
        Dim dx = toCell.Row - fromCell.Row
        Dim dy = toCell.Col - fromCell.Col

        ' Calculate alignment with wind direction
        If dx = windOffset.Item1 AndAlso dy = windOffset.Item2 Then
            Return 5.0 ' Stronger influence in the exact wind direction
        ElseIf (Math.Sign(dx) = Math.Sign(windOffset.Item1) AndAlso Math.Sign(dy) = Math.Sign(windOffset.Item2)) Then
            Return 2.5 ' Diagonal or near wind direction
        ElseIf dx = 0 AndAlso dy = 0 Then
            Return 1.0 ' No movement, no wind effect
        ElseIf Math.Abs(dx - windOffset.Item1) <= 1 OrElse Math.Abs(dy - windOffset.Item2) <= 1 Then
            Return 1.2 ' Slight boost for neighboring cells
        Else
            Return 0.8 ' Reduced probability for opposing wind direction
        End If
    End Function

    Private Sub StartSimulation()
        If Not IsSimulationStarted Then
            SimulationTimer = New DispatcherTimer With {.Interval = New TimeSpan(0, 0, 0, 0, SimulationDelay)}
            AddHandler SimulationTimer.Tick, AddressOf DispatcherTimer_Tick
            SimulationTimer.Start()
            IsSimulationStarted = True
            btnStop.IsEnabled = True
            btnStart.IsEnabled = False
        End If
    End Sub

    Private Sub DispatcherTimer_Tick(sender As Object, e As EventArgs)
        If IsSimulationStarted Then SpreadFire()
    End Sub

    Private Sub StopSimulation()
        If IsSimulationStarted Then
            SimulationTimer.Stop()
            IsSimulationStarted = False
            btnStart.IsEnabled = True
            btnStop.IsEnabled = False
        End If
    End Sub

    Private Sub ApplyFlamability()

        Dim bitmap As New Bitmap(ImagePath)
        Dim myBitmapImage As New BitmapImage()

        ' BitmapImage.UriSource must be in a BeginInit/EndInit block
        myBitmapImage.BeginInit()
        myBitmapImage.UriSource = New Uri(ImagePath)

        ' Resize the image to match the grid dimensions
        Dim resizedBitmap As New Bitmap(bitmap, New Size(WidthBoard \ LastRecSize, HeightBoard \ LastRecSize))

        ' Map image pixels to grid cells
        For i = 0 To resizedBitmap.Height - 1
            For j = 0 To resizedBitmap.Width - 1
                Dim pixelColor = resizedBitmap.GetPixel(j, i)
                Dim currentCell = TryCast(SimuBoard(i, j).Tag, Fire_Cell)
                If currentCell Is Nothing Then Continue For
                currentCell.Flammability = GetFlammability(pixelColor)
            Next
        Next

    End Sub

#End Region

End Class
