using System;

public static class GlobalData
{
    // ตัวแปรแบบ static จะไม่ถูกทำลายเมื่อเปลี่ยนฉาก
    
    // เหรียญทองที่มีอยู่ปัจจุบัน
    public static int Coins = 0;
    
    // อัปเกรดถาวร (Levels)
    public static int HpUpgradeLevel = 0;
    public static int DamageUpgradeLevel = 0;
    public static int SpeedUpgradeLevel = 0;
    public static int DashUpgradeLevel = 0;
}
