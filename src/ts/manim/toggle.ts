import type {Scene, ThreeDScene} from 'manim-web';
import {anchorInOverlay} from './draggable-value.ts';

export interface ToggleOptions<T> {
    /** Horizontal offset, in pixels, from the scene's container's top-left corner. */
    x: number;
    /** Vertical offset, in pixels, from the scene's container's top-left corner. */
    y: number;
    /** Initial value - should be either `onValue` or `offValue`. */
    value: T;
    /** Value reported (and switched to) when the toggle is on. */
    onValue: T;
    /** Value reported (and switched to) when the toggle is off. */
    offValue: T;
    /** Tooltip and accessible name, e.g. `"Toggle angle form"`. */
    label?: string;
    /** Called with the new value whenever it changes via click or keyboard. */
    onChange: (value: T) => void;
}

/**
 * A click-to-switch two-state control, overlaid directly on a manim-web scene's canvas - the toggle counterpart to
 * `DraggableValue`, for flipping between two discrete values (e.g. radians/degrees) rather than scrubbing a
 * continuous range. Generic over the value type so a scene can toggle between anything, not just booleans.
 */
export class Toggle<T> {
    public readonly element: HTMLButtonElement;
    private readonly options: ToggleOptions<T>;
    private currentValue: T;

    /**
     * The current value - reflects a click/keyboard change immediately, not just what the last `onChange` call reported.
     */
    public get value(): T {
        return this.currentValue;
    }

    /**
     * Initializes a new instance of the `Toggle` class.
     * @param scene The scene to overlay this toggle onto - anchored to `scene.getContainer()`.
     * @param options Configuration; see {@link ToggleOptions}.
     */
    constructor(scene: Scene | ThreeDScene, options: ToggleOptions<T>) {
        this.options = options;
        this.currentValue = options.value;

        this.element = document.createElement('button');
        this.element.type = 'button';
        this.element.className = 'manim-toggle';
        this.element.setAttribute('role', 'switch');
        if (options.label) {
            this.element.title = options.label;
            this.element.setAttribute('aria-label', options.label);
        }

        const track = document.createElement('span');
        track.className = 'manim-toggle-track';
        const knob = document.createElement('span');
        knob.className = 'manim-toggle-knob';
        track.append(knob);
        this.element.append(track);

        this.element.addEventListener('click', this.onActivate);

        anchorInOverlay(scene, this.element, options.x, options.y);
        this.render();
    }

    /** Sets the value programmatically, e.g. to keep this in sync with state that changed some other way. Updates the display but does not call `onChange`. */
    public setValue(value: T): void {
        this.currentValue = value;
        this.render();
    }

    /** Removes this toggle's overlay element from the scene. */
    public destroy(): void {
        this.element.remove();
    }

    private readonly onActivate = (): void => {
        this.currentValue = this.currentValue === this.options.onValue ? this.options.offValue : this.options.onValue;
        this.render();
        this.options.onChange(this.currentValue);
    };

    private render(): void {
        const isOn = this.currentValue === this.options.onValue;
        this.element.classList.toggle('on', isOn);
        this.element.setAttribute('aria-checked', String(isOn));
    }
}
