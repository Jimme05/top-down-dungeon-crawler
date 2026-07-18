using Godot;
using System;

public partial class SoulItem : Area2D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        // สีวิญญาณ (ฟ้าอ่อน/เขียวมิ้นต์)
        Modulate = new Color(0.0f, 1.0f, 0.8f); 
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player player)
        {
            player.Souls += 1; // เพิ่มวิญญาณให้ผู้เล่น 1 ดวง
            GD.Print($"เก็บวิญญาณได้! (วิญญาณรวมที่มี: {player.Souls})");
            QueueFree(); 
        }
    }
}
