using Godot;
using System;

public partial class DungeonGenerator : Node2D
{
    [Export] public PackedScene RoomScene;
    
    // ตั้งค่าความยาวของดันเจี้ยน
    [Export] public int TotalRooms = 10;
    
    // ระยะห่างระหว่างห้องแต่ละห้อง (พิกเซลแกน X)
    [Export] public float RoomSpacingX = 1200.0f; 

    public override void _Ready()
    {
        GenerateDungeon();
    }

    private void GenerateDungeon()
    {
        if (RoomScene == null)
        {
            GD.PrintErr("ยังไม่ได้ใส่ RoomScene ใน DungeonGenerator!");
            return;
        }

        GD.Print($"เริ่มสร้างดันเจี้ยนความยาว {TotalRooms} ห้อง...");

        for (int i = 0; i < TotalRooms; i++)
        {
            // เสกห้องออกมา
            Room room = RoomScene.Instantiate<Room>();
            
            // จัดเรียงต่อกันไปทางขวา (แกน X)
            room.Position = new Vector2(i * RoomSpacingX, 0);
            
            // กำหนดความยากของห้อง (ยิ่งลึกยิ่งศัตรูเยอะ)
            room.EnemyCount = 3 + (i / 2); 
            
            // ทุกๆ 2 ห้อง (ห้องที่เป็น index คี่) ให้มีแท่นบูชา
            if (i % 2 == 1)
            {
                room.HasAltar = true;
            }
            
            // ห้องสุดท้ายสุดกำหนดให้เป็นห้องบอส
            if (i == TotalRooms - 1)
            {
                room.IsBossRoom = true;
                room.HasAltar = false; // ห้องบอสไม่เอาแท่นบูชา
            }

            // เพิ่มห้องเข้าไปในฉาก
            CallDeferred("add_child", room);
        }
    }
}
