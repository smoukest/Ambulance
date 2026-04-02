using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Projections;
using Mapsui.Tiling;
using Mapsui.UI.Avalonia;
using Mapsui.Widgets.InfoWidgets;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

namespace Ambulance;

public partial class MapWindow : Window
{
    private static readonly HttpClient HttpClient = new();

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
        mapControl.PointerPressed += MapControl_PointerPressed;

        var southWest = SphericalMercator.FromLonLat(37.3193, 55.4899);
        var northEast = SphericalMercator.FromLonLat(37.9674, 55.9576);

        map.Navigator.ZoomToBox(
            new MRect(southWest.x, southWest.y, northEast.x, northEast.y),
            MBoxFit.Fit);
    }

    private async void MapControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not MapControl mapControl || e.ClickCount != 2)
            return;

        var point = e.GetCurrentPoint(mapControl);
        if (!point.Properties.IsLeftButtonPressed || mapControl.Map?.Navigator.Viewport is null)
            return;

        var position = e.GetPosition(mapControl);
        var world = mapControl.Map.Navigator.Viewport.ScreenToWorld(position.X, position.Y);
        var lonLat = SphericalMercator.ToLonLat(world.X, world.Y);

        var address = await GetAddressByCoordinatesAsync(lonLat.lon, lonLat.lat);
        var mainWindow = Owner as MainWindow ?? GetMainWindow();
        mainWindow?.SetAddressFromMap(address);
    }

    private static MainWindow? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return null;

        return desktop.Windows.OfType<MainWindow>().FirstOrDefault(w => w.IsVisible)
               ?? desktop.Windows.OfType<MainWindow>().FirstOrDefault();
    }

    private static async System.Threading.Tasks.Task<string> GetAddressByCoordinatesAsync(double lon, double lat)
    {
        try
        {
            if (!HttpClient.DefaultRequestHeaders.Contains("User-Agent"))
                HttpClient.DefaultRequestHeaders.Add("User-Agent", "AmbulanceApp/1.0");

            var url = $"https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat={lat.ToString(CultureInfo.InvariantCulture)}&lon={lon.ToString(CultureInfo.InvariantCulture)}";
            var json = await HttpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("address", out var address))
            {
                var index = GetAddressPart(address, "postcode");
                var country = GetAddressPart(address, "country");
                var city = GetAddressPart(address, "city", "town", "village", "municipality", "state");
                var district = GetAddressPart(address, "city_district", "suburb", "neighbourhood", "quarter");
                var street = GetAddressPart(address, "road", "pedestrian", "residential", "footway");
                var house = GetAddressPart(address, "house_number", "house");
                var corpus = GetAddressPart(address, "building", "block");

                var parts = new List<string>();
                AddPart(parts, index);
                AddPart(parts, country);
                AddPart(parts, city);
                AddPart(parts, district);
                AddPart(parts, street);

                var housePart = house;
                if (!string.IsNullOrWhiteSpace(corpus))
                    housePart = string.IsNullOrWhiteSpace(housePart) ? $"корпус {corpus}" : $"{housePart}, корпус {corpus}";

                AddPart(parts, housePart);

                if (parts.Count > 0)
                    return string.Join(", ", parts);
            }

            if (doc.RootElement.TryGetProperty("display_name", out var displayName))
                return displayName.GetString() ?? string.Empty;
        }
        catch
        {
        }

        return $"{lat.ToString("F6", CultureInfo.InvariantCulture)}, {lon.ToString("F6", CultureInfo.InvariantCulture)}";
    }

    private static string GetAddressPart(JsonElement address, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (address.TryGetProperty(key, out var value))
            {
                var text = value.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                    return text;
            }
        }

        return string.Empty;
    }

    private static void AddPart(List<string> parts, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            parts.Add(value);
    }
}
