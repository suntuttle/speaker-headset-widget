using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;

class TrayPresenceCheck
{
    delegate bool Callback(IntPtr window,IntPtr parameter);
    [StructLayout(LayoutKind.Sequential)] struct Identifier { public uint cbSize; public IntPtr hWnd; public uint uID; public Guid guidItem; }
    [StructLayout(LayoutKind.Sequential)] struct Rect { public int Left,Top,Right,Bottom; }
    [DllImport("user32.dll")] static extern bool EnumWindows(Callback callback,IntPtr parameter);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr window,out uint process);
    [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr window);
    [DllImport("user32.dll")] static extern bool PostMessage(IntPtr window,uint message,IntPtr wParam,IntPtr lParam);
    [DllImport("user32.dll")] static extern bool GetCursorPos(out Point point);
    [DllImport("user32.dll")] static extern bool SetCursorPos(int x,int y);
    [DllImport("user32.dll")] static extern void mouse_event(uint flags,uint dx,uint dy,int data,UIntPtr extra);
    [StructLayout(LayoutKind.Sequential)] struct Point { public int X,Y; }
    [DllImport("shell32.dll")] static extern int Shell_NotifyIconGetRect(ref Identifier identifier,out Rect rect);
    static void Main(string[] args)
    {try{Run(args);}catch(Exception ex){Console.WriteLine(ex.GetType().Name+": "+ex.Message);Environment.ExitCode=1;}}
    static void Run(string[] args)
    {
        uint target=uint.Parse(args[0]); bool visible=false,icon=false;Rect iconRect=new Rect();var windows=new List<IntPtr>();
        EnumWindows(delegate(IntPtr window,IntPtr unused){uint process;GetWindowThreadProcessId(window,out process);if(process!=target)return true;windows.Add(window);if(IsWindowVisible(window))visible=true;var id=new Identifier{cbSize=(uint)Marshal.SizeOf(typeof(Identifier)),hWnd=window,uID=1,guidItem=new Guid("716AA7A5-BB21-4FB0-84BB-90F421AC709A")};Rect rect;if(Shell_NotifyIconGetRect(ref id,out rect)==0&&rect.Right>rect.Left){icon=true;iconRect=rect;}return true;},IntPtr.Zero);
        if(visible)throw new Exception("A visible desktop widget still exists");
        if(!icon)throw new Exception("Notification-area icon was not registered");
        Console.WriteLine("PASS: no visible desktop widget");
        Console.WriteLine("PASS: notification-area icon registered");
        string before=Audio.DefaultId(0);
        IntPtr iconWindow=IntPtr.Zero;
        foreach(IntPtr window in windows){PostMessage(window,0x8000+41,IntPtr.Zero,new IntPtr(0x0202));System.Threading.Thread.Sleep(100);if(Audio.DefaultId(0)!=before){iconWindow=window;break;}}
        if(iconWindow==IntPtr.Zero)throw new Exception("Tray left click did not switch output");
        Console.WriteLine("PASS: tray left click switches output");
        PostMessage(iconWindow,0x8000+41,IntPtr.Zero,new IntPtr(0x0202));
        System.Threading.Thread.Sleep(250);
        if(Audio.DefaultId(0)!=before)throw new Exception("Second tray click did not restore output");
        Console.WriteLine("PASS: second tray click restores output");
        int muteVolume;bool muteBeforeClick;Audio.ReadVolume(out muteVolume,out muteBeforeClick);PostMessage(iconWindow,0x8000+41,IntPtr.Zero,new IntPtr(0x0208));System.Threading.Thread.Sleep(150);bool muteAfterClick;Audio.ReadVolume(out muteVolume,out muteAfterClick);if(muteAfterClick==muteBeforeClick)throw new Exception("Tray middle click did not toggle mute");PostMessage(iconWindow,0x8000+41,IntPtr.Zero,new IntPtr(0x0208));System.Threading.Thread.Sleep(150);Console.WriteLine("PASS: tray middle click toggles mute");
        int volumeBefore;bool muteBefore;Audio.ReadVolume(out volumeBefore,out muteBefore);int direction=volumeBefore<=95?1:-1;Point cursor;GetCursorPos(out cursor);
        try{SetCursorPos((iconRect.Left+iconRect.Right)/2,(iconRect.Top+iconRect.Bottom)/2);mouse_event(0x0800,0,0,direction*120,UIntPtr.Zero);System.Threading.Thread.Sleep(250);int volumeAfter;bool muted;Audio.ReadVolume(out volumeAfter,out muted);if(volumeAfter==volumeBefore)throw new Exception("Wheel over tray icon did not change volume");Console.WriteLine("PASS: wheel over tray icon changes volume");Audio.ChangeVolume(volumeBefore-volumeAfter);Audio.ReadVolume(out volumeAfter,out muted);if(muted!=muteBefore)Audio.ToggleMute();}
        finally{SetCursorPos(cursor.X,cursor.Y);}
    }
}
