using GameTimer;
using InputHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MenuBuddy
{
	/// <summary>
	/// This is a abs layout that is in a window with scroll bars
	/// </summary>
	public class ScrollLayout : AbsoluteLayout
	{
		#region Delegates

		private delegate void DrawStuffDelegate(IScreen screen, GameClock gameTime);

		#endregion //Delegates

		#region Fields

		private Vector2 _scrollPos = Vector2.Zero;

		private RenderTarget2D _renderTarget = null;

		private Vector2 _minScroll = Vector2.Zero;

		private Vector2 _maxScroll = Vector2.Zero;

		private Rectangle _verticalScrollBar = Rectangle.Empty;

		private Rectangle _horizScrollBar = Rectangle.Empty;

		public const float ScrollBarWidth = 16f;

		#endregion //Fields

		#region Properties

		/// <summary>
		/// the current scorll position of this thing
		/// </summary>
		public Vector2 ScrollPosition
		{
			get
			{
				return _scrollPos;
			}
			set
			{
				//constrain the scroll to within the total rect
				value = ConstrainScroll(value);

				if (_scrollPos != value)
				{
					//set the scroll position
					var delta = (_scrollPos - value).ToPoint();
					_scrollPos = value;

					//update the position of all the items
					foreach (var item in Items)
					{
						item.Position += delta;
					}

					UpdateScrollBars();
				}
			}
		}

		public override Vector2 Size
		{
			get
			{
				return base.Size;
			}
			set
			{
				//make sure to redo the rendertarget
				_renderTarget = null;
				base.Size = value;
			}
		}

		public ITransitionObject Transition
		{
			get; set;
		}

		/// <summary>
		/// This is the total max rect, containing all the widgets.
		/// </summary>
		public Rectangle TotalRect
		{
			get
			{
				Rectangle result = Rect;

				//add all the widgets in this dude
				foreach (var item in Items)
				{
					result = Rectangle.Union(result, item.Rect);
				}

				return result;
			}
		}

		public Vector2 MinScroll
		{
			get
			{
				return _minScroll;
			}
			set
			{
				_minScroll = value;
			}
		}

		public Vector2 MaxScroll
		{
			get
			{
				return _maxScroll;
			}
			set
			{
				_maxScroll = value;
			}
		}

		public Rectangle VerticalScrollBar
		{
			get
			{
				return _verticalScrollBar;
			}
		}

		public Rectangle HorizontalScrollBar
		{
			get
			{
				return _horizScrollBar;
			}
		}

		public bool DrawVerticalScrollBar { get; private set; }

		public bool DrawHorizontalScrollBar { get; private set; }

		private bool DrawScrollbars
		{
			get; set;
		}

		#endregion //Properties

		#region Initialization

		public ScrollLayout()
		{
			Transition = new WipeTransitionObject(TransitionWipeType.SlideLeft);
			DrawVerticalScrollBar = false;
			DrawHorizontalScrollBar = false;
			UpdateScrollBars();
			DrawScrollbars = false;
		}

		public ScrollLayout(ScrollLayout inst) : base(inst)
		{
			_scrollPos = new Vector2(inst._scrollPos.X, inst._scrollPos.Y);
			_renderTarget = inst._renderTarget;
			Transition = inst.Transition;
			_minScroll = new Vector2(inst._minScroll.X, inst._minScroll.Y);
			_maxScroll = new Vector2(inst._maxScroll.X, inst._maxScroll.Y);
			_verticalScrollBar = new Rectangle(inst._verticalScrollBar.Location, inst._verticalScrollBar.Size);
			_horizScrollBar = new Rectangle(inst._horizScrollBar.Location, inst._horizScrollBar.Size);
			DrawVerticalScrollBar = inst.DrawVerticalScrollBar;
			DrawHorizontalScrollBar = inst.DrawHorizontalScrollBar;
			DrawScrollbars = inst.DrawScrollbars;
			Transition = inst.Transition;
		}

		public override IScreenItem DeepCopy()
		{
			return new ScrollLayout(this);
		}

		#endregion //Initialization

		#region Methods

		public override void AddItem(IScreenItem item)
		{
			//Items in a scroll layout don't transition
			var widget = item as IWidget;
			if (null != widget)
			{
				widget.Transition = new WipeTransitionObject(TransitionWipeType.None);
			}

			base.AddItem(item);

			UpdateMinMaxScroll();
			UpdateScrollBars();
		}

		private void InitializeRenderTarget(IScreen screen)
		{
			if (null == _renderTarget)
			{
				_renderTarget = new RenderTarget2D(screen.ScreenManager.GraphicsDevice,
					(int)Size.X,
					(int)Size.Y,
					false,
					screen.ScreenManager.GraphicsDevice.PresentationParameters.BackBufferFormat,
					screen.ScreenManager.GraphicsDevice.PresentationParameters.DepthStencilFormat,
					0,
					RenderTargetUsage.PreserveContents);
			}
		}

		public override void Update(IScreen screen, GameClock gameTime)
		{
			//set the scroll bars to "not drawn" at the beginning of every update... input methods will set it correctly.
			DrawScrollbars = false;
			base.Update(screen, gameTime);
		}

		private void DrawStuff(IScreen screen, GameClock gameTime, DrawStuffDelegate del, bool clear)
		{
			//grab the old stuff
			var curPos = Position;
			var curHor = Horizontal;
			var curVert = Vertical;
			Position = Point.Zero;
			Horizontal = HorizontalAlignment.Left;
			Vertical = VerticalAlignment.Top;

			var screenManager = screen.ScreenManager;

			//initialize the render target if necessary
			InitializeRenderTarget(screen);

			//end the current draw loop
			screenManager.SpriteBatchEnd();

			var curRenderTarget = screenManager.GraphicsDevice.GetRenderTargets();

			//set the rendertarget
			screenManager.GraphicsDevice.SetRenderTarget(_renderTarget);

			if (clear)
			{
				screenManager.GraphicsDevice.Clear(Color.TransparentBlack);
			}

			//start a new draw loop
			screenManager.SpriteBatchBegin(BlendState.AlphaBlend);

			//call the provided delegate to draw everything
			del(screen, gameTime);

			//end the loop
			screenManager.SpriteBatchEnd();

			//set the position back
			Position = curPos;
			Vertical = curVert;
			Horizontal = curHor;

			//set the render target back
			screenManager.GraphicsDevice.SetRenderTarget(null);

			//start a new loop
			screenManager.SpriteBatchBegin();
		}

		public override void DrawBackground(IScreen screen, GameClock gameTime)
		{
			DrawStuff(screen, gameTime, base.DrawBackground, true);
		}

		public override void Draw(IScreen screen, GameClock gameTime)
		{
			DrawStuff(screen, gameTime, base.Draw, false);

			//render the texture
			var rect = CalculateRect();
			screen.ScreenManager.SpriteBatch.Draw(_renderTarget,
				Transition.Position(screen.Transition, rect.Location),
				Color.White);

			//Draw the scroll bars if the mouse pointer or a touch is inside the layout
			if (DrawScrollbars)
			{
				if (DrawVerticalScrollBar)
				{
					screen.ScreenManager.DrawHelper.DrawRect(StyleSheet.HighlightedBackgroundColor, VerticalScrollBar, screen.Transition, Transition);
				}

				if (DrawHorizontalScrollBar)
				{
					screen.ScreenManager.DrawHelper.DrawRect(StyleSheet.HighlightedBackgroundColor, HorizontalScrollBar, screen.Transition, Transition);
				}
			}
		}

		public void UpdateMinMaxScroll()
		{
			//get the total rectangle
			var total = TotalRect;

			//get the layout rectangle
			var current = Rect;

			//set the min and max to be the diff between the two
			_minScroll = new Vector2(total.Left - current.Left, total.Top - current.Top);
			_maxScroll = new Vector2(total.Right - current.Right, total.Bottom - current.Bottom);
		}

		private Vector2 ConstrainScroll(Vector2 value)
		{
			//set the x value
			if (value.X < _minScroll.X)
			{
				value.X = _minScroll.X;
			}
			else if (value.X > _maxScroll.X)
			{
				value.X = _maxScroll.X;
			}

			//set the y value
			if (value.Y < _minScroll.Y)
			{
				value.Y = _minScroll.Y;
			}
			else if (value.Y > _maxScroll.Y)
			{
				value.Y = _maxScroll.Y;
			}

			return value;
		}

		private void UpdateScrollBars()
		{
			//get the window size
			var windowSize = Size;

			//get the total size
			var totalRect = TotalRect;
			var totalSize = new Vector2(totalRect.Width, totalRect.Height);

			//get the size ratio
			var ratio = new Vector2()
			{
				X = (totalSize.X != 0f) ? windowSize.X / totalSize.X : 0f,
				Y = (totalSize.Y != 0f) ? windowSize.Y / totalSize.Y : 0f,
			};

			//Do we even need to draw these things?
			DrawHorizontalScrollBar = ((0f < ratio.X) && (ratio.X < 1f));
			DrawVerticalScrollBar = ((0f < ratio.Y) && (ratio.Y < 1f));

			//get the scroll bar sizes
			var scrollbarSize = windowSize * ratio;

			//get the scroll delta
			var scrollDelta = (MaxScroll - MinScroll);
			var deltaRatio = new Vector2()
			{
				X = (scrollDelta.X != 0f) ? (ScrollPosition.X - MinScroll.X) / scrollDelta.X : 0f,
				Y = (scrollDelta.Y != 0f) ? (ScrollPosition.Y - MinScroll.Y) / scrollDelta.Y : 0f,
			};

			//Get the max number of pixels to add to the scroll bar position
			var maxScrollDelta = windowSize - scrollbarSize;

			//get the delta to add to the window pos
			var delta = maxScrollDelta * deltaRatio;

			//set the scrollbar rectangles
			var rect = Rect;
			_verticalScrollBar = new Rectangle((int)(rect.Right - ScrollBarWidth),
				(int)(rect.Top + delta.Y),
				(int)(ScrollBarWidth),
				(int)(scrollbarSize.Y));

			_horizScrollBar = new Rectangle((int)(rect.Left + delta.X),
				(int)(rect.Bottom - ScrollBarWidth),
				(int)(scrollbarSize.X),
				(int)(ScrollBarWidth));

		}

		public override bool CheckHighlight(HighlightEventArgs highlight)
		{
			if (Rect.Contains(highlight.Position))
			{
				DrawScrollbars = true;
			}

			return base.CheckHighlight(highlight) || DrawScrollbars;
		}

		public override bool CheckDrag(DragEventArgs drag)
		{
			var result = Rect.Contains(drag.Start);
			if (result)
			{
				//add the delta to the scroll position
				ScrollPosition = ScrollPosition + drag.Delta;
				DrawScrollbars = true;
			}

			return result;
		}

		#endregion //Methods
	}
}