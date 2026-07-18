using Godot;
using System;

public partial class Altar : Area2D
{
    // ราคาที่ต้องจ่าย (วิญญาณ 3 ดวง) แลกกับการอัปเกรดดาเมจ +1
    public int SoulCost = 3;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player player)
        {
            if (player.Souls >= SoulCost)
            {
                player.Souls -= SoulCost;
                player.BonusDamage += 1; // เพิ่มโบนัสดาเมจถาวรในตานี้
                
                GD.Print($"บูชาแท่นสำเร็จ! เสียวิญญาณ {SoulCost} ดวง (เหลือ {player.Souls}) -> ดาเมจรวมแรงขึ้น +1!");
                
                // เอฟเฟกต์กระพริบเปลี่ยนสี เพื่อให้รู้ว่าใช้งานไปแล้ว
                Modulate = new Color(0.2f, 0.2f, 0.2f); // กลายเป็นสีเทา (หมดพลัง)
                
                // ปิดการชนเพื่อไม่ให้กดซ้ำได้อีก
                SetDeferred("monitoring", false); 
            }
            else
            {
                GD.Print($"วิญญาณไม่พอ! ต้องการ {SoulCost} ดวง (คุณมี {player.Souls})");
            }
        }
    }
}
