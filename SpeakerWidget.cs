using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Reflection;

[assembly: AssemblyTitle("Speaker Headset Switcher")]
[assembly: AssemblyDescription("Lightweight Windows tray audio switcher and volume controller")]
[assembly: AssemblyCompany("suntuttle")]
[assembly: AssemblyProduct("Speaker Headset Switcher")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[StructLayout(LayoutKind.Sequential)] public struct Key { public Guid fmt; public uint pid; public Key(string f,uint p){fmt=new Guid(f);pid=p;} }
[StructLayout(LayoutKind.Explicit,Size=24)] public struct PV { [FieldOffset(0)] public ushort vt; [FieldOffset(8)] public IntPtr ptr; [FieldOffset(8)] public uint number; }
[ComImport,Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface Enumerator {
 void EnumAudioEndpoints(int flow,uint mask,out Collection devices); void GetDefaultAudioEndpoint(int flow,int role,out Device device); void GetDevice([MarshalAs(UnmanagedType.LPWStr)]string id,out Device device);
}
[ComImport,Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface Collection { void GetCount(out uint count); void Item(uint index,out Device device); }
[ComImport,Guid("D666063F-1587-4E43-81F1-B948E807363F"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface Device { void Activate(ref Guid id,uint context,IntPtr parameters,out IntPtr result); void OpenPropertyStore(uint access,out Props store); void GetId([MarshalAs(UnmanagedType.LPWStr)]out string id); void GetState(out uint state); }
[ComImport,Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface Props { void GetCount(out uint count); void GetAt(uint index,out Key key); void GetValue(ref Key key,out PV value); }
[ComImport,Guid("F8679F50-850A-41CF-9C72-430F290290C8"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface Policy {
 void GetMixFormat(); void GetDeviceFormat(); void ResetDeviceFormat(); void SetDeviceFormat(); void GetProcessingPeriod(); void SetProcessingPeriod(); void GetShareMode(); void SetShareMode(); void GetPropertyValue(); void SetPropertyValue(); void SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)]string id,int role); void SetEndpointVisibility();
}
[ComImport,Guid("5CDF2C82-841E-4546-9722-0CF74078229A"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface EndpointVolume {
 void RegisterControlChangeNotify(IntPtr notify); void UnregisterControlChangeNotify(IntPtr notify); void GetChannelCount(out uint count);
 void SetMasterVolumeLevel(float level,IntPtr context); void SetMasterVolumeLevelScalar(float level,IntPtr context);
 void GetMasterVolumeLevel(out float level); void GetMasterVolumeLevelScalar(out float level);
 void SetChannelVolumeLevel(uint channel,float level,IntPtr context); void SetChannelVolumeLevelScalar(uint channel,float level,IntPtr context);
 void GetChannelVolumeLevel(uint channel,out float level); void GetChannelVolumeLevelScalar(uint channel,out float level);
 void SetMute([MarshalAs(UnmanagedType.Bool)]bool mute,IntPtr context); void GetMute([MarshalAs(UnmanagedType.Bool)]out bool mute);
 void GetVolumeStepInfo(out uint step,out uint count); void VolumeStepUp(IntPtr context); void VolumeStepDown(IntPtr context);
 void QueryHardwareSupport(out uint mask); void GetVolumeRange(out float min,out float max,out float increment);
}
public class Endpoint { public string Id,Name,Hardware; public int Kind; public override string ToString(){return Name;} }
public class Preferences { public Endpoint Speaker; public Endpoint Headset; public bool LightTheme; }
static class Audio {
 [DllImport("ole32.dll")] static extern int PropVariantClear(ref PV value);
 public static void Release(object o){if(o!=null && Marshal.IsComObject(o))Marshal.ReleaseComObject(o);}
 static string Read(Props p,string guid,uint pid){PV v=new PV();try{Key k=new Key(guid,pid);p.GetValue(ref k,out v);return v.vt==31?Marshal.PtrToStringUni(v.ptr):v.vt==19?v.number.ToString():"";}finally{PropVariantClear(ref v);}}
 public static List<Endpoint> Scan(out string current){Enumerator e=null;Collection c=null;Device d=null;current=null;var list=new List<Endpoint>();try{e=(Enumerator)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")));try{e.GetDefaultAudioEndpoint(0,0,out d);d.GetId(out current);}catch(COMException){}finally{Release(d);d=null;}e.EnumAudioEndpoints(0,1,out c);uint n;c.GetCount(out n);for(uint i=0;i<n;i++){Props p=null;try{c.Item(i,out d);string id;d.GetId(out id);d.OpenPropertyStore(0,out p);int kind;int.TryParse(Read(p,"1da5d803-d492-4edd-8c23-e0c0ffee7f0e",0),out kind);list.Add(new Endpoint{Id=id,Name=Read(p,"a45c254e-df1c-4efd-8020-67d146a850e0",14),Hardware=Read(p,"026e516e-b814-414b-83cd-856d6fef4822",2),Kind=kind});}finally{Release(p);Release(d);d=null;}}return list;}finally{Release(c);Release(e);}}
 public static string DefaultId(int role){Enumerator e=null;Device d=null;try{e=(Enumerator)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")));e.GetDefaultAudioEndpoint(0,role,out d);string id;d.GetId(out id);return id;}finally{Release(d);Release(e);}} public static void SetRole(string id,int role){object o=Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("870af99c-171d-4f9e-af0d-e63df40c2bc9")));try{((Policy)o).SetDefaultEndpoint(id,role);}finally{Release(o);}} public static void Switch(string id){object o=Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("870af99c-171d-4f9e-af0d-e63df40c2bc9")));try{Policy p=(Policy)o;for(int role=0;role<3;role++)p.SetDefaultEndpoint(id,role);}finally{Release(o);}}
 public static void ReadVolume(out int percent,out bool muted){int localPercent=0;bool localMuted=false;WithVolume(delegate(EndpointVolume volume){float level;volume.GetMasterVolumeLevelScalar(out level);volume.GetMute(out localMuted);localPercent=(int)Math.Round(level*100);});percent=localPercent;muted=localMuted;}
 public static int ChangeVolume(int delta){int result=0;WithVolume(delegate(EndpointVolume volume){float level;volume.GetMasterVolumeLevelScalar(out level);level=Math.Max(0f,Math.Min(1f,level+delta/100f));volume.SetMasterVolumeLevelScalar(level,IntPtr.Zero);if(level>0)volume.SetMute(false,IntPtr.Zero);result=(int)Math.Round(level*100);});return result;}
 public static bool ToggleMute(){bool result=false;WithVolume(delegate(EndpointVolume volume){bool muted;volume.GetMute(out muted);result=!muted;volume.SetMute(result,IntPtr.Zero);});return result;}
 delegate void VolumeAction(EndpointVolume volume);
 static void WithVolume(VolumeAction action){Enumerator e=null;Device d=null;object volumeObject=null;IntPtr pointer=IntPtr.Zero;try{e=(Enumerator)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")));e.GetDefaultAudioEndpoint(0,0,out d);Guid iid=new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");d.Activate(ref iid,23,IntPtr.Zero,out pointer);volumeObject=Marshal.GetObjectForIUnknown(pointer);action((EndpointVolume)volumeObject);}finally{if(pointer!=IntPtr.Zero)Marshal.Release(pointer);Release(volumeObject);Release(d);Release(e);}}
 public static string Normal(string s){return Regex.Replace((s??"").ToLowerInvariant(),@"\b\d+\s*-\s*","").Trim();}
 public static Endpoint Resolve(List<Endpoint> all,Endpoint saved,bool headset){
 if(saved!=null){var exact=all.FirstOrDefault(x=>x.Id==saved.Id);if(exact!=null)return exact;
 var hardware=all.Where(x=>!string.IsNullOrEmpty(saved.Hardware)&&Normal(x.Hardware)==Normal(saved.Hardware)&&x.Kind==saved.Kind).ToList();if(hardware.Count==1)return hardware[0];
 var named=all.Where(x=>Normal(x.Name)==Normal(saved.Name)&&x.Kind==saved.Kind).ToList();if(named.Count==1)return named[0];return null;}
 var candidates=all.Where(x=>headset?(x.Kind==3||x.Kind==5||Regex.IsMatch(x.Name,"headset|headphone|헤드셋|헤드폰",RegexOptions.IgnoreCase)):(x.Kind==1&&!Regex.IsMatch(x.Name,"headset|headphone|헤드셋|헤드폰|streaming|virtual|가상",RegexOptions.IgnoreCase))).ToList();return candidates.Count==1?candidates[0]:null;
 }
}

sealed class TrayController : NativeWindow, IDisposable
{
    const uint NIM_ADD=0, NIM_MODIFY=1, NIM_DELETE=2, NIM_SETVERSION=4;
    const uint NIF_MESSAGE=1, NIF_ICON=2, NIF_TIP=4, NIF_GUID=0x20, NIF_SHOWTIP=0x80, NOTIFYICON_VERSION_4=4;
    const int WM_APP=0x8000, WM_CONTEXTMENU=0x007B, WM_LBUTTONUP=0x0202, WM_RBUTTONUP=0x0205, WM_MBUTTONUP=0x0208, WM_MOUSEWHEEL=0x020A, WH_MOUSE_LL=14;
    static readonly Guid IconGuid=new Guid("716AA7A5-BB21-4FB0-84BB-90F421AC709A");
    readonly HookProc hookProc;
    readonly uint taskbarCreated;
    IntPtr hook;
    Icon icon;
    string text="스피커 · 헤드셋 전환";
    public event Action LeftClick,MiddleClick,RightClick;
    public event Action<int> Wheel;

    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] struct NotifyData {
        public uint cbSize; public IntPtr hWnd; public uint uID,uFlags,uCallbackMessage; public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr,SizeConst=128)] public string szTip;
        public uint dwState,dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr,SizeConst=256)] public string szInfo;
        public uint uVersion;
        [MarshalAs(UnmanagedType.ByValTStr,SizeConst=64)] public string szInfoTitle;
        public uint dwInfoFlags; public Guid guidItem; public IntPtr hBalloonIcon;
    }
    [StructLayout(LayoutKind.Sequential)] struct Identifier { public uint cbSize; public IntPtr hWnd; public uint uID; public Guid guidItem; }
    [StructLayout(LayoutKind.Sequential)] struct NativeRect { public int Left,Top,Right,Bottom; }
    [StructLayout(LayoutKind.Sequential)] struct Point { public int X,Y; }
    [StructLayout(LayoutKind.Sequential)] struct MouseData { public Point pt; public uint mouseData,flags,time; public IntPtr extra; }
    delegate IntPtr HookProc(int code,IntPtr message,IntPtr data);
    [DllImport("shell32.dll",CharSet=CharSet.Unicode)] static extern bool Shell_NotifyIcon(uint message,ref NotifyData data);
    [DllImport("shell32.dll")] static extern int Shell_NotifyIconGetRect(ref Identifier identifier,out NativeRect rect);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern uint RegisterWindowMessage(string name);
    [DllImport("user32.dll")] static extern IntPtr SetWindowsHookEx(int id,HookProc callback,IntPtr module,uint thread);
    [DllImport("user32.dll")] static extern bool UnhookWindowsHookEx(IntPtr hook);
    [DllImport("user32.dll")] static extern IntPtr CallNextHookEx(IntPtr hook,int code,IntPtr message,IntPtr data);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode)] static extern IntPtr GetModuleHandle(string name);

    public TrayController()
    {
        CreateHandle(new CreateParams());
        taskbarCreated=RegisterWindowMessage("TaskbarCreated");
        hookProc=MouseHook;
        hook=SetWindowsHookEx(WH_MOUSE_LL,hookProc,GetModuleHandle(null),0);
    }
    public void Update(Icon next,string tooltip)
    {
        Icon previous=icon; icon=next; text=string.IsNullOrEmpty(tooltip)?"스피커 · 헤드셋 전환":tooltip;
        NotifyData data=Data();
        if(!Shell_NotifyIcon(NIM_MODIFY,ref data)) Add();
        if(previous!=null)previous.Dispose();
    }
    void Add()
    {
        NotifyData data=Data(); Shell_NotifyIcon(NIM_ADD,ref data);
        data.uVersion=NOTIFYICON_VERSION_4; Shell_NotifyIcon(NIM_SETVERSION,ref data);
    }
    NotifyData Data(){return new NotifyData{cbSize=(uint)Marshal.SizeOf(typeof(NotifyData)),hWnd=Handle,uID=1,uFlags=NIF_MESSAGE|NIF_ICON|NIF_TIP|NIF_GUID|NIF_SHOWTIP,uCallbackMessage=WM_APP+41,hIcon=icon==null?SystemIcons.Application.Handle:icon.Handle,szTip=text.Length>127?text.Substring(0,127):text,guidItem=IconGuid};}
    protected override void WndProc(ref Message message)
    {
        if(message.Msg==taskbarCreated){Add();return;}
        if(message.Msg==WM_APP+41){int action=(int)((long)message.LParam&0xffff);if(action==WM_LBUTTONUP&&LeftClick!=null)LeftClick();else if(action==WM_MBUTTONUP&&MiddleClick!=null)MiddleClick();else if((action==WM_RBUTTONUP||action==WM_CONTEXTMENU)&&RightClick!=null)RightClick();return;}
        base.WndProc(ref message);
    }
    IntPtr MouseHook(int code,IntPtr message,IntPtr data)
    {
        if(code>=0&&(int)message==WM_MOUSEWHEEL&&Wheel!=null)
        {
            MouseData mouse=(MouseData)Marshal.PtrToStructure(data,typeof(MouseData));
            Identifier id=new Identifier{cbSize=(uint)Marshal.SizeOf(typeof(Identifier)),hWnd=Handle,uID=1,guidItem=IconGuid};
            NativeRect native;
            if(Shell_NotifyIconGetRect(ref id,out native)==0&&Rectangle.FromLTRB(native.Left,native.Top,native.Right,native.Bottom).Contains(mouse.pt.X,mouse.pt.Y))
            {short delta=unchecked((short)(mouse.mouseData>>16));if(delta!=0)Wheel(delta>0?1:-1);return new IntPtr(1);}
        }
        return CallNextHookEx(hook,code,message,data);
    }
    public void Dispose()
    {
        NotifyData data=Data();Shell_NotifyIcon(NIM_DELETE,ref data);
        if(hook!=IntPtr.Zero){UnhookWindowsHookEx(hook);hook=IntPtr.Zero;}
        if(icon!=null){icon.Dispose();icon=null;}
        DestroyHandle();
    }
}

class Widget : Form
{
    readonly TrayController tray = new TrayController();
    readonly System.Threading.EventWaitHandle restore = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset, Program.InstanceName + "Restore");
    readonly ToolTip tip = new ToolTip { InitialDelay = 550, ReshowDelay = 150, AutoPopDelay = 6000 };
    readonly Timer timer = new Timer();
    readonly Timer backdropTimer = new Timer { Interval = 350 };
    readonly Timer collapseTimer = new Timer { Interval = 900 };
    readonly Font headingFont = new Font("Malgun Gothic", 10.5f, FontStyle.Bold);
    readonly Font captionFont = new Font("Segoe UI", 8.5f, FontStyle.Regular);
    readonly string config = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "devices.xml");
    Preferences prefs = new Preferences();
    List<Endpoint> devices = new List<Endpoint>();
    string current, heading = "출력 확인 중", caption = "잠시만 기다려 주세요", tooltip = "";
    Endpoint speaker, headset;
    bool hover, pressed, isHeadset, available, expanded;
    readonly float scale;
    ContextMenuStrip menu;

    public Widget()
    {
        using (Graphics g = CreateGraphics()) scale = g.DpiX / 96f;
        AutoScaleMode = AutoScaleMode.None;
        Text = "스피커 · 헤드셋 전환";
        AccessibleRole = AccessibleRole.PushButton;
        AccessibleName = Text;
        AccessibleDescription = "클릭하면 위젯을 펼칩니다. 펼친 상태에서 클릭하거나 Enter 또는 Space 키로 출력을 전환합니다. Escape 키로 접습니다.";
        FormBorderStyle = FormBorderStyle.None;
        ClientSize = new Size((int)(48 * scale), (int)(48 * scale));
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        ShowInTaskbar = false;
        DoubleBuffered = true;
        SetStyle(ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
        Cursor = Cursors.Hand;
        try
        {
            if (File.Exists(config)) using (var f = File.OpenRead(config)) prefs = (Preferences)new XmlSerializer(typeof(Preferences)).Deserialize(f);
        }
        catch { }
        ApplyTheme();
        UpdateShape();
        menu = new ContextMenuStrip { ShowImageMargin = false, Font = new Font("Malgun Gothic", 9f), Padding = new Padding(5) };
        menu.Opening += (s, e) => { collapseTimer.Stop(); BuildMenu(menu); };
        menu.Closed += (s, e) => { if (expanded) collapseTimer.Start(); };
        collapseTimer.Tick += (s, e) => TryCollapse();
        tray.LeftClick += Toggle;
        tray.MiddleClick += ToggleMute;
        tray.RightClick += ShowTrayMenu;
        tray.Wheel += AdjustVolume;
        timer.Interval = 3000;
        timer.Tick += (s, e) => { restore.WaitOne(0); RefreshState(); };
        Shown += (s, e) => { Hide(); RefreshState(); timer.Start(); };
    }

    bool Light { get { return prefs.LightTheme; } }
    Color Surface { get { return Light ? Color.FromArgb(247, 248, 250) : Color.FromArgb(30, 33, 39); } }
    Color Ink { get { return Light ? Color.FromArgb(36, 43, 51) : Color.FromArgb(237, 240, 243); } }
    Color Muted { get { return Light ? Color.FromArgb(98, 108, 120) : Color.FromArgb(162, 171, 184); } }
    Color Accent { get { return !available ? Muted : isHeadset ? (Light ? Color.FromArgb(108, 83, 159) : Color.FromArgb(196, 179, 230)) : (Light ? Color.FromArgb(44, 117, 98) : Color.FromArgb(156, 206, 186)); } }
    void ApplyTheme() { BackColor = Surface; Invalidate(); }

    static System.Drawing.Drawing2D.GraphicsPath Round(RectangleF r, float radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        float d = radius * 2;
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        // Geometry uses logical pixels; fonts are already scaled by the output device DPI.
        g.ScaleTransform(scale, scale);
        Color bg = pressed ? (Light ? Color.FromArgb(229, 233, 239) : Color.FromArgb(43, 48, 56)) : hover ? (Light ? Color.FromArgb(240, 243, 247) : Color.FromArgb(36, 40, 47)) : Surface;
        using (var path = Round(new RectangleF(.5f, .5f, expanded ? 247 : 47, expanded ? 67 : 47), expanded ? 19.5f : 15.5f))
        using (var brush = new SolidBrush(bg))
        using (var border = new Pen(Light ? Color.FromArgb(213, 219, 227) : Color.FromArgb(64, 70, 80), 1))
        { g.FillPath(brush, path); g.DrawPath(border, path); }
        if (!expanded) { DrawDeviceIcon(g, isHeadset, Accent, 14, 14); g.ResetTransform(); return; }
        using (var fill = new SolidBrush(Color.FromArgb(Light ? 17 : 22, Accent)))
        using (var iconBox = Round(new RectangleF(14, 15, 38, 38), 13)) g.FillPath(fill, iconBox);
        DrawDeviceIcon(g, isHeadset, Accent, 23, 24);
        using (var line = new Pen(Light ? Color.FromArgb(217, 223, 230) : Color.FromArgb(58, 64, 73), 1)) g.DrawLine(line, 202, 23, 202, 45);
        if (hover || pressed)
            using (var fill = new SolidBrush(Color.FromArgb(Light ? 16 : 23, Accent)))
            using (var action = Round(new RectangleF(208, 20, 28, 28), 10)) g.FillPath(fill, action);
        using (var pen = new Pen(hover ? Accent : Muted, 1.5f))
        {
            pen.StartCap = pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
            g.DrawLines(pen, new PointF[] { new PointF(215, 30), new PointF(229, 30), new PointF(225.5f, 26.5f) });
            g.DrawLines(pen, new PointF[] { new PointF(229, 38), new PointF(215, 38), new PointF(218.5f, 41.5f) });
        }
        g.ResetTransform();
        TextRenderer.DrawText(g, heading, headingFont, Scaled(64, 11, 132, 25), Ink, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(g, caption, captionFont, Scaled(64, 36, 132, 19), Muted, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
    }
    Rectangle Scaled(int x, int y, int w, int h) { return new Rectangle((int)(x * scale), (int)(y * scale), (int)(w * scale), (int)(h * scale)); }
    static void DrawDeviceIcon(Graphics g, bool headphones, Color color, float x, float y)
    {
        using (var p = new Pen(color, 1.7f))
        {
            p.StartCap = p.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            p.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
            if (headphones)
            {
                g.DrawArc(p, x + 1, y + 1, 18, 18, 180, 180);
                using (var left = Round(new RectangleF(x + 1, y + 10, 4, 8), 1.7f)) g.DrawPath(p, left);
                using (var right = Round(new RectangleF(x + 15, y + 10, 4, 8), 1.7f)) g.DrawPath(p, right);
            }
            else
            {
                g.DrawPolygon(p, new PointF[] { new PointF(x, y + 7), new PointF(x + 4, y + 7), new PointF(x + 9, y + 3), new PointF(x + 9, y + 17), new PointF(x + 4, y + 13), new PointF(x, y + 13) });
                g.DrawArc(p, x + 7, y + 5, 9, 10, -55, 110);
                g.DrawArc(p, x + 7, y + 1, 15, 18, -50, 100);
            }
        }
    }
    Icon MakeTrayIcon(bool headphones,bool muted)
    {
        using(var bitmap=new Bitmap(32,32))
        using(var graphics=Graphics.FromImage(bitmap))
        {
            graphics.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.Clear(Color.Transparent);
            Color color=!available?Color.FromArgb(150,150,150):headphones?Color.FromArgb(184,157,229):Color.FromArgb(112,194,163);
            DrawDeviceIcon(graphics,headphones,color,5,6);
            if(muted)using(var pen=new Pen(Color.FromArgb(235,100,105),2.3f)){pen.StartCap=pen.EndCap=System.Drawing.Drawing2D.LineCap.Round;graphics.DrawLine(pen,6,25,26,5);}
            IntPtr handle=bitmap.GetHicon();
            try{return (Icon)Icon.FromHandle(handle).Clone();}finally{DestroyIcon(handle);}
        }
    }
    [DllImport("user32.dll")] static extern bool DestroyIcon(IntPtr icon);
    void ShowTrayMenu(){menu.Show(Cursor.Position);}
    void AdjustVolume(int direction)
    {
        try{Audio.ChangeVolume(direction*5);RefreshState();}
        catch(Exception ex){MessageBox.Show("볼륨을 조절하지 못했습니다: "+ex.Message,Text);}
    }
    void ToggleMute()
    {
        try{Audio.ToggleMute();RefreshState();}
        catch(Exception ex){MessageBox.Show("음소거를 전환하지 못했습니다: "+ex.Message,Text);}
    }
    protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); hover = true; collapseTimer.Stop(); Invalidate(); }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); hover = pressed = false; if (expanded) collapseTimer.Start(); Invalidate(); }
    protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); if (e.Button == MouseButtons.Left) { pressed = true; Invalidate(); } }
    protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); bool clicked = pressed && ClientRectangle.Contains(e.Location); pressed = false; Invalidate(); if (clicked && e.Button == MouseButtons.Left) ActivateWidget(); }
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Enter || keyData == Keys.Space) { ActivateWidget(); return true; }
        if (keyData == Keys.Escape && expanded) { SetExpanded(false); return true; }
        if (keyData == (Keys.Shift | Keys.F10) || keyData == Keys.Apps) { menu.Show(this, new Point(0, Height)); return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }
    void ActivateWidget() { if (!expanded) SetExpanded(true); else Toggle(); }
    void UpdateShape()
    {
        Region previous = Region;
        using (var path = Round(new RectangleF(0, 0, Width, Height), (expanded ? 20 : 16) * scale)) Region = new Region(path);
        if (previous != null) previous.Dispose();
    }
    void SetExpanded(bool value)
    {
        collapseTimer.Stop();
        expanded = value;
        var area = Screen.PrimaryScreen.WorkingArea;
        int width = (int)((expanded ? 248 : 48) * scale), height = (int)((expanded ? 68 : 48) * scale);
        SetBounds(area.Right - width - (int)(24 * scale), area.Top + (int)(24 * scale), width, height);
        UpdateShape();
        RefreshState();
        Invalidate();
    }
    void TryCollapse()
    {
        if (menu.Visible || Capture) return;
        collapseTimer.Stop();
        if (!Bounds.Contains(Cursor.Position)) SetExpanded(false);
    }
    void RestoreWidget() { WindowState = FormWindowState.Normal; Show(); Position(); BringToFront(); Activate(); }
    void UpdateBackdrop()
    {
        if (!IsHandleCreated || !Visible) return;
        if (!TopMost) TopMost = true;
        WindowBackdrop.EnsureTopmost(Handle);
        double target = WindowBackdrop.HasAppBehind(Handle, Bounds) ? 0.10 : 1.0;
        if (Math.Abs(Opacity - target) > 0.001) Opacity = target;
    }
    void Position() { var area = Screen.PrimaryScreen.WorkingArea; Location = new Point(area.Right - Width - (int)(24 * scale), area.Top + (int)(24 * scale)); }
    void Save()
    {
        try { using (var f = File.Create(config)) new XmlSerializer(typeof(Preferences)).Serialize(f, prefs); }
        catch (Exception ex) { MessageBox.Show("장치 설정 저장 실패: " + ex.Message); }
    }
    static string ShortName(Endpoint device)
    {
        if (device == null) return "장치를 연결해 주세요";
        string name = string.IsNullOrWhiteSpace(device.Hardware) ? device.Name : device.Hardware;
        name = Regex.Replace(name, @"\b\d+\s*-\s*", "").Trim();
        return name.Replace("High Definition Audio", "Audio").Replace("High Definition", "HD");
    }
    void RefreshState()
    {
        string oldHeading = heading, oldCaption = caption;
        bool oldHeadset = isHeadset, oldAvailable = available;
        try
        {
            devices = Audio.Scan(out current);
            speaker = Audio.Resolve(devices, prefs.Speaker, false);
            headset = Audio.Resolve(devices, prefs.Headset, true);
            if (speaker != null && headset != null && speaker.Id == headset.Id) { speaker = null; headset = null; }
            if (speaker != null && headset != null && (prefs.Speaker == null || prefs.Headset == null || prefs.Speaker.Id != speaker.Id || prefs.Headset.Id != headset.Id))
            { prefs.Speaker = speaker; prefs.Headset = headset; Save(); }
            var active = devices.FirstOrDefault(x => x.Id == current);
            available = active != null;
            isHeadset = headset != null && headset.Id == current;
            heading = active == null ? "연결 대기" : isHeadset ? "헤드셋" : speaker != null && speaker.Id == current ? "스피커" : "다른 출력";
            caption = ShortName(active);
            string nextTip = (active == null ? "출력 장치 없음" : active.Name) + "\n" + (!expanded ? "클릭 → 위젯 펼치기" : speaker == null || headset == null ? "우클릭 → 스피커 / 헤드셋 지정" : "클릭 → " + (isHeadset ? "스피커" : "헤드셋") + "로 전환") + "\n우클릭: 설정";
            if (tooltip != nextTip) { tooltip = nextTip; tip.SetToolTip(this, tooltip); }
            AccessibleName = heading + ", " + caption + (expanded ? ", 출력 전환" : ", 위젯 펼치기");
            int volume;bool muted;Audio.ReadVolume(out volume,out muted);
            string volumeTip=(active==null?"출력 장치 없음":active.Name)+": "+volume+"%"+(muted?" (음소거)":"");
            tray.Update(MakeTrayIcon(isHeadset,muted),volumeTip);
        }
        catch (Exception ex) { heading = "연결 확인"; caption = "잠시 후 다시 확인합니다"; available = false; tip.SetToolTip(this, ex.Message); }
        if (oldHeading != heading || oldCaption != caption || oldHeadset != isHeadset || oldAvailable != available) Invalidate();
    }
    void Toggle()
    {
        RefreshState();
        if (speaker == null || headset == null) { MessageBox.Show("스피커와 헤드셋을 모두 연결한 뒤 우클릭 메뉴에서 각각 지정해 주세요.\n장치 종류가 명확하면 자동으로 인식합니다.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        try { Audio.Switch(current == headset.Id ? speaker.Id : headset.Id); RefreshState(); }
        catch (Exception ex) { MessageBox.Show("전환하지 못했습니다: " + ex.Message, Text); RefreshState(); }
    }
    void BuildMenu(ContextMenuStrip target)
    {
        RefreshState();
        // Dispose old submenus and their event handlers before rebuilding.
        while (target.Items.Count > 0) { var item = target.Items[0]; target.Items.RemoveAt(0); item.Dispose(); }
        target.Renderer = new ToolStripProfessionalRenderer(new WidgetMenuColors(Light));
        target.BackColor = Surface;
        target.ForeColor = Ink;
        foreach (bool isTargetHeadset in new[] { false, true })
        {
            bool h = isTargetHeadset;
            var sub = new ToolStripMenuItem(h ? "헤드셋 지정" : "스피커 지정");
            Endpoint selected = h ? headset : speaker;
            foreach (var d in devices)
            {
                Endpoint chosen = d;
                var item = new ToolStripMenuItem(d.Name) { Checked = selected != null && selected.Id == d.Id };
                item.Click += (s, e) => { if (h) prefs.Headset = chosen; else prefs.Speaker = chosen; Save(); RefreshState(); };
                sub.DropDownItems.Add(item);
            }
            sub.DropDown.BackColor = Surface; sub.DropDown.ForeColor = Ink;
            target.Items.Add(sub);
        }
        target.Items.Add("자동 인식으로 초기화", null, (s, e) => { prefs.Speaker = prefs.Headset = null; Save(); RefreshState(); });
        target.Items.Add(new ToolStripSeparator());
        target.Items.Add("Windows 소리 설정", null, (s, e) => System.Diagnostics.Process.Start("ms-settings:sound"));
        target.Items.Add(new ToolStripSeparator());
        target.Items.Add("종료", null, (s, e) => Close());
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            timer.Dispose(); backdropTimer.Dispose(); collapseTimer.Dispose(); tip.Dispose(); tray.Dispose(); restore.Dispose();
            headingFont.Dispose(); captionFont.Dispose(); if (menu != null) menu.Dispose();
        }
        base.Dispose(disposing);
    }
}

