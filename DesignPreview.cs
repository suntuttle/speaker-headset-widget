using System;using System.Drawing;using System.Reflection;using System.Windows.Forms;
class DesignPreview {
 static void Field(Widget w,string name,object value){typeof(Widget).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(w,value);}
 [STAThread] static void Main(){Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);using(var sheet=new Bitmap(560,232))using(var g=Graphics.FromImage(sheet))using(var label=new Font("Segoe UI",9f))using(var w=new Widget()){
 g.Clear(Color.FromArgb(228,232,237));w.Show();Application.DoEvents();
 for(int i=0;i<4;i++){bool light=i>=2;bool headset=i%2==1;var prefs=(Preferences)typeof(Widget).GetField("prefs",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(w);prefs.LightTheme=light;Field(w,"isHeadset",headset);Field(w,"available",true);Field(w,"heading",headset?"헤드셋":"스피커");Field(w,"caption",headset?"ABKO N550":"Realtek Audio");w.BackColor=light?Color.FromArgb(247,248,250):Color.FromArgb(30,33,39);w.Refresh();using(var b=new Bitmap(w.Width,w.Height)){w.DrawToBitmap(b,w.ClientRectangle);int x=20+(i%2)*272,y=42+(i/2)*106;var state=g.Save();g.TranslateTransform(x,y);g.SetClip(w.Region,System.Drawing.Drawing2D.CombineMode.Intersect);g.DrawImageUnscaled(b,0,0);g.Restore(state);b.Save("preview-"+(light?"light":"dark")+"-"+(headset?"headset":"speaker")+".png");}}
 g.DrawString("DARK",label,Brushes.DimGray,22,17);g.DrawString("LIGHT",label,Brushes.DimGray,22,123);sheet.Save("design-preview.png");w.Close();}
 Console.WriteLine("PASS: rendered all four widget appearances");}
}

