﻿using Humper.Base;

namespace Humper
{
	public class Collision : ICollision
	{
		public Collision()
		{
		}

		public Box Box { get; set; }

		public Box Other { get { return this.Hit?.Box; } }

		public Rect Origin { get; set; }

		public Rect Goal { get; set; }

		public IHit Hit { get; set; }

		public bool HasCollided => this.Hit != null;
	}
}

