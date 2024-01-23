using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node2D
{
    public enum GameState
    {
        PlayerMovement,
        PlayerAttack,
        EnemyMovement,
        EnemyAttack,
        GameOver
    }
    [Export]
    public Player player;
    [Export]
    public Enemy[] enemies;
    private int currentEnemyIndex = 0;
    private GameState currentState;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        PlayerTurnStart();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}
    public void PlayerTurnStart()
    {
        currentState = GameState.PlayerMovement;
    }
    public void EnemyTurnStart()
    {
        if (currentState == GameState.PlayerAttack)
        {
            currentState = GameState.EnemyMovement;
            currentEnemyIndex = 0;
            if (enemies.Length > 0)
            {
                enemies[currentEnemyIndex].EnemyMove((Vector2I)player.GlobalPosition);
            }
        }
    }
    public void NextEnemyTurn()
    {
        currentEnemyIndex++;
        if (currentEnemyIndex < enemies.Length)
        {
            enemies[currentEnemyIndex].EnemyMove((Vector2I)player.GlobalPosition);
        }
        else
        {
            // All enemies have moved, now it's player's turn or some other state
            PlayerTurnStart();
        }
    }
    public void GameOver()
    {
        currentState = GameState.GameOver;
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mbEvent && mbEvent.IsPressed() && currentState == GameState.PlayerMovement)
        {
            if (mbEvent.ButtonIndex == MouseButton.Left)
            {
                player.PlayerMove();
                currentState = GameState.PlayerAttack;
            }
        }
    }
}
