using Godot;
using System;

public partial class DungeonGenerator : Node2D
{
    [Export] public PackedScene RoomScene;
    
    // ตั้งค่าความยาวของดันเจี้ยน
    [Export] public int TotalRooms = 10;
    
    // ระยะห่างระหว่างห้องแต่ละห้อง (พิกเซลแกน X)
    [Export] public float RoomSpacingX = 2200.0f; // ห้อง 1600 + ทางเดิน 600

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

        GD.Print($"เริ่มสร้างดันเจี้ยนความยาว {TotalRooms} ห้อง (พร้อมห้องเริ่มต้น)...");

        // สร้างห้องทั้งหมด (ห้องเริ่มต้น 1 ห้อง + ห้องต่อสู้ TotalRooms ห้อง)
        for (int i = 0; i <= TotalRooms; i++)
        {
            Room room = RoomScene.Instantiate<Room>();
            room.Position = new Vector2(i * RoomSpacingX, 0);

            if (i == 0)
            {
                // ห้องแรกสุด เป็นห้องปลอดภัย
                room.IsSpawnRoom = true;
                room.HasAltar = false;
                room.EnemyCount = 0;
            }
            else
            {
                // ห้องสู้รบปกติ
                room.IsSpawnRoom = false;
                room.EnemyCount = 3 + (i / 2); 
                
                // ทุกๆ 2 ห้อง (ยกเว้นบอส) ให้มีแท่นบูชา (จะไปเกิดที่ห้อง 2, 4, 6, 8 แทน)
                if (i % 2 == 0 && i != TotalRooms)
                {
                    room.HasAltar = true;
                }
                
                // ห้องสุดท้าย
                if (i == TotalRooms)
                {
                    room.IsBossRoom = true;
                    room.HasAltar = false; 
                }
            }

            CallDeferred("add_child", room);
        }
    }
}
