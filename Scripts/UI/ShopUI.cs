using Godot;
using System;

public partial class ShopUI : Control
{
    private Button hpBtn;
    private Button dmgBtn;
    private Button speedBtn;
    private Button dashBtn;
    private Label coinLabel;
    private Control panel;

    // ราคาเริ่มต้น
    private int baseHpCost = 10;
    private int baseDmgCost = 20;
    private int baseSpeedCost = 15;
    private int baseDashCost = 15;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore; // ป้องกัน Control บังเมาส์
        panel = GetNode<Control>("Panel");
        coinLabel = GetNode<Label>("Panel/CoinLabel");
        
        hpBtn = GetNode<Button>("Panel/VBoxContainer/BtnHp");
        dmgBtn = GetNode<Button>("Panel/VBoxContainer/BtnDmg");
        speedBtn = GetNode<Button>("Panel/VBoxContainer/BtnSpeed");
        dashBtn = GetNode<Button>("Panel/VBoxContainer/BtnDash");

        hpBtn.Pressed += () => BuyUpgrade("HP");
        dmgBtn.Pressed += () => BuyUpgrade("DMG");
        speedBtn.Pressed += () => BuyUpgrade("SPEED");
        dashBtn.Pressed += () => BuyUpgrade("DASH");
        
        // ปุ่มปิดร้าน
        var closeBtn = GetNode<Button>("Panel/CloseBtn");
        closeBtn.Pressed += HideUI;

        HideUI();
    }

    private bool uKeyPressed = false;

    public override void _Process(double delta)
    {
        if (Input.IsPhysicalKeyPressed(Key.U))
        {
            if (!uKeyPressed)
            {
                uKeyPressed = true;
                if (!panel.Visible) ShowUI();
                else HideUI();
            }
        }
        else
        {
            uKeyPressed = false;
        }
    }

    public void ShowUI()
    {
        UpdateUI();
        panel.Visible = true;
        GetTree().Paused = true; // หยุดเกมเวลาเข้าร้านค้า
    }

    public void HideUI()
    {
        panel.Visible = false;
        GetTree().Paused = false;
    }

    private void UpdateUI()
    {
        coinLabel.Text = $"Coins: {GlobalData.Coins}";

        int hpCost = baseHpCost + (GlobalData.HpUpgradeLevel * 5);
        int dmgCost = baseDmgCost + (GlobalData.DamageUpgradeLevel * 10);
        int speedCost = baseSpeedCost + (GlobalData.SpeedUpgradeLevel * 5);
        int dashCost = baseDashCost + (GlobalData.DashUpgradeLevel * 5);

        hpBtn.Text = $"Max HP +50 (Level {GlobalData.HpUpgradeLevel}) - Cost: {hpCost}";
        dmgBtn.Text = $"Damage +1 (Level {GlobalData.DamageUpgradeLevel}) - Cost: {dmgCost}";
        speedBtn.Text = $"Speed +10% (Level {GlobalData.SpeedUpgradeLevel}) - Cost: {speedCost}";
        dashBtn.Text = $"Dash Cooldown -10% (Level {GlobalData.DashUpgradeLevel}) - Cost: {dashCost}";
        
        // เช็คว่าเงินพอไหม ถ้าไม่พอก็ลด Opacity ลง
        hpBtn.Disabled = GlobalData.Coins < hpCost;
        dmgBtn.Disabled = GlobalData.Coins < dmgCost;
        speedBtn.Disabled = GlobalData.Coins < speedCost;
        dashBtn.Disabled = GlobalData.Coins < dashCost;
    }

    private void BuyUpgrade(string type)
    {
        int cost = 0;
        if (type == "HP") cost = baseHpCost + (GlobalData.HpUpgradeLevel * 5);
        else if (type == "DMG") cost = baseDmgCost + (GlobalData.DamageUpgradeLevel * 10);
        else if (type == "SPEED") cost = baseSpeedCost + (GlobalData.SpeedUpgradeLevel * 5);
        else if (type == "DASH") cost = baseDashCost + (GlobalData.DashUpgradeLevel * 5);

        if (GlobalData.Coins >= cost)
        {
            GlobalData.Coins -= cost;
            if (type == "HP") GlobalData.HpUpgradeLevel++;
            else if (type == "DMG") GlobalData.DamageUpgradeLevel++;
            else if (type == "SPEED") GlobalData.SpeedUpgradeLevel++;
            else if (type == "DASH") GlobalData.DashUpgradeLevel++;
            
            UpdateUI(); // อัปเดตราคาใหม่และปุ่มทันที
            GD.Print($"ซื้อ {type} สำเร็จ! (เงินเหลือ {GlobalData.Coins})");
        }
    }
}
