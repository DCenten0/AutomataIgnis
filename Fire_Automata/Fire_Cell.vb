Imports System.ComponentModel
Imports System.Runtime.CompilerServices

Public Class Fire_Cell

    Implements INotifyPropertyChanged

    Private _isOnFire As Boolean
    Private _isVegetation As Boolean
    Private _flammability As Double = 0.7
    Private _cellColor As Brush
    Private _recSize As Integer

    Public Property Row As Integer

    Public Property Col As Integer

    Public Property RecSize As Integer
        Get
            Return _recSize
        End Get
        Set
            If _recSize <> Value Then
                _recSize = Value
                OnPropertyChanged("RecSize")
            End If

        End Set
    End Property

    Public Property IsOnFire As Boolean
        Get
            Return _isOnFire
        End Get
        Set
            If _isVegetation = True AndAlso _isOnFire <> Value Then
                _isOnFire = Value
                OnPropertyChanged("IsOnFire")
            End If
        End Set
    End Property

    Public Property IsVegetation As Boolean
        Get
            Return _isVegetation
        End Get
        Set
            If _isVegetation <> Value Then
                _isVegetation = Value
                OnPropertyChanged("IsVegetation")
            End If
        End Set
    End Property

    Public Property Flammability As Double
        Get
            Return _flammability
        End Get
        Set
            If Value <> _flammability Then
                _flammability = Value
                OnPropertyChanged("Flammability")
            End If

        End Set
    End Property

    Public ReadOnly Property CellColor As Brush
        Get
            If IsVegetation AndAlso Not IsOnFire Then
                Dim greenIntensity = Math.Max(0.0, Math.Min(255, 255 * (Flammability + 0.15)))
                Return New SolidColorBrush(Color.FromRgb(0, greenIntensity, 0))
            End If

            If IsVegetation AndAlso IsOnFire Then
                Dim FireColor = New SolidColorBrush(Color.FromRgb(255, 51, 51))
                Return FireColor
            End If

            Return Brushes.Black
        End Get
    End Property

    Public Property Rec As New Rectangle With {
        .Width = RecSize,
        .Height = RecSize,
        .Stroke = Brushes.Gray,
        .StrokeThickness = 0.5,
        .Fill = CellColor,
        .Tag = Me}

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Protected Sub OnPropertyChanged(<CallerMemberName> Optional PropertyName As String = Nothing)

        Select Case PropertyName
            Case "RecSize"
                Rec.Width = RecSize
                Rec.Height = RecSize
                OnPropertyChanged("Rec")
            Case "IsOnFire", "IsVegetation", "Flammability"
                Rec.Fill = CellColor
                OnPropertyChanged("Rec")
        End Select

        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(PropertyName))
    End Sub

End Class
