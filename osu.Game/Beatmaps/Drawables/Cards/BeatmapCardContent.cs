// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Beatmaps.Drawables.Cards
{
    public class BeatmapCardContent : CompositeDrawable
    {
        public Drawable MainContent
        {
            set => bodyContent.Child = value;
        }

        public Drawable ExpandedContent
        {
            set => dropdownScroll.Child = value;
        }

        public IBindable<bool> Expanded => expanded;

        private readonly BindableBool expanded = new BindableBool();

        private readonly Box background;
        private readonly Container content;
        private readonly Container bodyContent;
        private readonly Container dropdownContent;
        private readonly OsuScrollContainer dropdownScroll;
        private readonly Container borderContainer;

        public BeatmapCardContent(float height)
        {
            RelativeSizeAxes = Axes.X;
            Height = height;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            InternalChild = content = new HoverHandlingContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                CornerRadius = BeatmapCard.CORNER_RADIUS,
                Masking = true,
                Unhovered = _ => updateFromHoverChange(),
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    bodyContent = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = height,
                        CornerRadius = BeatmapCard.CORNER_RADIUS,
                        Masking = true,
                    },
                    dropdownContent = new HoverHandlingContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Margin = new MarginPadding { Top = height },
                        Alpha = 0,
                        Hovered = _ =>
                        {
                            updateFromHoverChange();
                            return true;
                        },
                        Unhovered = _ => updateFromHoverChange(),
                        Child = dropdownScroll = new ExpandedContentScrollContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            ScrollbarVisible = false
                        }
                    },
                    borderContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        CornerRadius = BeatmapCard.CORNER_RADIUS,
                        Masking = true,
                        BorderThickness = 3,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            background.Colour = colourProvider.Background2;
            borderContainer.BorderColour = colourProvider.Highlight1;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Expanded.BindValueChanged(_ => updateState(), true);
            FinishTransforms(true);
        }

        private ScheduledDelegate? scheduledExpandedChange;

        public void ExpandAfterDelay() => queueExpandedStateChange(true, 100);

        public void CancelExpand() => scheduledExpandedChange?.Cancel();

        private void updateFromHoverChange() =>
            queueExpandedStateChange(content.IsHovered || dropdownContent.IsHovered, 100);

        private void queueExpandedStateChange(bool newState, int delay = 0)
        {
            if (Expanded.Disabled)
                return;

            scheduledExpandedChange?.Cancel();
            scheduledExpandedChange = Scheduler.AddDelayed(() => expanded.Value = newState, delay);
        }

        private void updateState()
        {
            // Scale value is intentionally chosen to fit in the spacing of listing displays, as to not overlap horizontally with adjacent cards.
            // This avoids depth issues where a hovered (scaled) card to the right of another card would be beneath the card to the left.
            this.ScaleTo(Expanded.Value ? 1.03f : 1, 500, Easing.OutQuint);

            background.FadeTo(Expanded.Value ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
            dropdownContent.FadeTo(Expanded.Value ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
            borderContainer.FadeTo(Expanded.Value ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);

            content.TweenEdgeEffectTo(new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Offset = new Vector2(0, 2),
                Radius = 10,
                Colour = Colour4.Black.Opacity(Expanded.Value ? 0.3f : 0f),
                Hollow = true,
            }, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
        }

        private class ExpandedContentScrollContainer : OsuScrollContainer
        {
            public ExpandedContentScrollContainer()
            {
                ScrollbarVisible = false;
            }

            protected override void Update()
            {
                base.Update();

                Height = Math.Min(Content.DrawHeight, 400);
            }

            private bool allowScroll => !Precision.AlmostEquals(DrawSize, Content.DrawSize);

            protected override bool OnDragStart(DragStartEvent e)
            {
                if (!allowScroll)
                    return false;

                return base.OnDragStart(e);
            }

            protected override void OnDrag(DragEvent e)
            {
                if (!allowScroll)
                    return;

                base.OnDrag(e);
            }

            protected override void OnDragEnd(DragEndEvent e)
            {
                if (!allowScroll)
                    return;

                base.OnDragEnd(e);
            }

            protected override bool OnScroll(ScrollEvent e)
            {
                if (!allowScroll)
                    return false;

                return base.OnScroll(e);
            }
        }
    }
}
