let mathJsPromise: Promise<void> | null = null;

/**
 * Lazily loads mathjs and bridges it onto `window.math`, matching the shape a manim block's raw JS previously got
 * from a plain CDN `<script>` tag (mathjs's browser bundle self-assigns `window.math`) - self-hosted and pinned via
 * package-lock.json now, like manim-web itself, instead of depending on a third-party CDN staying up. Also removes
 * the failure mode that motivated this: a layout that serves manim content but forgets the CDN tag (as `_AdminLayout`
 * did for the live-preview pane) silently breaks any block referencing `math` - now it's a JS-bundle concern, not a
 * per-layout one, so there's nothing for a new or existing layout to forget.
 */
export function ensureMathJsLoaded(): Promise<void> {
    mathJsPromise ??= import('mathjs').then(math => {
        (window as unknown as {math: typeof math}).math = math;
    });

    return mathJsPromise;
}
