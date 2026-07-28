namespace LemonGames
{
    using DG.Tweening;
    using UnityEngine;
    using UnityEngine.UI;

    public static class ScrollRectExtensions
    {
        /// <summary>
        /// Calculates the NormalizedScrollPosition the scrollView should use in order to be centered at the given focus point
        /// </summary>
        /// <param name="focusPoint_bottomLeft">This is the point you want to scroll to, in content-space, relative to the bottom-left corner of the content rectangle</param>
        /// <returns>The NormalizedScrollPosition the scrollView should use in order to be centered at the given focus point</returns>
        public static Vector2 CalculateNormalizedScrollPosition(this ScrollRect scrollView, Vector2 focusPoint_bottomLeft)
        {
            Vector2 viewportSize = ((RectTransform)scrollView.content.parent).rect.size;
            return CalculateNormalizedScrollPosition(scrollView, focusPoint_bottomLeft, viewportSize * 0.5f);
        }
        /// <summary>
        /// Calculates the NormalizedScrollPosition the scrollView should use in order to be centered at the given focus point
        /// </summary>
        /// <param name="focusPoint_bottomLeft">This is the point you want to scroll to, in content-space, relative to the bottom-left corner of the content rectangle</param>
        /// <param name="desiredPosition_bottomLeft">This is the position, in viewport-space, relative to the bottom-left corner, where you want the focusPoint to be</param>
        /// <returns>The NormalizedScrollPosition the scrollView should use in order to be centered at the given focus point</returns>
        public static Vector2 CalculateNormalizedScrollPosition(this ScrollRect scrollView, Vector2 focusPoint_bottomLeft, Vector2 desiredPosition_bottomLeft)
        {
            Vector2 contentSize = scrollView.content.rect.size;
            Vector2 viewportSize = ((RectTransform)scrollView.content.parent).rect.size;
            Vector2 contentScale = scrollView.content.localScale;
            contentSize.Scale(contentScale);
            focusPoint_bottomLeft.Scale(contentScale);

            Vector2 scrollPosition = scrollView.normalizedPosition;
            if (scrollView.horizontal && contentSize.x > viewportSize.x)
                scrollPosition.x = Mathf.Clamp01((focusPoint_bottomLeft.x - desiredPosition_bottomLeft.x) / (contentSize.x - viewportSize.x));
            if (scrollView.vertical && contentSize.y > viewportSize.y)
                scrollPosition.y = Mathf.Clamp01((focusPoint_bottomLeft.y - desiredPosition_bottomLeft.y) / (contentSize.y - viewportSize.y));

            return scrollPosition;
        }

        public static Vector2 CalculateNormalizedScrollPosition(this ScrollRect scrollView, RectTransform item)
        {
            // content.pivot to item.center
            Vector2 itemCenter_pivot = scrollView.content.InverseTransformPoint(item.transform.TransformPoint(item.rect.center));

            // content.bottomLeft to content.pivot
            Vector2 pivot_bottomLeft = scrollView.content.rect.size;
            pivot_bottomLeft.Scale(scrollView.content.pivot);

            // content.bottomLeft to item.center
            Vector2 itemCenter_bottomLeft = itemCenter_pivot + pivot_bottomLeft;

            return scrollView.CalculateNormalizedScrollPosition(itemCenter_bottomLeft);
        }
        public static Vector2 CalculateNormalizedScrollPosition(this ScrollRect scrollView, RectTransform item, Vector2 desiredPosition_bottomLeft)
        {
            // content.pivot to item.center
            Vector2 itemCenter_pivot = scrollView.content.InverseTransformPoint(item.transform.TransformPoint(item.rect.center));

            // content.bottomLeft to content.pivot
            Vector2 pivot_bottomLeft = scrollView.content.rect.size;
            pivot_bottomLeft.Scale(scrollView.content.pivot);

            // content.bottomLeft to item.center
            Vector2 itemCenter_bottomLeft = itemCenter_pivot + pivot_bottomLeft;

            return scrollView.CalculateNormalizedScrollPosition(itemCenter_bottomLeft, desiredPosition_bottomLeft);
        }

        public static void FocusAtPoint(this ScrollRect scrollView, Vector2 focusPoint)
        {
            scrollView.normalizedPosition = scrollView.CalculateNormalizedScrollPosition(focusPoint);
        }

        public static void FocusOnItem(this ScrollRect scrollView, RectTransform item)
        {
            scrollView.normalizedPosition = scrollView.CalculateNormalizedScrollPosition(item);
        }

        public static Tweener DoFocusAtPoint(this ScrollRect scrollView, Vector2 focusPoint, float duration)
        {

            var targetPosition = scrollView.CalculateNormalizedScrollPosition(focusPoint);
            return scrollView.DONormalizedPos(targetPosition, duration);
        }
        public static Tweener DoFocusOnItem(this ScrollRect scrollView, RectTransform item, float duration)
        {
            var targetPosition = scrollView.CalculateNormalizedScrollPosition(item);
            return scrollView.DONormalizedPos(targetPosition, duration);
        }
        public static Tweener DoFocusOnItem(this ScrollRect scrollView, RectTransform item, Vector2 desiredPosition, float duration)
        {
            var targetPosition = scrollView.CalculateNormalizedScrollPosition(item, desiredPosition);
            return scrollView.DONormalizedPos(targetPosition, duration);
        }
    }
}
