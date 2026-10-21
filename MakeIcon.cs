using System;using System.Drawing;using System.Drawing.Drawing2D;using System.Drawing.Imaging;using System.IO;using System.Reflection;
class MakeIcon {
 static void Main(){using(var b=new Bitmap(32,32))using(var g=Graphics.FromImage(b))using(var bytes=new MemoryStream()){
 g.SmoothingMode=SmoothingMode.AntiAlias;g.Clear(Color.Transparent);
 var path=(GraphicsPath)typeof(Widget).GetMethod("Round",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{new RectangleF(0,0,32,32),8f});
 using(path)using(var fill=new SolidBrush(Color.FromArgb(30,33,39)))g.FillPath(fill,path);
 typeof(Widget).GetMethod("DrawDeviceIcon",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{g,false,Color.FromArgb(156,206,186),5f,6f});
 b.Save(bytes,ImageFormat.Png);byte[] png=bytes.ToArray();using(var f=new BinaryWriter(File.Create("app.ico"))){f.Write((ushort)0);f.Write((ushort)1);f.Write((ushort)1);f.Write((byte)32);f.Write((byte)32);f.Write((byte)0);f.Write((byte)0);f.Write((ushort)1);f.Write((ushort)32);f.Write(png.Length);f.Write(22);f.Write(png);}}
 }
}
