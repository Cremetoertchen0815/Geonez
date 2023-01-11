﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Tweens;
using System;
using System.Collections;


namespace Nez
{
	public class TransformTransition : SceneTransition
	{
		public enum TransformTransitionType
		{
			ZoomOut,
			ZoomIn,
			SlideRight,
			SlideLeft,
			SlideUp,
			SlideDown,
			SlideBottomRight,
			SlideBottomLeft,
			SlideTopRight,
			SlideTopLeft
		}

		/// <summary>
		/// duration for the animation
		/// </summary>
		public float Duration = 1f;

		/// <summary>
		/// ease equation for the transition
		/// </summary>
		public EaseType TransitionEaseType = EaseType.QuartIn;
		private Rectangle _destinationRect;
		private Rectangle _finalRenderRect;
		private Rectangle _textureBounds;


		public TransformTransition(Func<Scene> sceneLoadAction,
								   TransformTransitionType transitionType = TransformTransitionType.ZoomOut) : base(
			sceneLoadAction, true)
		{
			_destinationRect = PreviousSceneRender.Bounds;
			_textureBounds = _destinationRect;

			switch (transitionType)
			{
				case TransformTransitionType.ZoomOut:
					_finalRenderRect = new Rectangle(Screen.BackbufferWidth / 2, Screen.BackbufferHeight / 2, 0, 0);
					break;
				case TransformTransitionType.ZoomIn:
					_finalRenderRect = new Rectangle(-Screen.BackbufferWidth * 5, -Screen.BackbufferHeight * 5, _destinationRect.Width * 10,
						_destinationRect.Height * 10);
					break;
				case TransformTransitionType.SlideRight:
					_finalRenderRect = new Rectangle(Screen.BackbufferWidth, 0, _destinationRect.Width, _destinationRect.Height);
					break;
				case TransformTransitionType.SlideLeft:
					_finalRenderRect = new Rectangle(-Screen.BackbufferWidth, 0, _destinationRect.Width, _destinationRect.Height);
					break;
				case TransformTransitionType.SlideUp:
					_finalRenderRect =
						new Rectangle(0, -Screen.BackbufferHeight, _destinationRect.Width, _destinationRect.Height);
					break;
				case TransformTransitionType.SlideDown:
					_finalRenderRect = new Rectangle(0, Screen.BackbufferHeight, _destinationRect.Width, _destinationRect.Height);
					break;
				case TransformTransitionType.SlideBottomRight:
					_finalRenderRect = new Rectangle(Screen.BackbufferWidth, Screen.BackbufferHeight, _destinationRect.Width,
						_destinationRect.Height);
					break;
				case TransformTransitionType.SlideBottomLeft:
					_finalRenderRect = new Rectangle(-Screen.BackbufferWidth, Screen.BackbufferHeight, _destinationRect.Width,
						_destinationRect.Height);
					break;
				case TransformTransitionType.SlideTopRight:
					_finalRenderRect = new Rectangle(Screen.BackbufferWidth, -Screen.BackbufferHeight, _destinationRect.Width,
						_destinationRect.Height);
					break;
				case TransformTransitionType.SlideTopLeft:
					_finalRenderRect = new Rectangle(-Screen.BackbufferWidth, -Screen.BackbufferHeight, _destinationRect.Width,
						_destinationRect.Height);
					break;
			}
		}


		public TransformTransition(TransformTransitionType transitionType = TransformTransitionType.ZoomOut) : this(
			null, transitionType)
		{
		}


		public override IEnumerator OnBeginTransition()
		{
			yield return null;

			// load up the new Scene
			yield return Core.StartCoroutine(LoadNextScene());

			float elapsed = 0f;
			while (elapsed < Duration)
			{
				elapsed += Time.UnscaledDeltaTime;
				_destinationRect = Lerps.Ease(TransitionEaseType, ref _textureBounds, ref _finalRenderRect, elapsed,
					Duration);

				yield return null;
			}

			TransitionComplete();
		}


		public override void Render(Batcher batcher)
		{
			Core.GraphicsDevice.SetRenderTarget(null);
			batcher.Begin(BlendState.NonPremultiplied, Core.DefaultSamplerState, DepthStencilState.None, null);
			batcher.Draw(PreviousSceneRender, _destinationRect, Color.White);
			batcher.End();
		}
	}
}