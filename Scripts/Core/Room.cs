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

    // สถานะของห้อง
    private bool isRoomActive = false;
    private bool isCleared = false;
    private int currentEnemies = 0;

    // ส่วนประกอบของห้อง
    private Area2D trigger;
    private StaticBody2D walls; // เปลี่ยนมาใช้การสร้างกำแพงด้วยโค้ดแทน

    // วาดขอบเขตห้องให้เห็นชัดเจนบนหน้าจอ
    public override void _Draw()
    {
        // ขยายห้องเป็น 1000x800 พิกเซล
        Rect2 roomBounds = new Rect2(-500, -400, 1000, 800);
        DrawRect(roomBounds, new Color(0.15f, 0.15f, 0.2f, 0.8f)); // สีพื้น
        
        // วาดเส้นขอบห้องสีขาว (ความหนา 2)
        DrawRect(roomBounds, new Color(1.0f, 1.0f, 1.0f, 0.5f), false, 2.0f);
    }

    public override void _Ready()
    {
        // ---------------------------------------------------------
        // สร้าง Trigger อัตโนมัติ (ขยับลึกเข้ามาในห้อง เพื่อให้ผู้เล่นเดินผ่านประตูก่อน)
        // ---------------------------------------------------------
        trigger = new Area2D();
        CollisionShape2D triggerCol = new CollisionShape2D();
        RectangleShape2D triggerShape = new RectangleShape2D();
        triggerShape.Size = new Vector2(100, 800); 
        triggerCol.Shape = triggerShape;
        triggerCol.Position = new Vector2(-350, 0); // ลึกเข้ามาจากกำแพงซ้าย (-500)
        trigger.AddChild(triggerCol);
        AddChild(trigger);
        
        trigger.BodyEntered += OnPlayerEntered;

        // ลบ Trigger และ Doors เก่าที่ผู้เล่นอาจจะสร้างไว้ใน Godot ทิ้ง
        var oldTrigger = GetNodeOrNull<Area2D>("Trigger");
        if (oldTrigger != null) oldTrigger.QueueFree();
        
        var oldDoors = GetNodeOrNull<StaticBody2D>("Doors");
        if (oldDoors != null) oldDoors.QueueFree();

        // ---------------------------------------------------------
        // สร้างระบบกำแพงล่องหน 4 ด้าน ขังผู้เล่นไว้ในขนาด 1000x800 เป๊ะๆ
        // ---------------------------------------------------------
        walls = new StaticBody2D();
        walls.ProcessMode = ProcessModeEnum.Disabled; // ตอนแรกปิดไว้ก่อน ให้เดินเข้าห้องได้
        AddChild(walls);

        // ฟังก์ชันช่วยสร้างกำแพง
        Action<Vector2, Vector2> CreateWall = (pos, size) => 
        {
            CollisionShape2D col = new CollisionShape2D();
            RectangleShape2D shape = new RectangleShape2D();
            shape.Size = size;
            col.Shape = shape;
            col.Position = pos;
            walls.AddChild(col);
        };

        CreateWall(new Vector2(0, -400), new Vector2(1000, 20)); // กำแพงบน
        CreateWall(new Vector2(0, 400), new Vector2(1000, 20));  // กำแพงล่าง
        CreateWall(new Vector2(-500, 0), new Vector2(20, 800));  // กำแพงซ้าย
        CreateWall(new Vector2(500, 0), new Vector2(20, 800));   // กำแพงขวา
        
        // ถ้าห้องนี้มีการเสกแท่นบูชา ให้เสกออกมารอเลย
        if (HasAltar && AltarScene != null && !IsBossRoom)
        {
            Node2D altar = AltarScene.Instantiate<Node2D>();
            altar.Position = new Vector2(0, 0); // วางไว้กลางห้อง
            AddChild(altar);
        }
    }

    private void OnPlayerEntered(Node2D body)
    {
        // ถ้า Player เดินเข้ามา และห้องยังไม่เคยถูกเคลียร์
        if (body is Player && !isRoomActive && !isCleared)
        {
            isRoomActive = true;
            GD.Print("เข้าห้องแล้ว! สร้างกำแพงเวทมนตร์ขัง 4 ด้าน!");
            
            // เปิดกำแพงที่สร้างไว้ (ผู้เล่นจะเดินออกไม่ได้แล้ว)
            walls.ProcessMode = ProcessModeEnum.Inherit;
            
            SpawnEnemies();
        }
    }

    private void SpawnEnemies()
    {
        // ถ้าเป็นห้องบอส จะเสกบอสแค่ตัวเดียว
        int spawnAmount = IsBossRoom ? 1 : EnemyCount;
        
        for (int i = 0; i < spawnAmount; i++)
        {
            Enemy enemy = EnemyScene.Instantiate<Enemy>();
            
            // ถ้าเป็นบอส บังคับให้เป็น Boss ถ้าเป็นธรรมดา ให้สุ่ม 0,1,2 เหมือนเดิม
            if (IsBossRoom)
            {
                // สมมติว่า Boss คือ Type เบอร์ 3 (เดี๋ยวเราไปเพิ่มใน Enemy.cs)
                enemy.Type = (EnemyType)3;
            }
            else
            {
                Random random = new Random();
                enemy.Type = (EnemyType)random.Next(0, 3);
            }
            
            // สุ่มตำแหน่งกระจายๆ กันในขอบเขตห้อง (1000x800)
            Random randPos = new Random();
            float randX = (float)randPos.NextDouble() * 800 - 400; // -400 ถึง 400 (เว้นขอบนิดนึง)
            float randY = (float)randPos.NextDouble() * 600 - 300; // -300 ถึง 300
            enemy.Position = new Vector2(randX, randY);
            
            // เมื่อศัตรูตาย (ถูกลบออกจาก SceneTree) ให้เรียก OnEnemyDied
            enemy.TreeExited += OnEnemyDied;
            
            CallDeferred("add_child", enemy);
            currentEnemies++;
        }
    }

    private void OnEnemyDied()
    {
        currentEnemies--;
        
        // ถ้าศัตรูตายหมดแล้ว และห้องยังติดสถานะ Active อยู่
        if (currentEnemies <= 0 && isRoomActive)
        {
            isRoomActive = false;
            isCleared = true;
            GD.Print(IsBossRoom ? "กำจัดบอสสำเร็จ!! ชนะเกม!" : "ห้องเคลียร์แล้ว! กำแพงเวทมนตร์สลายไป!");
            
            // ปิดกำแพง ให้เดินทะลุไปห้องต่อไปได้
            walls.ProcessMode = ProcessModeEnum.Disabled;
        }
    }
}
