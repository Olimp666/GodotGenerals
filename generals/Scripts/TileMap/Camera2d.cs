using Godot;
using System;
using System.Diagnostics;

namespace TileMap;

public partial class Camera2d : Camera2D
{
    Global globals;

    float moveSpeed = 400.0f;
    float zoomSpeed = 0.1f;
    Vector2 zoomLowest = new Vector2(0.5f, 0.5f);
    Vector2 zoomHighest = new Vector2(1f, 1f);

    Vector2 currentCamCoords;
    bool dragging = false;
    Vector2 lastMousePos = Vector2.Zero;
    public override void _Ready()
    {
        globals = GetNode<Global>("/root/Global");
        currentCamCoords = Position;
    }

    public override void _Process(double delta)
    {
        HandleMovement(delta);
        HandleZoom(delta);
    }

    public override void _Input(InputEvent @event)
    {
        Viewport viewport = GetViewport();
        Vector2 mousePos = viewport.GetMousePosition();
        mousePos -= viewport.GetWindow().Size / 2; //adjust position by window size
        mousePos += currentCamCoords; //adjust position by camera coords

        if (@event is InputEventMouseButton ev && ev.ButtonIndex == MouseButton.Right)
        {
            if (ev.Pressed)
            {
                dragging = true;
                lastMousePos = mousePos;
            }
            else
            {
                currentCamCoords = Position;
                dragging = false;
            }
        }


        if (@event is InputEventMouseMotion mv && dragging)
        {
            var evPos = mv.Position;
            evPos -= viewport.GetWindow().Size / 2;
            evPos += currentCamCoords;

            var delta = evPos - lastMousePos;
            lastMousePos = evPos;
            Position -= delta/Zoom;
        }

    }

    private void HandleMovement(double delta)
    {
        Vector2 direction = Vector2.Zero;

        if (Input.IsActionPressed("ui_up"))
        {
            direction.Y -= 1;
        }
        if (Input.IsActionPressed("ui_down"))
        {
            direction.Y += 1;
        }
        if (Input.IsActionPressed("ui_left"))
        {
            direction.X -= 1;
        }
        if (Input.IsActionPressed("ui_right"))
        {
            direction.X += 1;
        }

        Position += direction.Normalized() * (float)(moveSpeed * delta);
    }

    private void HandleZoom(double delta)
    {
        if (globals.IsOverHUD())
            return;
        if (Input.IsActionJustReleased("zoom_in"))
        {
            if (Zoom < zoomHighest)
                Zoom += new Vector2(0.1f, 0.1f);
        }
        if (Input.IsActionJustReleased("zoom_out"))
        {

            if (Zoom > zoomLowest)
                Zoom -= new Vector2(0.1f, 0.1f);
        }
    }

}
