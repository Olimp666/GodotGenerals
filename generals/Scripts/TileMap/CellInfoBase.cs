using Godot;
using System;
using System.Collections.Generic;

public partial class CellInfoBase : PanelContainer
{
    public Vector2I unitCoords { get; set; }
    public int UnitId { get; set; }
    public TextureButton MoveButton { get; set; }
    public TextureButton.ToggledEventHandler Handler { get; set; }
    public void Setup(Vector2I coords, int health, int maxHealth, int id, List<Vector3I> atlasCoords, string name)
    {
        unitCoords = coords;
        UnitId = id;

        Label nameLabel = GetNode<Label>("MarginContainer/UnitInfoBase/UnitInfo/Label");
        Label healthLabel = GetNode<Label>("MarginContainer/UnitInfoBase/Control/HealthLabel");
        TextureProgressBar healthbar = GetNode<TextureProgressBar>("MarginContainer/UnitInfoBase/Control/HpFill");
        nameLabel.Text = $"{name}";
        healthLabel.Text = $"{health}/{maxHealth}";
        healthbar.Value = (double)health/maxHealth*100;

        MoveButton = GetNode<TextureButton>("MarginContainer/UnitInfoBase/UnitCommands/MoveButton");



        CreateTexture(atlasCoords);
    }
    private void CreateTexture(List<Vector3I> atlasCoords)
    {
        TextureRect unitIcon = GetNode<TextureRect>("MarginContainer/UnitInfoBase/UnitInfo/TextureRect");

        var tileSet = GD.Load<TileSet>("res://Assets/tile_set.tres");
        var _ = tileSet.GetSource(0) as TileSetAtlasSource;
        var reg = _.GetTileTextureRegion(new Vector2I(0, 0));
        Image tileImage = Image.CreateEmpty(reg.Size.X, reg.Size.Y, false, Image.Format.Rgba8);
        for (int i = 0; i < atlasCoords.Count; i++)
        {
            var source = tileSet.GetSource(atlasCoords[i].X) as TileSetAtlasSource;
            var atlasTexture = source.GetTexture();
            var region = source.GetTileTextureRegion(new Vector2I(atlasCoords[i].Y, atlasCoords[i].Z));
            Image baseImage = atlasTexture.GetImage();
            //tileImage.BlitRect(baseImage, region, Vector2I.Zero);
            AlphaBlit(tileImage, baseImage, region);
        }

        unitIcon.Texture = ImageTexture.CreateFromImage(tileImage);
    }
    private void AlphaBlit(Image dest, Image src, Rect2I region)
    {
        Vector2I size = dest.GetSize();

        int offsetX = (size.X - region.Size.X) / 2;
        int offsetY = (size.Y - region.Size.Y) / 2;

        for (int y = 0; y < region.Size.Y; y++)
        {
            for (int x = 0; x < region.Size.X; x++)
            {
                Color srcColor = src.GetPixel(x + region.Position.X, y + region.Position.Y);
                if (srcColor.A > 0.0)
                {
                    if (x + offsetX > size.X || y + offsetY > size.Y)
                        continue;
                    dest.SetPixel(x + offsetX, y + offsetY, srcColor);
                }
            }
        }
    }
}
