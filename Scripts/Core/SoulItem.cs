using Godot;
using System;

public partial class SoulItem : Area2D
{
    private Node2D player;
    public float MagneticRange = 250.0f; // รัศมีดูดวิญญาณ
    public float MoveSpeed = 600.0f; // วิญญาณอาจจะลอยเข้าหาไวกว่าเหรียญหน่อย
    private bool isCollected = false;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        player = GetTree().GetFirstNodeInGroup("player") as Node2D;
        // ให้เป็นสีโทนวิญญาณ (ฟ้าอ่อน/เขียวมิ้นต์)
        Modulate = new Color(0.0f, 1.0f, 0.8f); 

        Label nameLabel = new Label();
        nameLabel.Text = "Soul";
        nameLabel.Position = new Vector2(-40, -40);
        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        AddChild(nameLabel);
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player player && !isCollected)
        {
            isCollected = true;
            player.Souls += 1; // เพิ่มวิญญาณให้ผู้เล่น 1 ดวง
            GD.Print($"เก็บวิญญาณได้! (วิญญาณรวมที่มี: {player.Souls})");
            QueueFree(); 
        }
    }

}