class WidgetMenuColors : ProfessionalColorTable
{
    readonly bool light;
    public WidgetMenuColors(bool lightTheme) { light = lightTheme; UseSystemColors = false; }
    Color Background { get { return light ? Color.FromArgb(247,248,250) : Color.FromArgb(30,33,39); } }
    Color Selection { get { return light ? Color.FromArgb(226,232,239) : Color.FromArgb(52,59,68); } }
    Color Border { get { return light ? Color.FromArgb(213,219,227) : Color.FromArgb(64,70,80); } }
    public override Color ToolStripDropDownBackground { get { return Background; } }
    public override Color MenuItemSelected { get { return Selection; } }
    public override Color MenuItemSelectedGradientBegin { get { return Selection; } }
    public override Color MenuItemSelectedGradientEnd { get { return Selection; } }
    public override Color MenuItemPressedGradientBegin { get { return Selection; } }
    public override Color MenuItemPressedGradientMiddle { get { return Selection; } }
    public override Color MenuItemPressedGradientEnd { get { return Selection; } }
    public override Color MenuItemBorder { get { return Selection; } }
    public override Color MenuBorder { get { return Border; } }
    public override Color SeparatorDark { get { return Border; } }
    public override Color SeparatorLight { get { return Background; } }
    public override Color CheckBackground { get { return Selection; } }
    public override Color CheckSelectedBackground { get { return Selection; } }
    public override Color CheckPressedBackground { get { return Selection; } }
}
// Read the windows actually beneath this widget, rather than the foreground app.
static class WindowBackdrop
{
    delegate bool EnumCallback(IntPtr window, IntPtr param);
    [StructLayout(LayoutKind.Sequential)] struct NativeRect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] static extern bool EnumWindows(EnumCallback callback, IntPtr param);
    [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr window);
    [DllImport("user32.dll")] static extern bool IsIconic(IntPtr window);
    [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr window, out NativeRect rect);
    [DllImport("user32.dll")] static extern IntPtr GetWindow(IntPtr window, uint command);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int GetClassName(IntPtr window, System.Text.StringBuilder name, int length);
    [DllImport("dwmapi.dll")] static extern int DwmGetWindowAttribute(IntPtr window, int attribute, out int value, int size);
    [DllImport("dwmapi.dll", EntryPoint = "DwmGetWindowAttribute")] static extern int GetFrameBounds(IntPtr window, int attribute, out NativeRect value, int size);

    [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr window, int index);
    [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr window, IntPtr after, int x, int y, int width, int height, uint flags);
    public static void EnsureTopmost(IntPtr window)
    {
        if ((GetWindowLong(window, -20) & 8) == 0)
            SetWindowPos(window, new IntPtr(-1), 0, 0, 0, 0, 0x13);
    }
    public static bool HasAppBehind(IntPtr widget, Rectangle bounds)
    {
        bool found = false;
        EnumWindows(delegate(IntPtr window, IntPtr unused)
        {
            if (window == widget) return true;
            if (!IsWindowVisible(window) || IsIconic(window)) return true;
            for (IntPtr owner = GetWindow(window, 4); owner != IntPtr.Zero; owner = GetWindow(owner, 4))
                if (owner == widget) return true;
            int cloaked;
            if (DwmGetWindowAttribute(window, 14, out cloaked, 4) == 0 && cloaked != 0) return true;
            NativeRect native;
            if (GetFrameBounds(window, 9, out native, Marshal.SizeOf(typeof(NativeRect))) != 0 || native.Right <= native.Left || native.Bottom <= native.Top)
                if (!GetWindowRect(window, out native)) return true;
            Rectangle rect = Rectangle.FromLTRB(native.Left, native.Top, native.Right, native.Bottom);
            if (!rect.IntersectsWith(bounds)) return true;
            var name = new System.Text.StringBuilder(256);
            GetClassName(window, name, name.Capacity);
            string kind = name.ToString();
            if (kind == "Progman" || kind == "WorkerW") return !rect.Contains(bounds);
            if (kind == "Shell_TrayWnd" || kind == "Shell_SecondaryTrayWnd" || kind == "tooltips_class32") return true;
            found = true;
            return false;
        }, IntPtr.Zero);
        return found;
    }
}

