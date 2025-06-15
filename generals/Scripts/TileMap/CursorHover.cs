using Godot;
using System;

public partial class CursorHover : Node
{
    VBoxContainer coordsBox;
    TileMapLayer borderLayer;
    Camera2D camera;
    Vector2I? currentTile = null;
    public override void _Ready()
    {
        borderLayer = GetParent<TileMapLayer>();
        camera = GetNode<Camera2D>("/root/RootNode/Camera2D");
        coordsBox = GetNode<VBoxContainer>("/root/RootNode/HUD/CoordsWindow/CoordsBox");
        borderLayer.Connect(SignalName.Ready, new Callable(this, nameof(_OnParentReady)));
    }
    void _OnParentReady()
    {

    }

    public override void _Process(double delta)
    {
        Viewport viewport = GetViewport();
        Vector2 mousePos = viewport.GetMousePosition();
        mousePos -= viewport.GetWindow().Size / 2; //adjust position by window size
        mousePos /= camera.Zoom; //adjust position by camera zoom
        mousePos += camera.Position; //adjust position by camera coords
        Vector2I tilePos = borderLayer.LocalToMap(mousePos);

        var data = borderLayer.GetCellTileData(tilePos);

        if (data == null)
            return;

        if (currentTile == null || tilePos != currentTile)
        {
            if (currentTile != null && borderLayer.GetCellAtlasCoords(currentTile.Value)[0] != 4)
            {
                OnMouseExitedTile(currentTile.Value);
            }
            currentTile = tilePos;
            if (data != null && borderLayer.GetCellAtlasCoords(tilePos)[0] != 4)
                OnMouseEnteredTile(tilePos);
        }
    }

    private void OnMouseEnteredTile(Vector2I tileCoords)
    {
        borderLayer.SetCell(tileCoords, 1, new Vector2I(2, 0));
        UpdateHUD(tileCoords);
    }

    private void OnMouseExitedTile(Vector2I tileCoords)
    {
        borderLayer.SetCell(tileCoords, 1, new Vector2I(1, 0));
    }

    void UpdateHUD(Vector2I coords)
    {
        Label xLabel = coordsBox.GetChild<Label>(0);
        Label yLabel = coordsBox.GetChild<Label>(1);

        xLabel.Text = $"X: {coords[0]}";
        yLabel.Text = $"Y: {coords[1]}";
    }
}
