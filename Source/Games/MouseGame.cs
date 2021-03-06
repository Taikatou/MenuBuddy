using MouseBuddy;

namespace MenuBuddy
{
	/// <summary>
	/// This is some boilerplate code for doing a mousepointer based game.
	/// </summary>
	public abstract class MouseGame : DefaultGame
	{
		#region Methods

		protected MouseGame() : base(GameType.Mouse)
		{
		}

		protected override void InitInput()
		{
			var mouse = new MouseComponent(this, ResolutionBuddy.Resolution.ScreenToGameCoord);

			//add the input helper for menus
			var input = new MouseInputHelper(this);
		}

		#endregion //Methods
	}
}