static class Program {
 [DllImport("kernel32.dll")] static extern uint GetCurrentThreadId(); [DllImport("user32.dll")] static extern IntPtr GetThreadDesktop(uint thread); [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern bool GetUserObjectInformation(IntPtr handle,int index,System.Text.StringBuilder value,int length,out int needed); public static readonly string InstanceName=GetInstanceName(); static string GetInstanceName(){var name=new System.Text.StringBuilder(512);int needed;bool ok=GetUserObjectInformation(GetThreadDesktop(GetCurrentThreadId()),2,name,1024,out needed);return "Local\\SpeakerSwitchWidget-"+(ok?name.ToString():"Default");}
 [STAThread] static void Main(string[] args){if(args.Length>0&&args[0]=="--diagnose"){string id;var list=Audio.Scan(out id);File.WriteAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"diagnostics.txt"),list.Select(x=>(x.Id==id?"DEFAULT ":"")+x.Name+" | hardware="+x.Hardware+" | kind="+x.Kind+" | "+x.Id));return;}
 bool created;using(var mutex=new System.Threading.Mutex(true,InstanceName,out created)){if(!created){using(var signal=new System.Threading.EventWaitHandle(false,System.Threading.EventResetMode.AutoReset,Program.InstanceName + "Restore")){signal.Set();}return;}Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);Application.Run(new Widget());}}
}














