using Godot;
using System;

public partial class InventorySlot : ColorRect
{
    [Export] public int SlotIndex = 0; 
    // 0, 1, 2 = Equip Slots
    // 3 to 11 = Inventory Slots

    private Player player;
    private Label iconLabel; // ใส่ Text เพื่อเป็นสัญลักษณ์ (เช่น ไฟ, น้ำแข็ง, ยิงรัว)

    public override void _Ready()
    {
        player = GetTree().GetFirstNodeInGroup("player") as Player;
        iconLabel = GetNodeOrNull<Label>("IconLabel");
    }

    public override void _Process(double delta)
    {
        if (player == null || iconLabel == null) return;

        ModifierType currentType = GetModifierInSlot();
        
        // อัปเดตไอคอน สี และคำอธิบาย (Tooltip)
        if (currentType == ModifierType.None)
        {
            iconLabel.Text = "";
            TooltipText = ""; // ไม่มีของก็ไม่มีคำอธิบาย
            Color = new Color(0.2f, 0.2f, 0.2f, 0.8f); 
        }
        else
        {
            if (currentType == ModifierType.Fire) { iconLabel.Text = "Fire"; TooltipText = "Fire Stone (เผาไหม้ศัตรู)"; Color = new Color(0.8f, 0.3f, 0.1f, 1.0f); }
            else if (currentType == ModifierType.MultiShot) { iconLabel.Text = "Multi"; TooltipText = "MultiShot Stone (ยิงกระจาย 3 แฉก)"; Color = new Color(0.1f, 0.5f, 0.9f, 1.0f); }
            else if (currentType == ModifierType.Pierce) { iconLabel.Text = "Pierce"; TooltipText = "Pierce Stone (ยิงทะลุ 2 ตัว)"; Color = new Color(0.9f, 0.7f, 0.0f, 1.0f); }
            else if (currentType == ModifierType.Chain) { iconLabel.Text = "Chain"; TooltipText = "Chain Stone (กระสุนชิ่งหาศัตรูถัดไป)"; Color = new Color(0.6f, 0.2f, 0.9f, 1.0f); }
            else if (currentType == ModifierType.Cold) { iconLabel.Text = "Cold"; TooltipText = "Cold Stone (แช่แข็งลดความเร็ว 50%)"; Color = new Color(0.6f, 0.9f, 1.0f, 1.0f); }
            else if (currentType == ModifierType.FasterAttacks) { iconLabel.Text = "Fast"; TooltipText = "Faster Attacks Stone (ลดคูลดาวน์ยิง 30%)"; Color = new Color(0.9f, 0.2f, 0.4f, 1.0f); }
            else if (currentType == ModifierType.Fork) { iconLabel.Text = "Fork"; TooltipText = "Fork Stone (ยิงโดนเป้าแล้วแตกเป็น 2 นัด)"; Color = new Color(0.3f, 0.9f, 0.6f, 1.0f); }
            else if (currentType == ModifierType.Homing) { iconLabel.Text = "Home"; TooltipText = "Homing Stone (กระสุนติดตามเป้าหมายอัตโนมัติ)"; Color = new Color(0.9f, 0.9f, 0.9f, 1.0f); }
            else { iconLabel.Text = "Skill"; TooltipText = "Unknown Stone"; Color = new Color(1.0f, 1.0f, 1.0f, 1.0f); }
        }
    }

    private ModifierType GetModifierInSlot()
    {
        if (SlotIndex == 0) return player.SkillSlot1;
        if (SlotIndex == 1) return player.SkillSlot2;
        if (SlotIndex == 2) return player.SkillSlot3;
        
        int invIndex = SlotIndex - 3;
        if (invIndex >= 0 && invIndex < player.Inventory.Length)
        {
            return player.Inventory[invIndex];
        }
        return ModifierType.None;
    }

    private void SetModifierInSlot(ModifierType type)
    {
        if (SlotIndex == 0) player.SkillSlot1 = type;
        else if (SlotIndex == 1) player.SkillSlot2 = type;
        else if (SlotIndex == 2) player.SkillSlot3 = type;
        else
        {
            int invIndex = SlotIndex - 3;
            if (invIndex >= 0 && invIndex < player.Inventory.Length)
            {
                player.Inventory[invIndex] = type;
            }
        }
    }

    // --- ระบบ Drag & Drop ---
    public static InventorySlot DraggingSlot = null;

    public override Variant _GetDragData(Vector2 atPosition)
    {
        ModifierType type = GetModifierInSlot();
        if (type == ModifierType.None) return default; // ห้ามลากช่องว่าง

        // สร้างภาพ Preview ตอนลาก
        Label previewLabel = new Label();
        previewLabel.Text = iconLabel.Text;
        previewLabel.AddThemeFontSizeOverride("font_size", 40);
        
        Control previewControl = new Control();
        previewControl.AddChild(previewLabel);
        previewLabel.Position = new Vector2(-20, -20); // จัดกลาง

        SetDragPreview(previewControl);

        DraggingSlot = this; // จำไว้ว่ากำลังลากจากช่องไหน

        // ส่งค่า SlotIndex ไปพร้อมกับตอนลาก
        return SlotIndex;
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        // ยอมให้ดรอปถ้าข้อมูลที่ลากมาคือ int (SlotIndex)
        return data.VariantType == Variant.Type.Int;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        int fromSlot = data.AsInt32();
        int toSlot = SlotIndex;
        
        if (fromSlot == toSlot) return; // ลากใส่ช่องเดิม

        // หาช่องต้นทางเพื่อทำการ Swap ข้อมูล
        var uiNodes = GetTree().GetNodesInGroup("inventory_slot");
        InventorySlot sourceUI = null;
        foreach(InventorySlot slot in uiNodes)
        {
            if (slot.SlotIndex == fromSlot)
            {
                sourceUI = slot;
                break;
            }
        }

        if (sourceUI != null)
        {
            ModifierType sourceMod = sourceUI.GetModifierInSlot();
            ModifierType myMod = GetModifierInSlot();

            // สลับข้อมูล
            sourceUI.SetModifierInSlot(myMod);
            SetModifierInSlot(sourceMod);
            
            GD.Print($"Swapped Slot {fromSlot} with Slot {toSlot}");
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationDragEnd)
        {
            if (DraggingSlot == this && !GetViewport().GuiIsDragging())
            {
                if (!IsDragSuccessful())
                {
                    // ลากไปปล่อยนอกกรอบ Inventory => โยนของทิ้ง
                    ModifierType myMod = GetModifierInSlot();
                    if (myMod != ModifierType.None)
                    {
                        PackedScene itemScene = GD.Load<PackedScene>("res://Scenes/modifier_item.tscn");
                        var dropItem = itemScene.Instantiate<ModifierItem>();
                        dropItem.Type = myMod;
                        
                        // วางของรอบๆ ผู้เล่นแบบสุ่มตำแหน่งนิดหน่อย
                        dropItem.GlobalPosition = player.GlobalPosition + new Vector2((float)GD.RandRange(-50, 50), (float)GD.RandRange(-50, 50));
                        
                        // ป้องกันไม่ให้โดนดูดกลับทันที (ถ้าใช้ระบบแม่เหล็ก)
                        dropItem.SetDeferred("MagneticRange", 0); 
                        
                        GetTree().CurrentScene.AddChild(dropItem);
                        
                        SetModifierInSlot(ModifierType.None); // ล้างช่องทิ้ง
                        GD.Print("Dropped " + myMod + " from slot " + SlotIndex);
                    }
                }
                DraggingSlot = null; // Reset
            }
        }
    }
}
