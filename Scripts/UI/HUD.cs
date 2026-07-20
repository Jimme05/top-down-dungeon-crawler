using Godot;
using System;

public partial class HUD : CanvasLayer
{
    private ProgressBar hpBar;
    private Label hpLabel;
    private Label coinLabel;
    private Label soulLabel;
    private Label skillLabel;
    private Label announceLabel;
    private Control inventoryPanel;
    
    private ProgressBar dashCooldownBar;
    private ProgressBar ultCooldownBar;

    private Player player;

    public override void _Ready()
    {
        // ป้องกัน HUD บังการคลิกเมาส์
        var control = GetNodeOrNull<Control>("Control");
        if (control != null) control.MouseFilter = Control.MouseFilterEnum.Ignore;

        hpBar = GetNodeOrNull<ProgressBar>("Control/VBoxContainer/HPHBox/HPBar");
        hpLabel = GetNodeOrNull<Label>("Control/VBoxContainer/HPHBox/HPLabel");
        coinLabel = GetNodeOrNull<Label>("Control/VBoxContainer/CoinLabel");
        soulLabel = GetNodeOrNull<Label>("Control/VBoxContainer/SoulLabel");
        skillLabel = GetNodeOrNull<Label>("Control/VBoxContainer/SkillLabel");
        announceLabel = GetNodeOrNull<Label>("Control/AnnounceLabel");
        inventoryPanel = GetNodeOrNull<Control>("Control/InventoryPanel");
        
        dashCooldownBar = GetNodeOrNull<ProgressBar>("Control/CooldownContainer/DashCooldown");
        ultCooldownBar = GetNodeOrNull<ProgressBar>("Control/CooldownContainer/UltCooldown");

        if (announceLabel != null)
        {
            announceLabel.Modulate = new Color(1, 1, 1, 0); // ซ่อนตอนเริ่มเกม
        }

        // ค้นหาผู้เล่น
        player = GetTree().GetFirstNodeInGroup("player") as Player;
        
        if (player != null && hpBar != null)
        {
            hpBar.MaxValue = player.Hp;
        }
    }

    public override void _Process(double delta)
    {
        if (player == null) return;

        // อัปเดตข้อมูลแบบ Real-time
        if (hpBar != null) hpBar.Value = player.Hp;
        if (hpLabel != null) hpLabel.Text = $"HP: {player.Hp}";
        if (coinLabel != null) coinLabel.Text = $"💰 Coins: {GlobalData.Coins}";
        if (soulLabel != null) soulLabel.Text = $"👻 Souls: {player.Souls}";
        
        // อัปเดต UI แสดงผลสกิล
        string s1 = GetSkillText(player.SkillSlot1);
        string s2 = GetSkillText(player.SkillSlot2);
        string s3 = GetSkillText(player.SkillSlot3);
        
        string allSkills = "";
        if(s1 != "") allSkills += s1 + "  ";
        if(s2 != "") allSkills += s2 + "  ";
        if(s3 != "") allSkills += s3;

        if (allSkills == "") allSkills = "None";

        if (skillLabel != null) skillLabel.Text = $"Skills: {allSkills}";

        // เปิด/ปิดกระเป๋าด้วยปุ่ม Tab
        if (Input.IsActionJustPressed("ui_focus_next")) // Tab key is mapped to ui_focus_next by default
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.Visible = !inventoryPanel.Visible;
            }
        }
        
        // อัปเดตคูลดาวน์ UI
        if (dashCooldownBar != null) dashCooldownBar.Value = player.GetDashCooldownRatio();
        if (ultCooldownBar != null) ultCooldownBar.Value = player.GetUltCooldownRatio();
    }

    // ฟังก์ชันสำหรับแจ้งเตือน
    public void ShowAnnouncement(string message)
    {
        if (announceLabel != null)
        {
            announceLabel.Text = message;
            
            // สร้าง Tween เพื่อทำแอนิเมชันเด้งขึ้นมา
            Tween tween = GetTree().CreateTween();
            tween.TweenProperty(announceLabel, "modulate", new Color(1, 1, 1, 1), 0.5f);
            tween.TweenProperty(announceLabel, "scale", new Vector2(1.2f, 1.2f), 0.3f).SetTrans(Tween.TransitionType.Bounce);
            tween.TweenProperty(announceLabel, "scale", new Vector2(1.0f, 1.0f), 0.3f);
            
            // รอ 1.5 วินาทีแล้วจางหาย
            tween.TweenInterval(1.5f);
            tween.TweenProperty(announceLabel, "modulate", new Color(1, 1, 1, 0), 0.5f);
        }
    }

    private string GetSkillText(ModifierType type)
    {
        if (type == ModifierType.Fire) return "Fire";
        if (type == ModifierType.MultiShot) return "Multi";
        if (type == ModifierType.Pierce) return "Pierce";
        if (type == ModifierType.Chain) return "Chain";
        if (type == ModifierType.Cold) return "Cold";
        if (type == ModifierType.FasterAttacks) return "Fast";
        if (type == ModifierType.Fork) return "Fork";
        if (type == ModifierType.Homing) return "Homing";
        return "";
    }
}
