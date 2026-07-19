using Godot;
using System;
using System.Collections.Generic;

public partial class Room : Node2D
{
    // ตั้งค่าเบื้องต้นของห้อง
    [Export] public PackedScene EnemyScene;
    [Export] public PackedScene AltarScene;
    
    // ตั้งค่าระบบจากการสุ่ม (DungeonGenerator จะมาเปลี่ยนค่าพวกนี้)
    public int EnemyCount = 3;
    public bool HasAltar = false;
    public bool IsBossRoom = false;
    public bool IsSpawnRoom = false; // ห้องจุดเริ่มต้น

    // สถานะของห้อง
    private bool isRoomActive = false;
    private bool isCleared = false;
    private int currentEnemies = 0;

    // ส่วนประกอบของห้อง
    private Area2D trigger;
    private StaticBody2D walls; 
    private StaticBody2D doors; // ประตูเวทมนตร์สำหรับล็อกตอนสู้

    public override void _Ready()
    {
        // ตั้งค่าให้ห้องอยู่ Layer ล่างสุด เพื่อไม่ให้พื้นหลังสีเทาไปบังตัวละครหรือไอเทม
        ZIndex = -10;

        // ---------------------------------------------------------
        // กราฟิก (พื้น/กำแพง/ของตกแต่ง) จะถูกจัดการผ่าน TileMapLayer 
        // ที่คุณวาดเองใน Godot Editor แทนการใช้โค้ดทั้งหมดแล้วครับ!
        // ---------------------------------------------------------

        // ---------------------------------------------------------
        // 1. สร้างกำแพงถาวร (มีช่องโหว่ซ้าย-ขวา เป็นประตู)
        // ---------------------------------------------------------
        walls = new StaticBody2D();
        AddChild(walls);

        Action<Vector2, Vector2> CreateWall = (pos, size) => 
        {
            CollisionShape2D col = new CollisionShape2D();
            RectangleShape2D shape = new RectangleShape2D();
            shape.Size = size;
            col.Shape = shape;
            col.Position = pos;
            walls.AddChild(col);
        };

        CreateWall(new Vector2(0, -500), new Vector2(1600, 20)); // บน
        CreateWall(new Vector2(0, 500), new Vector2(1600, 20));  // ล่าง
        
        CreateWall(new Vector2(-800, -325), new Vector2(20, 350)); // ซ้าย-บน
        CreateWall(new Vector2(-800, 325), new Vector2(20, 350));  // ซ้าย-ล่าง
        
        CreateWall(new Vector2(800, -325), new Vector2(20, 350));  // ขวา-บน
        CreateWall(new Vector2(800, 325), new Vector2(20, 350));   // ขวา-ล่าง
        
        // กำแพงทางเดิน
        if (!IsBossRoom)
        {
            CreateWall(new Vector2(1100, -150), new Vector2(600, 20)); // ทางเดินบน
            CreateWall(new Vector2(1100, 150), new Vector2(600, 20));  // ทางเดินล่าง
        }
        
        // ---------------------------------------------------------
        // 2. สร้างประตูเวทมนตร์ (เปิด/ปิด ได้ตอนต่อสู้)
        // ---------------------------------------------------------
        doors = new StaticBody2D();
        doors.ProcessMode = ProcessModeEnum.Disabled; // ปิดไว้ก่อน
        AddChild(doors);
        
        Action<Vector2> CreateDoor = (pos) => 
        {
            CollisionShape2D col = new CollisionShape2D();
            RectangleShape2D shape = new RectangleShape2D();
            shape.Size = new Vector2(20, 300); // อุดช่องโหว่พอดี
            col.Shape = shape;
            col.Position = pos;
            doors.AddChild(col);
        };
        
        CreateDoor(new Vector2(-800, 0)); // ประตูซ้าย
        if (!IsBossRoom) CreateDoor(new Vector2(800, 0)); // ประตูขวา

        // ---------------------------------------------------------
        // 3. สร้าง Trigger ดักจับ (X = -600 เพื่อให้เดินเข้ามาลึกๆ ก่อน)
        // ---------------------------------------------------------
        if (!IsSpawnRoom)
        {
            trigger = new Area2D();
            CollisionShape2D triggerCol = new CollisionShape2D();
            RectangleShape2D triggerShape = new RectangleShape2D();
            triggerShape.Size = new Vector2(100, 1000); 
            triggerCol.Shape = triggerShape;
            triggerCol.Position = new Vector2(-600, 0); 
            trigger.AddChild(triggerCol);
            AddChild(trigger);
            trigger.BodyEntered += OnPlayerEntered;
        }

        // ลบของเก่าทิ้ง
        var oldTrigger = GetNodeOrNull<Area2D>("Trigger");
        if (oldTrigger != null) oldTrigger.QueueFree();
        var oldDoors = GetNodeOrNull<StaticBody2D>("Doors");
        if (oldDoors != null) oldDoors.QueueFree();

    }



    private void OnPlayerEntered(Node2D body)
    {
        if (body is Player && !isRoomActive && !isCleared && !IsSpawnRoom)
        {
            isRoomActive = true;
            GD.Print("เข้าห้องแล้ว! ล็อกประตู!");
            doors.ProcessMode = ProcessModeEnum.Inherit; // ปิดประตู
            SpawnEnemies();
        }
    }

    private void SpawnEnemies()
    {
        int spawnAmount = IsBossRoom ? 1 : EnemyCount;
        
        for (int i = 0; i < spawnAmount; i++)
        {
            Enemy enemy = EnemyScene.Instantiate<Enemy>();
            
            if (IsBossRoom) enemy.Type = (EnemyType)3;
            else
            {
                Random random = new Random();
                enemy.Type = (EnemyType)random.Next(0, 3);
            }
            
            // สุ่มในขอบเขตห้อง (1600x1000)
            Random randPos = new Random();
            float randX = (float)randPos.NextDouble() * 1200 - 600; 
            float randY = (float)randPos.NextDouble() * 800 - 400; 
            enemy.Position = new Vector2(randX, randY);
            
            enemy.TreeExited += OnEnemyDied;
            CallDeferred("add_child", enemy);
            currentEnemies++;
        }
    }

    private void OnEnemyDied()
    {
        currentEnemies--;
        
        if (currentEnemies <= 0 && isRoomActive)
        {
            isRoomActive = false;
            isCleared = true;
            GD.Print(IsBossRoom ? "กำจัดบอสสำเร็จ!! ชนะเกม!" : "ห้องเคลียร์แล้ว! ประตูเปิดออก!");
            
            doors.ProcessMode = ProcessModeEnum.Disabled; // เปิดประตู
            
            // เสกแท่นบูชาเป็นรางวัลหลังจากเคลียร์มอนสเตอร์หมดแล้ว
            if (HasAltar && AltarScene != null && !IsBossRoom)
            {
                Node2D altar = AltarScene.Instantiate<Node2D>();
                altar.Position = new Vector2(0, 0); // โผล่มากลางห้อง
                CallDeferred("add_child", altar);
                GD.Print("แท่นบูชาปรากฏขึ้นแล้ว!");
            }
        }
    }
}
