using Avalonia.Controls;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Projections;
using Mapsui.Tiling;
using Mapsui.UI.Avalonia;
using Mapsui.Widgets.InfoWidgets;

namespace Ambulance;

public partial class MapWindow : Window
{
    public MapWindow()
    {
        InitializeComponent();
        Title = "Карта";
        InitializeMap();
    }

    private void InitializeMap()
    {
        var mapControl = this.FindControl<MapControl>("MapControl");
        if (mapControl is null)
        {
            return;
        }

        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());

        while (map.Widgets.TryDequeue(out _))
        {
        }

        map.Widgets.Add(new MouseCoordinatesWidget());

        mapControl.UseContinuousMouseWheelZoom = false;
        mapControl.Map = map;

        var southWest = SphericalMercator.FromLonLat(37.3193, 55.4899);
        var northEast = SphericalMercator.FromLonLat(37.9674, 55.9576);

        map.Navigator.ZoomToBox(
            new MRect(southWest.x, southWest.y, northEast.x, northEast.y),
            MBoxFit.Fit);
    }
}
