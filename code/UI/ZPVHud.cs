namespace ZPViral.UI;

public class ZPVHud : RootPanel
{
	public static ZPVHud Current;

	public ZPVHud()
	{
		Current?.Delete();
		Current = null;

		AddChild<Vitals>();
		AddChild<AmmoStats>();
		AddChild<ChatBox>();

		Current = this;
	}
}

