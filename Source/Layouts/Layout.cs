using GameTimer;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using MouseBuddy;

namespace MenuBuddy
{
	/// <summary>
	/// This is a list of items on a screen
	/// </summary>
	public abstract class Layout : ILayout
	{
		#region Fields

		private HorizontalAlignment _horizontal;
		private VerticalAlignment _vertical;

		public event EventHandler<ClickEventArgs> OnClick;
		public event EventHandler<HighlightEventArgs> OnHighlight;
		public event EventHandler<DragEventArgs> OnDrag;

		#endregion //Fields

		#region Properties

		public bool DrawWhenInactive
		{
			set
			{
				foreach (var item in Items)
				{
					item.DrawWhenInactive = value;
				}
			}
		}

		public bool Highlight
		{
			set 
			{
				foreach (var item in Items)
				{
					item.Highlight = value;
				}
			}
		}

		/// <summary>
		/// all the items that are in this layout
		/// </summary>
		public List<IScreenItem> Items { get; protected set; }

		public virtual Rectangle Rect
		{
			get
			{
				//check if there are any items at all
				if (0 < Items.Count)
				{
					//add up all the rectangles of the contained items
					var result = Items[0].Rect;
					for (int i = 1; i < Items.Count; i++)
					{
						//get the rect of the item
						var itemRect = Items[i].Rect;

						//check if the current rect is bogus
						if (result.IsEmpty)
						{
							result = itemRect;
						}

						//check if it is valid
						if (!itemRect.IsEmpty)
						{
							result = Rectangle.Union(result, itemRect);
						}
					}

					return result;
				}
				else
				{
					return new Rectangle(Position.X, Position.Y, 0, 0);
				}
			}
		}

		public virtual Point Position { get; set; }

		/// <summary>
		/// Where to layer the item.
		/// low numbers go in the back, higher numbers in the front
		/// </summary>
		public float Layer { get; set; }

		public virtual HorizontalAlignment Horizontal { get; set; }

		public virtual VerticalAlignment Vertical { get; set; }

		public virtual float Scale { get; set; }

		#endregion //Properties

		#region Methods

		protected Layout()
		{
			Items = new List<IScreenItem>();
			Scale = 1.0f;
		}

		public abstract void AddItem(IScreenItem item);

		protected void Sort()
		{
			Items.Sort((x, y) => x.Layer.CompareTo(y.Layer));
		}

		public virtual bool RemoveItem(IScreenItem item)
		{
			return Items.Remove(item);
		}

		public void Update(IScreen screen, GameClock gameTime)
		{
			//update all the items
			for (int i = 0; i < Items.Count; i++)
			{
				Items[i].Update(screen, gameTime);
			}
		}

		public virtual void DrawBackground(IScreen screen, GameClock gameTime)
		{
			//draw the backgrounds of all the items
			for (int i = 0; i < Items.Count; i++)
			{
				Items[i].DrawBackground(screen, gameTime);
			}
		}

		public virtual void Draw(IScreen screen, GameClock gameTime)
		{
			//draw all the items
			for (int i = 0; i < Items.Count; i++)
			{
				Items[i].Draw(screen, gameTime);
			}
		}

		public virtual bool CheckHighlight(HighlightEventArgs highlight)
		{
			var highlighted = false;

			foreach (var item in Items)
			{
				if (item.CheckHighlight(highlight))
				{
					highlighted = true;
				}
			}

			return highlighted;
        }

		public virtual bool CheckClick(ClickEventArgs click)
		{
			if (Rect.Contains(click.Position))
			{
				foreach (var item in Items)
				{
					if (item.CheckClick(click))
					{
						return true;
					}
				}
			}

			//None of the items in this container were clicked
			return false;
		}

		public virtual bool CheckDrag(DragEventArgs drag)
		{
			if (Rect.Contains(drag.Start))
			{
				foreach (var item in Items)
				{
					if (item.CheckDrag(drag))
					{
						return true;
					}
				}
			}

			//None of the items in this container were clicked
			return false;
		}

		#endregion //Methods
	}
}