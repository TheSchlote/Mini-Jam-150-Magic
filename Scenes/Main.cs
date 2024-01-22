using Godot;
using System;
using System.Collections.Generic;
public enum GameState
{
    PlayerTurn,
    EnemyTurn,
    GameOver
}
public partial class Main : Node2D
{
    [Export]
    public PackedScene enemyScene;
    [Export]
    public Player player;
    private List<Enemy> enemies = new List<Enemy>();
    private TileMap WorldTileMap;
    private bool isPlayerTurn = true;
    private int currentEnemyIndex = 0;
    private GameState currentState;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        WorldTileMap = GetNode<TileMap>("TileMap");
        Vector2I[] enemyTilePositions = new Vector2I[]
{
            new Vector2I(12, 1),
            new Vector2I(22, 2),
            new Vector2I(22, 11),
            new Vector2I(11, 12),
            new Vector2I(1, 12)
};
        // Instantiate and add a few enemies
        foreach (Vector2I tilePosition in enemyTilePositions)
        {
            Enemy enemy = (Enemy)enemyScene.Instantiate();
            Vector2 worldPosition = WorldTileMap.MapToLocal(tilePosition);
            enemy.Position = worldPosition;
            enemy.SetPlayer(player);
            enemies.Add(enemy);
            AddChild(enemy);
        }
        StartPlayerTurn();
    }
    public void EndPlayerTurn()
    {
        currentState = GameState.EnemyTurn;
        currentEnemyIndex = 0; // Reset the index for enemy turns
        ProcessEnemyTurn(); // Start processing the first enemy's turn
    }
    private void ProcessEnemyTurn()
    {
        if (currentEnemyIndex < enemies.Count)
        {
            GD.Print("Enemy's turn: " + currentEnemyIndex);
            Enemy enemy = enemies[currentEnemyIndex];
            enemy.PerformTurn(this); // Perform the enemy's turn
        }
        else
        {
            StartPlayerTurn(); // All enemies have had their turn, now it's the player's turn
        }
    }
    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        // Reset the player's turn state, allow input, etc.
    }
    public void OnEnemyTurnFinished()
    {
        currentEnemyIndex++; // Increment the index to move to the next enemy
        ProcessEnemyTurn(); // Process the next enemy's turn
    }



    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
