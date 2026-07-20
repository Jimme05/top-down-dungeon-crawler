using Godot;
using System;

public partial class Bullet : Area2D
{
    public float Speed = 400.0f; 
    public Vector2 Direction = Vector2.Right; 
    public int Damage = 1; 

    public ModifierType[] Modifiers;

    // --- PoE Support Stats ---
    private int pierceCount = 0;
    private int chainCount = 0;
    private int forkCount = 0;
    private bool isHoming = false;
    private bool hasCold = false;
    private bool hasFire = false;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;

        if (Modifiers != null)
        {
            foreach (var mod in Modifiers)
            {
                if (mod == ModifierType.Fire) hasFire = true;
                if (mod == ModifierType.Cold) hasCold = true;
                if (mod == ModifierType.Pierce) pierceCount += 2; // หินทะลุ 1 ก้อน ยิงทะลุได้ 2 ตัว
                if (mod == ModifierType.Chain) chainCount += 1;
                if (mod == ModifierType.Fork) forkCount += 1;
                if (mod == ModifierType.Homing) isHoming = true;
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (isHoming)
        {
            // หาศัตรูที่ใกล้ที่สุดแล้วหักเลี้ยวไปหา
            Node2D target = FindNearestEnemy(300.0f, null);
            if (target != null)
            {
                Vector2 dirToTarget = (target.GlobalPosition - GlobalPosition).Normalized();
                // หักเลี้ยวอย่างนุ่มนวล (lerp direction)
                Direction = Direction.Lerp(dirToTarget, 2.0f * (float)delta).Normalized();
            }
        }

        Position += Direction * Speed * (float)delta;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Enemy enemy)
        {
            enemy.TakeDamage(Damage); 

            if (hasFire) enemy.ApplyFire();
            if (hasCold) enemy.ApplyCold();

            // ลำดับการทำงาน: Fork -> Chain -> Pierce
            if (forkCount > 0)
            {
                forkCount--;
                SpawnForkBullet(-0.5f); // แตกซ้าย
                SpawnForkBullet(0.5f);  // แตกขวา
                QueueFree(); // ทำลายตัวแม่
                return;
            }
            else if (chainCount > 0)
            {
                chainCount--;
                Node2D nextTarget = FindNearestEnemy(400.0f, enemy);
                if (nextTarget != null)
                {
                    Direction = (nextTarget.GlobalPosition - GlobalPosition).Normalized();
                    return; // ไม่ทำลายกระสุน ปล่อยให้มันวิ่งไปหาเป้าหมายใหม่
                }
            }
            else if (pierceCount > 0)
            {
                pierceCount--;
                return; // ไม่ทำลายกระสุน ปล่อยให้มันทะลุไป
            }

            QueueFree(); // ถ้าไม่มี support พิเศษ หรือโควต้าหมดแล้ว ให้กระสุนพัง
        }
    }

    private void SpawnForkBullet(float angleOffset)
    {
        var scene = GD.Load<PackedScene>("res://Scenes/Player/bullet.tscn");
        Bullet newBullet = scene.Instantiate<Bullet>();
        
        newBullet.GlobalPosition = GlobalPosition;
        newBullet.Direction = Direction.Rotated(angleOffset);
        newBullet.Speed = Speed;
        newBullet.Damage = Damage;
        newBullet.Modifiers = Modifiers; // สืบทอด modifiers

        // ต้องลดค่า fork ในกระสุนลูกลง เพื่อไม่ให้แตกอนันต์
        // ซึ่งเราหักลบ forkCount ในตัวแม่แล้วส่ง modifier ไป แต่เพื่อความชัวร์ควร copy modifiers
        // แต่เนื่องจาก Modifiers เป็น Array อ้างอิง การลบในนี้อาจจะยาก 
        // ดังนั้นเราจะใช้ forkCount ที่ถูกเซ็ตใน _Ready แล้วลบด้วย 1 ทิ้งไป (หรือใช้วิธีง่ายสุดคือกำหนดตัวแปรตรงๆ)
        
        GetTree().CurrentScene.CallDeferred("add_child", newBullet);
        
        // เราตั้งค่า forkCount แมนนวลหลังจาก AddChild (เพื่อให้มันทับ _Ready) 
        // แต่จริงๆ _Ready ของกระสุนลูกจะคำนวณใหม่จาก Modifiers
        // เพื่อแก้ปัญหานี้ เราจะสร้าง Array ใหม่ที่หักหิน Fork ออก 1 ก้อน (ไม่งั้นจะบัคกระสุนล้นจอ)
        newBullet.Modifiers = RemoveOneForkModifier(Modifiers);
    }

    private ModifierType[] RemoveOneForkModifier(ModifierType[] mods)
    {
        if (mods == null) return null;
        var list = new System.Collections.Generic.List<ModifierType>(mods);
        list.Remove(ModifierType.Fork); // ลบ Fork ออก 1 ตัว
        return list.ToArray();
    }

    private Node2D FindNearestEnemy(float radius, Enemy exclude)
    {
        var enemies = GetTree().GetNodesInGroup("enemies");
        Node2D nearest = null;
        float minDistance = radius;

        foreach (Node2D e in enemies)
        {
            if (e == exclude) continue; // ไม่หาตัวที่เพิ่งชนไป

            float dist = GlobalPosition.DistanceTo(e.GlobalPosition);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = e;
            }
        }
        return nearest;
    }
}
