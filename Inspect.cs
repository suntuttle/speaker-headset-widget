using System;using System.Runtime.InteropServices;using System.Text;
public class WindowInspect {
 public delegate bool Callback(IntPtr h,IntPtr p);
 [DllImport("user32.dll")] public static extern bool EnumWindows(Callback c,IntPtr p);
 [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h,out uint p);
 [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr h);
 [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h,out Rect r);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] public static extern int GetWindowText(IntPtr h,StringBuilder s,int n);
 public struct Rect{public int L,T,R,B;}
 public static void Dump(int pid){EnumWindows((h,p)=>{uint id;GetWindowThreadProcessId(h,out id);if(id==pid){Rect r;GetWindowRect(h,out r);var s=new StringBuilder(256);GetWindowText(h,s,256);Console.WriteLine(h+" Visible="+IsWindowVisible(h)+" Rect="+r.L+","+r.T+","+r.R+","+r.B+" Text="+s);}return true;},IntPtr.Zero);}
}
