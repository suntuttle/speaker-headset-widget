using System;

class TrayIntegrationCheck
{
    static void Check(bool condition,string message){if(!condition)throw new Exception(message);Console.WriteLine("PASS: "+message);}
    [STAThread]
    static void Main()
    {
        int before; bool mutedBefore;
        Audio.ReadVolume(out before,out mutedBefore);
        int delta=before<=95?5:-5;
        int changed=Audio.ChangeVolume(delta);
        Check(changed!=before,"wheel path changes default output volume");
        int readBack; bool muted;
        Audio.ReadVolume(out readBack,out muted);
        Check(Math.Abs(readBack-changed)<=1,"changed volume can be read back");
        bool toggled=Audio.ToggleMute();
        Audio.ReadVolume(out readBack,out muted);
        Check(muted==toggled,"middle-click path toggles mute");
        Audio.ChangeVolume(before-changed);
        Audio.ReadVolume(out readBack,out muted);
        if(muted!=mutedBefore)Audio.ToggleMute();
        Audio.ReadVolume(out readBack,out muted);
        Check(Math.Abs(readBack-before)<=1&&muted==mutedBefore,"original volume and mute state restored");
    }
}
