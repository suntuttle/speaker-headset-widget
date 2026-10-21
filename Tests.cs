using System;
using System.Collections.Generic;
class Tests {
 static void Assert(bool pass,string text){if(!pass)throw new Exception(text);Console.WriteLine("PASS: "+text);}
 static void Main(){var saved=new Endpoint{Id="old",Name="headset(3- ABKO N550)",Hardware="3- ABKO N550",Kind=1};var plugged=new Endpoint{Id="new",Name="새 출력 이름",Hardware="4- ABKO N550",Kind=1};var list=new List<Endpoint>{plugged};Assert(Audio.Resolve(list,saved,true)==plugged,"Reconnect with renamed endpoint and numbered hardware");list.Add(new Endpoint{Id="duplicate",Hardware=plugged.Hardware,Name="other",Kind=1});Assert(Audio.Resolve(list,saved,true)==null,"Ambiguous hardware requires selection");list.Clear();Assert(Audio.Resolve(list,saved,true)==null,"Disconnected endpoint is not replaced");string current;list=Audio.Scan(out current);Assert(Audio.Resolve(list,null,false)!=null,"Actual speaker auto detection");Assert(Audio.Resolve(list,null,true)!=null,"Actual headset auto detection");}
}
