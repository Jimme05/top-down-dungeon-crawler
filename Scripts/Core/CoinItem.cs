using Godot;
using System;

public partial class CoinItem : Area2D
{
    // มูลค่าของเหรียญ (ตัว Tank จะดรอปเหรียญที่มีมูลค่า 5)
    public int Value = 1;

    public override void _Ready()
    {
        // เมื่อมีใครมาเดินชนเหรียญ ให้เรียกฟังก์ชัน OnBodyEntered
        BodyEntered += OnBodyEntered;
        
        // ทำให้เหรียญเป็นสีเหลืองทอง และกระพริบเบาๆ (ถ้าอยากให้สวยขึ้น)
        Modulate = new Color(1.0f, 0.8f, 0.0f); // สีเหลืองทอง
        
        // ถ้าเป็นเหรียญใหญ่ (มูลค่าเยอะ) ให้ขนาดใหญ่ขึ้น
        if (Value > 1)
        {
            Scale = new Vector2(1.5f, 1.5f);
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        // ถ้าคนที่มาเก็บคือ Player
        if (body is Player)
        {
            GlobalData.Coins += Value; // เพิ่มเงิน
            GD.Print($"เก็บเหรียญได้ {Value}! (เหรียญรวม: {GlobalData.Coins})");
            QueueFree(); // เหรียญหายไปจากพื้น
        }
    }
}
