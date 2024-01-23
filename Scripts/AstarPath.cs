using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class AstarPath : Node2D
{
	[Export]
	public bool VisualizeAstarPath;
	[Export]
	public bool UseDiagonalPathFinding = false;
	public Godot.Collections.Array NonWalkableTiles;
	public List<Vector2> PathNodeList = new List<Vector2>();
	public TileMap TileMapWorld;
	private AStar2D AStar2DPath = new AStar2D();
	private Vector2 PathStarPos = new Vector2();
	private Vector2 PathEndPos = new Vector2();
	private Vector2 MapSize = new Vector2();
	public override void _Ready()
	{
        TopLevel = true;
    }

    private void GetTileMapBounds()
	{
        Rect2I mapSize = TileMapWorld.GetUsedRect();
		MapSize = new Vector2(mapSize.Size.X, mapSize.Size.Y);
	}

	private bool IsTileOutsideTheMap(Vector2 tile)
	{
		if(tile.X < 0 || tile.Y < 0 || tile.X >= MapSize.X || tile.Y >= MapSize.Y)
		{
			return true;
		}
        return false;
    }

	private int CalculateUniqueTileIdentifier(Vector2 tile)
	{
        return (int)((tile.Y * MapSize.X) + tile.X);
	}

	private void SetPathStartPosition(Vector2I value)
	{

		if (NonWalkableTiles.Contains(value))
		{
            GD.Print("Path start position is non-walkable.");
            return;
		}
		if(IsTileOutsideTheMap(value))
		{
            GD.Print("Path start position is outside the map bounds.");
            return;
		}
		PathStarPos = value;
        GD.Print($"Path start position set to {PathStarPos}.");

        if (PathEndPos != null && PathEndPos != PathStarPos)
		{
			CalculateAstarPath();
		}
	}

    private void SetPathEndPosition(Vector2I value)
	{
		if (NonWalkableTiles.Contains(value))
		{
			return;
		}
		if (IsTileOutsideTheMap(value))
		{
			return;
		}
		PathEndPos = value;
	}

    private void CalculateAstarPath()
    {
        int tileStartId = CalculateUniqueTileIdentifier(PathStarPos);
        int tileEndId = CalculateUniqueTileIdentifier(PathEndPos);
        GD.Print($"Start ID: {tileStartId}, End ID: {tileEndId}.");
        if (!AStar2DPath.HasPoint(tileStartId))
        {
            GD.Print("Start point has not been added to AStar2D.");
        }
        if (!AStar2DPath.HasPoint(tileEndId))
        {
            GD.Print("End point has not been added to AStar2D.");
        }
        PathNodeList = AStar2DPath.GetPointPath(tileStartId, tileEndId).ToList();
        if (PathNodeList.Count == 0)
        {
            GD.Print("No path found, or one of the points does not exist.");
        }
        else
        {
            GD.Print($"Path found with {PathNodeList.Count} points.");
            foreach (var point in PathNodeList)
            {
                GD.Print($"Path point: {point}");
            }
            QueueRedraw();  // Request redraw when path is updated
        }
    }

	private Godot.Collections.Array AddWalkableTiles(Godot.Collections.Array nonWalkableTiles)
	{
		Godot.Collections.Array walkableTiles = new Godot.Collections.Array();

        for (int y = 0; y < MapSize.Y; y++)
        {
            for (int x = 0; x < MapSize.X; x++)
            {
				var tile = new Vector2I(x, y);
				if (nonWalkableTiles.Contains(tile))
				{
                    continue;
				}

				walkableTiles.Add(tile);
                int tileId = CalculateUniqueTileIdentifier(tile);
                AStar2DPath.AddPoint(tileId, tile);
            }
        }
		return walkableTiles;
    }

	private void ConnectWalkableTiles(Godot.Collections.Array walkableTiles)
	{
        foreach (Vector2 tile in walkableTiles)
        {
			int tileId = CalculateUniqueTileIdentifier(tile);

            Vector2[] neighbouringTiles = {
            new Vector2(tile.X + 1 , tile.Y),//Right
			new Vector2(tile.X - 1, tile.Y),//Left
			new Vector2(tile.X, tile.Y - 1),//Up
			new Vector2(tile.X, tile.Y + 1)//Down
			};

            foreach (var neighborTile in neighbouringTiles)
            {
                int neighbourTileId = CalculateUniqueTileIdentifier(neighborTile);

				if (IsTileOutsideTheMap(neighborTile))
				{
                    continue;
				}
				if (!AStar2DPath.HasPoint(neighbourTileId))
				{
                    continue;
				}
				AStar2DPath.ConnectPoints(tileId, neighbourTileId, false);
            }
        }
    }
	public void SetTileMap(TileMap tileMap)
	{
        TileMapWorld = tileMap;

        if (TileMapWorld == null)
        {
            GD.Print("TileMapWorld is null.");
        }
        else
        {
            InitAstarPathFind();
        }

	}

    private void InitAstarPathFind()
	{
		GetTileMapBounds();
        NonWalkableTiles = (Godot.Collections.Array)TileMapWorld.GetUsedCellsById(1);
        Godot.Collections.Array walkableTilesList = AddWalkableTiles(NonWalkableTiles);

		ConnectWalkableTiles(walkableTilesList);

		SetPathStartPosition(Vector2I.Zero);
		SetPathEndPosition(Vector2I.Zero);
	}

	public bool SetAstarPath(Vector2 startPosition, Vector2 endPosition)
	{
		if(TileMapWorld == null)
		{
            GD.PrintErr("SetAstarPath failed: TileMapWorld is not set.");
            return false;
		}

        PathStarPos = TileMapWorld.LocalToMap(startPosition);
        PathEndPos = TileMapWorld.LocalToMap(endPosition);
        GD.Print($"Converted start position: {PathStarPos}, Converted end position: {PathEndPos}.");

        //This could mess up
        CalculateAstarPath();
        return true;
	}

    public override void _Draw()
    {
        if (!VisualizeAstarPath)
        {
            return;
        }
        List<Vector2> path = PathNodeList;

		if(path == null || path.Count <= 0)
		{
			return;
		}
        Vector2 lastPathPos = TileMapWorld.MapToLocal((Vector2I)PathNodeList[0]);

        for (int i = 0; i < path.Count; i++)
        {
            Vector2 currentPathPos = TileMapWorld.MapToLocal((Vector2I)PathNodeList[i]);
            DrawLine(lastPathPos, currentPathPos, Colors.White, 3f, true);
            if(i == 1)
			{
				DrawCircle(lastPathPos, 8f, Colors.Red);
                DrawCircle(currentPathPos, 5f, Colors.White);
            }
			else if(i == path.Count-1)
			{
				DrawCircle(currentPathPos, 8f, Colors.Green);
			}
			else
			{
				DrawCircle(currentPathPos, 5f, Colors.White);
			}
			lastPathPos = currentPathPos;
        }
    }
    public override void _Process(double delta)
    {
		if (VisualizeAstarPath)
		{
			QueueRedraw();
		}
	}
    public void UpdateNonWalkableTiles(IEnumerable<Vector2> additionalNonWalkableTiles)
    {
        foreach (Vector2 pos in additionalNonWalkableTiles)
        {
            NonWalkableTiles.Add(pos);
        }
    }
}