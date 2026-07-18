using System;

public static class GlobalData
{
    // ตัวแปรแบบ static จะไม่ถูกทำลายเมื่อเปลี่ยนฉาก หรือ ReloadCurrentScene()
    
    // เหรียญทองที่มีอยู่ปัจจุบัน
    public static int Coins = 0;
    
    // อัปเกรดถาวร: โบนัสเลือดสูงสุด (ยิ่งอัปเกรดยิ่งเริ่มเกมมาเลือดเยอะ)
    public static int PermanentBonusHp = 0;
}
