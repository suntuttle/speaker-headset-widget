using System;using System.Linq;using System.Drawing;using System.Reflection;using System.Windows.Forms;
class IntegrationCheck {
 static object Call(Widget w,string method,params object[] args){return typeof(Widget).GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(w,args);}
 static void Assert(bool value,string text){if(!value)throw new Exception(text);Console.WriteLine("PASS: "+text);}
 static void Click(Widget w,int x,int y){var e=new MouseEventArgs(MouseButtons.Left,1,x,y,0);Call(w,"OnMouseDown",e);Call(w,"OnMouseUp",e);Application.DoEvents();}
 [STAThread] static void Main(){try{Run();}catch(Exception ex){Console.WriteLine(ex.GetType().FullName+": "+ex.Message);Environment.ExitCode=1;}} static void Run(){string[] before=Enumerable.Range(0,3).Select(Audio.DefaultId).ToArray();Application.EnableVisualStyles();try{using(var w=new Widget())using(var sheet=new Bitmap(352,116))using(var g=Graphics.FromImage(sheet)){
 w.Show();Application.DoEvents();int right=w.Right,top=w.Top;Assert(w.Width==w.Height,"Starts as icon only");g.Clear(Color.FromArgb(228,232,237));Draw(w,g,16,24);
 Click(w,w.Width/2,w.Height/2);Assert(w.Width>w.Height,"First click expands");Assert(w.Right==right&&w.Top==top,"Expansion keeps top-right anchor");for(int r=0;r<3;r++){string now=Audio.DefaultId(r);Console.WriteLine("Role "+r+" before="+before[r]+" after="+now);Assert(now==before[r],"Expansion does not change audio role "+r);}Draw(w,g,88,24);sheet.Save("expand-preview.png");
 Click(w,100,30);Assert(Audio.DefaultId(0)!=before[0],"Expanded click changes actual audio output");Click(w,100,30);Assert(Audio.DefaultId(0)==before[0],"Next click restores original output");
 Call(w,"OnMouseLeave",EventArgs.Empty);var timer=(Timer)typeof(Widget).GetField("collapseTimer",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(w);Assert(timer.Enabled,"Pointer leave starts collapse delay");Call(w,"OnMouseEnter",EventArgs.Empty);Assert(!timer.Enabled,"Pointer return cancels collapse delay");
 Call(w,"SetExpanded",false);Assert(w.Width==w.Height&&w.Right==right&&w.Top==top,"Collapse restores anchored icon");Assert(!timer.Enabled,"No collapse timer running while compact");w.Close();
 }}finally{for(int r=0;r<3;r++)Audio.SetRole(before[r],r);}Console.WriteLine("PASS: original output roles restored");}
 static void Draw(Widget w,Graphics g,int x,int y){using(var b=new Bitmap(w.Width,w.Height)){w.DrawToBitmap(b,w.ClientRectangle);var state=g.Save();g.TranslateTransform(x,y);g.SetClip(w.Region,System.Drawing.Drawing2D.CombineMode.Intersect);g.DrawImageUnscaled(b,0,0);g.Restore(state);}}
}

