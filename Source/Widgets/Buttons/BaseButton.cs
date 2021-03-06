using GameTimer;
using InputHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

namespace MenuBuddy
{
	/// <summary>
	/// This is a button that can be clicked on
	/// </summary>
	public abstract class BaseButton : Widget, IButton
	{
		#region Fields

		/// <summary>
		/// whether or not to draw this item when inactive
		/// </summary>
		private bool _drawWhenInactive;

		private Vector2 _size;

		public event EventHandler<ClickEventArgs> OnClick;

		private CountdownTimer _clickTimer;

		private const float _clickCountdownTime = 0.2f;

		#endregion //Fields

		#region Properties

		public Vector2 Size
		{
			protected get
			{
				return _size;
			}
			set
			{
				if (_size != value)
				{
					_size = value;
					CalculateRect();
				}
			}
		}

		public override float Scale
		{
			get
			{
				return base.Scale;
			}
			set
			{
				base.Scale = value;
				Layout.Scale = value;
			}
		}

		/// <summary>
		/// The layout to add to this button
		/// </summary>
		public ILayout Layout { get; protected set; }

		public override bool IsHighlighted
		{
			protected get
			{
				return base.IsHighlighted;
			}
			set
			{
				base.IsHighlighted = value;
				Layout.IsHighlighted = value;
			}
		}

		public override bool DrawWhenInactive
		{
			get
			{
				return _drawWhenInactive;
			}
			set
			{
				_drawWhenInactive = value;
			}
		}

		public override Point Position
		{
			get { return base.Position; }
			set
			{
				if (base.Position != value)
				{
					base.Position = value;
					CalculateRect();
				}
			}
		}

		/// <summary>
		/// A description of the function of the menu entry.
		/// </summary>
		public string Description { get; set; }

		public bool IsQuiet { get; set; }

		protected SoundEffect HighlightedSoundEffect { get; set; }

		protected SoundEffect ClickedSoundEffect { get; set; }

		public bool IsClicked
		{
			get
			{
				return _clickTimer.HasTimeRemaining();
			}
			set
			{
				Layout.IsClicked = value;
			}
		}

		public override ITransitionObject Transition
		{
			get
			{
				return base.Transition;
			}

			set
			{
				base.Transition = value;

				if (null != Layout)
				{
					for (int i = 0; i < Layout.Items.Count; i++)
					{
						var widget = Layout.Items[i] as IWidget;
						if (null != widget)
						{
							widget.Transition = value;
						}
					}
				}
			}
		}

		public string HighlightedSound { get; set; }

		public string ClickedSound { get; set; }

		#endregion //Properties

		#region Initialization

		/// <summary>
		/// Constructs a new button with the specified text.
		/// </summary>
		protected BaseButton()
		{
			IsQuiet = false;
			_clickTimer = new CountdownTimer();

			//by default, just play a sound when this item is selected
			OnClick += PlaySelectedSound;

			OnClick += ((obj, e) =>
			{
				_clickTimer.Start(_clickCountdownTime);
			});

			OnHighlight += PlayHighlightSound;
			OnHighlight += ((obj, e) =>
			{
				IsHighlighted = true;
			});

			HighlightedSound = StyleSheet.HighlightedSoundResource;
			ClickedSound = StyleSheet.ClickedSoundResource;
		}

		/// <summary>
		/// Constructs a new button with the specified text.
		/// </summary>
		protected BaseButton(BaseButton inst) : base(inst)
		{
			_drawWhenInactive = inst._drawWhenInactive;
			_size = inst._size;
			Description = inst.Description;
			OnClick = inst.OnClick;
			IsQuiet = inst.IsQuiet;
			HighlightedSoundEffect = inst.HighlightedSoundEffect;
			ClickedSoundEffect = inst.ClickedSoundEffect;
			_clickTimer = inst._clickTimer;
		}

		public override void LoadContent(IScreen screen)
		{
			if (null != screen.ScreenManager)
			{
				HighlightedSoundEffect = screen.ScreenManager.Game.Content.Load<SoundEffect>(HighlightedSound);
				ClickedSoundEffect = screen.ScreenManager.Game.Content.Load<SoundEffect>(ClickedSound);
			}

			Layout.LoadContent(screen);
			base.LoadContent(screen);
		}

		#endregion //Initialization

		#region Methods

		public void AddItem(IScreenItem item)
		{
			Layout.AddItem(item);
			CalculateRect();

			var widget = item as IWidget;
			if (null != widget)
			{
				widget.Transition = Transition;
			}

		}

		public bool RemoveItem(IScreenItem item)
		{
			var result = Layout.RemoveItem(item);
			CalculateRect();
			return result;
		}

		public override void Update(IScreen screen, GameTimer.GameClock gameTime)
		{
			_clickTimer.Update(gameTime);
			Layout.Update(screen, gameTime);
			Layout.IsClicked = IsClicked;
		}

		public override void Draw(IScreen screen, GameTimer.GameClock gameTime)
		{
			Layout.Draw(screen, gameTime);
		}

		protected override void CalculateRect()
		{
			_rect = Layout.Rect;
		}

		/// <summary>
		/// Method for raising the Selected event.
		/// </summary>
		public void PlaySelectedSound(object obj, ClickEventArgs e)
		{
			//play the sound effect
			if (!IsQuiet && (null != ClickedSoundEffect))
			{
				ClickedSoundEffect.Play();
			}
		}

		public void PlayHighlightSound(object obj, HighlightEventArgs e)
		{
			if (!IsQuiet && (null != HighlightedSoundEffect))
			{
				//play menu noise
				HighlightedSoundEffect.Play();
			}
		}

		public virtual bool CheckClick(ClickEventArgs click)
		{
			//check if the widget was clicked
			if (Rect.Contains(click.Position))
			{
				if (OnClick != null)
				{
					OnClick(this, click);
				}

				return true;
			}

			return false;
		}

		public void Clicked(object obj, ClickEventArgs e)
		{
			OnClick(obj, e);
		}

		#endregion //Methods
	}
}