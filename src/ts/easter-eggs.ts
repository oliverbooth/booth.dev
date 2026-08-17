import {registerShortcut} from './input.ts';

const KONAMI_CODE = [
    'ArrowUp', 'ArrowUp',
    'ArrowDown', 'ArrowDown',
    'ArrowLeft', 'ArrowRight',
    'ArrowLeft', 'ArrowRight',
    'b', 'a',
    'Enter'
];

/**
 * Initializes the website's Easter eggs.
 */
export function initEasterEggs(): void {
    registerShortcut(KONAMI_CODE, () => window.open('https://www.youtube.com/watch?v=dQw4w9WgXcQ', '_blank'));
}
