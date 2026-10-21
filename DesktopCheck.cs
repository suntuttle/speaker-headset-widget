using System;using System.Runtime.InteropServices;using System.Diagnostics;using System.Text;
class DesktopCheck {
 [DllImport("user32.dll")]static extern IntPtr GetThreadDesktop(uint id);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern bool GetUserObjectInformation(IntPtr h,int index,StringBuilder s,int n,out int need);
 static void Main(){foreach(string name in new[]{"SpeakerWidget","explorer"})foreach(var p in Process.GetProcessesByName(name)){foreach(ProcessThread t in p.Threads){var s=new StringBuilder(512);int n;if(GetUserObjectInformation(GetThreadDesktop((uint)t.Id),2,s,1024,out n)) {Console.WriteLine(name+" pid="+p.Id+" thread="+t.Id+" desktop="+s);break;}}}}
}
